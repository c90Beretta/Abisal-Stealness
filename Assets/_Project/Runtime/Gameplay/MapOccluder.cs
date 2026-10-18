using UnityEngine;

namespace Zombtoy.Gameplay
{
    public sealed class MapOccluder : MonoBehaviour
    {
        private Renderer[] parts;
        public void SetHidden(bool hidden)
        {
            if (parts == null) parts = GetComponentsInChildren<Renderer>();
            foreach (var part in parts)
                if (part != null) part.forceRenderingOff = hidden;
        }
        private void OnDisable() => SetHidden(false);
    }
}
