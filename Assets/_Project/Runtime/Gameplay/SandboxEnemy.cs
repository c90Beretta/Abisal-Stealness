using UnityEngine;
using UnityEngine.AI;

namespace Zombtoy.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class SandboxEnemy : MonoBehaviour
    {
        public SandboxSession session;
        public float speed = 1.2f;
        public float health = 70f;
        public float sightRange = 7f;
        public bool useNavigation;
        private CharacterController controller;
        private Vector3 lastHeard;
        private float alertUntil;
        private float nextAttack;
        private float nextPath;
        private NavMeshPath path;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            path = new NavMeshPath();
        }

        public void Hear(Vector3 position, float radius)
        {
            if (Vector3.Distance(transform.position, position) > radius) return;
            lastHeard = position;
            alertUntil = Time.time + 6f;
        }

        private void Update()
        {
            if (session == null || !session.Running || health <= 0f) return;
            Vector3 delta = session.player.transform.position - transform.position;
            Vector3 eye = transform.position + Vector3.up * 0.9f;
            bool sees = delta.magnitude <= sightRange &&
                Physics.Raycast(eye, delta.normalized, out RaycastHit hit, sightRange,
                    ~0, QueryTriggerInteraction.Ignore) && hit.collider.GetComponent<SandboxPlayer>() != null;
            if (sees)
            {
                lastHeard = session.player.transform.position;
                alertUntil = Time.time + 6f;
            }
            if (Time.time > alertUntil) return;
            Vector3 chase = lastHeard - transform.position;
            chase.y = 0;
            if (chase.magnitude > 1.1f)
            {
                if (useNavigation)
                {
                    if (Time.time >= nextPath)
                    {
                        nextPath = Time.time + 0.35f;
                        NavMesh.CalculatePath(transform.position, lastHeard, NavMesh.AllAreas, path);
                    }
                    if (path.status != NavMeshPathStatus.PathComplete || path.corners.Length < 2) return;
                    chase = path.corners[1] - transform.position;
                    chase.y = 0;
                    if (chase.sqrMagnitude < 0.01f) return;
                }
                transform.rotation = Quaternion.LookRotation(chase);
                controller.Move((chase.normalized * speed + Vector3.down * 8f) * Time.deltaTime);
            }
            if (sees && delta.magnitude < 1.5f && Time.time >= nextAttack)
            {
                nextAttack = Time.time + 1.2f;
                session.State.health = Mathf.Max(0f, session.State.health - 12f);
            }
        }

        public void Damage(float amount)
        {
            health -= amount;
            if (health > 0f) return;
            gameObject.SetActive(false);
            session.Kills++;
        }
    }
}
