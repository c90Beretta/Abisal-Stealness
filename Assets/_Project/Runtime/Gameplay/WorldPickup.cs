using UnityEngine;

namespace Zombtoy.Gameplay
{
    public sealed class WorldPickup : MonoBehaviour
    {
        public string id;
        public string label = "Suministros";
        public int ammo, doses, medkits, bandages;
        public bool Collected { get; private set; }
        public bool CanReach(Transform player)
        {
            Vector3 from = player.position + Vector3.up * 0.8f;
            return !Physics.Linecast(from, transform.position, out var hit, ~0, QueryTriggerInteraction.Ignore) ||
                hit.transform == transform || hit.transform.IsChildOf(transform);
        }
        public void Collect(SandboxSession session)
        {
            if (Collected || !session.Running) return;
            session.reserve += ammo; session.State.doses += doses;
            session.State.medkits += medkits; session.State.bandages += bandages;
            session.SetMessage(label + " recogido."); Restore(true);
        }
        public void Restore(bool collected) { Collected = collected; gameObject.SetActive(!collected); }
    }
}
