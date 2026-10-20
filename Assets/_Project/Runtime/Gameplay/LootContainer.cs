using UnityEngine;

namespace Zombtoy.Gameplay
{
    public sealed class LootContainer : MonoBehaviour
    {
        public string id, label = "Contenedor";
        public LootStock stock = new LootStock();
        public SandboxEnemy corpse;
        public Vector3 Point => corpse != null && corpse.IsDead ? corpse.CorpseAnchor : transform.position;
        public bool Available => gameObject.activeInHierarchy && (corpse == null || corpse.IsDead);
        public bool Reachable(SandboxSession s)
        {
            if (!Available || Vector3.Distance(Point, s.player.transform.position) > 2.5f) return false;
            Vector3 end = Point + Vector3.up * (corpse != null ? .25f : .5f);
            return !Physics.Linecast(s.player.transform.position + Vector3.up, end, out var hit, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) || hit.transform == transform || hit.transform.IsChildOf(transform);
        }
        public bool Take(SandboxSession s)
        {
            if (!s.Running || s.Busy || s.State.InventoryLocked || !Reachable(s)) return false;
            int transferred = s.Night.Inventory.Transfer(s, stock);
            s.SetMessage(transferred > 0 ? "Botin guardado en la mochila." : stock.Empty ? "Contenedor vacio." : "Mochila o pila llena. Usa suministros para hacer espacio.");
            return transferred > 0;
        }
    }
}
