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
        private float nextMelee, strikeAt;
        private bool strikePending, strikeAxe;
        private int combo;
        private float comboUntil;
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
            if (session == null || !session.Running) { strikePending = false; return; }
            if (strikePending && Time.time >= strikeAt) { strikePending = false; ResolveMelee(strikeAxe); }
            if (Keyboard.current == null) return;
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
                if (stepSound != null) audioSource.PlayOneShot(stepSound, sprint ? .22f : .12f);
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
                    if (aim.sqrMagnitude > 0.01f && GetComponent<MeleeSwing>()?.Active != true) transform.rotation = Quaternion.LookRotation(aim);
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
                if (kb.digit4Key.wasPressedThisFrame) loadout.Equip(3);
                if (kb.digit5Key.wasPressedThisFrame) loadout.Equip(4);
                if (kb.digit6Key.wasPressedThisFrame) loadout.Equip(5);
            }
            if (kb.qKey.wasPressedThisFrame) session.BeginInjection();
            if (kb.eKey.wasPressedThisFrame) session.Interact();
            if (kb.hKey.wasPressedThisFrame) session.BeginHeal();
            if (kb.bKey.wasPressedThisFrame) session.BeginBandage();
            if (kb.fKey.wasPressedThisFrame) { if (session.Night != null) session.Night.ToggleLamp(); else if (flashlight != null) flashlight.enabled = !flashlight.enabled; }
            if (kb.spaceKey.wasPressedThisFrame) TryMelee();
            if (kb.vKey.wasPressedThisFrame) Shove();
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
            (GetComponent<MeleeSwing>() ?? gameObject.AddComponent<MeleeSwing>()).Play(axe, session.tracerMaterial, session.combatArt != null ? axe ? session.combatArt.axe : session.combatArt.knife : null);
            strikeAxe = axe; strikePending = true; strikeAt = Time.time + (axe ? .18f : .10f);
            return true;
        }

        private void ResolveMelee(bool axe)
        {
            if (!session.Running || session.Busy || session.Hud.BlocksCombat) return;
            combo = Time.time < comboUntil ? (combo + 1) % 3 : 0; comboUntil = Time.time + 1.5f;
            int hits = 0;
            foreach (var enemy in session.Enemies)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy || enemy.IsDead) continue;
                Vector3 delta = enemy.transform.position - transform.position; delta.y = 0;
                if (delta.magnitude > (axe ? 1.8f : 1.55f) || Vector3.Angle(transform.forward, delta) > 55) continue;
                Vector3 from = transform.position + Vector3.up * .95f, to = enemy.transform.position + Vector3.up * .95f;
                if (Physics.Linecast(from, to, out var hit, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && hit.collider.GetComponentInParent<SandboxEnemy>() != enemy) continue;
                enemy.Hit((axe ? 65 : 40) * (combo == 2 ? 1.25f : 1), to + transform.right * .25f, true); session.RegisterHit();
                if (++hits >= (axe ? 3 : 1)) break;
            }
            if (hits > 0) TacticalAudio.Play(transform.position, TacticalCue.Impact);
        }

        private void Shove()
        {
            if (session.Busy || session.Hud.BlocksCombat || Time.time < nextMelee || (session.Night != null && !session.Night.SpendStamina(22))) return;
            nextMelee = Time.time + .8f;
            foreach (var enemy in session.Enemies)
            {
                if (enemy == null || enemy.IsDead || !enemy.gameObject.activeInHierarchy) continue;
                var d = enemy.transform.position - transform.position; d.y = 0;
                if (d.magnitude > 1.9f || Vector3.Angle(transform.forward, d) > 65) continue;
                if (Physics.Linecast(transform.position + Vector3.up, enemy.transform.position + Vector3.up, out var h) && h.collider.GetComponentInParent<SandboxEnemy>() != enemy) continue;
                enemy.Damage(8); enemy.Stagger(.9f);
            }
            TacticalAudio.Play(transform.position, TacticalCue.Swing);
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
            TacticalAudio.Play(transform.position, TacticalCue.Gunshot, weapon != null ? weapon.fireInterval : .22f);
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
                float range = weapon != null ? weapon.range : 25f;
                Vector3 end = origin + aim * range;
                if (Physics.Raycast(origin, aim, out RaycastHit hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
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
