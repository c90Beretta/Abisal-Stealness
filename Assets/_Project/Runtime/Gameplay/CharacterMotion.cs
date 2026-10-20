using UnityEngine;

namespace Zombtoy.Gameplay
{
    public sealed class CharacterMotion : MonoBehaviour
    {
        public Animation rig;
        public string idleState = "idle", moveState = "move";
        private bool moving, stopped;
        private void Start() { if (rig != null) rig.Play(idleState); }
        public void SetMoving(bool value, float speed)
        {
            if (rig == null) return;
            if (stopped) { stopped = false; rig.Play(idleState); moving = false; }
            if (value != moving) { moving = value; rig.CrossFade(moving ? moveState : idleState, 0.14f); }
            if (rig[moveState] != null) rig[moveState].speed = Mathf.Clamp(speed / 3, 0.35f, 1.5f);
        }
        public void Stop() { stopped = true; if (rig != null) rig.Stop(); }
    }
}
