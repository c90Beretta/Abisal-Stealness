using UnityEngine;

namespace Zombtoy.Gameplay
{
    [DefaultExecutionOrder(150)]
    public sealed class InfectedVisibility : MonoBehaviour
    {
        private SandboxEnemy enemy;
        private Renderer[] parts;
        public bool Visible { get; private set; } = true;
        private void Awake() { enemy = GetComponent<SandboxEnemy>(); parts = GetComponentsInChildren<Renderer>(); }
        private void LateUpdate()
        {
            var night = enemy.session.Night;
            Visible = night == null || night.Perceives(transform.position + Vector3.up * .9f);
            foreach (var part in parts) if (part != null) part.enabled = Visible;
        }
    }
}
