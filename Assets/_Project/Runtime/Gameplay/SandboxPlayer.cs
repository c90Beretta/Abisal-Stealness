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
        public AudioClip meleeSound;
        private CharacterController controller;
        private float nextShot;
        private float nextStep;
        private AudioSource audioSource;
        private CharacterMotion motion;
        private float nextMelee;
        private Light flashlight;
        private WeaponLoadout loadout;
        private Vector3 aimPoint;
        private bool preciseAim;

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
            bool sprint = kb.leftShiftKey.isPressed && direction.sqrMagnitude > .01f;
            if (sprint && session.Night != null) sprint = session.Night.SpendStamina(Time.deltaTime * 30);
            if (session.Hud.BlocksCombat) { direction = Vector3.zero; sprint = false; }
            float speed = (sprint ? 5f : 3f) * (session.State.IsManic ? 1.3f : 1f);
            if (session.Busy) speed *= 0.3f;
            controller.Move((direction * speed + Vector3.down * 8f) * Time.deltaTime);
            if (motion != null) motion.SetMoving(direction.sqrMagnitude > 0.01f, speed);
            if (direction.sqrMagnitude > .01f && Time.time >= nextStep)
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
                preciseAim = Physics.Raycast(ray, out var cursorHit, 100, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && cursorHit.collider.GetComponentInParent<SandboxEnemy>() != null;
                if (preciseAim) aimPoint = cursorHit.point;
                if (plane.Raycast(ray, out float distance))
                {
                    Vector3 aim = (preciseAim ? aimPoint : ray.GetPoint(distance)) - transform.position;
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
            if (kb.fKey.wasPressedThisFrame) { if (session.Night != null) session.Night.ToggleLamp(); else if (flashlight != null) flashlight.enabled = !flashlight.enabled; }
            if (kb.spaceKey.wasPressedThisFrame) TryMelee();
        }

        public bool TryMelee()
        {
            if (!session.Running || session.Busy || session.Hud.BlocksCombat || Time.time < nextMelee) return false;
            bool axe = session.Night != null && session.Night.Inventory.melee == 1;
            if (session.Night != null && !session.Night.SpendStamina(axe ? 28 : 16)) { session.SetMessage("Sin resistencia para golpear."); return false; }
            nextMelee = Time.time + (axe ? 1.1f : .65f);
            session.EmitNoise(transform.position, axe ? 8 : 5);
            if (meleeSound != null) audioSource.PlayOneShot(meleeSound, .35f);
            CombatEffects.Swing(transform, session.tracerMaterial);
            (GetComponent<MeleeSwing>() ?? gameObject.AddComponent<MeleeSwing>()).Play(axe, session.tracerMaterial);
            int hits = 0;
            foreach (var enemy in session.Enemies)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy || enemy.IsDead) continue;
                Vector3 delta = enemy.transform.position - transform.position; delta.y = 0;
                if (delta.magnitude > (axe ? 1.8f : 1.55f) || Vector3.Angle(transform.forward, delta) > 55) continue;
                Vector3 from = transform.position + Vector3.up * .95f, to = enemy.transform.position + Vector3.up * .95f;
                if (Physics.Linecast(from, to, out var hit, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && hit.collider.GetComponentInParent<SandboxEnemy>() != enemy) continue;
                enemy.Hit(axe ? 65 : 40, to + transform.right * .25f, true); session.RegisterHit();
                if (++hits >= (axe ? 3 : 1)) break;
            }
            return true;
        }

        private void Shoot()
        {
            if (GetComponent<MeleeSwing>()?.Active == true) return;
            var weapon = loadout != null ? loadout.Current : null;
            nextShot = Time.time + (weapon != null ? weapon.fireInterval : 0.22f);
            if (!session.UseRound()) return;
            session.EmitNoise(transform.position, (weapon != null ? weapon.noiseRadius : 18) * (session.State.IsManic ? 1.35f : 1));
            var clip = weapon != null && weapon.shotClip != null ? weapon.shotClip : shotSound;
            if (clip != null) audioSource.PlayOneShot(clip, 0.35f);
            if (loadout != null) loadout.Recoil();
            Vector3 origin = loadout != null && loadout.Muzzle != null ? loadout.Muzzle.position : transform.position + Vector3.up * 0.85f + transform.forward * 0.5f;
            // A long visible barrel may touch a wall; it must not shoot through it.
            Vector3 chest = transform.position + Vector3.up * 1.1f;
            if (Physics.Linecast(chest, origin, out var obstruction, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) origin = chest;
            CombatEffects.Shot(origin, transform, session.tracerMaterial);
            for (int pellet = 0; pellet < (weapon != null ? weapon.pellets : 1); pellet++)
            {
            Vector3 aim = preciseAim ? (aimPoint - origin).normalized : transform.forward;
            if (weapon != null && weapon.spread > 0) aim = Quaternion.AngleAxis(Random.Range(-weapon.spread, weapon.spread), Vector3.up) * aim;
            if (session.State.elapsed < session.State.tremorUntil || session.State.IsManic)
                aim = Quaternion.AngleAxis(Random.Range(-7f, 7f), Vector3.up) * aim;
            Vector3 end = origin + aim * 25f;
            if (Physics.Raycast(origin, aim, out RaycastHit hit, 25f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                end = hit.point;
                var enemy = hit.collider.GetComponentInParent<SandboxEnemy>();
                if (enemy != null && !enemy.IsDead) { enemy.Hit(weapon != null ? weapon.damage : 35, hit.point, false); session.RegisterHit(); }
                else CombatEffects.Impact(hit.point, hit.normal, session.tracerMaterial);
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
