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
        private CharacterMotion motion;
        private float nextMelee;
        private Light flashlight;
        private WeaponLoadout loadout;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
            motion = GetComponent<CharacterMotion>();
            flashlight = GetComponentInChildren<Light>();
            loadout = GetComponent<WeaponLoadout>();
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
            if (session.Busy) speed *= 0.3f;
            controller.Move((direction * speed + Vector3.down * 8f) * Time.deltaTime);
            if (motion != null) motion.SetMoving(direction.sqrMagnitude > 0.01f, speed);
            if (input.sqrMagnitude > 0 && Time.time >= nextStep)
            {
                nextStep = Time.time + (sprint ? 0.28f : 0.48f);
                session.EmitNoise(transform.position, sprint ? 10f : 3f);
                if (stepSound != null) audioSource.PlayOneShot(stepSound, 0.15f);
            }
            if (Mouse.current != null)
            {
                // The native-resolution 3D camera fills the viewport.
                Vector2 cursor = Mouse.current.position.ReadValue();
                Ray ray = view.ViewportPointToRay(new Vector3(cursor.x / Screen.width, cursor.y / Screen.height, 0));
                Plane plane = new Plane(Vector3.up, transform.position + Vector3.up * 0.8f);
                if (plane.Raycast(ray, out float distance))
                {
                    Vector3 aim = ray.GetPoint(distance) - transform.position;
                    aim.y = 0;
                    if (aim.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(aim);
                }
                if (Mouse.current.leftButton.isPressed && Time.time >= nextShot && !session.Busy && !session.Hud.BlocksCombat)
                    Shoot();
            }
            if (kb.rKey.wasPressedThisFrame) session.Reload();
            if (loadout != null)
            {
                if (kb.digit1Key.wasPressedThisFrame) loadout.Equip(0);
                if (kb.digit2Key.wasPressedThisFrame) loadout.Equip(1);
                if (kb.digit3Key.wasPressedThisFrame) loadout.Equip(2);
            }
            if (kb.qKey.wasPressedThisFrame) session.BeginInjection();
            if (kb.eKey.wasPressedThisFrame) session.Interact();
            if (kb.hKey.wasPressedThisFrame) session.BeginHeal();
            if (kb.bKey.wasPressedThisFrame) session.BeginBandage();
            if (kb.fKey.wasPressedThisFrame && flashlight != null) flashlight.enabled = !flashlight.enabled;
            if (kb.spaceKey.wasPressedThisFrame && !session.Busy && !session.Hud.BlocksCombat && Time.time >= nextMelee)
            {
                nextMelee = Time.time + 0.85f;
                session.EmitNoise(transform.position, 5);
                if (Physics.SphereCast(transform.position + Vector3.up * 0.85f, 0.3f, transform.forward, out var meleeHit, 1.3f, ~0, QueryTriggerInteraction.Ignore))
                {
                    var enemy = meleeHit.collider.GetComponentInParent<SandboxEnemy>();
                    if (enemy != null && !enemy.IsDead) { enemy.Damage(25); session.RegisterHit(); }
                }
            }
        }

        private void Shoot()
        {
            var weapon = loadout != null ? loadout.Current : null;
            nextShot = Time.time + (weapon != null ? weapon.fireInterval : 0.22f);
            if (!session.UseRound()) return;
            session.EmitNoise(transform.position, (weapon != null ? weapon.noiseRadius : 18) * (session.State.IsManic ? 1.35f : 1));
            if (shotSound != null) audioSource.PlayOneShot(shotSound, 0.25f);
            if (loadout != null) loadout.Recoil();
            Vector3 origin = loadout != null && loadout.Muzzle != null ? loadout.Muzzle.position : transform.position + Vector3.up * 0.85f + transform.forward * 0.5f;
            // A long visible barrel may touch a wall; it must not shoot through it.
            Vector3 chest = transform.position + Vector3.up * 1.1f;
            if (Physics.Linecast(chest, origin, out var obstruction, ~0, QueryTriggerInteraction.Ignore)) origin = chest;
            for (int pellet = 0; pellet < (weapon != null ? weapon.pellets : 1); pellet++)
            {
            Vector3 aim = transform.forward;
            if (weapon != null && weapon.spread > 0) aim = Quaternion.AngleAxis(Random.Range(-weapon.spread, weapon.spread), Vector3.up) * aim;
            if (session.State.elapsed < session.State.tremorUntil || session.State.IsManic)
                aim = Quaternion.AngleAxis(Random.Range(-7f, 7f), Vector3.up) * aim;
            Vector3 end = origin + aim * 25f;
            if (Physics.Raycast(origin, aim, out RaycastHit hit, 25f, ~0, QueryTriggerInteraction.Ignore))
            {
                end = hit.point;
                var enemy = hit.collider.GetComponentInParent<SandboxEnemy>();
                if (enemy != null && !enemy.IsDead) { enemy.Damage(weapon != null ? weapon.damage : 35); session.RegisterHit(); }
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
}
