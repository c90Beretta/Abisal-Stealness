using UnityEngine;
using UnityEngine.UI;

namespace Zombtoy.Gameplay
{
    public sealed class NightHud : MonoBehaviour
    {
        private SandboxSession session;
        private Text carried, gear, lootTitle, contents, feedback;
        private Button take, batteryButton, oxygenButton, plateButton;
        private LootContainer selected;
        private Font font;

        public void Setup(SandboxSession s, Transform parent)
        {
            session = s; font = s.hudFont != null ? s.hudFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            TextAt(parent, "MOCHILA Y ESCUADRON", 22, 26, -68, 460, 30, new Color(.88f, .82f, .64f));
            carried = TextAt(parent, "", 17, 26, -104, 460, 96, Color.white);
            gear = TextAt(parent, "", 16, 26, -206, 460, 74, new Color(.76f, .74f, .66f));
            batteryButton = Action(parent, "Cambiar pila", 26, -288, () => session.Night.UseSupply(0));
            oxygenButton = Action(parent, "Recargar oxigeno", 26, -330, () => session.Night.UseSupply(1));
            plateButton = Action(parent, "Cambiar placa", 26, -372, () => session.Night.UseSupply(2));
            Action(parent, "Mascara: activar / quitar", 260, -288, () => ChangeGear(0));
            Action(parent, "Chaleco: equipar / quitar", 260, -330, () => ChangeGear(1));
            Action(parent, "Arma blanca [C]: siguiente", 260, -372, () => ChangeGear(2));
            var charBtn = Action(parent, "Personaje [F4]: cambiar", 26, -414, () => session.player.Roster?.CycleCharacter(1));
            ((RectTransform)charBtn.transform).sizeDelta = new Vector2(224, 34);
            var ammoBtn = Action(parent, "Municion [T]: cambiar tipo", 260, -414, () => session.player.GetComponent<WeaponLoadout>()?.CycleAmmoType());
            ((RectTransform)ammoBtn.transform).sizeDelta = new Vector2(224, 34);

            TextAt(parent, "ARSENAL  [TECLAS 1 - 6]", 17, 26, -456, 440, 24, Color.white);
            string[] gunLabels = { "1 Pistola", "2 Carabina", "3 Escopeta", "4 Precision", "5 Subfusil", "6 Pesada" };
            for (int i = 0; i < 6; i++)
            {
                int index = i;
                var button = Action(parent, gunLabels[i], 26 + (i % 3) * 153, -482 - (i / 3) * 34, () => session.player.GetComponent<WeaponLoadout>().Equip(index));
                ((RectTransform)button.transform).sizeDelta = new Vector2(146, 31);
            }

            lootTitle = TextAt(parent, "BOTIN CERCANO", 22, 536, -72, 410, 32, new Color(.88f, .82f, .64f));
            contents = TextAt(parent, "", 19, 536, -126, 410, 160, Color.white);
            take = Action(parent, "Recoger lo que quepa", 536, -306, () => { if (selected != null) selected.Take(session); });
            ((RectTransform)take.transform).sizeDelta = new Vector2(410, 42);
            feedback = TextAt(parent, "", 17, 536, -354, 410, 44, new Color(1, .72f, .55f));
            TextAt(parent, "E abre cuerpos, cajas y estaciones a menos de 2.5 m.\nEspacio: combo · G: golpe pesado · Clic Der: parry.\nAlt/X: esquiva · C: cambiar arma blanca · T: municion.\nTAB cierra / El mundo sigue avanzando.", 16, 536, -410, 420, 110, new Color(.76f, .74f, .66f));
        }

        public void Select(LootContainer box) { selected = box; }

        private void ChangeGear(int index)
        {
            if (!session.Running || session.Busy || session.State.InventoryLocked) return;
            var bag = session.Night.Inventory;
            if (index == 0) bag.maskOn = !bag.maskOn;
            if (index == 1) bag.armorOn = !bag.armorOn;
            if (index == 2) session.player.CycleMeleeWeapon();
        }

        private void LateUpdate()
        {
            if (session == null || session.Night == null) return;
            var n = session.Night; var b = n.Inventory; var s = session.State;
            bool allowed = session.Running && !session.Busy && !s.InventoryLocked;
            batteryButton.interactable = allowed && b.cells > 0 && b.battery <= 70;
            oxygenButton.interactable = allowed && b.oxygen > 0 && b.air <= 70;
            plateButton.interactable = allowed && b.plates > 0 && b.armor <= 70;
            feedback.text = Time.unscaledTime < session.MessageUntil ? session.Message : "";
            float maxCap = b.MaxCapacity(session);
            carried.text = $"Carga {b.Weight(session):0.0} / {maxCap:0} kg\nCartuchos {session.reserve} · Botiquines {s.medkits} · Vendas {s.bandages}\nSuero {s.doses} · Pilas {b.cells} · Botellas O2 {b.oxygen} · Placas {b.plates}";
            string charLine = session.player.Roster != null ? $"{session.player.Roster.Current.CodeName} ({session.player.Roster.Current.RoleTitle})" : "Operador";
            var loadout = session.player.GetComponent<WeaponLoadout>();
            string ammoName = loadout != null ? WeaponLoadout.AmmoNames[(int)loadout.AmmoType] : "FMJ";
            gear.text = $"{charLine}\nChaleco {(b.armorOn ? "puesto" : "quitado")} {b.armor:0}% · Mascara {(b.maskOn ? "puesta" : "quitada")} · O2 {b.air:0}%\nCuerpo a cuerpo: {session.player.CurrentMeleeName} · Municion: {ammoName}";
            if (selected == null || !selected.Reachable(session)) selected = n.NearestLoot();
            lootTitle.text = selected != null ? selected.label.ToUpperInvariant() : "BOTIN CERCANO";
            contents.text = selected != null ? selected.stock.Empty ? "Vacio. Ya recogiste su contenido." : selected.stock.Description : "Acercate a un contenedor o a un\ninfectado abatido y pulsa E.\n\nNo hay botin al alcance.";
            take.interactable = selected != null && !selected.stock.Empty && selected.Reachable(session) && !session.Busy && !s.InventoryLocked;
        }

        private Text TextAt(Transform parent, string text, int size, float x, float y, float w, float h, Color color)
        {
            var go = new GameObject(text.Length > 25 ? text.Substring(0, 25) : text, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(w, h);
            var label = go.AddComponent<Text>(); label.font = font; label.text = text; label.fontSize = size; label.color = color; label.raycastTarget = false;
            return label;
        }

        private Button Action(Transform parent, string text, float x, float y, UnityEngine.Events.UnityAction callback)
        {
            var node = new GameObject(text, typeof(RectTransform)).GetComponent<RectTransform>(); node.SetParent(parent, false);
            node.anchorMin = node.anchorMax = new Vector2(0, 1); node.pivot = new Vector2(0, 1); node.anchoredPosition = new Vector2(x, y); node.sizeDelta = new Vector2(224, 34);
            var image = node.gameObject.AddComponent<Image>(); image.color = new Color(.18f, .17f, .14f);
            var button = node.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(callback);
            TextAt(node.transform, text, 16, 10, -6, 210, 24, Color.white);
            return button;
        }
    }
}
