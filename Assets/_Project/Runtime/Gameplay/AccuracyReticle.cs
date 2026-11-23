using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Zombtoy.Gameplay
{
    /// <summary>
    /// Reticula tactica dinamica sobre el cursor del raton que muestra en tiempo real
    /// la precision efectiva del arma equipada (dispersion base, movimiento, retroceso/bloom,
    /// temblor/mania, bloqueo, recarga, fijacion sobre enemigo y hitmarker).
    /// Usa unicamente Image + Sprites procedurales (sin RawImage).
    /// </summary>
    public sealed class AccuracyReticle : MonoBehaviour
    {
        private SandboxSession session;
        private RectTransform canvasRect;
        private RectTransform reticleRoot;
        private Image centerDot, centerOutline;
        private Image spreadRing, spreadRingShadow;
        private Image reloadRing;
        private RectTransform[] tickRects = new RectTransform[4];
        private Image[] tickImages = new Image[4];
        private Image[] tickShadows = new Image[4];
        private RectTransform[] hitRects = new RectTransform[4];
        private Image[] hitImages = new Image[4];
        private Text statusLabel;

        private Texture2D ringTex, dotTex, whiteTex;
        private Sprite ringSprite, dotSprite, whiteSprite;

        private float smoothedRadius = 18f;
        private float shotPunch;
        private float lastTrackedShot = -10f;
        private bool cursorHiddenByReticle;

        private static readonly Color ColorNormal = new Color(0.92f, 0.87f, 0.66f, 0.95f);
        private static readonly Color ColorHighPrecision = new Color(0.45f, 0.96f, 0.72f, 0.95f);
        private static readonly Color ColorTargetLock = new Color(1.00f, 0.34f, 0.24f, 0.98f);
        private static readonly Color ColorLowAccuracy = new Color(1.00f, 0.68f, 0.22f, 0.94f);
        private static readonly Color ColorBlock = new Color(0.38f, 0.88f, 1.00f, 0.92f);
        private static readonly Color ColorReload = new Color(0.84f, 0.80f, 0.68f, 0.75f);
        private static readonly Color ColorShadow = new Color(0.02f, 0.02f, 0.02f, 0.72f);

        public float CurrentReticleRadius => smoothedRadius;

        public void Setup(SandboxSession targetSession, RectTransform hudRoot, Font font)
        {
            session = targetSession;
            canvasRect = hudRoot;

            CreateProceduralSprites();

            var go = new GameObject("AccuracyReticle", typeof(RectTransform));
            go.transform.SetParent(hudRoot, false);
            reticleRoot = go.GetComponent<RectTransform>();
            reticleRoot.anchorMin = Vector2.zero;
            reticleRoot.anchorMax = Vector2.zero;
            reticleRoot.pivot = Vector2.one * 0.5f;
            reticleRoot.sizeDelta = new Vector2(180f, 180f);

            // Outer spread ring (shadow + crisp ring)
            spreadRingShadow = CreateImage("SpreadRingShadow", reticleRoot, ringSprite, ColorShadow, Vector2.one * 42f);
            spreadRing = CreateImage("SpreadRing", reticleRoot, ringSprite, ColorNormal, Vector2.one * 40f);

            // Radial reload / action progress ring
            reloadRing = CreateImage("ReloadRing", reticleRoot, ringSprite, ColorReload, Vector2.one * 34f);
            reloadRing.type = Image.Type.Filled;
            reloadRing.fillMethod = Image.FillMethod.Radial360;
            reloadRing.fillOrigin = (int)Image.Origin360.Top;
            reloadRing.fillClockwise = true;
            reloadRing.fillAmount = 0f;
            reloadRing.enabled = false;

            // 4 cardinal accuracy ticks (0: Top, 1: Bottom, 2: Left, 3: Right)
            for (int i = 0; i < 4; i++)
            {
                var holder = new GameObject("Tick_" + i, typeof(RectTransform)).GetComponent<RectTransform>();
                holder.SetParent(reticleRoot, false);
                holder.anchorMin = holder.anchorMax = holder.pivot = Vector2.one * 0.5f;
                tickRects[i] = holder;

                bool vertical = i < 2;
                Vector2 barSize = vertical ? new Vector2(2.2f, 10f) : new Vector2(10f, 2.2f);
                tickShadows[i] = CreateImage("Shadow", holder, whiteSprite, ColorShadow, barSize + new Vector2(2f, 2f));
                tickImages[i] = CreateImage("Bar", holder, whiteSprite, ColorNormal, barSize);
            }

            // 4 diagonal hitmarker ticks (X)
            for (int i = 0; i < 4; i++)
            {
                var holder = new GameObject("Hit_" + i, typeof(RectTransform)).GetComponent<RectTransform>();
                holder.SetParent(reticleRoot, false);
                holder.anchorMin = holder.anchorMax = holder.pivot = Vector2.one * 0.5f;
                holder.localRotation = Quaternion.Euler(0f, 0f, 45f + i * 90f);
                hitRects[i] = holder;
                hitImages[i] = CreateImage("Bar", holder, whiteSprite, ColorTargetLock, new Vector2(2.2f, 9f));
                hitImages[i].enabled = false;
            }

            // Center precision dot + dark outline
            centerOutline = CreateImage("CenterOutline", reticleRoot, dotSprite, ColorShadow, Vector2.one * 7f);
            centerDot = CreateImage("CenterDot", reticleRoot, dotSprite, ColorNormal, Vector2.one * 4.5f);

            // Micro tactical precision tag below reticle
            var labelGo = new GameObject("PrecisionTag", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(reticleRoot, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = labelRect.anchorMax = labelRect.pivot = Vector2.one * 0.5f;
            labelRect.sizeDelta = new Vector2(150f, 20f);
            labelRect.anchoredPosition = new Vector2(0f, -34f);

            statusLabel = labelGo.GetComponent<Text>();
            statusLabel.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusLabel.fontSize = 11;
            statusLabel.alignment = TextAnchor.MiddleCenter;
            statusLabel.color = ColorNormal;
            statusLabel.raycastTarget = false;
            var outline = labelGo.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1f, -1f);

            reticleRoot.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (session == null || reticleRoot == null || session.player == null)
                return;

            bool combatCursorActive = session.Running
                && session.Hud != null
                && !session.Hud.BlocksCombat
                && Mouse.current != null;

            reticleRoot.gameObject.SetActive(combatCursorActive);
            SetSystemCursorVisible(!combatCursorActive);

            if (!combatCursorActive)
                return;

            var player = session.player;
            var loadout = player.GetComponent<WeaponLoadout>();
            var weapon = loadout != null ? loadout.Current : null;
            int weaponIndex = loadout != null ? loadout.Index : 0;

            // Detect new shot for snappy recoil punch
            if (player.LastShotTime > lastTrackedShot + 0.001f)
            {
                lastTrackedShot = player.LastShotTime;
                shotPunch = Mathf.Min(16f, shotPunch + (weaponIndex == 2 || weaponIndex == 3 ? 9f : 4.5f));
            }
            shotPunch = Mathf.MoveTowards(shotPunch, 0f, Time.unscaledDeltaTime * 48f);

            // Position reticle at exact mouse cursor in canvas coordinates
            Vector2 screenPos = Mouse.current.position.ReadValue();
            if (canvasRect != null && Screen.width > 0 && Screen.height > 0)
            {
                Vector2 canvasSize = canvasRect.rect.size;
                Vector2 anchored = new Vector2(
                    (screenPos.x / Screen.width) * canvasSize.x,
                    (screenPos.y / Screen.height) * canvasSize.y);

                // Subtle tremor shake when under serum tremor or mania
                bool shaky = session.State.elapsed < session.State.tremorUntil || session.State.IsManic;
                if (shaky)
                {
                    float t = Time.unscaledTime * 38f;
                    anchored += new Vector2(Mathf.Sin(t) * 2.5f, Mathf.Cos(t * 1.37f) * 2.5f);
                }

                reticleRoot.anchoredPosition = anchored;
            }

            // Compute real visual spread radius from actual ballistic cone + camera scale
            float spreadDeg = player.CurrentEffectiveSpreadDegrees;
            float targetRadius = ComputeReticleRadiusPixels(player, spreadDeg, weaponIndex) + shotPunch;
            float smoothFactor = 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime);
            smoothedRadius = Mathf.Lerp(smoothedRadius, targetRadius, smoothFactor);

            // Determine tactical color & state
            bool reloadingOrBusy = session.Busy;
            bool outOfAmmo = session.rounds <= 0 && !reloadingOrBusy;
            bool blocking = player.IsBlocking;
            bool aiming = player.IsAiming;
            bool crouching = player.IsCrouching;
            bool targetLocked = player.PreciseAim;
            bool isShotgun = weapon != null && weapon.pellets > 1;
            bool isSniper = weaponIndex == 3;
            bool isHeavy = weaponIndex == 5;

            Color primaryColor;
            if (blocking) primaryColor = ColorBlock;
            else if (reloadingOrBusy) primaryColor = ColorReload;
            else if (outOfAmmo) primaryColor = Color.Lerp(ColorTargetLock, ColorLowAccuracy, Mathf.PingPong(Time.unscaledTime * 4f, 1f));
            else if (targetLocked) primaryColor = ColorTargetLock;
            else if (aiming || spreadDeg <= 1.25f) primaryColor = ColorHighPrecision;
            else if (spreadDeg >= 5.5f) primaryColor = ColorLowAccuracy;
            else primaryColor = ColorNormal;

            // Update outer spread ring (always shows real dispersion cone; strongest for shotgun/high spread)
            float ringDiameter = Mathf.Clamp(smoothedRadius * 2f, 14f, 176f);
            spreadRing.rectTransform.sizeDelta = Vector2.one * ringDiameter;
            spreadRingShadow.rectTransform.sizeDelta = Vector2.one * (ringDiameter + 2.5f);

            float ringAlpha = isShotgun ? 0.88f : isSniper ? 0.36f : Mathf.Lerp(0.38f, 0.78f, Mathf.Clamp01(spreadDeg / 8f));
            if (reloadingOrBusy) ringAlpha *= 0.45f;
            spreadRing.color = new Color(primaryColor.r, primaryColor.g, primaryColor.b, ringAlpha);
            spreadRingShadow.color = new Color(0.02f, 0.02f, 0.02f, ringAlpha * 0.65f);

            // Update 4 cardinal ticks (adapted per weapon archetype)
            float tickLength = isSniper ? 14f : isShotgun ? 7f : isHeavy ? 11f : 9f;
            float tickThickness = isSniper ? 1.6f : isHeavy ? 2.8f : isShotgun ? 2.6f : 2.1f;
            float tickOffset = smoothedRadius + tickLength * 0.5f;

            Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            for (int i = 0; i < 4; i++)
            {
                bool vertical = i < 2;
                Vector2 size = vertical ? new Vector2(tickThickness, tickLength) : new Vector2(tickLength, tickThickness);
                tickRects[i].anchoredPosition = dirs[i] * tickOffset;
                tickImages[i].rectTransform.sizeDelta = size;
                tickShadows[i].rectTransform.sizeDelta = size + new Vector2(2f, 2f);
                tickImages[i].color = primaryColor;
                tickImages[i].enabled = !reloadingOrBusy;
                tickShadows[i].enabled = !reloadingOrBusy;
            }

            // Center dot
            centerDot.color = targetLocked ? ColorTargetLock : primaryColor;
            float dotSize = isSniper ? 3.5f : 4.5f;
            centerDot.rectTransform.sizeDelta = Vector2.one * dotSize;
            centerOutline.rectTransform.sizeDelta = Vector2.one * (dotSize + 2.5f);

            // Radial reload / busy progress ring
            reloadRing.enabled = reloadingOrBusy;
            if (reloadingOrBusy)
            {
                reloadRing.fillAmount = Mathf.Clamp01(session.ActionProgress);
                reloadRing.rectTransform.sizeDelta = Vector2.one * Mathf.Max(30f, ringDiameter * 0.72f);
                reloadRing.color = ColorNormal;
            }

            // Hitmarker X flash
            float hitAlpha = Mathf.Clamp01(session.HitFlash);
            bool showHit = hitAlpha > 0.04f;
            float hitOffset = 9f + (1f - hitAlpha) * 6f;
            Vector2[] diagDirs =
            {
                new Vector2(0.7071f, 0.7071f),
                new Vector2(-0.7071f, 0.7071f),
                new Vector2(-0.7071f, -0.7071f),
                new Vector2(0.7071f, -0.7071f)
            };
            for (int i = 0; i < 4; i++)
            {
                hitImages[i].enabled = showHit;
                if (showHit)
                {
                    hitRects[i].anchoredPosition = diagDirs[i] * hitOffset;
                    hitImages[i].color = new Color(1f, 0.35f, 0.22f, hitAlpha);
                }
            }

            // Compact precision readout below the bottom tick
            var labelRect = statusLabel.rectTransform;
            labelRect.anchoredPosition = new Vector2(0f, -(smoothedRadius + tickLength + 12f));
            statusLabel.color = new Color(primaryColor.r, primaryColor.g, primaryColor.b, 0.92f);

            if (reloadingOrBusy)
            {
                statusLabel.text = session.Reloading
                    ? $"RECARGANDO {session.CurrentCaliberShort} {Mathf.RoundToInt(session.ActionProgress * 100f)}%"
                    : session.ActionLabel.ToUpperInvariant();
            }
            else if (blocking)
            {
                statusLabel.text = "GUARDIA / PARRY [Z]";
            }
            else if (outOfAmmo)
            {
                statusLabel.text = $"VACIO {session.CurrentCaliberShort} [R]";
            }
            else
            {
                int precisionPct = Mathf.RoundToInt(player.CurrentAccuracyNormalized * 100f);
                string stancePrefix = aiming && crouching ? "ADS+SIG " : aiming ? "ADS " : crouching ? "SIGILO " : "";
                if (isShotgun)
                    statusLabel.text = $"{stancePrefix}±{spreadDeg:0.0}° · {weapon.pellets}x";
                else
                    statusLabel.text = $"{stancePrefix}PREC {precisionPct}% · ±{spreadDeg:0.1}°";
            }
        }

        private float ComputeReticleRadiusPixels(SandboxPlayer player, float spreadDeg, int weaponIndex)
        {
            // Combine true physical cone projection at aim distance with readable angular scaling
            float physicalPixels = 16f;
            if (player.view != null && player.view.orthographic && player.view.orthographicSize > 0.1f)
            {
                float aimDist = Mathf.Clamp(Vector3.Distance(player.transform.position, player.AimWorldPoint), 3.5f, 16f);
                float worldRadius = aimDist * Mathf.Tan(spreadDeg * Mathf.Deg2Rad);
                float canvasUnitsPerMeter = 720f / (2f * player.view.orthographicSize);
                physicalPixels = worldRadius * canvasUnitsPerMeter;
            }

            // Blend with angular readability floor so sniper stays razor-tight and shotgun stays clearly wide
            float angularFloor = weaponIndex == 3
                ? Mathf.Lerp(5.5f, 34f, Mathf.Clamp01(spreadDeg / 8f))
                : Mathf.Lerp(8f, 64f, Mathf.Clamp01(spreadDeg / 10f));

            float combined = Mathf.Lerp(angularFloor, physicalPixels, 0.55f);
            return Mathf.Clamp(combined, weaponIndex == 3 ? 6f : 9f, 82f);
        }

        private void SetSystemCursorVisible(bool visible)
        {
            if (visible)
            {
                if (cursorHiddenByReticle)
                {
                    Cursor.visible = true;
                    cursorHiddenByReticle = false;
                }
            }
            else
            {
                Cursor.visible = false;
                cursorHiddenByReticle = true;
            }
        }

        private void CreateProceduralSprites()
        {
            // 1. White pixel sprite for ticks and hitmarkers
            whiteTex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var whitePixels = new Color32[16];
            for (int i = 0; i < whitePixels.Length; i++) whitePixels[i] = new Color32(255, 255, 255, 255);
            whiteTex.SetPixels32(whitePixels);
            whiteTex.Apply();
            whiteSprite = Sprite.Create(whiteTex, new Rect(0, 0, 4, 4), Vector2.one * 0.5f);

            // 2. Anti-aliased 128x128 thin circle ring for spread cone & radial reload progress
            const int ringSize = 128;
            ringTex = new Texture2D(ringSize, ringSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var ringPixels = new Color32[ringSize * ringSize];
            float half = (ringSize - 1) * 0.5f;
            float outerR = half - 2f;
            float thickness = 3.2f;
            for (int y = 0; y < ringSize; y++)
            {
                for (int x = 0; x < ringSize; x++)
                {
                    float dx = x - half;
                    float dy = y - half;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float delta = Mathf.Abs(dist - (outerR - thickness * 0.5f));
                    float alpha = Mathf.Clamp01(1f - Mathf.Max(0f, delta - thickness * 0.35f) / 1.4f);
                    byte a = (byte)Mathf.RoundToInt(alpha * 255f);
                    ringPixels[y * ringSize + x] = new Color32(255, 255, 255, a);
                }
            }
            ringTex.SetPixels32(ringPixels);
            ringTex.Apply();
            ringSprite = Sprite.Create(ringTex, new Rect(0, 0, ringSize, ringSize), Vector2.one * 0.5f);

            // 3. Anti-aliased 32x32 center dot
            const int dotSize = 32;
            dotTex = new Texture2D(dotSize, dotSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var dotPixels = new Color32[dotSize * dotSize];
            float dHalf = (dotSize - 1) * 0.5f;
            float dRad = dHalf - 2f;
            for (int y = 0; y < dotSize; y++)
            {
                for (int x = 0; x < dotSize; x++)
                {
                    float dx = x - dHalf;
                    float dy = y - dHalf;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01((dRad - dist + 1.2f) / 2.2f);
                    dotPixels[y * dotSize + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }
            dotTex.SetPixels32(dotPixels);
            dotTex.Apply();
            dotSprite = Sprite.Create(dotTex, new Rect(0, 0, dotSize, dotSize), Vector2.one * 0.5f);
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * 0.5f;
            rect.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        private void OnDisable()
        {
            SetSystemCursorVisible(true);
        }

        private void OnDestroy()
        {
            SetSystemCursorVisible(true);
            if (ringSprite != null) Destroy(ringSprite);
            if (dotSprite != null) Destroy(dotSprite);
            if (whiteSprite != null) Destroy(whiteSprite);
            if (ringTex != null) Destroy(ringTex);
            if (dotTex != null) Destroy(dotTex);
            if (whiteTex != null) Destroy(whiteTex);
        }
    }
}
