using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zombtoy.Core;
using Zombtoy.Gameplay;
using Object = UnityEngine.Object;

namespace Zombtoy.Editor
{
    // Authored adult proportions and a weighted, jointed body. No extracted game models.
    public static class HumanCharacterBuilder
    {
        private const string Root = "Assets/_Project/Art/Industrial/Characters";
        private static readonly string[] Names = { "Hips", "Spine", "Chest", "Neck", "Head", "LeftShoulder", "LeftArm", "LeftForeArm", "LeftHand", "RightShoulder", "RightArm", "RightForeArm", "RightHand", "LeftUpLeg", "LeftLeg", "LeftFoot", "RightUpLeg", "RightLeg", "RightFoot" };
        private static readonly int[] Parents = { -1, 0, 1, 2, 3, 2, 5, 6, 7, 2, 9, 10, 11, 0, 13, 14, 0, 16, 17 };
        private static readonly Vector3[] Points = {
            new Vector3(0,.94f,0), new Vector3(0,1.12f,0), new Vector3(0,1.35f,0), new Vector3(0,1.52f,0), new Vector3(0,1.64f,0),
            new Vector3(-.15f,1.43f,0), new Vector3(-.24f,1.43f,0), new Vector3(-.49f,1.43f,0), new Vector3(-.72f,1.43f,0),
            new Vector3(.15f,1.43f,0), new Vector3(.24f,1.43f,0), new Vector3(.49f,1.43f,0), new Vector3(.72f,1.43f,0),
            new Vector3(-.105f,.93f,0), new Vector3(-.105f,.52f,0), new Vector3(-.105f,.12f,.02f),
            new Vector3(.105f,.93f,0), new Vector3(.105f,.52f,0), new Vector3(.105f,.12f,.02f) };

        public static void ReplaceAll(SandboxSession session)
        {
            Directory.CreateDirectory(Root); Directory.CreateDirectory("Assets/_Project/Prefabs/Actors"); AssetDatabase.Refresh();
            Replace(session.player.gameObject, true, InfectedKind.Walker, 0);
            int[] kindCounts = new int[4];
            var saved = new System.Collections.Generic.HashSet<string>();
            foreach (var enemy in Object.FindObjectsByType<SandboxEnemy>(FindObjectsInactive.Include).OrderBy(e => e.name))
            {
                var kind = enemy.GetComponent<InfectedVariant>().kind; int variant = kindCounts[(int)kind]++ % 2;
                Replace(enemy.gameObject, false, kind, variant);
                enemy.definition = Definition(kind, variant);
                enemy.health = enemy.definition.health; enemy.speed = enemy.definition.speed; enemy.sightRange = enemy.definition.sightRange;
                var ragdoll = enemy.GetComponent<RagdollController>();
                if (ragdoll == null) ragdoll = enemy.gameObject.AddComponent<RagdollController>();
                AddPhysics(enemy.gameObject, ragdoll);
                string prefab = "Assets/_Project/Prefabs/Actors/Infected_" + kind + "_" + variant + ".prefab";
                if (saved.Add(prefab))
                {
                    var old = enemy.session; bool wasActive = enemy.gameObject.activeSelf; enemy.session = null; enemy.gameObject.SetActive(true);
                    PrefabUtility.SaveAsPrefabAsset(enemy.gameObject, prefab); enemy.session = old; enemy.gameObject.SetActive(wasActive);
                }
            }
        }
        private static EnemyDefinition Definition(InfectedKind kind, int variant)
        {
            string path = Root + "/" + kind + "_" + variant + ".asset";
            var def = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if (def == null) { def = ScriptableObject.CreateInstance<EnemyDefinition>(); AssetDatabase.CreateAsset(def, path); }
            def.displayName = (kind == InfectedKind.Walker ? "Errante" : kind == InfectedKind.Runner ? "Corredor" : kind == InfectedKind.Brute ? "Acorazado" : "Aullador") + (variant == 0 ? " / estibador" : " / cuarentena");
            def.health = kind == InfectedKind.Brute ? 180 : kind == InfectedKind.Runner ? 55 : kind == InfectedKind.Screamer ? 85 : 70;
            def.speed = kind == InfectedKind.Runner ? 2.8f : kind == InfectedKind.Brute ? .9f : 1.15f;
            if (variant == 1) { def.health *= 1.12f; def.speed *= .9f; }
            def.sightRange = kind == InfectedKind.Screamer ? 10 : 7; def.hearingMultiplier = variant == 0 ? 1 : 1.2f;
            def.damage = 12; def.attackWindup = kind == InfectedKind.Brute ? .9f : .6f; def.attackRecovery = 1.3f;
            EditorUtility.SetDirty(def); return def;
        }
        private static void Replace(GameObject actor, bool soldier, InfectedKind kind, int variant)
        {
            bool active = actor.activeSelf; actor.SetActive(true);
            if (PrefabUtility.IsPartOfPrefabInstance(actor)) PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(actor), PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            GameObject tank = null;
            if (soldier)
            {
                var source = actor.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Soldier oxygen tank");
                if (source != null) { tank = Object.Instantiate(source.gameObject); tank.transform.SetParent(null); }
            }
            // Unpacking an imported model restores its source root name, so identify the old visual by its skin.
            var oldRoots = new HashSet<GameObject>();
            foreach (var renderer in actor.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                var node = renderer.transform; while (node.parent != null && node.parent != actor.transform) node = node.parent;
                if (node != actor.transform) oldRoots.Add(node.gameObject);
            }
            foreach (var oldRoot in oldRoots) Object.DestroyImmediate(oldRoot);
            var visual = new GameObject("Character visual"); visual.transform.SetParent(actor.transform, false);
            var rig = new GameObject("Rig").transform; rig.SetParent(visual.transform, false);
            var bones = new Transform[Names.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i] = new GameObject(Names[i]).transform; bones[i].SetParent(Parents[i] < 0 ? rig : bones[Parents[i]], false);
                bones[i].position = rig.TransformPoint(Points[i]);
            }
            string key = soldier ? "Soldier" : kind + "_" + variant;
            var skin = visual.AddComponent<SkinnedMeshRenderer>();
            skin.bones = bones; skin.rootBone = bones[0]; skin.updateWhenOffscreen = true; skin.localBounds = new Bounds(Vector3.up, new Vector3(8, 6, 8));
            string meshPath = Root + "/Body_" + key + ".asset"; var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            var authoredMesh = Body(key, soldier, kind, variant); authoredMesh.bindposes = bones.Select(b => b.worldToLocalMatrix * visual.transform.localToWorldMatrix).ToArray();
            if (mesh == null) { mesh = authoredMesh; AssetDatabase.CreateAsset(mesh, meshPath); }
            else { mesh.Clear(); mesh.vertices = authoredMesh.vertices; mesh.uv = authoredMesh.uv; mesh.boneWeights = authoredMesh.boneWeights; mesh.bindposes = authoredMesh.bindposes; mesh.subMeshCount = authoredMesh.subMeshCount; for (int s = 0; s < mesh.subMeshCount; s++) mesh.SetTriangles(authoredMesh.GetTriangles(s), s); mesh.RecalculateNormals(); mesh.RecalculateBounds(); EditorUtility.SetDirty(mesh); Object.DestroyImmediate(authoredMesh); }
            skin.sharedMesh = mesh;
            var clothing = Mat("Cloth_" + key, soldier ? new Color(.58f, .60f, .62f) : variant == 0 ? new Color(.68f, .56f, .42f) : new Color(.75f, .76f, .64f));
            clothing.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Industrial/Textures/WornCloth_v1.png"));
            var flesh = Mat("Skin_" + key, soldier ? new Color(.48f, .38f, .31f) : variant == 0 ? new Color(.43f, .45f, .35f) : new Color(.48f, .41f, .36f));
            var boots = Mat("Boot_leather", new Color(.10f, .09f, .08f)); var armour = Mat("Tactical_plates", new Color(.14f, .15f, .16f));
            var wound = Mat("Exposed_tissue", new Color(.25f, .035f, .03f));
            var face = Mat("Face_" + key, variant == 0 ? new Color(.94f, .93f, .84f) : new Color(.80f, .83f, .76f));
            var faceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Industrial/Textures/InfectedFace_v1.png"); if (faceTexture != null) face.SetTexture("_BaseMap", faceTexture);
            skin.sharedMaterials = new[] { clothing, flesh, boots, armour, wound, face };
            var animation = rig.gameObject.AddComponent<Animation>();
            var idle = Animation(key, bones, rig, soldier, false); var move = Animation(key, bones, rig, soldier, true);
            AnimationUtility.SetAnimationClips(animation, new[] { idle, move }); animation.clip = idle; animation.playAutomatically = true; animation.cullingType = AnimationCullingType.AlwaysAnimate; idle.SampleAnimation(rig.gameObject, 0);
            var motion = actor.GetComponent<CharacterMotion>() ?? actor.AddComponent<CharacterMotion>(); motion.rig = animation; motion.idleState = idle.name; motion.moveState = move.name;
            if (soldier)
            {
                var ik = actor.GetComponent<ArmedPose>();
                ik.rightUpper = bones[10]; ik.rightLower = bones[11]; ik.rightHand = bones[12]; ik.leftUpper = bones[6]; ik.leftLower = bones[7]; ik.leftHand = bones[8];
                ik.grip.localPosition = new Vector3(.21f, 1.29f, .38f); ik.support.localPosition = new Vector3(-.06f, 1.29f, .53f);
                var loadout = actor.GetComponent<WeaponLoadout>(); loadout.socket.localPosition = new Vector3(.18f, 1.28f, .4f);
                if (tank != null)
                {
                    tank.name = "Soldier oxygen tank"; tank.transform.SetParent(bones[2], false); tank.transform.position = rig.TransformPoint(new Vector3(0, 1.23f, -.22f)); tank.transform.rotation = actor.transform.rotation;
                    var renderers = tank.GetComponentsInChildren<Renderer>(); foreach (var r in renderers) r.sharedMaterial = Mat("Oxygen_tank_matte", new Color(.27f, .29f, .25f), .08f);
                }
                Attach(bones[2], rig, "Harness left", new Vector3(-.13f, 1.27f, .135f), new Vector3(.045f, .4f, .035f), armour);
                Attach(bones[2], rig, "Harness right", new Vector3(.13f, 1.27f, .135f), new Vector3(.045f, .4f, .035f), armour);
                var stitching=Mat("Tactical seams",new Color(.24f,.25f,.22f));
                Attach(bones[14],rig,"Left knee pad",new Vector3(-.105f,.53f,.083f),new Vector3(.13f,.16f,.045f),armour);
                Attach(bones[17],rig,"Right knee pad",new Vector3(.105f,.53f,.083f),new Vector3(.13f,.16f,.045f),armour);
                Attach(bones[2],rig,"Vest lower webbing",new Vector3(0,1.13f,.17f),new Vector3(.36f,.028f,.018f),stitching);
                Attach(bones[2],rig,"Vest upper webbing",new Vector3(0,1.31f,.17f),new Vector3(.36f,.025f,.018f),stitching);
                Attach(bones[0],rig,"Belt utility pouch",new Vector3(.19f,.94f,.045f),new Vector3(.115f,.13f,.1f),stitching);
                Attach(bones[2], rig, "Armoured vest", new Vector3(0, 1.26f, .02f), new Vector3(.41f, .4f, .28f), armour);
                Attach(bones[4], rig, "Respirator mask", new Vector3(0, 1.63f, .115f), new Vector3(.17f, .105f, .10f), boots);
                foreach (int sign in new[] { -1, 1 })
                {
                    Attach(bones[4], rig, "Mask lens", new Vector3(sign * .048f, 1.707f, .105f), new Vector3(.065f, .036f, .03f), Mat("Respirator_glass", new Color(.12f, .16f, .15f), .18f));
                    Attach(bones[4], rig, "Mask filter", new Vector3(sign * .07f, 1.615f, .14f), new Vector3(.065f, .065f, .065f), boots);
                }
                var hose = new GameObject("Oxygen hose").AddComponent<LineRenderer>(); hose.transform.SetParent(bones[2], false); hose.useWorldSpace = false;
                hose.positionCount = 5; hose.startWidth = hose.endWidth = .025f; hose.sharedMaterial = boots;
                hose.SetPositions(new[] { rig.TransformPoint(new Vector3(.1f, 1.5f, -.22f)), rig.TransformPoint(new Vector3(.25f, 1.48f, -.1f)), rig.TransformPoint(new Vector3(.22f, 1.47f, .1f)), rig.TransformPoint(new Vector3(.16f, 1.56f, .16f)), rig.TransformPoint(new Vector3(.065f, 1.61f, .17f)) }.Select(p => bones[2].InverseTransformPoint(p)).ToArray());
            }
            else if (kind == InfectedKind.Brute)
            {
                Attach(bones[2], rig, "Damaged riot vest", new Vector3(0, 1.24f, .025f), new Vector3(.5f, .43f, .35f), armour);
            }
            else if (kind == InfectedKind.Screamer)
                Attach(bones[3], rig, "Infected throat", new Vector3(0, 1.52f, .07f), new Vector3(.11f, .12f, .095f), wound);
            var cc = actor.GetComponent<CharacterController>(); cc.height = 1.82f; cc.center = Vector3.up * .91f; cc.radius = .29f;
            actor.SetActive(active);
        }
        private static Mesh Body(string key, bool soldier, InfectedKind kind, int variant)
        {
            var m = new Surface(); float girth = !soldier && kind == InfectedKind.Brute ? 1.3f : !soldier && kind == InfectedKind.Runner ? .9f : 1;
            float belly = !soldier && kind == InfectedKind.Brute ? .17f : .11f;
            m.Rings(new[] { new Vector3(0, .86f, 0), new Vector3(0, 1.02f, 0), new Vector3(0, 1.23f, 0), new Vector3(0, 1.4f, 0), new Vector3(0, 1.48f, 0) },
                new[] { .15f * girth, .165f * girth, .23f * girth, .215f * girth, .13f }, new[] { .095f, belly, .135f, .12f, .08f }, new[] { 0, 1, 2, 2, 2 }, 0, false);
            foreach (int sign in new[] { -1, 1 })
            {
                int thigh = sign < 0 ? 13 : 16, calf = thigh + 1, foot = thigh + 2;
                m.Rings(new[] { new Vector3(sign * .105f, .94f, 0), new Vector3(sign * .105f, .78f, 0), new Vector3(sign * .105f, .53f, .01f) }, new[] { .105f, .1f, .075f }, new[] { .105f, .105f, .075f }, new[] { thigh, thigh, thigh }, 0, false);
                m.Rings(new[] { new Vector3(sign * .105f, .53f, .01f), new Vector3(sign * .105f, .34f, 0), new Vector3(sign * .105f, .12f, .015f) }, new[] { .075f, .075f, .055f }, new[] { .075f, .075f, .055f }, new[] { calf, calf, calf }, 0, false);
                m.Rings(new[]{new Vector3(sign*.105f,.025f,.075f),new Vector3(sign*.105f,.07f,.085f),new Vector3(sign*.105f,.13f,.04f),new Vector3(sign*.105f,.20f,0)},new[]{.08f,.086f,.071f,.058f},new[]{.15f,.17f,.12f,.06f},new[]{foot,foot,foot,foot},2,false);
                m.Rings(new[]{new Vector3(sign*.105f,.015f,.075f),new Vector3(sign*.105f,.035f,.075f)},new[]{.083f,.083f},new[]{.155f,.155f},new[]{foot,foot},2,false);
                int arm = sign < 0 ? 6 : 10, fore = arm + 1, hand = arm + 2;
                m.Rings(new[] { new Vector3(sign * .21f, 1.43f, 0), new Vector3(sign * .34f, 1.43f, 0), new Vector3(sign * .49f, 1.43f, 0) }, new[] { .088f, .08f, .06f }, new[] { .09f, .085f, .062f }, new[] { arm, arm, arm }, 0, true);
                m.Rings(new[] { new Vector3(sign * .49f, 1.43f, 0), new Vector3(sign * .61f, 1.43f, 0), new Vector3(sign * .72f, 1.43f, 0) }, new[] { .06f, .06f, .04f }, new[] { .062f, .055f, .04f }, new[] { fore, fore, fore }, 0, true);
                m.Rings(new[]{new Vector3(sign*.72f,1.43f,0),new Vector3(sign*.77f,1.43f,.01f),new Vector3(sign*.82f,1.425f,.025f),new Vector3(sign*.855f,1.42f,.035f)},new[]{.035f,.039f,.032f,.01f},new[]{.04f,.055f,.048f,.015f},new[]{hand,hand,hand,hand},soldier?2:1,true);
                m.Rings(new[]{new Vector3(sign*.75f,1.415f,.04f),new Vector3(sign*.78f,1.40f,.085f)},new[]{.022f,.012f},new[]{.025f,.016f},new[]{hand,hand},soldier?2:1,true);
            }
            m.Rings(new[] { new Vector3(0, 1.46f, 0), new Vector3(0, 1.57f, 0) }, new[] { .052f, .052f }, new[] { .055f, .055f }, new[] { 3, 3 }, 1, false);
            m.Rings(new[] { new Vector3(0, 1.565f, .005f), new Vector3(0, 1.60f, .01f), new Vector3(0, 1.68f, .005f), new Vector3(0, 1.77f, 0), new Vector3(0, 1.83f, 0), new Vector3(0, 1.85f, -.005f) },
                new[] { .055f, .078f, .091f, .091f, .067f, .025f }, new[] { .064f, .078f, .088f, .095f, .075f, .035f }, new[] { 4, 4, 4, 4, 4, 4 }, 5, false);
            m.Box(new Vector3(0, 1.685f, .091f), new Vector3(.025f, .045f, .025f), 4, 1);
            if (soldier)
                m.Rings(new[] { new Vector3(0, 1.74f, 0), new Vector3(0, 1.80f, 0), new Vector3(0, 1.855f, 0), new Vector3(0, 1.88f, 0) }, new[] { .112f, .112f, .082f, .024f }, new[] { .126f, .125f, .09f, .025f }, new[] { 4, 4, 4, 4 }, 3, false);
            else
            {
                m.Rings(new[] { new Vector3(0, 1.78f, -.012f), new Vector3(0, 1.83f, -.01f), new Vector3(0, 1.856f, -.01f) }, new[] { .092f, .073f, .025f }, new[] { .091f, .077f, .033f }, new[] { 4, 4, 4 }, 2, false);
                if (variant == 1) m.Box(new Vector3(.11f, 1.25f, .135f), new Vector3(.11f, .2f, .012f), 2, 4);
            }
            return m.Finish("Adult body " + key);
        }
        private static AnimationClip Animation(string key, Transform[] bones, Transform rig, bool soldier, bool moving)
        {
            string path = Root + "/" + key + (moving ? "_move" : "_idle") + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path); if (clip != null) return clip;
            clip = new AnimationClip { name = moving ? "move" : "idle", legacy = true, wrapMode = WrapMode.Loop, frameRate = 30 };
            float duration = moving ? .82f : 3.2f;
            for (int i = 0; i < bones.Length; i++)
            {
                string bonePath = AnimationUtility.CalculateTransformPath(bones[i], rig);
                float z = i == 6 ? 76 : i == 10 ? -76 : 0;
                clip.SetCurve(bonePath, typeof(Transform), "localEulerAnglesRaw.z", UnityEngine.AnimationCurve.Constant(0, duration, z));
                float amplitude = moving ? i == 13 || i == 16 ? 28 : i == 14 || i == 17 ? 18 : i == 6 || i == 10 ? soldier ? 13 : 24 : i == 2 ? 2 : 0 : i == 2 ? 1.2f : 0;
                float phase = i == 16 || i == 17 || i == 6 ? Mathf.PI : 0; float baseline = i == 2 ? moving ? 7 : 0 : i == 7 || i == 11 ? -8 : 0;
                var curve = new AnimationCurve(); for (int f = 0; f <= 16; f++) { float t = f / 16f; curve.AddKey(t * duration, baseline + Mathf.Sin(t * Mathf.PI * 2 + phase) * amplitude); }
                for (int f = 0; f < curve.length; f++) AnimationUtility.SetKeyLeftTangentMode(curve, f, AnimationUtility.TangentMode.Auto);
                clip.SetCurve(bonePath, typeof(Transform), "localEulerAnglesRaw.x", curve);
            }
            var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = true; AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, path); return clip;
        }
        private static void AddPhysics(GameObject actor, RagdollController ragdoll)
        {
            var skin = actor.GetComponentInChildren<SkinnedMeshRenderer>(); var bones = skin.bones; ragdoll.poseBones = bones; ragdoll.pelvis = bones[0]; ragdoll.visualRoot = skin.transform;
            var parts = new List<RagdollBody>(); var mapping = new Dictionary<int, Rigidbody>();
            int[] indices = { 0, 2, 4, 6, 7, 10, 11, 13, 14, 15, 16, 17, 18 };
            foreach (int i in indices)
            {
                var rb = bones[i].gameObject.AddComponent<Rigidbody>(); rb.mass = i == 0 ? 12 : i == 2 ? 15 : i == 4 ? 5 : i == 13 || i == 16 ? 8 : i == 14 || i == 17 ? 5 : 2;
                rb.isKinematic = true; rb.useGravity = false; rb.interpolation = RigidbodyInterpolation.Interpolate; rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rb.solverIterations = 10; rb.solverVelocityIterations = 4; rb.maxAngularVelocity = 12; rb.maxDepenetrationVelocity = 2;
                Collider collider;
                if (i == 0 || i == 2 || i == 15 || i == 18)
                {
                    var box = bones[i].gameObject.AddComponent<BoxCollider>(); box.size = i == 0 ? new Vector3(.26f, .2f, .2f) : i == 2 ? new Vector3(.36f, .35f, .23f) : new Vector3(.13f, .14f, .25f);
                    box.center = i == 0 ? new Vector3(0, .02f, 0) : i == 2 ? new Vector3(0, -.04f, 0) : new Vector3(0, -.02f, .08f); collider = box;
                }
                else
                {
                    var capsule = bones[i].gameObject.AddComponent<CapsuleCollider>(); bool arm = i == 6 || i == 7 || i == 10 || i == 11;
                    capsule.direction = arm ? 0 : 1; capsule.radius = i == 4 ? .09f : arm ? .065f : .075f; capsule.height = i == 4 ? .26f : arm ? .27f : .42f;
                    capsule.center = i == 4 ? new Vector3(0, .07f, 0) : arm ? new Vector3(i < 9 ? -.12f : .12f, 0, 0) : new Vector3(0, -.19f, 0); collider = capsule;
                }
                collider.enabled = false; bones[i].gameObject.layer = 2;
                Severed region = i == 4 ? Severed.Head : i == 6 || i == 7 ? Severed.LeftArm : i == 10 || i == 11 ? Severed.RightArm : i >= 13 && i <= 15 ? Severed.LeftLeg : i >= 16 ? Severed.RightLeg : Severed.None;
                parts.Add(new RagdollBody { bone = bones[i], body = rb, collider = collider, region = region }); mapping[i] = rb;
            }
            foreach (int i in indices)
            {
                if (i == 0) continue; int parent = i == 2 ? 0 : i == 4 ? 2 : i == 6 || i == 10 ? 2 : i == 7 ? 6 : i == 11 ? 10 : i == 13 || i == 16 ? 0 : i - 1;
                var joint = bones[i].gameObject.AddComponent<ConfigurableJoint>(); joint.connectedBody = mapping[parent];
                joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Locked;
                joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Limited;
                joint.lowAngularXLimit = new SoftJointLimit { limit = i == 14 || i == 17 ? -5 : -45 }; joint.highAngularXLimit = new SoftJointLimit { limit = i == 14 || i == 17 ? 110 : 60 };
                joint.angularYLimit = new SoftJointLimit { limit = 25 }; joint.angularZLimit = new SoftJointLimit { limit = 30 };
                joint.projectionMode = JointProjectionMode.PositionAndRotation; joint.projectionDistance = .025f; joint.projectionAngle = 8; joint.enablePreprocessing = false;
            }
            ragdoll.bodies = parts.ToArray();
        }
        private static Material Mat(string name, Color color, float smooth = .035f)
        {
            string path = Root + "/" + name + ".mat"; var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
            m.shader=Shader.Find("Abisal/Retro Surface");m.SetColor("_BaseColor", color); m.SetFloat("_Metallic", 0); m.SetFloat("_Smoothness", smooth); EditorUtility.SetDirty(m); return m;
        }
        private static void Attach(Transform bone, Transform rig, string name, Vector3 center, Vector3 size, Material material)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(bone, false); go.transform.position = rig.TransformPoint(center); go.transform.rotation = rig.rotation;
            var surface = new Surface(); surface.Box(Vector3.zero, size, 0, 0); var mesh = surface.Finish(name); mesh.subMeshCount = 1; string path = Root + "/Part_" + name.Replace(" ", "_") + ".asset";
            var stored = AssetDatabase.LoadAssetAtPath<Mesh>(path); if (stored == null) { AssetDatabase.CreateAsset(mesh, path); stored = mesh; } else {stored.Clear();stored.vertices=mesh.vertices;stored.uv=mesh.uv;stored.subMeshCount=mesh.subMeshCount;for(int sub=0;sub<mesh.subMeshCount;sub++)stored.SetTriangles(mesh.GetTriangles(sub),sub);stored.RecalculateNormals();stored.RecalculateBounds();EditorUtility.SetDirty(stored);Object.DestroyImmediate(mesh);}
            go.GetComponent<MeshFilter>().sharedMesh = stored; go.GetComponent<MeshRenderer>().sharedMaterial = material;
        }
        private sealed class Surface
        {
            private readonly List<Vector3> vertices = new List<Vector3>(); private readonly List<Vector2> uv = new List<Vector2>(); private readonly List<BoneWeight> weights = new List<BoneWeight>();
            private readonly List<int>[] indices = Enumerable.Range(0, 6).Select(_ => new List<int>()).ToArray();
            private void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int bone, int mat, float u0 = 0, float u1 = 1)
            {
                int first = vertices.Count; vertices.AddRange(new[] { a, b, c, d }); uv.AddRange(new[] { new Vector2(u0, 0), new Vector2(u0, 1), new Vector2(u1, 1), new Vector2(u1, 0) });
                if (mat == 5) { var points = new[] { a, b, c, d }; for (int i = 0; i < 4; i++) { var p = points[i]; uv[first + i] = p.z > -.005f ? new Vector2(Mathf.Clamp01(.5f + p.x / .19f), Mathf.Clamp01((p.y - 1.565f) / .285f)) : new Vector2(.01f, Mathf.Clamp01((p.y - 1.565f) / .285f)); } }
                for (int i = 0; i < 4; i++) weights.Add(new BoneWeight { boneIndex0 = bone, weight0 = 1 }); indices[mat].AddRange(new[] { first, first + 1, first + 2, first, first + 2, first + 3 });
            }
            public void Rings(Vector3[] centers, float[] radiusA, float[] radiusB, int[] bones, int mat, bool alongX)
            {
                const int sides = 10; var rings = new Vector3[centers.Length, sides];
                for (int r = 0; r < centers.Length; r++) for (int j = 0; j < sides; j++) { float a = j * Mathf.PI * 2 / sides; rings[r, j] = centers[r] + (alongX ? new Vector3(0, Mathf.Cos(a) * radiusA[r], Mathf.Sin(a) * radiusB[r]) : new Vector3(Mathf.Cos(a) * radiusA[r], 0, Mathf.Sin(a) * radiusB[r])); }
                for (int r = 0; r < centers.Length - 1; r++) for (int j = 0; j < sides; j++)
                    { int k = (j + 1) % sides; if ((alongX && centers[r + 1].x > centers[r].x) || (!alongX && centers[r + 1].y < centers[r].y)) Quad(rings[r, j], rings[r, k], rings[r + 1, k], rings[r + 1, j], bones[r], mat, j / (float)sides, k / (float)sides); else Quad(rings[r, j], rings[r + 1, j], rings[r + 1, k], rings[r, k], bones[r], mat, j / (float)sides, k / (float)sides); }
                // Small cap fans share the region's bone, so detached regions remain closed.
                bool reverse = (alongX && centers[1].x > centers[0].x) || (!alongX && centers[1].y < centers[0].y);
                for (int j = 0; j < sides; j++) { int k = (j + 1) % sides; Quad(centers[0], rings[0, reverse ? k : j], rings[0, reverse ? j : k], centers[0], bones[0], mat); int r = centers.Length - 1; Quad(centers[r], rings[r, reverse ? j : k], rings[r, reverse ? k : j], centers[r], bones[r], mat); }
            }
            public void Box(Vector3 center, Vector3 size, int bone, int mat)
            {
                Vector3 half=size*.5f;float radius=Mathf.Min(size.x,size.y,size.z)*.22f;
                Vector3[] normals={Vector3.forward,Vector3.back,Vector3.left,Vector3.right,Vector3.up,Vector3.down};
                Vector3[] us={Vector3.right,Vector3.left,Vector3.forward,Vector3.back,Vector3.right,Vector3.right};
                Vector3[] vs={Vector3.up,Vector3.up,Vector3.up,Vector3.up,Vector3.back,Vector3.forward};
                Vector3 Round(Vector3 p) {var inner=Vector3.Max(Vector3.zero,half-Vector3.one*radius);var core=new Vector3(Mathf.Clamp(p.x,-inner.x,inner.x),Mathf.Clamp(p.y,-inner.y,inner.y),Mathf.Clamp(p.z,-inner.z,inner.z));return center+core+(p-core).normalized*radius;}
                for(int face=0;face<6;face++)
                {
                    var n=normals[face];var u=us[face];var v=vs[face];
                    float hn=Mathf.Abs(Vector3.Dot(n,half)),hu=Mathf.Abs(Vector3.Dot(u,half)),hv=Mathf.Abs(Vector3.Dot(v,half));
                    float[] xs={-hu,-hu+radius,hu-radius,hu},ys={-hv,-hv+radius,hv-radius,hv};
                    for(int x=0;x<3;x++)for(int y=0;y<3;y++)Quad(Round(n*hn+u*xs[x]+v*ys[y]),Round(n*hn+u*xs[x+1]+v*ys[y]),Round(n*hn+u*xs[x+1]+v*ys[y+1]),Round(n*hn+u*xs[x]+v*ys[y+1]),bone,mat);
                }
            }
            public Mesh Finish(string name)
            {
                var mesh = new Mesh { name = name }; mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.boneWeights = weights.ToArray(); mesh.subMeshCount = indices.Length;
                for (int i = 0; i < indices.Length; i++) mesh.SetTriangles(indices[i], i); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
            }
        }
    }
}
