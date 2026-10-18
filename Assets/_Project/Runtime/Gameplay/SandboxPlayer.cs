using UnityEngine;
using UnityEngine.InputSystem;

namespace Zombtoy.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class SandboxPlayer : MonoBehaviour
    {
        public Camera view;
        public SandboxSession session;
        public AudioClip shotSound;
        public AudioClip stepSound;
        private CharacterController controller;
        private float nextShot;
        private float nextStep;
        private AudioSource audioSource;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
        }

        private void Update()
        {
            if (session == null || !session.Running || Keyboard.current == null) return;
            var kb = Keyboard.current;
            Vector2 input = new Vector2((kb.dKey.isPressed ? 1 : 0) - (kb.aKey.isPressed ? 1 : 0),
                (kb.wKey.isPressed ? 1 : 0) - (kb.sKey.isPressed ? 1 : 0));
            Vector3 forward = Vector3.ProjectOnPlane(view.transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(view.transform.right, Vector3.up).normalized;
            Vector3 direction = Vector3.ClampMagnitude(right * input.x + forward * input.y, 1f);
            bool sprint = kb.leftShiftKey.isPressed;
            float speed = (sprint ? 5f : 3f) * (session.State.IsManic ? 1.3f : 1f);
            if (session.Injecting) speed *= 0.3f;
            controller.Move((direction * speed + Vector3.down * 8f) * Time.deltaTime);
            if (input.sqrMagnitude > 0 && Time.time >= nextStep)
            {
                nextStep = Time.time + (sprint ? 0.28f : 0.48f);
                session.EmitNoise(transform.position, sprint ? 10f : 3f);
                if (stepSound != null) audioSource.PlayOneShot(stepSound, 0.15f);
            }
            if (Mouse.current != null)
            {
                // The camera fills the viewport, so cursor coordinates match the low-res image.
                Vector2 cursor = Mouse.current.position.ReadValue();
                Ray ray = view.ViewportPointToRay(new Vector3(cursor.x / Screen.width, cursor.y / Screen.height, 0));
                Plane plane = new Plane(Vector3.up, transform.position + Vector3.up * 0.8f);
                if (plane.Raycast(ray, out float distance))
                {
                    Vector3 aim = ray.GetPoint(distance) - transform.position;
                    aim.y = 0;
                    if (aim.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(aim);
                }
                if (Mouse.current.leftButton.isPressed && Time.time >= nextShot && !session.Injecting)
                    Shoot();
            }
            if (kb.rKey.wasPressedThisFrame) session.Reload();
            if (kb.qKey.wasPressedThisFrame) session.BeginInjection();
            if (kb.eKey.wasPressedThisFrame) session.Interact();
        }

        private void Shoot()
        {
            nextShot = Time.time + 0.22f;
            if (!session.UseRound()) return;
            session.EmitNoise(transform.position, session.State.IsManic ? 24f : 18f);
            if (shotSound != null) audioSource.PlayOneShot(shotSound, 0.25f);
            Vector3 origin = transform.position + Vector3.up * 0.85f + transform.forward * 0.5f;
            Vector3 aim = transform.forward;
            if (session.State.elapsed < session.State.tremorUntil || session.State.IsManic)
                aim = Quaternion.AngleAxis(Random.Range(-7f, 7f), Vector3.up) * aim;
            Vector3 end = origin + aim * 25f;
            if (Physics.Raycast(origin, aim, out RaycastHit hit, 25f, ~0, QueryTriggerInteraction.Ignore))
            {
                end = hit.point;
                var enemy = hit.collider.GetComponentInParent<SandboxEnemy>();
                if (enemy != null) enemy.Damage(35f);
            }
            var tracer = new GameObject("Shot trace").AddComponent<LineRenderer>();
            tracer.sharedMaterial = session.tracerMaterial;
            tracer.startWidth = 0.04f;
            tracer.endWidth = 0.01f;
            tracer.positionCount = 2;
            tracer.SetPosition(0, origin);
            tracer.SetPosition(1, end);
            Destroy(tracer.gameObject, 0.06f);
        }
    }
}
