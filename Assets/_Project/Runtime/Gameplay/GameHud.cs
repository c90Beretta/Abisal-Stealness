using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Zombtoy.Gameplay
{
    // Native screen-space UI above the isometric 3D world.
    public sealed class GameHud : MonoBehaviour
    {
        private SandboxSession session;
        private Font font;
        private RectTransform root, gameplay, menu, help, equipment;
        private Text health, serum, ammo, weaponTitle, inventory, objective, prompt, message, action, heading, subtitle, stats, objectiveMarker;
        private Image healthFill, serumFill, actionFill, damage;
        private Button primary, secondary, controls, exit;
        private Text primaryLabel, secondaryLabel;
        private bool showingHelp, showingEquipment, checkpointAvailable;
        private SessionMode previous = (SessionMode)(-1);
        private readonly Dictionary<SandboxEnemy, RectTransform> enemyBars = new Dictionary<SandboxEnemy, RectTransform>();
        private readonly Dictionary<SandboxEnemy, Image> enemyFills = new Dictionary<SandboxEnemy, Image>();
        private static readonly Color Ink = new Color(0.025f, 0.055f, 0.055f, 0.96f);
        private static readonly Color Green = new Color(0.55f, 0.94f, 0.72f);
        private static readonly Color Muted = new Color(0.67f, 0.77f, 0.74f);
        private Sprite whiteSprite;
        public bool BlocksCombat => showingHelp || showingEquipment || session.Mode != SessionMode.Playing;

        public void Setup(SandboxSession value)
        {
            session = value; font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var white = Texture2D.whiteTexture;
            whiteSprite = Sprite.Create(white, new Rect(0, 0, white.width, white.height), Vector2.one * 0.5f);
            root = Node("ABISAL HUD", transform);
            var canvas = root.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 100;
            var scaler = root.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 0.5f;
            root.gameObject.AddComponent<GraphicRaycaster>();
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("UI input", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            damage = Panel("Damage feedback", root, Color.clear, Vector2.zero, Vector2.one);
            gameplay = Node("Mission HUD", root); Stretch(gameplay);
            var top = Panel("Mission strip", gameplay, Ink, new Vector2(0, 1), new Vector2(1, 1));
            top.rectTransform.pivot = new Vector2(0.5f, 1); top.rectTransform.sizeDelta = new Vector2(0, 86);
            Label(top.transform, "ABISAL   /   PUERTO 01", 18, Green, 28, -14, 700, 24);
            objective = Label(top.transform, "", 21, Color.white, 28, -44, 1050, 30);
            Label(top.transform, "ESC  PAUSA     F1  AYUDA", 14, Muted, -250, -16, 225, 24, true);
            var vital = Panel("Vital signs", gameplay, Ink, new Vector2(0, 0), new Vector2(0, 0));
            Place(vital.rectTransform, 24, 214, 310, 190, false);
            health = Label(vital.transform, "VIDA", 18, Color.white, 16, -12, 280, 25);
            healthFill = Bar(vital.transform, 16, -45, 278, 9, Green);
            serum = Label(vital.transform, "SUERO", 16, Muted, 16, -66, 280, 24);
            serumFill = Bar(vital.transform, 16, -99, 278, 7, Green);
            nightStatus = Label(vital.transform, "", 15, Muted, 16, -119, 282, 65);
            var weapon = Panel("Weapon and equipment", gameplay, Ink, new Vector2(1, 0), new Vector2(1, 0));
            Place(weapon.rectTransform, -346, 178, 322, 154, false);
            weaponTitle = Label(weapon.transform, "", 15, Green, 18, -10, 290, 22);
            ammo = Label(weapon.transform, "", 32, Color.white, 18, -36, 290, 42);
            inventory = Label(weapon.transform, "", 15, Muted, 18, -86, 290, 54);
            prompt = Label(gameplay, "", 22, Green, 0, 118, 670, 34, false, true);
            message = Label(gameplay, "", 17, Color.white, 0, 76, 660, 50, false, true);
            action = Label(gameplay, "", 18, Green, 0, 189, 430, 30, false, true);
            actionFill = Bar(gameplay, 0, 153, 330, 7, Green, true);
            Label(gameplay, "WASD mover   /   RATON apuntar/disparar   /   E interactuar   /   R recargar   /   TAB equipo", 14, Muted, 0, 18, 900, 24, false, true);
            var markerBack = Panel("Objective marker", gameplay, Ink, Vector2.one * 0.5f, Vector2.one * 0.5f);
            markerBack.rectTransform.sizeDelta = new Vector2(200, 30);
            objectiveMarker = Label(markerBack.transform, "", 16, Green, 8, -5, 184, 23);

            menu = Node("Mission menu", root); Stretch(menu);
            Panel("Menu shade", menu, new Color(0.015f, 0.025f, 0.03f, 0.68f), Vector2.zero, Vector2.one).raycastTarget = true;
            var card = Panel("Mission card", menu, Ink, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            card.rectTransform.pivot = new Vector2(0.5f, 0.5f); card.rectTransform.sizeDelta = new Vector2(900, 542);
            Label(card.transform, "ABISAL", 52, Green, 36, -28, 790, 70);
            heading = Label(card.transform, "", 27, Color.white, 38, -110, 800, 42);
            subtitle = Label(card.transform, "", 18, Muted, 38, -166, 800, 86);
            primary = Button(card.transform, "Comenzar nivel", 38, -280, () => { if (session.Mode == SessionMode.Title) session.BeginMission(); else if (session.Mode == SessionMode.Paused) session.Resume(); else if (session.Mode == SessionMode.Dead || session.Mode == SessionMode.Complete) session.Restart(); }, out primaryLabel);
            secondary = Button(card.transform, "Continuar checkpoint", 38, -340, () => session.LoadCheckpoint(), out secondaryLabel);
            controls = Button(card.transform, "Controles / F1", 480, -280, ToggleHelp, out _);
            exit = Button(card.transform, "Salir del juego", 480, -340, session.Quit, out _);
            stats = Label(card.transform, "", 16, Muted, 38, -424, 800, 84);

            help = Node("Controls", root); Stretch(help);
            Panel("Controls shade", help, new Color(0, 0, 0, 0.9f), Vector2.zero, Vector2.one).raycastTarget = true;
            var helpCard = Panel("Controls card", help, Ink, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            helpCard.rectTransform.sizeDelta = new Vector2(860, 580); helpCard.rectTransform.pivot = Vector2.one * 0.5f;
            Label(helpCard.transform, "CONTROLES / SUPERVIVENCIA", 26, Green, 30, -24, 800, 45);
            Label(helpCard.transform, "WASD  Moverte     SHIFT  Correr (produce mas ruido)\nRATON  Apuntar     CLIC IZQUIERDO  Disparar\n1 / 2 / 3  Cambiar arma     RUEDA  Zoom\nESPACIO  Cuchillo / hacha (consume resistencia)     R  Recargar\nE  Interactuar / abrir botin     TAB  Mochila y equipo\nH  Botiquin (+40 vida)     B  Venda (detiene hemorragia)\nQ  Suero     F  Linterna (pilas y oxigeno en TAB)\nESC  Pausa     F1  Esta ayuda\nF5  Guardar lejos de enemigos     F9  Recuperar checkpoint\n\nLos infectados oyen disparos y pasos. Su ataque tiene una\npreparacion: alejate antes del golpe. Curarte lleva tiempo\ny recibir un golpe interrumpe el tratamiento.\nMascara + O2 protegen de la fuga toxica del generador.\n\nRuta: ALMACEN 03 > GENERADOR > CONTROL > SUBMARINO.\nEl generador guarda automaticamente tu progreso.", 20, Color.white, 30, -88, 800, 414);
            Button(helpCard.transform, "Cerrar ayuda / F1", 30, -510, ToggleHelp, out _);
            help.gameObject.SetActive(false);
            equipment = Node("Equipment", root); Stretch(equipment);
            var gear = Panel("Equipment card", equipment, Ink, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            gear.rectTransform.sizeDelta = new Vector2(1000, 550); gear.rectTransform.pivot = Vector2.one * 0.5f;
            gear.raycastTarget = true;
            Label(gear.transform, "EQUIPO / TAB PARA CERRAR", 24, Green, 24, -18, 600, 40);
            equipmentText = Label(gear.transform, "", 21, Color.white, 24, -76, 600, 150);
            equipmentText.gameObject.SetActive(session.GetComponent<NightSurvival>() == null);
            if (session.GetComponent<NightSurvival>() != null) { fieldHud = gameObject.AddComponent<NightHud>(); fieldHud.Setup(session, gear.transform); }
            equipment.gameObject.SetActive(false);
        }
        private Text equipmentText, nightStatus;
        private NightHud fieldHud;
        public void OpenLoot(LootContainer box) { showingEquipment = true; equipment.gameObject.SetActive(true); fieldHud?.Select(box); }

        public void ToggleHelp() { showingHelp = !showingHelp; if (showingHelp) { showingEquipment = false; equipment.gameObject.SetActive(false); } help.gameObject.SetActive(showingHelp); }
        public void ToggleEquipment() { showingEquipment = !showingEquipment; if (showingEquipment) { showingHelp = false; help.gameObject.SetActive(false); } equipment.gameObject.SetActive(showingEquipment); }
        public void ClosePanels() { showingHelp = showingEquipment = false; help.gameObject.SetActive(false); equipment.gameObject.SetActive(false); }

        private void LateUpdate()
        {
            if (session == null) return;
            var state = session.State;
            gameplay.gameObject.SetActive(session.Mode == SessionMode.Playing || session.Mode == SessionMode.Paused);
            menu.gameObject.SetActive(session.Mode != SessionMode.Playing);
            if (session.Mode != previous)
            {
                previous = session.Mode; checkpointAvailable = session.HasCheckpoint;
                bool title = previous == SessionMode.Title, paused = previous == SessionMode.Paused, win = previous == SessionMode.Complete;
                heading.text = title ? "01 / PUERTO CARGUERO" : paused ? "PARTIDA EN PAUSA" : win ? "EMBARQUE COMPLETADO" : "HAS CAIDO";
                subtitle.text = title ? "El puerto esta sin energia. Recupera el repuesto del almacen,\nreactiva el generador y abre la ruta al submarino." : paused ? "Tu progreso se guarda al reparar el generador.\nTambien puedes guardar con F5 cuando estes a salvo." : win ? "Has sobrevivido al puerto. El submarino esta listo para partir." : "Una herida sin tratar puede ser mortal. Usa vendas y botiquines.\nRecupera tu checkpoint o vuelve a intentarlo.";
                primaryLabel.text = title ? "Comenzar nivel / Enter" : paused ? "Continuar / Esc" : "Reintentar / Enter";
                secondary.interactable = checkpointAvailable;
                secondaryLabel.text = checkpointAvailable ? "Continuar checkpoint" : "Sin checkpoint guardado";
                stats.text = title ? "Primera partida: pulsa COMENZAR o ENTER.\nWASD mover  /  raton apuntar  /  clic disparar  /  E interactuar  /  F1 ayuda" : $"Tiempo {Mathf.FloorToInt(state.elapsed / 60):00}:{Mathf.FloorToInt(state.elapsed % 60):00}    Infectados abatidos {session.Kills}    Impactos {session.Hits}\nMunicion {session.rounds + session.reserve}    Vida {Mathf.CeilToInt(state.health)}    Suero {Mathf.CeilToInt(state.serum)}%";
            }
            health.text = $"VIDA  {Mathf.CeilToInt(state.health):000}" + (state.bleeding > 0 ? "   / HEMORRAGIA [B]" : "");
            health.color = state.bleeding > 0 || state.health < 30 ? new Color(1, 0.55f, 0.46f) : Color.white;
            healthFill.fillAmount = state.health / 100;
            serum.text = $"SUERO  {Mathf.CeilToInt(state.serum)}%    INFECCION {Mathf.CeilToInt(state.infection)}%";
            serumFill.fillAmount = state.serum / 100;
            var loadout = session.player.GetComponent<WeaponLoadout>();
            string weaponName = loadout != null && loadout.Current != null ? loadout.Current.displayName : "Pistola";
            weaponTitle.text = "[1 / 2 / 3] " + weaponName;
            ammo.text = $"{session.rounds:00} <size=19>/ {session.reserve:000}   [R]</size>";
            inventory.text = $"[H] Botiquin {state.medkits}    [B] Venda {state.bandages}\n[Q] Suero {state.doses}    [F] Linterna";
            equipmentText.text = $"Botiquines: {state.medkits}    Vendas: {state.bandages}    Suero: {state.doses}\nMunicion: {session.rounds} en arma / {session.reserve} en reserva\nRepuesto: {(session.portMission != null && session.portMission.HasFuse ? "recogido" : "pendiente")}\nEste panel no pausa la partida.";
            if (session.Night != null)
            {
                var n = session.Night; var b = n.Inventory;
                nightStatus.text = $"RESISTENCIA {b.stamina:0}%   O2 {b.air:0}%\n[F] LUZ {(n.LampLit ? "ON" : "OFF")} {b.battery:0}%   RUIDO {n.NoiseRadius:0} m\n{(n.InGas ? "FUGA TOXICA / MASCARA" : "Hordas " + n.hordes.waves + " / Reserva " + n.hordes.Remaining)}";
            }
            objective.text = session.portMission != null ? session.portMission.Objective : "Activa la terminal y alcanza la extraccion.";
            var pickup = session.NearestPickup();
            var loot = session.Night != null ? session.Night.NearestLoot() : null;
            prompt.text = loot != null ? "[E] Registrar " + loot.label : pickup != null ? "[E] " + pickup.label : session.portMission != null ? session.portMission.Prompt : "";
            message.text = Time.unscaledTime < session.MessageUntil ? session.Message : "";
            action.text = session.ActionLabel;
            actionFill.transform.parent.gameObject.SetActive(session.Busy);
            actionFill.fillAmount = session.ActionProgress;
            damage.color = new Color(0.6f, 0, 0, session.DamageFlash * 0.2f);
            UpdateMarkers();
        }

        private void UpdateMarkers()
        {
            var camera = session.player.view;
            objectiveMarker.transform.parent.gameObject.SetActive(session.portMission != null && session.Mode == SessionMode.Playing);
            if (session.portMission != null && session.Mode == SessionMode.Playing)
            {
                Vector3 target = session.portMission.ActiveObjective.position;
                Vector3 viewport = camera.WorldToViewportPoint(target + Vector3.up * 1.5f);
                Vector2 pos = new Vector2(Mathf.Clamp(viewport.x, 0.13f, 0.87f), Mathf.Clamp(viewport.y, 0.27f, 0.83f));
                var rect = (RectTransform)objectiveMarker.transform.parent; rect.anchorMin = rect.anchorMax = pos; rect.anchoredPosition = Vector2.zero;
                rect.pivot = Vector2.one * 0.5f;
                objectiveMarker.text = $"  OBJETIVO  {Vector3.Distance(target, session.player.transform.position):0}m";
            }
            else objectiveMarker.text = "";
            if (session.Enemies == null) return;
            foreach (var enemy in session.Enemies)
            {
                if (enemy == null) continue;
                if (!enemyBars.TryGetValue(enemy, out var rect))
                {
                    var back = Panel("Infected health", gameplay, Ink, Vector2.zero, Vector2.zero);
                    rect = back.rectTransform; rect.sizeDelta = new Vector2(62, 6); rect.pivot = Vector2.one * 0.5f;
                    var fill = Panel("Health", rect, new Color(0.9f, 0.36f, 0.27f), Vector2.zero, Vector2.one);
                    fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal;
                    enemyBars.Add(enemy, rect); enemyFills.Add(enemy, fill);
                }
                Vector3 p = camera.WorldToViewportPoint(enemy.transform.position + Vector3.up * 2.1f);
                bool perceived = enemy.GetComponent<InfectedVisibility>() == null || enemy.GetComponent<InfectedVisibility>().Visible;
                bool visible = perceived && session.Running && enemy.gameObject.activeInHierarchy && !enemy.IsDead && (enemy.Alerted || enemy.HealthFraction < 1) && p.z > 0 && p.x > 0 && p.x < 1 && p.y > 0.2f && p.y < 0.85f;
                rect.gameObject.SetActive(visible);
                if (visible) { rect.anchorMin = rect.anchorMax = new Vector2(p.x, p.y); rect.anchoredPosition = Vector2.zero; enemyFills[enemy].fillAmount = enemy.HealthFraction; }
            }
        }

        private RectTransform Node(string name, Transform parent)
        {
            var node = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); node.SetParent(parent, false); return node;
        }
        private static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private Image Panel(string name, Transform parent, Color color, Vector2 min, Vector2 max)
        {
            var rect = Node(name, parent); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = rect.gameObject.AddComponent<Image>(); image.sprite = whiteSprite; image.color = color; image.raycastTarget = false; return image;
        }
        private static void Place(RectTransform rect, float x, float y, float w, float h, bool top = true)
        {
            rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(w, h);
        }
        private Text Label(Transform parent, string value, int size, Color color, float x, float y, float w, float h, bool right = false, bool bottomCenter = false)
        {
            var rect = Node(value.Length > 40 ? value.Substring(0, 40) : value, parent);
            rect.anchorMin = rect.anchorMax = bottomCenter ? new Vector2(0.5f, 0) : right ? Vector2.one : new Vector2(0, 1);
            Place(rect, x, y, w, h); if (bottomCenter) { rect.pivot = new Vector2(0.5f, 0); rect.anchoredPosition = new Vector2(x, y); }
            var text = rect.gameObject.AddComponent<Text>(); text.font = font; text.fontSize = size; text.color = color; text.text = value; text.raycastTarget = false;
            text.alignment = bottomCenter ? TextAnchor.MiddleCenter : TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate; return text;
        }
        private Image Bar(Transform parent, float x, float y, float w, float h, Color color, bool bottomCenter = false)
        {
            var back = Panel("Gauge", parent, new Color(0.15f, 0.25f, 0.23f), bottomCenter ? new Vector2(0.5f, 0) : new Vector2(0, 1), bottomCenter ? new Vector2(0.5f, 0) : new Vector2(0, 1));
            Place(back.rectTransform, x, y, w, h);
            if (bottomCenter) back.rectTransform.pivot = new Vector2(0.5f, 0);
            var fill = Panel("Fill", back.transform, color, Vector2.zero, Vector2.one); fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; return fill;
        }
        private Button Button(Transform parent, string label, float x, float y, UnityEngine.Events.UnityAction action, out Text text)
        {
            var image = Panel(label, parent, new Color(0.14f, 0.28f, 0.23f), new Vector2(0, 1), new Vector2(0, 1));
            Place(image.rectTransform, x, y, 382, 48); image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = new Color(1.4f, 1.4f, 1.4f); colors.selectedColor = colors.highlightedColor; colors.disabledColor = new Color(0.45f, 0.5f, 0.48f); button.colors = colors;
            button.onClick.AddListener(action);
            text = Label(image.transform, label, 20, Color.white, 16, -11, 350, 30); return button;
        }
        private void OnDestroy() { if (whiteSprite != null) Destroy(whiteSprite); }
    }
}
