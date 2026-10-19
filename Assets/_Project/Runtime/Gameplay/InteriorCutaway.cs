using UnityEngine;

namespace Zombtoy.Gameplay
{
    // Keep the near shell out of an occupied isometric room without changing collision or AI sight.
    [DefaultExecutionOrder(140)]
    public sealed class InteriorCutaway : MonoBehaviour
    {
        public Transform actor;
        public Vector2 min,max;
        public Renderer[] nearShell;
        private bool wasInside;
        private void LateUpdate()
        {
            if(actor==null)return;
            var p=actor.position;bool inside=p.x>=min.x&&p.x<=max.x&&p.z>=min.y&&p.z<=max.y;
            if(inside){foreach(var part in nearShell)if(part!=null)part.forceRenderingOff=true;}
            else if(wasInside){foreach(var part in nearShell)if(part!=null)part.forceRenderingOff=false;}
            wasInside=inside;
        }
        private void OnDisable(){if(nearShell!=null)foreach(var part in nearShell)if(part!=null)part.forceRenderingOff=false;}
    }
}
