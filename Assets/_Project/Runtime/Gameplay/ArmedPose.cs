using UnityEngine;

namespace Zombtoy.Gameplay
{
    // Constrain the two arms on legacy rigs OR anchor the weapon socket cleanly onto Humanoid hands.
    [DefaultExecutionOrder(60)]
    public sealed class ArmedPose : MonoBehaviour
    {
        public Transform rightUpper, rightLower, rightHand, leftUpper, leftLower, leftHand;
        public Transform grip, support;

        private bool useHumanoidHands;
        private Transform humanoidRightHand;
        private Transform humanoidLeftHand;
        private Transform weaponSocket;
        private Vector3 handOffset = new Vector3(0.01f, 0.02f, 0.06f);
        private Vector3 eulerOffset;
        private Vector3 defaultSocketLocalPos = new Vector3(0.18f, 1.28f, 0.4f);
        private Quaternion defaultSocketLocalRot = Quaternion.identity;
        private bool savedDefaultSocket;
        private SandboxPlayer cachedPlayer;

        public void ConfigureHumanoidHands(Transform socket, Transform rHand, Transform lHand, Vector3 offset, Vector3 euler)
        {
            if (socket != null && !savedDefaultSocket)
            {
                defaultSocketLocalPos = socket.localPosition;
                defaultSocketLocalRot = socket.localRotation;
                savedDefaultSocket = true;
            }

            if (cachedPlayer == null) cachedPlayer = GetComponent<SandboxPlayer>();

            weaponSocket = socket;
            humanoidRightHand = rHand;
            humanoidLeftHand = lHand;
            handOffset = offset;
            eulerOffset = euler;
            useHumanoidHands = socket != null && rHand != null;
        }

        public void ClearHumanoidHands()
        {
            useHumanoidHands = false;
            humanoidRightHand = null;
            humanoidLeftHand = null;
            if (weaponSocket != null && savedDefaultSocket)
            {
                weaponSocket.localPosition = defaultSocketLocalPos;
                weaponSocket.localRotation = defaultSocketLocalRot;
            }
        }

        private void LateUpdate()
        {
            if (useHumanoidHands)
            {
                UpdateHumanoidWeaponSocket();
                return;
            }

            if (grip == null || rightUpper == null || rightHand == null) return;
            Aim(rightUpper, rightLower, rightHand, grip.position, transform.right * 0.18f);
            if (leftUpper != null) Aim(leftUpper, leftLower, leftHand, support.position, -transform.right * 0.08f);
        }

        private void UpdateHumanoidWeaponSocket()
        {
            if (weaponSocket == null || humanoidRightHand == null) return;

            bool aiming = cachedPlayer != null && (cachedPlayer.IsAiming || Time.time - cachedPlayer.LastShotTime < 0.45f);
            bool crouching = cachedPlayer != null && cachedPlayer.IsCrouching;

            Vector3 desiredWorldPos = humanoidRightHand.position
                + transform.right * handOffset.x
                + transform.up * handOffset.y
                + transform.forward * handOffset.z;

            Vector3 localPos = transform.InverseTransformPoint(desiredWorldPos);
            float minY = crouching ? 0.42f : 0.72f;
            float maxY = crouching ? 1.22f : 1.58f;
            localPos.x = Mathf.Clamp(localPos.x, -0.32f, 0.42f);
            localPos.y = Mathf.Clamp(localPos.y, minY, maxY);
            localPos.z = Mathf.Clamp(localPos.z, 0.06f, 0.48f);
            weaponSocket.localPosition = localPos;

            Vector3 aimDir = transform.forward;
            if (humanoidLeftHand != null)
            {
                Vector3 handLine = humanoidLeftHand.position - humanoidRightHand.position;
                if (handLine.sqrMagnitude > 0.01f)
                {
                    Vector3 handDir = handLine.normalized;
                    if (aiming)
                    {
                        // Locked horizontally to player aim while keeping subtle vertical alignment with hands
                        Vector3 flat = Vector3.ProjectOnPlane(handLine, Vector3.up);
                        float pitchY = flat.magnitude > 0.05f ? Mathf.Clamp(handLine.y / flat.magnitude, -0.20f, 0.20f) : 0f;
                        aimDir = (transform.forward + Vector3.up * (pitchY * 0.35f)).normalized;
                    }
                    else
                    {
                        // Follow natural two-handed carry pose when relaxed/running
                        aimDir = Vector3.Slerp(transform.forward, handDir, 0.55f).normalized;
                    }
                }
            }

            if (aimDir.sqrMagnitude > 0.001f)
                weaponSocket.rotation = Quaternion.LookRotation(aimDir, Vector3.up) * Quaternion.Euler(eulerOffset);
        }

        private static void Aim(Transform upper, Transform lower, Transform hand, Vector3 target, Vector3 bend)
        {
            for (int i = 0; i < 3; i++)
            {
                Rotate(upper, hand.position, target + bend * 0.12f);
                Rotate(lower, hand.position, target);
            }
        }

        private static void Rotate(Transform joint, Vector3 current, Vector3 target)
        {
            Vector3 a = current - joint.position, b = target - joint.position;
            if (a.sqrMagnitude > 0.0001f && b.sqrMagnitude > 0.0001f) joint.rotation = Quaternion.FromToRotation(a, b) * joint.rotation;
        }
    }
}
