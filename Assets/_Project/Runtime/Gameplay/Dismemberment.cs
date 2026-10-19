using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    [Flags] public enum Severed { None = 0, Head = 1, LeftArm = 2, RightArm = 4, LeftLeg = 8, RightLeg = 16 }

    // Split the original weighted triangles, retaining its rig and texture on both sides.
    [DefaultExecutionOrder(160)]
    public sealed class Dismemberment : MonoBehaviour
    {
        public Severed Lost { get; private set; }
        public Material woundMaterial;
        private readonly List<GameObject> caps = new List<GameObject>();
        private SkinnedMeshRenderer skin;
        private Mesh original, live;
        private BoneWeight[] weights;
        private Severed[] regions;
        private SandboxEnemy enemy;
        private readonly List<GameObject> pieces = new List<GameObject>();
        private readonly List<Mesh> pieceMeshes = new List<Mesh>();
        private void Awake() => Initialize();
        private void Initialize()
        {
            if (live != null) return;
            enemy = GetComponent<SandboxEnemy>(); skin = GetComponentInChildren<SkinnedMeshRenderer>();
            if (skin == null) return;
            original = skin.sharedMesh; live = Instantiate(original); live.name = "Infected detachable mesh"; skin.sharedMesh = live;
            weights = original.boneWeights; regions = new Severed[original.vertexCount];
            for (int i = 0; i < regions.Length; i++) regions[i] = Region(i);
        }
        private Severed Region(int vertex)
        {
            var w = weights[vertex]; int i = w.boneIndex0; float v = w.weight0;
            if (w.weight1 > v) { i = w.boneIndex1; v = w.weight1; }
            if (w.weight2 > v) { i = w.boneIndex2; v = w.weight2; }
            if (w.weight3 > v) i = w.boneIndex3;
            var bone = skin.bones[i];
            for (var b = bone; b != null && b != transform; b = b.parent)
            {
                if (b.name == "Head") return Severed.Head;
                if (b.name == "LeftArm") return Severed.LeftArm;
                if (b.name == "RightArm") return Severed.RightArm;
                if (b.name == "LeftUpLeg") return Severed.LeftLeg;
                if (b.name == "RightUpLeg") return Severed.RightLeg;
            }
            return Severed.None;
        }
        public bool Sever(Severed part, bool effects = true)
        {
            if (skin == null || part == Severed.None || (Lost & part) != 0) return false;
            var cut = new List<int>[original.subMeshCount]; bool any = false;
            for (int s = 0; s < cut.Length; s++)
            {
                cut[s] = new List<int>(); var keep = new List<int>(); var triangles = live.GetTriangles(s);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    bool remove = (regions[triangles[i]] & part) != 0 || (regions[triangles[i + 1]] & part) != 0 || (regions[triangles[i + 2]] & part) != 0;
                    var list = remove ? cut[s] : keep; list.Add(triangles[i]); list.Add(triangles[i + 1]); list.Add(triangles[i + 2]); any |= remove;
                }
                live.SetTriangles(keep, s);
            }
            if (!any) return false;
            Lost |= part;
            string boneName = part == Severed.Head ? "Head" : part == Severed.LeftArm ? "LeftArm" : part == Severed.RightArm ? "RightArm" : part == Severed.LeftLeg ? "LeftUpLeg" : "RightUpLeg";
            var bone = Array.Find(skin.bones, b => b.name == boneName);
            if (bone != null)
            {
                foreach (var attachment in bone.GetComponentsInChildren<MeshRenderer>()) attachment.enabled = false;
                var cap = GameObject.CreatePrimitive(PrimitiveType.Sphere); cap.name = "Wound cap " + part; cap.GetComponent<Collider>().enabled = false; Destroy(cap.GetComponent<Collider>());
                cap.transform.position = bone.position; cap.transform.localScale = new Vector3(.18f, .09f, .18f); cap.transform.SetParent(bone.parent, true);
                cap.GetComponent<Renderer>().sharedMaterial = woundMaterial != null ? woundMaterial : skin.sharedMaterial; caps.Add(cap);
            }
            if (effects)
            {
                var baked = new Mesh(); skin.BakeMesh(baked);
                baked.subMeshCount = original.subMeshCount; for (int s = 0; s < cut.Length; s++) baked.SetTriangles(cut[s], s);
                // BakeMesh uses the source transform. Pivot at the visible cut, not at the actor's feet.
                var vertices = baked.vertices; Vector3 center = Vector3.zero; int count = 0;
                foreach (var tris in cut) foreach (int v in tris) { center += vertices[v]; count++; }
                center /= Mathf.Max(1, count); for (int v = 0; v < vertices.Length; v++) vertices[v] -= center; baked.vertices = vertices; baked.RecalculateBounds();
                var go = new GameObject("Severed " + part); go.transform.SetPositionAndRotation(skin.transform.TransformPoint(center), skin.transform.rotation); go.transform.localScale = skin.transform.lossyScale;
                go.AddComponent<MeshFilter>().sharedMesh = baked; go.AddComponent<MeshRenderer>().sharedMaterials = skin.sharedMaterials;
                var rigid = go.AddComponent<Rigidbody>(); rigid.mass = 1; rigid.linearVelocity = (transform.forward + Vector3.up) * 2;
                var collider = go.AddComponent<SphereCollider>(); collider.radius = .12f / Mathf.Max(.01f, go.transform.lossyScale.x);
                go.layer = 2; pieces.Add(go); pieceMeshes.Add(baked); Destroy(go, 22);
                CombatEffects.Blood(go.transform.position, enemy.session.tracerMaterial, 9);
            }
            ApplyDisability(); return true;
        }
        public void Hit(Vector3 point, float amount, bool cutting)
        {
            if (skin == null || enemy.IsDead) return;
            float height = point.y - transform.position.y;
            var local = transform.InverseTransformPoint(point);
            Severed part = height > 1.48f ? Severed.Head : height < .65f ? (local.x < 0 ? Severed.LeftLeg : Severed.RightLeg) : (local.x < 0 ? Severed.LeftArm : Severed.RightArm);
            if ((height > 1.48f && amount >= 30) || (cutting && amount >= 35) || amount >= 60) Sever(part);
        }
        private void ApplyDisability()
        {
            var variant = GetComponent<InfectedVariant>(); if (variant == null) return;
            variant.LegsLost = ((Lost & Severed.LeftLeg) != 0 ? 1 : 0) + ((Lost & Severed.RightLeg) != 0 ? 1 : 0);
            var cc = GetComponent<CharacterController>(); if (cc != null) { cc.height = variant.LegsLost > 0 ? .9f : 1.7f; cc.center = Vector3.up * cc.height * .5f; }
            variant.ArmsLost = ((Lost & Severed.LeftArm) != 0 ? 1 : 0) + ((Lost & Severed.RightArm) != 0 ? 1 : 0);
        }
        public void Restore(Severed mask)
        {
            Initialize();
            foreach (var go in caps) if (go != null) Destroy(go); caps.Clear();
            if (skin != null) foreach (var bone in skin.bones) foreach (var attachment in bone.GetComponentsInChildren<MeshRenderer>()) attachment.enabled = true;
            foreach (var go in pieces) if (go != null) Destroy(go); pieces.Clear();
            foreach (var mesh in pieceMeshes) if (mesh != null) Destroy(mesh); pieceMeshes.Clear();
            Lost = Severed.None; if (live == null) return;
            for (int s = 0; s < original.subMeshCount; s++) live.SetTriangles(original.GetTriangles(s), s);
            foreach (Severed part in new[] { Severed.Head, Severed.LeftArm, Severed.RightArm, Severed.LeftLeg, Severed.RightLeg }) if ((mask & part) != 0) Sever(part, false);
            ApplyDisability();
        }
        private void LateUpdate()
        {
            if (skin == null || enemy.session == null) return;
            foreach (var bone in skin.bones)
            {
                Severed part = bone.name == "Head" ? Severed.Head : bone.name == "LeftArm" ? Severed.LeftArm : bone.name == "RightArm" ? Severed.RightArm : bone.name == "LeftUpLeg" ? Severed.LeftLeg : bone.name == "RightUpLeg" ? Severed.RightLeg : Severed.None;
                if ((Lost & part) != 0) foreach (var attachment in bone.GetComponentsInChildren<MeshRenderer>()) attachment.enabled = false;
            }
            bool visible = enemy.session.Night == null || enemy.session.Night.Perceives(transform.position + Vector3.up * .9f);
            foreach (var cap in caps) if (cap != null) cap.GetComponent<Renderer>().enabled = visible;
        }
        private void OnDestroy() { if (live != null) Destroy(live); foreach (var mesh in pieceMeshes) if (mesh != null) Destroy(mesh); foreach (var go in pieces) if (go != null) Destroy(go); }
    }
}
