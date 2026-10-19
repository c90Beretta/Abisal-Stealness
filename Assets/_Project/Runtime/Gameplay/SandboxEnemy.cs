using UnityEngine;
using UnityEngine.AI;
using Zombtoy.Core;

namespace Zombtoy.Gameplay
{
    public enum EnemyMode { Patrol, Investigate, Chase, Search, Windup, Recover, Stagger, Dead }

    [RequireComponent(typeof(CharacterController))]
    public sealed class SandboxEnemy : MonoBehaviour
    {
        public SandboxSession session;
        public EnemyDefinition definition;
        public float speed = 1.2f, health = 70f, sightRange = 7f;
        public bool useNavigation;
        public EnemyMode Mode { get; private set; }
        public bool IsDead => health <= 0;
        public float MaxHealth { get; private set; } = 70;
        public float HealthFraction => Mathf.Clamp01(health / MaxHealth);
        public bool Alerted => Mode != EnemyMode.Patrol && Mode != EnemyMode.Dead;
        private CharacterController controller;
        private CharacterMotion motion;
        private Vector3 home, destination;
        private Quaternion initialRotation;
        private float alertedUntil, stateUntil, nextPath, patrolUntil, flashUntil;
        private int patrolIndex;
        private NavMeshPath path;
        private Renderer[] renderers;
        private MaterialPropertyBlock block;
        private Transform visual;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            path = new NavMeshPath();
            home = destination = transform.position;
            initialRotation = transform.rotation;
            renderers = GetComponentsInChildren<Renderer>();
            block = new MaterialPropertyBlock();
            motion = GetComponent<CharacterMotion>();
            visual = transform.Find("CC0 visual");
            if (definition != null) { health = definition.health; speed = definition.speed; sightRange = definition.sightRange; }
            MaxHealth = health;
            patrolUntil = Time.time + 2;
        }

        public void Hear(Vector3 position, float radius)
        {
            if (IsDead || Vector3.Distance(transform.position, position) > radius * (definition == null ? 1 : definition.hearingMultiplier)) return;
            destination = position; alertedUntil = Time.time + 8;
            if (Mode == EnemyMode.Patrol || Mode == EnemyMode.Search) Mode = EnemyMode.Investigate;
        }

        private bool SeesPlayer(bool fieldOfView)
        {
            Vector3 delta = session.player.transform.position - transform.position;
            delta.y = 0;
            if (delta.magnitude > sightRange) return false;
            float fov = definition == null ? 140 : definition.fieldOfView;
            if (fieldOfView && delta.magnitude > 2 && Vector3.Angle(transform.forward, delta) > fov * 0.5f) return false;
            Vector3 eye = transform.position + Vector3.up * 0.9f;
            Vector3 end = session.player.transform.position + Vector3.up * 0.9f;
            return Physics.Linecast(eye, end, out var hit, ~0, QueryTriggerInteraction.Ignore) && hit.collider.GetComponentInParent<SandboxPlayer>() == session.player;
        }

        private void Update()
        {
            if (session == null || !session.Running) return;
            if (IsDead)
            {
                if (visual != null) visual.localRotation = Quaternion.Slerp(visual.localRotation, Quaternion.Euler(-88, 0, 0), Time.deltaTime * 7);
                return;
            }
            foreach (var r in renderers)
            {
                block.Clear(); block.SetColor("_EmissionColor", Time.time < flashUntil ? Color.red * 1.8f : Color.black);
                if (Time.time < flashUntil) block.SetColor("_BaseColor", new Color(1, 0.15f, 0.1f));
                r.SetPropertyBlock(block);
            }
            if (motion != null) motion.SetMoving(false, 0);
            bool sees = SeesPlayer(Mode == EnemyMode.Patrol);
            float distance = Vector3.Distance(session.player.transform.position, transform.position);
            if (Mode == EnemyMode.Windup)
            {
                if (visual != null) visual.localRotation = Quaternion.Slerp(visual.localRotation, Quaternion.Euler(22, 0, 0), Time.deltaTime * 8);
                Face(session.player.transform.position - transform.position);
                if (Time.time >= stateUntil)
                {
                    if (distance < 1.8f && SeesPlayer(false)) session.TakeDamage(definition == null ? 12 : definition.damage);
                    Mode = EnemyMode.Recover; stateUntil = Time.time + (definition == null ? 1.2f : definition.attackRecovery);
                }
                return;
            }
            if (visual != null) visual.localRotation = Quaternion.Slerp(visual.localRotation, Quaternion.identity, Time.deltaTime * 8);
            if (Mode == EnemyMode.Recover || Mode == EnemyMode.Stagger)
            {
                if (Time.time < stateUntil) return;
                Mode = EnemyMode.Investigate;
            }
            if (sees)
            {
                destination = session.player.transform.position; alertedUntil = Time.time + 8; Mode = EnemyMode.Chase;
                if (distance < 1.5f)
                {
                    Mode = EnemyMode.Windup; stateUntil = Time.time + (definition == null ? 0.55f : definition.attackWindup);
                    return;
                }
            }
            else if (Mode != EnemyMode.Patrol && Time.time >= alertedUntil) { Mode = EnemyMode.Patrol; destination = home; }
            Vector3 delta = destination - transform.position; delta.y = 0;
            if (delta.magnitude < 0.6f)
            {
                if (Mode != EnemyMode.Patrol) { Mode = EnemyMode.Search; transform.Rotate(0, 45 * Time.deltaTime, 0); return; }
                if (Time.time < patrolUntil) return;
                patrolUntil = Time.time + 5;
                Vector3 candidate = home + initialRotation * new Vector3((patrolIndex++ % 2 == 0 ? 2 : -2), 0, 1.5f);
                destination = useNavigation && NavMesh.SamplePosition(candidate, out var sampled, 2, NavMesh.AllAreas) ? sampled.position : home;
                delta = destination - transform.position; delta.y = 0;
            }
            if (useNavigation)
            {
                if (Time.time >= nextPath) { nextPath = Time.time + 0.3f; NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path); }
                if (path.status != NavMeshPathStatus.PathComplete || path.corners.Length < 2) return;
                delta = path.corners[1] - transform.position; delta.y = 0;
            }
            if (delta.sqrMagnitude < 0.01f) return;
            Face(delta);
            float moveSpeed = speed * (Mode == EnemyMode.Patrol ? 0.45f : 1);
            controller.Move((delta.normalized * moveSpeed + Vector3.down * 8) * Time.deltaTime);
            if (motion != null) motion.SetMoving(true, moveSpeed);
        }

        private void Face(Vector3 delta)
        {
            delta.y = 0;
            if (delta.sqrMagnitude > 0.01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(delta), Time.deltaTime * 9);
        }

        public void Damage(float amount)
        {
            if (IsDead || amount <= 0) return;
            health = Mathf.Max(0, health - amount);
            Hear(session.player.transform.position, 100);
            flashUntil = Time.time + 0.15f;
            if (health > 0) { Mode = EnemyMode.Stagger; stateUntil = Time.time + 0.22f; return; }
            Mode = EnemyMode.Dead; controller.enabled = false;
            if (motion != null) motion.Stop();
            session.Kills++;
        }

        public void Restore(float value, Vector3 position)
        {
            controller.enabled = false;
            transform.position = position; health = Mathf.Clamp(value, 0, MaxHealth);
            controller.enabled = !IsDead;
            Mode = IsDead ? EnemyMode.Dead : EnemyMode.Patrol;
            destination = position; alertedUntil = stateUntil = nextPath = 0;
            if (visual != null) visual.localRotation = IsDead ? Quaternion.Euler(-88, 0, 0) : Quaternion.identity;
            if (motion != null) motion.SetMoving(false, 0);
        }
    }
}
