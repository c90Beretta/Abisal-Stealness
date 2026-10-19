using UnityEngine;

namespace Zombtoy.Gameplay
{
    public sealed class CharacterMotion : MonoBehaviour
    {
        public Animation rig;
        private bool moving, stopped;
        private void Start() { if (rig != null) rig.Play("idle"); }
        public void SetMoving(bool value, float speed)
        {
            if (rig == null) return;
            if (stopped) { stopped = false; rig.Play("idle"); moving = false; }
            if (value != moving) { moving = value; rig.CrossFade(moving ? "move" : "idle", 0.14f); }
            if (rig["move"] != null) rig["move"].speed = Mathf.Clamp(speed / 3, 0.35f, 1.5f);
        }
        public void Stop() { stopped = true; if (rig != null) rig.Stop(); }
    }
}
