using UnityEngine;

namespace Zombtoy.Gameplay
{
    public sealed class MapOccluder : MonoBehaviour
    {
        private Renderer[] parts;
        private Bounds bounds;
        private bool hasBounds;
        public bool Intersects(Ray ray,float distance)
        {
            Cache();return gameObject.activeInHierarchy&&hasBounds&&bounds.IntersectRay(ray,out float enter)&&enter<distance;
        }
        private void Cache()
        {
            if(parts!=null)return;
            parts=GetComponentsInChildren<Renderer>();
            foreach(var part in parts)if(part!=null&&part.enabled){if(!hasBounds){bounds=part.bounds;hasBounds=true;}else bounds.Encapsulate(part.bounds);}
        }
        public void SetHidden(bool hidden)
        {
            Cache();
            foreach (var part in parts)
                if (part != null) part.forceRenderingOff = hidden;
        }
        private void OnDisable() => SetHidden(false);
    }
}
