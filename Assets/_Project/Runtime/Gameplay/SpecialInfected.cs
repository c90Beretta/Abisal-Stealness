using UnityEngine;

namespace Zombtoy.Gameplay
{
    public enum SpecialSpecies { Bloater, Colossus }

    [DefaultExecutionOrder(160)]
    public sealed class SpecialInfected : MonoBehaviour
    {
        public SpecialSpecies Species { get; private set; }
        public bool IsBoss { get; private set; }
        private SandboxSession session;
        private SandboxEnemy enemy;
        private bool dead;
        private float nextAbility, nextVoice, chargeUntil;

        public void Initialize(SandboxSession value, SpecialSpecies species, bool boss = false)
        {
            session = value;
            enemy = GetComponent<SandboxEnemy>();
            Species = species;
            IsBoss = boss;
            var definition = Instantiate(enemy.definition);
            enemy.definition = definition;
            definition.displayName = boss ? "Coloso Abisal B-07" :
                species == SpecialSpecies.Bloater ? "Hinchado" : "Coloso";
            definition.health = boss ? 640 : species == SpecialSpecies.Bloater ? 140 : 420;
            definition.speed = boss ? 1.45f : species == SpecialSpecies.Bloater ? .95f : 1.15f;
            definition.damage = boss ? 52 : species == SpecialSpecies.Bloater ? 22 : 46;
            definition.attackWindup = species == SpecialSpecies.Colossus ? .85f : .45f;
            enemy.ResetWaveMultipliers();
            enemy.health = definition.health;
            enemy.speed = definition.speed;

            float uniformScale = boss ? 1.45f : species == SpecialSpecies.Colossus ? 1.28f : 1.15f;
            var visual = transform.Find("Character visual");
            if (visual != null)
                visual.localScale = Vector3.one * uniformScale;

            var controller = GetComponent<CharacterController>();
            if (controller != null)
            {
                float scaledHeight = Mathf.Clamp(1.8f * uniformScale, 1.8f, 2.55f);
                controller.height = scaledHeight;
                controller.center = Vector3.up * (scaledHeight * 0.5f);
                controller.radius = Mathf.Clamp(0.32f * uniformScale, 0.32f, 0.48f);
            }

            if (enemy.VisualAdapter != null)
                enemy.VisualAdapter.ApplySpecialScale(species, boss);

            nextAbility = Time.time + 4.5f;
        }

        private bool ClearToPlayer()
        {
            return Physics.Linecast(transform.position + Vector3.up, session.player.transform.position + Vector3.up, out var hit, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) &&
                hit.collider.GetComponentInParent<SandboxPlayer>() == session.player;
        }

        private void Update()
        {
            if (session == null || !session.Running) return;
            if (enemy.IsDead)
            {
                if (!dead)
                {
                    dead = true;
                    if (Species == SpecialSpecies.Bloater)
                    {
                        session.EmitNoise(transform.position, 28);
                        CombatEffects.Blood(transform.position + Vector3.up, session.tracerMaterial);
                        TacticalAudio.Play(transform.position, TacticalCue.Burst);
                        if (Vector3.Distance(transform.position, session.player.transform.position) < 4.5f && ClearToPlayer())
                            session.TakeDamage(28, false);
                    }
                }
                return;
            }
            dead = false;
            if (enemy.Alerted && Time.time > nextVoice)
            {
                nextVoice = Time.time + Random.Range(4, 7);
                TacticalAudio.Play(transform.position, TacticalCue.Growl, 1.3f);
            }
            if (Species == SpecialSpecies.Colossus)
            {
                float baseSpd = enemy.definition != null ? enemy.definition.speed : (IsBoss ? 1.45f : 1.15f);
                enemy.speed = Time.time < chargeUntil ? (IsBoss ? 3.4f : 3.0f) : Mathf.Max(enemy.speed, baseSpd);
                if (Time.time >= chargeUntil && enemy.speed > baseSpd * 1.6f)
                    enemy.speed = baseSpd;
                if (enemy.Alerted && Time.time >= nextAbility && ClearToPlayer())
                {
                    nextAbility = Time.time + 10f;
                    chargeUntil = Time.time + 2.6f;
                    session.EmitNoise(transform.position, 22);
                    TacticalAudio.Play(transform.position, TacticalCue.Growl, 1.4f);
                }
                return;
            }
            if (Species != SpecialSpecies.Bloater || Time.time < nextAbility || !enemy.Alerted) return;
            float distance = Vector3.Distance(transform.position, session.player.transform.position);
            if (distance > 6.5f || distance < 2 || !ClearToPlayer()) return;
            nextAbility = Time.time + 7f;
            enemy.Stagger(0.85f);
            session.EmitNoise(transform.position, 18);
            CombatEffects.Blood(transform.position + transform.forward + Vector3.up, session.tracerMaterial);
            TacticalAudio.Play(transform.position, TacticalCue.Burst);
            session.TakeDamage(14, false);
            session.SetMessage("Salpicadura del hinchado: rompe su linea de vision.");
        }
    }
}
