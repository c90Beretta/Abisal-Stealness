using UnityEngine;
namespace Zombtoy.Gameplay
{
    [DefaultExecutionOrder(80)]
    public sealed class AttackPose : MonoBehaviour
    {
        private Transform left, right;
        private SandboxEnemy enemy;
        private MeleeSwing swing;
        private float started;
        private bool attacking;
        private void Start()
        {
            enemy=GetComponent<SandboxEnemy>();
            foreach(var bone in GetComponentsInChildren<Transform>())
            {if(bone.name=="RightArm")right=bone;if(bone.name=="LeftArm")left=bone;}
        }
        private void LateUpdate()
        {
            if(enemy!=null&&enemy.IsDead)return;
            if(swing==null)swing=GetComponent<MeleeSwing>();
            bool active=enemy!=null?enemy.Mode==EnemyMode.Windup||enemy.Mode==EnemyMode.Recover:swing!=null&&swing.Active;
            if(active&&!attacking)started=Time.time;
            attacking=active;if(!active||right==null)return;
            float t=Mathf.Clamp01((Time.time-started)/.4f);
            // Authored anticipation, sweep and follow-through layered over locomotion/weapon IK.
            right.localRotation*=Quaternion.Euler(-55*Mathf.Sin(t*Mathf.PI),0,60*Mathf.Sin(t*Mathf.PI*2));
            if(left!=null&&enemy!=null)left.localRotation*=Quaternion.Euler(-45*Mathf.Sin(t*Mathf.PI),0,-25);
        }
    }
}
