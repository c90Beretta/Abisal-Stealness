using UnityEngine;

namespace Zombtoy.Gameplay
{
    // Constrain the two arms after locomotion so the visible weapon stays in the hands.
    [DefaultExecutionOrder(60)]
    public sealed class ArmedPose : MonoBehaviour
    {
        public Transform rightUpper, rightLower, rightHand, leftUpper, leftLower, leftHand;
        public Transform grip, support;
        private void LateUpdate()
        {
            if (grip == null || rightUpper == null || rightHand == null) return;
            Aim(rightUpper, rightLower, rightHand, grip.position, transform.right * 0.18f);
            if (leftUpper != null) Aim(leftUpper, leftLower, leftHand, support.position, -transform.right * 0.08f);
        }
        private static void Aim(Transform upper, Transform lower, Transform hand, Vector3 target, Vector3 bend)
        {
            // Two CCD passes retain the source rig's own bone axes and proportions.
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
