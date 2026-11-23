using UnityEngine;

namespace Zombtoy.Gameplay
{
    public sealed class CharacterMotion : MonoBehaviour
    {
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AimingHash = Animator.StringToHash("Aiming");
        private static readonly int CrouchingHash = Animator.StringToHash("Crouching");
        private static readonly int DeadHash = Animator.StringToHash("Dead");

        public Animation rig;
        public string idleState = "idle", moveState = "move";
        public Animator humanoidAnimator;

        private bool moving, stopped;
        private float smoothX, smoothY;

        private void Start()
        {
            if (rig != null && rig.gameObject.activeInHierarchy)
                rig.Play(idleState);
        }

        public void BindHumanoidAnimator(Animator animator)
        {
            humanoidAnimator = animator;
            smoothX = 0f;
            smoothY = 0f;
            if (humanoidAnimator != null)
            {
                humanoidAnimator.SetFloat(MoveXHash, 0f);
                humanoidAnimator.SetFloat(MoveYHash, 0f);
                humanoidAnimator.SetFloat(SpeedHash, 0f);
                humanoidAnimator.SetBool(AimingHash, false);
                humanoidAnimator.SetBool(CrouchingHash, false);
                humanoidAnimator.SetBool(DeadHash, false);
                humanoidAnimator.Update(0.02f);
            }
        }

        public void SetLocomotion(Vector3 worldVelocity, float referenceRunSpeed, bool aiming, bool crouching, bool dead)
        {
            float speedMag = new Vector2(worldVelocity.x, worldVelocity.z).magnitude;
            bool isMoving = !dead && speedMag > 0.1f;

            if (rig != null && rig.gameObject.activeInHierarchy)
                SetMovingLegacy(isMoving, speedMag);

            if (humanoidAnimator == null || !humanoidAnimator.isActiveAndEnabled) return;

            humanoidAnimator.SetBool(DeadHash, dead);
            humanoidAnimator.SetBool(AimingHash, !dead && aiming);
            humanoidAnimator.SetBool(CrouchingHash, !dead && crouching);

            Vector3 flatVel = dead ? Vector3.zero : new Vector3(worldVelocity.x, 0f, worldVelocity.z);
            Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 rgt = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
            if (rgt.sqrMagnitude < 0.001f) rgt = Vector3.right;

            float scale = Mathf.Max(0.5f, referenceRunSpeed);
            float maxAxis = crouching ? 0.65f : aiming ? 1.0f : 1.5f;
            float targetX = Mathf.Clamp(Vector3.Dot(flatVel, rgt) / scale, -maxAxis, maxAxis);
            float targetY = Mathf.Clamp(Vector3.Dot(flatVel, fwd) / scale, -maxAxis, maxAxis);

            float dt = Time.deltaTime > 0f ? Time.deltaTime : Time.unscaledDeltaTime;
            float damp = 1f - Mathf.Exp(-12f * dt);
            smoothX = Mathf.Lerp(smoothX, targetX, damp);
            smoothY = Mathf.Lerp(smoothY, targetY, damp);

            if (Mathf.Abs(smoothX) < 0.01f && Mathf.Abs(targetX) < 0.01f) smoothX = 0f;
            if (Mathf.Abs(smoothY) < 0.01f && Mathf.Abs(targetY) < 0.01f) smoothY = 0f;

            humanoidAnimator.SetFloat(MoveXHash, smoothX);
            humanoidAnimator.SetFloat(MoveYHash, smoothY);
            humanoidAnimator.SetFloat(SpeedHash, new Vector2(smoothX, smoothY).magnitude);
        }

        public void SetMoving(bool value, float speed)
        {
            if (rig != null && rig.gameObject.activeInHierarchy)
                SetMovingLegacy(value, speed);

            if (humanoidAnimator != null && humanoidAnimator.isActiveAndEnabled)
            {
                float normalized = value ? Mathf.Clamp(speed / 3f, 0.35f, 1.5f) : 0f;
                float dt = Time.deltaTime > 0f ? Time.deltaTime : Time.unscaledDeltaTime;
                float damp = 1f - Mathf.Exp(-12f * dt);
                smoothX = Mathf.Lerp(smoothX, 0f, damp);
                smoothY = Mathf.Lerp(smoothY, normalized, damp);
                humanoidAnimator.SetFloat(MoveXHash, smoothX);
                humanoidAnimator.SetFloat(MoveYHash, smoothY);
                humanoidAnimator.SetFloat(SpeedHash, Mathf.Abs(smoothY));
            }
        }

        public void Stop()
        {
            stopped = true;
            if (rig != null && rig.gameObject.activeInHierarchy)
                rig.Stop();

            if (humanoidAnimator != null && humanoidAnimator.isActiveAndEnabled)
            {
                smoothX = 0f;
                smoothY = 0f;
                humanoidAnimator.SetFloat(MoveXHash, 0f);
                humanoidAnimator.SetFloat(MoveYHash, 0f);
                humanoidAnimator.SetFloat(SpeedHash, 0f);
            }
        }

        private void SetMovingLegacy(bool value, float speed)
        {
            if (stopped) { stopped = false; rig.Play(idleState); moving = false; }
            if (value != moving) { moving = value; rig.CrossFade(moving ? moveState : idleState, 0.14f); }
            if (rig[moveState] != null) rig[moveState].speed = Mathf.Clamp(speed / 3f, 0.35f, 1.5f);
        }
    }
}
