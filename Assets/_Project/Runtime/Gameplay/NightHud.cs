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
            session = s; font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            TextAt(parent, "MOCHILA", 22, 26, -72, 440, 30, new Color(.55f, .94f, .72f));
            carried = TextAt(parent, "", 18, 26, -112, 450, 100, Color.white);
            gear = TextAt(parent, "", 17, 26, -220, 450, 65, new Color(.67f, .77f, .74f));
            batteryButton = Action(parent, "Cambiar pila", 26, -294, () => session.Night.UseSupply(0));
            oxygenButton = Action(parent, "Recargar oxigeno", 26, -340, () => session.Night.UseSupply(1));
            plateButton = Action(parent, "Cambiar placa", 26, -386, () => session.Night.UseSupply(2));
            Action(parent, "Mascara: activar / quitar", 260, -294, () => ChangeGear(0));
            Action(parent, "Chaleco: equipar / quitar", 260, -340, () => ChangeGear(1));
            Action(parent, "Cuchillo / hacha", 260, -386, () => ChangeGear(2));
            TextAt(parent, "ARMAS  [1 / 2 / 3]", 18, 26, -442, 440, 28, Color.white);
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                var button = Action(parent, (i + 1) + "  " + new[] { "Pistola", "Carabina", "Dispersor" }[i], 26 + i * 151, -480, () => session.player.GetComponent<WeaponLoadout>().Equip(index));
                ((RectTransform)button.transform).sizeDelta = new Vector2(143, 38);
            }
            lootTitle = TextAt(parent, "BOTIN CERCANO", 22, 536, -72, 410, 32, new Color(.55f, .94f, .72f));
            contents = TextAt(parent, "", 19, 536, -126, 410, 160, Color.white);
            take = Action(parent, "Recoger lo que quepa", 536, -306, () => { if (selected != null) selected.Take(session); });
            ((RectTransform)take.transform).sizeDelta = new Vector2(410, 42);
            feedback = TextAt(parent, "", 17, 536, -354, 410, 44, new Color(1, .72f, .55f));
            TextAt(parent, "E abre cuerpos y cajas a menos de 2.5 m.\nLas sobras permanecen en el contenedor.\nRepuestos: carga <=70% y uno disponible.\nTAB cierra / El mundo sigue avanzando.", 17, 536, -410, 410, 110, new Color(.67f, .77f, .74f));
        }
        public void Select(LootContainer box) { selected = box; }
        private void ChangeGear(int index)
        {
            if (!session.Running || session.Busy || session.State.InventoryLocked) return;
            var bag = session.Night.Inventory;
            if (index == 0) bag.maskOn = !bag.maskOn;
            if (index == 1) bag.armorOn = !bag.armorOn;
            if (index == 2) bag.melee = 1 - bag.melee;
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
            carried.text = $"Carga {b.Weight(session):0.0} / {FieldInventory.Capacity} kg\nCartuchos {session.reserve} · Botiquines {s.medkits} · Vendas {s.bandages}\nSuero {s.doses} · Pilas {b.cells} · Botellas O2 {b.oxygen}\nPlacas de repuesto {b.plates}";
            gear.text = $"Chaleco {(b.armorOn ? "puesto" : "quitado")} {b.armor:0}% · Mascara {(b.maskOn ? "puesta" : "quitada")}\nO2 {b.air:0}% · Linterna {b.battery:0}%\nCuerpo a cuerpo: {(b.melee == 0 ? "cuchillo" : "hacha de abordaje")}";
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
            node.anchorMin = node.anchorMax = new Vector2(0, 1); node.pivot = new Vector2(0, 1); node.anchoredPosition = new Vector2(x, y); node.sizeDelta = new Vector2(224, 38);
            var image = node.gameObject.AddComponent<Image>(); image.color = new Color(.14f, .28f, .23f);
            var button = node.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(callback);
            TextAt(node.transform, text, 17, 10, -8, 210, 28, Color.white);
            return button;
        }
    }
}
