using UnityEngine;
using UnityEngine.UI;
using Zombtoy.Core;

namespace Zombtoy.Gameplay
{
    // Extends the existing scene without duplicating encounter or checkpoint identities.
    public sealed class ArsenalExpansion : MonoBehaviour
    {
        private SandboxSession session;
        private SurvivorRoster roster;
        private GameObject panel;
        private Text summary;
        private int role, upgrade, selected, selectedMelee;
        private bool applied;
        private readonly System.Collections.Generic.Dictionary<Button,Text> choices=new System.Collections.Generic.Dictionary<Button,Text>();
        private readonly string[] kits = { "Placa reforzada", "Suministros medicos", "Reserva de municion" };

        public void Initialize(SandboxSession value)
        {
            session = value;
            var loadout = session.player.GetComponent<WeaponLoadout>();
            if (loadout == null || loadout.weapons.Length < 3) return;

            Material weaponMat = null;
            if (loadout.weapons[0] != null && loadout.weapons[0].visualPrefab != null)
            {
                var r = loadout.weapons[0].visualPrefab.GetComponentInChildren<Renderer>();
                if (r != null) weaponMat = r.sharedMaterial;
            }
            if (weaponMat == null) weaponMat = session.tracerMaterial;

            var originals = loadout.weapons;
            loadout.weapons = new WeaponDefinition[6];
            UpgradeExistingWeapon(loadout, 0, originals[0], "Pistola DP-9", 36f, .20f, 1.1f, 1, 26f, 16f, weaponMat);
            UpgradeExistingWeapon(loadout, 1, originals[1], "Carabina C3", 30f, .12f, 2.0f, 1, 32f, 22f, weaponMat);
            UpgradeExistingWeapon(loadout, 2, originals[2], "Escopeta S12", 19f, .62f, 6.2f, 6, 17f, 28f, weaponMat);
            CreateWeapon(loadout, 3, originals[1], "Fusil de precision", 115f, 1.15f, .12f, 1, 65f, 30f, weaponMat);
            CreateWeapon(loadout, 4, originals[1], "Subfusil", 24f, .082f, 2.6f, 1, 25f, 14f, weaponMat);
            CreateWeapon(loadout, 5, originals[1], "Ametralladora pesada", 50f, .125f, 3.6f, 1, 28f, 30f, weaponMat);
            loadout.Equip(0, true);

            roster = session.player.GetComponent<SurvivorRoster>() ?? session.player.gameObject.AddComponent<SurvivorRoster>();
            roster.Initialize(session);
            session.player.AttachRoster(roster);

            if (session.GetComponent<PortMapExpander>() == null)
                session.gameObject.AddComponent<PortMapExpander>().Initialize(session);

            int special = 0;
            var encounters = (SandboxEnemy[])session.Enemies.Clone();
            System.Array.Sort(encounters, (a, b) => string.CompareOrdinal(a.name, b.name));
            foreach (var enemy in encounters)
            {
                enemy.gameObject.AddComponent<AttackPose>();
                if (special < 3 && enemy.gameObject.activeInHierarchy)
                    enemy.gameObject.AddComponent<SpecialInfected>().Initialize(session, (SpecialSpecies)special++);
            }
            session.player.gameObject.AddComponent<AttackPose>();
            BuildMenu();
            TacticalAudio.Ambience(gameObject);
        }

        private void UpgradeExistingWeapon(WeaponLoadout loadout, int index, WeaponDefinition basis, string title, float damage, float interval, float spread, int pellets, float range, float noise, Material mat)
        {
            var weapon = Instantiate(basis);
            weapon.displayName = title;
            weapon.damage = damage;
            weapon.fireInterval = interval;
            weapon.spread = spread;
            weapon.pellets = pellets;
            weapon.range = range;
            weapon.noiseRadius = noise;
            weapon.visualPrefab = ModelUpgradeBuilder.CreateFirearmTemplate(index, title, transform, mat);
            loadout.weapons[index] = weapon;
        }

        private void CreateWeapon(WeaponLoadout loadout, int index, WeaponDefinition basis, string title, float damage, float interval, float spread, int pellets, float range, float noise, Material mat)
        {
            var weapon = Instantiate(basis);
            weapon.displayName = title;
            weapon.damage = damage;
            weapon.range = range;
            weapon.fireInterval = interval;
            weapon.spread = spread;
            weapon.pellets = pellets;
            weapon.noiseRadius = noise;
            weapon.visualPrefab = ModelUpgradeBuilder.CreateFirearmTemplate(index, title, transform, mat);
            loadout.weapons[index] = weapon;
        }

        private void BuildMenu()
        {
            var canvas = new GameObject("Deployment armory", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.transform.SetParent(transform);
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.GetComponent<Canvas>().sortingOrder = 101;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = .5f;

            panel = new GameObject("Weapon locker", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            var r = panel.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(1, .5f);
            r.pivot = new Vector2(1, .5f);
            r.anchoredPosition = new Vector2(-30, 0);
            r.sizeDelta = new Vector2(665, 670);
            panel.GetComponent<Image>().color = new Color(.025f, .023f, .019f, .98f);

            Label("PREPARAR DESPLIEGUE // ESCUADRON ABISAL", 24, 22, 14, 620, 34);
            Label("ESPECIALISTA (4 PERSONAJES / F3-F4 EN PARTIDA)", 16, 22, 50, 620, 24);
            for (int i = 0; i < 4; i++)
            {
                int n = i;
                var p = SurvivorRoster.Profiles[i];
                Button(p.ButtonId, p.ShortButton, 22 + (i % 2) * 312, 76 + (i / 2) * 42, 302, 38, () =>
                {
                    role = n;
                    if (roster != null) roster.ApplyCharacter(n, true);
                });
            }

            Label("ARMA INICIAL / LLEVAS LAS SEIS [1-6 / T EN PARTIDA]", 16, 22, 168, 620, 24);
            var names = new[] { "1  Arma corta", "2  Fusil de asalto", "3  Escopeta", "4  Precision", "5  Subfusil", "6  Arma pesada" };
            for (int i = 0; i < 6; i++)
            {
                int n = i;
                Button(names[i], names[i], 22 + (i % 2) * 312, 194 + (i / 2) * 42, 302, 38, () => selected = n);
            }

            Label("CUERPO A CUERPO [C / ESPACIO / G / CLIC DER EN PARTIDA]", 16, 22, 326, 620, 24);
            for (int i = 0; i < 4; i++)
            {
                int n = i;
                Button(MeleeSwing.WeaponNames[i], MeleeSwing.WeaponNames[i], 22 + (i % 2) * 312, 352 + (i / 2) * 40, 302, 36, () => selectedMelee = n);
            }

            Label("KIT TACTICO DE SALIDA", 16, 22, 438, 620, 24);
            for (int i = 0; i < 3; i++)
            {
                int n = i;
                Button(kits[i], kits[i], 22 + i * 208, 464, 198, 40, () => upgrade = n);
            }

            summary = Label("", 16, 22, 514, 620, 112);
            Label("ENTER / COMENZAR EN EL PANEL IZQUIERDO", 16, 22, 634, 620, 26);
        }

        private Text Label(string text, int size, float x, float y, float w, float h)
        {
            var obj = new GameObject(text, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(panel.transform, false);
            var r = obj.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(x, -y);
            r.sizeDelta = new Vector2(w, h);
            var t = obj.GetComponent<Text>();
            t.text = text;
            t.font = session.hudFont;
            t.fontSize = size;
            t.color = new Color(.88f, .82f, .64f);
            t.raycastTarget = false;
            return t;
        }

        private void Button(string id, string text, float x, float y, float w, float h, UnityEngine.Events.UnityAction click)
        {
            var obj = new GameObject(id, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(panel.transform, false);
            var r = obj.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(x, -y);
            r.sizeDelta = new Vector2(w, h);
            var image = obj.GetComponent<Image>();
            image.color = new Color(.12f, .115f, .09f);
            var child = Label(text, 17, x, y, w, h);
            child.alignment = TextAnchor.MiddleCenter;
            var button = obj.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(click);choices[button]=child;
            var outline=obj.AddComponent<Outline>();outline.effectColor=new Color(.95f,.9f,.73f);outline.effectDistance=new Vector2(2,2);
        }

        public void UseCheckpoint() => applied = true;

        private void Update()
        {
            if (session == null || panel == null) return;
            panel.SetActive(session.Mode == SessionMode.Title && !session.Hud.ShowingHelp);
            if (panel.activeSelf)
            {
                var p = SurvivorRoster.Profiles[Mathf.Clamp(role, 0, 3)];
                string[] guns={"1  Arma corta","2  Fusil de asalto","3  Escopeta","4  Precision","5  Subfusil","6  Arma pesada"};
                foreach(var pair in choices)
                {
                    bool active=pair.Key.name==p.ButtonId||pair.Key.name==guns[selected]||pair.Key.name==kits[upgrade]||pair.Key.name==MeleeSwing.WeaponNames[selectedMelee];
                    pair.Key.GetComponent<Image>().color=active?new Color(.38f,.32f,.18f):new Color(.12f,.115f,.09f);
                    if(!pair.Value.text.StartsWith("["))pair.Value.text="[ ] "+pair.Value.text;
                    pair.Value.text=(active?"[X] ":"[ ] ")+pair.Value.text.Substring(4);
                    pair.Key.GetComponent<Outline>().enabled=UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject==pair.Key.gameObject;
                }
                string gun = session.player.GetComponent<WeaponLoadout>().weapons[selected].displayName;
                string melee = MeleeSwing.WeaponNames[Mathf.Clamp(selectedMelee, 0, 3)];
                var supplies = new[] { "+1 placa de repuesto", "+2 vendas", "+24 cartuchos compartidos" };
                var starting = new[] { "dotacion base", "+1 pila y +1 placa", "+1 botiquin y +1 suero", "+2 vendas" };
                summary.text = $"{p.CodeName} ({p.RoleTitle})\nPasiva: {p.PassiveSummary}\nSalida: {starting[role]} en mochila, sujeto al limite.\nArsenal: {gun} + {melee}; reserva compartida\nKit: {supplies[upgrade]} en mochila, sujeto al limite; una vez por salida.";
            }
            if (!applied && session.Running)
            {
                applied = true;
                // Loading a checkpoint keeps its resources and weapon; only a new sortie gets a kit.
                if (session.State.elapsed > 0.2f) return;
                if (roster != null) roster.ApplyCharacter(role, true);
                var loadout = session.player.GetComponent<WeaponLoadout>();
                loadout.Equip(selected, true);
                if (session.Night != null) session.Night.Inventory.melee = selectedMelee;
                if (role == 2)
                {
                    session.State.medkits = Mathf.Min(8, session.State.medkits + 1);
                    session.State.doses = Mathf.Min(8, session.State.doses + 1);
                }
                if (role == 1 && session.Night != null)
                {
                    session.Night.Inventory.cells = Mathf.Min(8, session.Night.Inventory.cells + 1);
                    session.Night.Inventory.plates = Mathf.Min(4, session.Night.Inventory.plates + 1);
                }
                if (role == 3)
                    session.State.bandages = Mathf.Min(12, session.State.bandages + 2);
                if (upgrade == 1) session.State.bandages = Mathf.Min(12, session.State.bandages + 2);
                if (upgrade == 2) session.reserve = Mathf.Min(240, session.reserve + 24);
                if (upgrade == 0 && session.Night != null) session.Night.Inventory.plates = Mathf.Min(4, session.Night.Inventory.plates + 1);
                var profile = SurvivorRoster.Profiles[Mathf.Clamp(role, 0, 3)];
                session.SetMessage($"{profile.CodeName} desplegado. Espacio: combo / G: pesado / Clic Der: parry / C: melee.");
            }
        }
    }
}
