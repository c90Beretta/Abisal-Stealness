using System.Collections.Generic;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    [DefaultExecutionOrder(100)]
    public sealed class PortVisibility : MonoBehaviour
    {
        public Transform target;
        private readonly HashSet<MapOccluder> hidden = new HashSet<MapOccluder>();

        private void LateUpdate()
        {
            Restore();
            if (target == null) return;
            Vector3 end = target.position + Vector3.up * 0.9f;
            Vector3 delta = end - transform.position;
            foreach (var hit in Physics.RaycastAll(transform.position, delta.normalized, delta.magnitude,
                         ~0, QueryTriggerInteraction.Ignore))
            {
                var part = hit.collider.GetComponentInParent<MapOccluder>();
                if (part != null && hidden.Add(part)) part.SetHidden(true);
            }
        }

        private void Restore()
        {
            foreach (var part in hidden) if (part != null) part.SetHidden(false);
            hidden.Clear();
        }
        private void OnDisable() => Restore();
    }
}
