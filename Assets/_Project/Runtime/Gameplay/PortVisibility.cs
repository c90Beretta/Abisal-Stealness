using System.Collections.Generic;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    [DefaultExecutionOrder(100)]
    public sealed class PortVisibility : MonoBehaviour
    {
        public Transform target;
        private readonly HashSet<MapOccluder> hidden = new HashSet<MapOccluder>();
        private MapOccluder[] occluders;
        private SandboxSession session;
        private void Start(){occluders=FindObjectsByType<MapOccluder>(FindObjectsInactive.Include);session=target!=null?target.GetComponent<SandboxPlayer>()?.session:null;}

        private void LateUpdate()
        {
            Restore();
            if (target == null) return;
            Reveal(target.position+Vector3.up*.9f);
            Reveal(target.position+Vector3.up*.15f);
            Reveal(target.position+Vector3.up*1.65f);
            if(session?.Enemies==null||session.Night==null)return;
            foreach(var enemy in session.Enemies)
                if(enemy.gameObject.activeInHierarchy&&Vector3.Distance(enemy.transform.position,target.position)<8&&session.Night.Perceives(enemy.IsDead?enemy.CorpseAnchor:enemy.transform.position+Vector3.up*.9f))
                    Reveal(enemy.IsDead?enemy.CorpseAnchor:enemy.transform.position+Vector3.up*.9f);
        }
        private void Reveal(Vector3 end)
        {
            if(occluders==null)return;Vector3 delta=end-transform.position;var ray=new Ray(transform.position,delta.normalized);
            foreach(var part in occluders)if(part!=null&&part.Intersects(ray,delta.magnitude)&&hidden.Add(part))part.SetHidden(true);
        }

        private void Restore()
        {
            foreach (var part in hidden) if (part != null) part.SetHidden(false);
            hidden.Clear();
        }
        private void OnDisable() => Restore();
    }
}
