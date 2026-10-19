using UnityEngine;
using UnityEngine.InputSystem;

namespace Zombtoy.Gameplay
{
    [RequireComponent(typeof(Camera))]
    public sealed class IsometricView : MonoBehaviour
    {
        public Transform target;
        public float pitch = 35.264f, yaw = 45f;
        public float minZoom = 6, maxZoom = 13;
        private Camera view;

        private void Awake()
        {
            view = GetComponent<Camera>();
            // Render meshes straight to the native window, with no pixel-grid upscaling.
            view.targetTexture = null;
            view.orthographic = true;
            view.allowMSAA = true;
            view.allowHDR = true;
        }

        private void LateUpdate()
        {
            if (target == null) return;
            var player = target.GetComponent<SandboxPlayer>();
            if (Mouse.current != null && player != null && player.session.Running && !player.session.Hud.BlocksCombat)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f) view.orthographicSize = Mathf.Clamp(view.orthographicSize - scroll / 120, minZoom, maxZoom);
            }
            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
            transform.position = target.position + Vector3.up * 0.9f - transform.forward * 28;
        }
    }
}
