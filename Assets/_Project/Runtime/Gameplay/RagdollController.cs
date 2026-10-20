using System;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    [Serializable]
    public sealed class RagdollBody
    {
        public Transform bone;
        public Rigidbody body;
        public Collider collider;
        public Severed region;
    }
    [Serializable]
    public sealed class RagdollPose
    {
        public bool present;
        public Vector3[] positions;
        public Quaternion[] rotations;
        public Vector3 rootPosition;
        public Quaternion rootRotation = Quaternion.identity;
        public bool Valid
        {
            get
            {
                if (!present) return false;
                if (!float.IsFinite(rootPosition.x) || !float.IsFinite(rootPosition.y) || !float.IsFinite(rootPosition.z) || rootPosition.sqrMagnitude > 100 || !float.IsFinite(rootRotation.x) || !float.IsFinite(rootRotation.y) || !float.IsFinite(rootRotation.z) || !float.IsFinite(rootRotation.w) || Mathf.Abs(Quaternion.Dot(rootRotation, rootRotation) - 1) > .05f) return false;
                if (positions == null || rotations == null || positions.Length != rotations.Length || positions.Length < 1 || positions.Length > 64) return false;
                for (int i = 0; i < positions.Length; i++)
                {
                    var p = positions[i]; var q = rotations[i];
                    if (!float.IsFinite(p.x) || !float.IsFinite(p.y) || !float.IsFinite(p.z) || p.sqrMagnitude > 10000 ||
                        !float.IsFinite(q.x) || !float.IsFinite(q.y) || !float.IsFinite(q.z) || !float.IsFinite(q.w) || Mathf.Abs(Quaternion.Dot(q, q) - 1) > .05f) return false;
                }
                return true;
            }
        }
    }

    [DefaultExecutionOrder(170)]
    public sealed class RagdollController : MonoBehaviour
    {
        public RagdollBody[] bodies = Array.Empty<RagdollBody>();
        public Transform[] poseBones = Array.Empty<Transform>();
        public Transform pelvis, visualRoot;
        public bool Active { get; private set; }
        public bool Settled { get; private set; }
        public Vector3 Anchor => pelvis != null && Active ? pelvis.position : transform.position;
        private Vector3[] bindPositions;
        private Quaternion[] bindRotations;
        private float activatedAt;
        private bool initialized;
        private CharacterMotion motion;

        private void Awake() => Initialize();
        private void Initialize()
        {
            if (initialized) return; initialized = true;
            motion = GetComponent<CharacterMotion>();
            bindPositions = new Vector3[poseBones.Length]; bindRotations = new Quaternion[poseBones.Length];
            for (int i = 0; i < poseBones.Length; i++) { bindPositions[i] = poseBones[i].localPosition; bindRotations[i] = poseBones[i].localRotation; }
            foreach (var part in bodies)
            {
                part.body.isKinematic = true; part.body.useGravity = false; part.body.interpolation = RigidbodyInterpolation.None; part.collider.enabled = false;
                // Self contacts make lightweight animated rigs explode at the first physics step.
                foreach (var other in bodies) if (other != part) Physics.IgnoreCollision(part.collider, other.collider, true);
            }
        }
        public void Activate(Vector3 impulse, Vector3 hitPoint, Severed lost)
        {
            Initialize(); if (Active || bodies.Length == 0) return;
            Active = true; Settled = false; activatedAt = Time.time;
            if (motion != null) motion.Stop();
            var control = GetComponent<CharacterController>(); if (control != null) control.enabled = false;
            RagdollBody nearest = null; float distance = float.PositiveInfinity;
            foreach (var part in bodies)
            {
                part.collider.enabled = (part.region & lost) == 0;
                part.body.isKinematic = false; part.body.useGravity = true; part.body.interpolation = RigidbodyInterpolation.Interpolate;
                part.body.linearVelocity = Vector3.zero; part.body.angularVelocity = Vector3.zero;
                if (!part.collider.enabled) continue;
                float d = (part.body.worldCenterOfMass - hitPoint).sqrMagnitude; if (d < distance) { distance = d; nearest = part; }
            }
            if (nearest != null) nearest.body.AddForceAtPosition(Vector3.ClampMagnitude(impulse, 7), hitPoint, ForceMode.Impulse);
        }
        private void FixedUpdate()
        {
            if (!Active || Settled || Time.time - activatedAt < 1) return;
            bool still = true;
            foreach (var part in bodies) if (part.body.linearVelocity.sqrMagnitude > .035f || part.body.angularVelocity.sqrMagnitude > .08f) { still = false; break; }
            if (still || Time.time - activatedAt > 5) Freeze();
        }
        private void Freeze()
        {
            foreach (var part in bodies)
            {
                if (!part.body.isKinematic) { part.body.linearVelocity = Vector3.zero; part.body.angularVelocity = Vector3.zero; }
                part.body.isKinematic = true; part.body.useGravity = false; part.body.interpolation = RigidbodyInterpolation.None; part.collider.enabled = false;
            }
            Settled = true;
        }
        public void ResetPose()
        {
            Initialize();
            foreach (var part in bodies) { part.body.isKinematic = true; part.body.useGravity = false; part.body.interpolation = RigidbodyInterpolation.None; part.collider.enabled = false; }
            Active = Settled = false;
            if (visualRoot != null) { visualRoot.localPosition = Vector3.zero; visualRoot.localRotation = Quaternion.identity; }
            for (int i = 0; i < poseBones.Length; i++) { poseBones[i].localPosition = bindPositions[i]; poseBones[i].localRotation = bindRotations[i]; }
        }
        public RagdollPose Capture()
        {
            if (!Active) return null;
            var pose = new RagdollPose { present = true, positions = new Vector3[poseBones.Length], rotations = new Quaternion[poseBones.Length], rootPosition = visualRoot != null ? visualRoot.localPosition : Vector3.zero, rootRotation = visualRoot != null ? visualRoot.localRotation : Quaternion.identity };
            for (int i = 0; i < poseBones.Length; i++) { pose.positions[i] = poseBones[i].localPosition; pose.rotations[i] = poseBones[i].localRotation; }
            return pose;
        }
        public bool RestorePose(RagdollPose pose)
        {
            Initialize();
            if (pose == null || !pose.Valid || pose.positions.Length != poseBones.Length) return false;
            if (motion != null) motion.Stop();
            if (visualRoot != null) { visualRoot.localPosition = pose.rootPosition; visualRoot.localRotation = pose.rootRotation; }
            for (int i = 0; i < poseBones.Length; i++) { poseBones[i].localPosition = pose.positions[i]; poseBones[i].localRotation = pose.rotations[i]; }
            Active = true; Freeze(); return true;
        }
    }
}
