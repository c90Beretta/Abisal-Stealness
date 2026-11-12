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
        private bool strikePending, strikeHeavy;
        private int strikeMeleeKind;
        private int combo;
        private float comboUntil;
        private float parryWindowUntil;
        private bool isBlocking;
        private float dodgeUntil, nextDodge;
        private Vector3 dodgeDirection;
        private int fallbackMelee;
        private Light flashlight;
        private WeaponLoadout loadout;
        private Vector3 aimPoint;
        private bool preciseAim;

        public SurvivorRoster Roster { get; private set; }
        public int CurrentMeleeIndex => session != null && session.Night != null ? Mathf.Clamp(session.Night.Inventory.melee, 0, 3) : fallbackMelee;
        public string CurrentMeleeName => MeleeSwing.WeaponNames[CurrentMeleeIndex];
        public bool IsBlocking => isBlocking;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
            motion = GetComponent<CharacterMotion>();
            flashlight = GetComponentInChildren<Light>();
            loadout = GetComponent<WeaponLoadout>();
            Roster = GetComponent<SurvivorRoster>();
        }

        public void AttachRoster(SurvivorRoster roster) => Roster = roster;

        private void Update()
        {
            if (session == null || !session.Running)
            {
                strikePending = false;
                SetBlockState(false);
                return;
            }
            if (strikePending && Time.time >= strikeAt)
            {
                strikePending = false;
                ResolveMelee(strikeMeleeKind, strikeHeavy);
            }
            if (Keyboard.current == null) return;
            var kb = Keyboard.current;
            Vector2 input = new Vector2((kb.dKey.isPressed ? 1 : 0) - (kb.aKey.isPressed ? 1 : 0),
                (kb.wKey.isPressed ? 1 : 0) - (kb.sKey.isPressed ? 1 : 0));
            Vector3 forward = Vector3.ProjectOnPlane(view.transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(view.transform.right, Vector3.up).normalized;
            Vector3 direction = Vector3.ClampMagnitude(right * input.x + forward * input.y, 1f);

            bool rightMouseHeld = Mouse.current != null && Mouse.current.rightButton.isPressed && !session.Busy && !session.Hud.BlocksCombat;
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame && !session.Busy && !session.Hud.BlocksCombat)
            {
                parryWindowUntil = Time.time + 0.38f;
                TacticalAudio.Play(transform.position, TacticalCue.Swing, 1.4f);
            }
            SetBlockState(rightMouseHeld);

            if ((kb.leftAltKey.wasPressedThisFrame || kb.xKey.wasPressedThisFrame) && !session.Busy && !session.Hud.BlocksCombat)
                TryDodge(direction.sqrMagnitude > .01f ? direction : -transform.forward);

            float charMoveMult = Roster != null ? Roster.Current.MoveMultiplier : 1f;
            float charNoiseMult = Roster != null ? Roster.Current.NoiseMultiplier : 1f;

            if (Time.time < dodgeUntil)
            {
                controller.Move((dodgeDirection * 9.5f + Vector3.down * 8f) * Time.deltaTime);
                if (motion != null) motion.SetMoving(true, 6f);
            }
            else
            {
                bool sprint = kb.leftShiftKey.isPressed && direction.sqrMagnitude > .01f && !isBlocking;
                if (sprint && session.Night != null) sprint = session.Night.SpendStamina(Time.deltaTime * 30);
                if (session.Hud.BlocksCombat) { direction = Vector3.zero; sprint = false; }
                float speed = (sprint ? 5f : 3f) * (session.State.IsManic ? 1.3f : 1f) * charMoveMult;
                if (isBlocking) speed *= 0.62f;
                if (session.Busy) speed *= 0.3f;
                controller.Move((direction * speed + Vector3.down * 8f) * Time.deltaTime);
                if (motion != null) motion.SetMoving(direction.sqrMagnitude > 0.01f, speed);
                if (direction.sqrMagnitude > .01f && Time.time >= nextStep)
                {
                    nextStep = Time.time + (sprint ? 0.28f : 0.48f);
                    session.EmitNoise(transform.position, (sprint ? 10f : 3f) * charNoiseMult);
                    if (stepSound != null) audioSource.PlayOneShot(stepSound, sprint ? .22f : .12f);
                }
            }

            if (Mouse.current != null)
            {
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
                if (Mouse.current.leftButton.isPressed && !isBlocking && Time.time >= nextShot && !session.Busy && !session.Hud.BlocksCombat)
                    Shoot();
                if (Mouse.current.middleButton.wasPressedThisFrame)
                    TryHeavyMelee();
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
                if (kb.tKey.wasPressedThisFrame) loadout.CycleAmmoType();
            }
            if (kb.cKey.wasPressedThisFrame) CycleMeleeWeapon();
            if (kb.f3Key.wasPressedThisFrame && Roster != null) Roster.CycleCharacter(-1);
            if (kb.f4Key.wasPressedThisFrame && Roster != null) Roster.CycleCharacter(1);
            if (kb.nKey.wasPressedThisFrame && session.Night != null && session.Night.hordes != null) session.Night.hordes.CallWaveEarly();
            if (kb.qKey.wasPressedThisFrame) session.BeginInjection();
            if (kb.eKey.wasPressedThisFrame) session.Interact();
            if (kb.hKey.wasPressedThisFrame) session.BeginHeal();
            if (kb.bKey.wasPressedThisFrame) session.BeginBandage();
            if (kb.fKey.wasPressedThisFrame) { if (session.Night != null) session.Night.ToggleLamp(); else if (flashlight != null) flashlight.enabled = !flashlight.enabled; }
            if (kb.spaceKey.wasPressedThisFrame) TryMelee();
            if (kb.gKey.wasPressedThisFrame) TryHeavyMelee();
            if (kb.vKey.wasPressedThisFrame) Shove();
        }

        private void SetBlockState(bool active)
        {
            isBlocking = active;
            var swing = GetComponent<MeleeSwing>();
            if (swing != null || active)
                (swing ?? gameObject.AddComponent<MeleeSwing>()).SetBlocking(active, CurrentMeleeIndex, session != null ? session.tracerMaterial : null);
        }

        public void CycleMeleeWeapon(int? explicitIndex = null)
        {
            if (session == null || (!session.Running && session.Mode != SessionMode.Title) || session.Busy || session.State.InventoryLocked) return;
            int next = explicitIndex ?? ((CurrentMeleeIndex + 1) % 4);
            next = Mathf.Clamp(next, 0, 3);
            if (session.Night != null) session.Night.Inventory.melee = next;
            else fallbackMelee = next;
            if (session.Running)
            {
                TacticalAudio.Play(transform.position, TacticalCue.Swing, 1.1f);
                string traits = next == 0 ? "Veloz / combo quirurgico" :
                    next == 1 ? "Tajo pesado / desmembramiento (3 obj)" :
                    next == 2 ? "Barrido amplio / hemorragia DoT" :
                    "Impacto sismico / rompe armadura y aturde";
                session.SetMessage($"Arma blanca: {MeleeSwing.WeaponNames[next]} ({traits}).");
            }
        }

        public bool TryDodge(Vector3 dir)
        {
            if (!session.Running || session.Busy || session.Hud.BlocksCombat || Time.time < nextDodge) return false;
            float cost = 20f * (Roster != null ? Roster.Current.DodgeCostMultiplier : 1f);
            if (session.Night != null && !session.Night.SpendStamina(cost))
            {
                session.SetMessage("Sin resistencia para esquivar.");
                return false;
            }
            dir.y = 0;
            dodgeDirection = dir.sqrMagnitude > .01f ? dir.normalized : -transform.forward;
            dodgeUntil = Time.time + 0.20f;
            nextDodge = Time.time + 0.75f;
            session.GrantImmunity(0.36f);
            TacticalAudio.Play(transform.position, TacticalCue.Swing, 1.3f);
            return true;
        }

        public bool TryInterceptEnemyAttack(SandboxEnemy attacker, ref float damage, out bool wound)
        {
            wound = true;
            if (attacker == null || session == null) return false;
            Vector3 toEnemy = attacker.transform.position - transform.position;
            toEnemy.y = 0;
            bool facing = Vector3.Angle(transform.forward, toEnemy) <= 95f;

            if (facing && Time.time < parryWindowUntil)
            {
                parryWindowUntil = 0;
                damage = 0;
                wound = false;
                attacker.Stagger(2.2f);
                attacker.PushBack(toEnemy.normalized, 3.8f);
                if (session.Night != null)
                    session.Night.Inventory.stamina = Mathf.Min(100f, session.Night.Inventory.stamina + 15f);
                CombatEffects.ParryBurst(transform.position + Vector3.up * 1.1f + transform.forward * 0.55f, session.tracerMaterial);
                TacticalAudio.Play(transform.position, TacticalCue.Impact, 1.45f);
                session.SetMessage("¡PARRY PERFECTO! Enemigo aturdido: pulsa ESPACIO para ejecutar.");
                return true;
            }

            if (isBlocking && facing)
            {
                float blockCost = 10f * (Roster != null ? Roster.Current.DodgeCostMultiplier : 1f);
                if (session.Night == null || session.Night.SpendStamina(blockCost))
                {
                    damage *= 0.35f;
                    wound = false;
                    attacker.Stagger(0.45f);
                    attacker.PushBack(toEnemy.normalized, 1.8f);
                    CombatEffects.Impact(transform.position + Vector3.up * 1.1f + transform.forward * 0.5f, -transform.forward, session.tracerMaterial);
                    TacticalAudio.Play(transform.position, TacticalCue.Impact, 0.9f);
                    session.SetMessage("Guardia: impacto reducido (-65% dano, sin sangrado).");
                    return true;
                }
            }
            return false;
        }

        public bool TryMelee() => BeginMeleeStrike(false);
        public bool TryHeavyMelee() => BeginMeleeStrike(true);

        private bool BeginMeleeStrike(bool heavy)
        {
            if (!session.Running || session.Busy || session.Hud.BlocksCombat || Time.time < nextMelee) return false;
            int meleeKind = CurrentMeleeIndex;
            // Preserve exact stamina cost (16 for knife, 28 for axe) on standard swings for NightLevelTests
            float baseStamina = meleeKind == 0 ? 16f : meleeKind == 1 ? 28f : meleeKind == 2 ? 20f : 30f;
            if (heavy) baseStamina += 10f;
            if (session.Night != null && !session.Night.SpendStamina(baseStamina))
            {
                session.SetMessage("Sin resistencia para golpear.");
                return false;
            }
            SetBlockState(false);
            float baseCooldown = meleeKind == 0 ? .65f : meleeKind == 1 ? 1.1f : meleeKind == 2 ? .78f : 1.22f;
            nextMelee = Time.time + (heavy ? baseCooldown * 1.25f : baseCooldown);
            float noise = (meleeKind == 0 ? 5f : meleeKind == 1 ? 8f : meleeKind == 2 ? 6f : 10f) * (heavy ? 1.35f : 1f) * (Roster != null ? Roster.Current.NoiseMultiplier : 1f);
            session.EmitNoise(transform.position, noise);
            if (meleeSound != null) audioSource.PlayOneShot(meleeSound, heavy ? .48f : .35f);
            if (heavy) CombatEffects.HeavySwing(transform, session.tracerMaterial, meleeKind == 3 ? 2.25f : 1.95f);
            else CombatEffects.Swing(transform, session.tracerMaterial);

            int nextComboPreview = Time.time < comboUntil ? (combo + 1) % 3 : 0;
            GameObject prefab = session.combatArt != null ? (meleeKind == 1 ? session.combatArt.axe : meleeKind == 0 ? session.combatArt.knife : null) : null;
            (GetComponent<MeleeSwing>() ?? gameObject.AddComponent<MeleeSwing>()).PlayWeapon(meleeKind, nextComboPreview, heavy, session.tracerMaterial, prefab);

            strikeMeleeKind = meleeKind;
            strikeHeavy = heavy;
            strikePending = true;
            float delay = meleeKind == 0 ? .10f : meleeKind == 1 ? .18f : meleeKind == 2 ? .13f : .20f;
            strikeAt = Time.time + delay;
            return true;
        }

        private void ResolveMelee(int meleeKind, bool heavy)
        {
            if (!session.Running || session.Busy || session.Hud.BlocksCombat) return;
            combo = Time.time < comboUntil ? (combo + 1) % 3 : 0;
            comboUntil = Time.time + 1.6f;

            // Lunge forward slightly on 3rd combo hit or heavy attack if path is clear
            if ((combo == 2 || heavy) && !Physics.Raycast(transform.position + Vector3.up * 0.8f, transform.forward, 1.1f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                controller.Move(transform.forward * (heavy ? 0.38f : 0.25f));

            float reach = meleeKind == 0 ? 1.55f : meleeKind == 1 ? 1.8f : meleeKind == 2 ? 1.95f : 2.15f;
            if (heavy) reach += 0.25f;
            float maxAngle = heavy ? 95f : meleeKind == 0 ? 55f : meleeKind == 1 ? 58f : meleeKind == 2 ? 70f : 75f;
            float baseDamage = meleeKind == 0 ? 40f : meleeKind == 1 ? 65f : meleeKind == 2 ? 54f : 88f;
            int maxTargets = heavy ? 4 : meleeKind == 0 ? 1 : meleeKind == 1 ? 3 : meleeKind == 2 ? 2 : 4;

            float charMeleeMult = Roster != null ? Roster.Current.MeleeDamageMultiplier : 1f;
            if (meleeKind == 3 && Roster != null && Roster.Current.Id == SurvivorId.ElenaEngineer)
                charMeleeMult *= 1.15f;
            float comboMult = heavy ? 1.65f : combo == 2 ? 1.35f : combo == 1 ? 1.12f : 1f;

            int hits = 0;
            bool executedAny = false;
            foreach (var enemy in session.Enemies)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy || enemy.IsDead) continue;
                Vector3 delta = enemy.transform.position - transform.position;
                delta.y = 0;
                if (delta.magnitude > reach || Vector3.Angle(transform.forward, delta) > maxAngle) continue;
                Vector3 from = transform.position + Vector3.up * .95f, to = enemy.transform.position + Vector3.up * .95f;
                if (Physics.Linecast(from, to, out var hit, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && hit.collider.GetComponentInParent<SandboxEnemy>() != enemy) continue;

                bool staggeredTarget = enemy.Mode == EnemyMode.Stagger;
                float executionMult = staggeredTarget ? 1.55f : 1f;
                float finalDamage = baseDamage * comboMult * charMeleeMult * executionMult;

                Vector3 impactPoint = (staggeredTarget && finalDamage >= enemy.health)
                    ? enemy.transform.position + Vector3.up * 1.66f // Aim for head execution
                    : to + transform.right * .25f;

                enemy.Hit(finalDamage, impactPoint, true, meleeKind == 3 || heavy);
                session.RegisterHit();

                if (meleeKind == 2 || (meleeKind == 0 && combo == 2))
                    enemy.InflictBleed(4f);
                if (meleeKind == 3 || heavy || combo == 2)
                {
                    enemy.Stagger(heavy ? 1.4f : meleeKind == 3 ? 1.2f : 0.65f);
                    enemy.PushBack(delta.normalized, heavy ? 4.2f : meleeKind == 3 ? 3.5f : 1.8f);
                }
                if (staggeredTarget && enemy.IsDead) executedAny = true;
                if (++hits >= maxTargets) break;
            }
            if (hits > 0)
            {
                TacticalAudio.Play(transform.position, TacticalCue.Impact, heavy ? 0.8f : 1f);
                if (executedAny) session.SetMessage("¡Ejecucion critica cuerpo a cuerpo!");
            }
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
                enemy.Damage(8); enemy.Stagger(.9f); enemy.PushBack(d.normalized, 3.2f);
            }
            TacticalAudio.Play(transform.position, TacticalCue.Swing);
        }

        private void Shoot()
        {
            if (GetComponent<MeleeSwing>()?.Active == true) return;
            var weapon = loadout != null ? loadout.Current : null;
            int weaponIndex = loadout != null ? loadout.Index : 0;
            AmmoGrade ammoGrade = loadout != null ? loadout.AmmoType : AmmoGrade.StandardFMJ;

            nextShot = Time.time + (weapon != null ? weapon.fireInterval : 0.22f);
            if (!session.UseRound()) return;

            float noiseMult = Roster != null ? Roster.Current.NoiseMultiplier : 1f;
            session.EmitNoise(transform.position, (weapon != null ? weapon.noiseRadius : 18) * (session.State.IsManic ? 1.35f : 1) * noiseMult);
            var clip = weapon != null && weapon.shotClip != null ? weapon.shotClip : shotSound;
            if (clip != null) audioSource.PlayOneShot(clip, 0.35f);
            TacticalAudio.Play(transform.position, TacticalCue.Gunshot, weapon != null ? weapon.fireInterval : .22f);
            if (loadout != null) loadout.Recoil();
            Vector3 origin = loadout != null && loadout.Muzzle != null ? loadout.Muzzle.position : transform.position + Vector3.up * 0.85f + transform.forward * 0.5f;
            // A long visible barrel may touch a wall; it must not shoot through it.
            Vector3 chest = transform.position + Vector3.up * 1.1f;
            if (Physics.Linecast(chest, origin, out var obstruction, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) origin = chest;
            CombatEffects.Shot(origin, transform, session.tracerMaterial);

            float charGunMult = Roster != null ? Roster.Current.FirearmDamageMultiplier : 1f;
            if ((weaponIndex == 2 || weaponIndex == 5) && Roster != null)
                charGunMult *= Roster.Current.HeavyShotgunMultiplier;
            float ammoMult = ammoGrade == AmmoGrade.HollowPoint ? 1.25f : ammoGrade == AmmoGrade.ArmorPiercingIncendiary ? 1.10f : 1f;
            bool armorPiercing = weaponIndex == 3 || ammoGrade == AmmoGrade.ArmorPiercingIncendiary;

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
                    if (enemy != null && !enemy.IsDead)
                    {
                        float dmg = (weapon != null ? weapon.damage : 35f) * charGunMult * ammoMult;
                        enemy.Hit(dmg, hit.point, ammoGrade == AmmoGrade.HollowPoint, armorPiercing);
                        if (ammoGrade == AmmoGrade.HollowPoint) enemy.InflictBleed(2.5f);
                        if (ammoGrade == AmmoGrade.ArmorPiercingIncendiary) enemy.InflictBurn(3.0f);
                        if (weaponIndex == 2 || weaponIndex == 5) enemy.PushBack(aim, weaponIndex == 2 ? 2.2f : 1.4f);
                        session.RegisterHit();

                        // Ballistic penetration for Sniper (M82) or AP-I rounds
                        if (armorPiercing && hit.distance + 0.5f < range)
                        {
                            Vector3 pierceOrigin = hit.point + aim * 0.45f;
                            float remRange = range - hit.distance - 0.45f;
                            if (Physics.Raycast(pierceOrigin, aim, out RaycastHit hit2, remRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                            {
                                end = hit2.point;
                                var enemy2 = hit2.collider.GetComponentInParent<SandboxEnemy>();
                                if (enemy2 != null && enemy2 != enemy && !enemy2.IsDead)
                                {
                                    enemy2.Hit(dmg * 0.75f, hit2.point, false, true);
                                    if (ammoGrade == AmmoGrade.ArmorPiercingIncendiary) enemy2.InflictBurn(2.5f);
                                    session.RegisterHit();
                                }
                                else CombatEffects.Impact(hit2.point, hit2.normal, session.tracerMaterial);
                            }
                            else end = pierceOrigin + aim * remRange;
                        }
                    }
                    else CombatEffects.Impact(hit.point, hit.normal, session.tracerMaterial);
                }
                var tracer = new GameObject("Shot trace").AddComponent<LineRenderer>();
                tracer.sharedMaterial = session.tracerMaterial;
                tracer.startWidth = weaponIndex == 3 ? 0.06f : 0.04f;
                tracer.endWidth = 0.01f;
                tracer.positionCount = 2;
                tracer.SetPosition(0, origin);
                tracer.SetPosition(1, end);
                Destroy(tracer.gameObject, 0.06f);
            }
        }
    }
}
