using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Zombtoy.Core;
using Zombtoy.Gameplay;
using Object = UnityEngine.Object;

namespace Zombtoy.Editor
{
    public static class Port3DBuilder
    {
        private const string Root = "Assets/_Project";
        private const string Warehouse = "Assets/SciFi Warehouse Kit/Prefabs/";
        private const string Rusty = "Assets/Data/Sci-fi_assets/Sci-Fi Old rusty props PBR/Prefabs/";
        private const string Modular = "Assets/Sci Fi Modular Pack/Prefabs/";
        private static readonly Dictionary<Material, Material> materials = new Dictionary<Material, Material>();
        private static int count;

        [MenuItem("Abisal/Construir puerto con assets 3D")]
        public static void Build()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!Directory.Exists(Warehouse) || !Directory.Exists(Rusty)) throw new InvalidOperationException("Import the local Warehouse and Old Rusty Props packs first; see docs/PORT_3D.md.");
            var scene = EditorSceneManager.OpenScene(PortMapBuilder.ScenePath);
            if (GameObject.Find("Port_Environment_3D") != null) throw new InvalidOperationException("3D port already authored. Restore the pre-migration scene in a validation copy to regenerate.");
            foreach (string dir in new[] { "/Art/Materials/Imported3D", "/Prefabs/Weapons" }) Directory.CreateDirectory(Root + dir);
            AssetDatabase.Refresh(); materials.Clear(); count = 0;
            var root = GameObject.Find("Port_Environment");
            PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            root.name = "Port_Environment_3D";
            foreach (var part in root.GetComponentsInChildren<Transform>().ToArray())
            {
                if (part == null) continue;
                if (part.name == "Container body")
                {
                    var box = part.GetComponent<BoxCollider>();
                    foreach (var r in part.parent.GetComponentsInChildren<Renderer>()) r.enabled = false;
                    Fit(Rusty + "Container_Big.prefab", box.bounds.center, box.bounds.size, part.parent);
                }
                else if (part.name == "Storage crate") Replace(part, Warehouse + "Props/Crates Barrels Pallets/Crate Long.prefab");
                else if (part.name.StartsWith("Warehouse ") && part.name != "Warehouse floor") Wall(part, false);
                else if (part.name.StartsWith("Control ") && (part.name.Contains("north") || part.name.Contains("east") || part.name.Contains("west") || part.name.Contains("entry"))) Wall(part, part.name.Contains("east"));
                else if (part.name == "Warehouse floor" || part.name == "Control floor")
                {
                    var r = part.GetComponent<Renderer>(); var b = r.bounds; r.enabled = false;
                    Tiles(b, part, Warehouse + "Structures/Floor/Floor Tile 04.prefab", 3);
                }
                else if (part.name == "Control desk") Replace(part, Rusty + "Rack.prefab");
                else if (part.name == "Radio console" || part.name.StartsWith("Generator vents"))
                { var r = part.GetComponent<Renderer>(); if (r != null) r.enabled = false; }
            }
            // Loading apron and boarding walkway have real grating and panel seams.
            Tiles(new Bounds(new Vector3(-22, 0.04f, -15), new Vector3(10, 0.08f, 8)), root.transform, Warehouse + "Structures/Floor/Floor Tile 02.prefab", 2);
            Tiles(new Bounds(new Vector3(32.5f, 0.05f, -8), new Vector3(8, 0.08f, 4)), root.transform, Warehouse + "Structures/Floor/Floor Tile 03.prefab", 2);
            var session = Object.FindAnyObjectByType<SandboxSession>();
            // Replace the resource cubes too, so pickups belong to the 3D environment.
            foreach (var pickup in Object.FindObjectsByType<WorldPickup>())
            {
                Vector3 p = pickup.transform.position; p.y = 0.35f; pickup.transform.position = p;
                Replace(pickup.transform, Rusty + "Supply_box.prefab");
            }
            Replace(session.portMission.supplies, Rusty + "Supply_box.prefab");
            Replace(session.portMission.fuse, Rusty + "Broken_panel.prefab");
            var deck = root.GetComponentsInChildren<Transform>().Single(t => t.name == "Quay deck").GetComponent<Renderer>();
            var floorMaterial = Convert(AssetDatabase.LoadAssetAtPath<Material>("Assets/SciFi Warehouse Kit/Art/Materials/Floor Tile Mat.mat"));
            const string groundPath = Root + "/Art/Materials/Imported3D/Harbour_ground.mat";
            var ground = AssetDatabase.LoadAssetAtPath<Material>(groundPath);
            if (ground == null) { ground = new Material(floorMaterial); AssetDatabase.CreateAsset(ground, groundPath); }
            ground.SetTextureScale("_BaseMap", new Vector2(14, 11)); EditorUtility.SetDirty(ground); deck.sharedMaterial = ground;
            Replace(session.portMission.generator, Rusty + "Oxy_agregate.prefab");
            Replace(session.terminal, Rusty + "Old_computer.prefab");
            Replace(session.portMission.controlDoor.transform, Rusty + "Doors.prefab");
            Replace(session.portMission.boardingGate.transform, Rusty + "BulkHead.prefab");
            Decor(root.transform, Warehouse + "Props/Shelves/Shelves Empty.prefab", new Vector3(-24.4f, 0, 18), new Vector3(1.2f, 2.3f, 3), true);
            Decor(root.transform, Warehouse + "Props/Crates Barrels Pallets/Pallet.prefab", new Vector3(-21, 0, -17.5f), new Vector3(2, 0.25f, 1.4f), true);
            Decor(root.transform, Warehouse + "Props/Crates Barrels Pallets/Crate Short.prefab", new Vector3(-21, 0.25f, -17.5f), new Vector3(1.4f, 0.9f, 1), true);
            foreach (float z in new[] { -11f, -8f, -5f })
                Decor(root.transform, Warehouse + "Props/Crates Barrels Pallets/Barrel.prefab", new Vector3(-26.3f, 0, z), new Vector3(0.9f, 1.3f, 0.9f), true);
            Decor(root.transform, Warehouse + "Props/Misc Props/Cart.prefab", new Vector3(8, 0, 20.5f), new Vector3(1.4f, 1.2f, 2), true);
            Decor(root.transform, Rusty + "Electroboxes.prefab", new Vector3(7.5f, 0, 17.5f), new Vector3(1.5f, 2, 1.2f), true);
            Decor(root.transform, Rusty + "Oxy_ballon.prefab", new Vector3(23, 0, 20.5f), new Vector3(0.6f, 1.9f, 0.6f), true);
            Decor(root.transform, Rusty + "Supply_box.prefab", new Vector3(-10, 0, -11), new Vector3(1.1f, 0.8f, 0.8f), true);
            // This second user pack supplies an industrial service partition by the generator.
            string partition = AssetDatabase.FindAssets("t:GameObject", new[] { Modular }).Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == "Wall12");
            if (partition != null) Decor(root.transform, partition, new Vector3(10, 0, 19), new Vector3(0.3f, 2.6f, 3), true);
            Weapons(session.player);
            var locator = session.player.transform.Find("Player locator"); if (locator != null) locator.gameObject.SetActive(false);
            var camera = session.player.view; camera.orthographicSize = 7.5f; camera.allowMSAA = true; camera.allowHDR = true;
            camera.transform.rotation = Quaternion.Euler(35.264f, 45, 0);
            camera.transform.position = session.player.transform.position + Vector3.up * 0.9f - camera.transform.forward * 28;
            camera.backgroundColor = new Color(0.055f, 0.09f, 0.12f);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.63f, 0.72f); RenderSettings.ambientEquatorColor = new Color(0.33f, 0.37f, 0.41f); RenderSettings.ambientGroundColor = new Color(0.15f, 0.18f, 0.2f);
            var sunlight = Object.FindObjectsByType<Light>().First(l => l.type == LightType.Directional);
            sunlight.intensity = 1.5f; sunlight.color = new Color(1, 0.95f, 0.85f); sunlight.shadows = LightShadows.Soft;
            var pipeline = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (pipeline != null) { pipeline.msaaSampleCount = 4; pipeline.renderScale = 1; pipeline.shadowDistance = 65; EditorUtility.SetDirty(pipeline); }
            var surface = root.GetComponent<NavMeshSurface>(); surface.RemoveData(); surface.navMeshData = null;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders; surface.BuildNavMesh();
            const string nav = Root + "/Navigation/Port03_NavMesh.asset";
            if (File.Exists(nav)) AssetDatabase.DeleteAsset(nav);
            AssetDatabase.CreateAsset(surface.navMeshData, nav);
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, Root + "/Prefabs/Levels/Port_Environment_3D.prefab", InteractionMode.AutomatedAction);
            PlayerSettings.bundleVersion = "0.3.0";
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Directory.CreateDirectory("TestResults"); File.WriteAllText("TestResults/port-3d-authoring.txt", "Imported mesh instances: " + count + "\nURP material adapters: " + materials.Count + "\nNative camera: orthographic 35.264 / 45; render scale 1; MSAA4\n");
        }

        private static void Wall(Transform part, bool window)
        {
            var r = part.GetComponent<Renderer>(); if (r == null) return;
            var b = r.bounds; r.enabled = false;
            bool alongX = b.size.x > b.size.z; float length = alongX ? b.size.x : b.size.z;
            int pieces = Mathf.Max(1, Mathf.RoundToInt(length / 3)); float segment = length / pieces;
            for (int i = 0; i < pieces; i++)
            {
                Vector3 center = b.center + (alongX ? Vector3.right : Vector3.forward) * (-length * 0.5f + segment * (i + 0.5f));
                Fit(Warehouse + "Structures/Walls/" + (window ? "Wall Window" : "Wall Plain") + ".prefab", center, alongX ? new Vector3(segment, b.size.y, b.size.z) : new Vector3(b.size.x, b.size.y, segment), part, alongX ? 90 : 0);
            }
        }
        private static void Replace(Transform target, string path)
        {
            var renderer = target.GetComponent<Renderer>(); if (renderer == null) return;
            var bounds = renderer.bounds; renderer.enabled = false; Fit(path, bounds.center, bounds.size, target);
        }
        private static void Tiles(Bounds bounds, Transform parent, string source, float unit)
        {
            int nx = Mathf.RoundToInt(bounds.size.x / unit), nz = Mathf.RoundToInt(bounds.size.z / unit);
            for (int x = 0; x < nx; x++) for (int z = 0; z < nz; z++)
                Fit(source, new Vector3(bounds.min.x + (x + 0.5f) * unit, 0.0125f, bounds.min.z + (z + 0.5f) * unit), new Vector3(unit, 0.025f, unit), parent);
        }
        private static void Decor(Transform parent, string path, Vector3 foot, Vector3 size, bool solid)
        {
            var item = Fit(path, foot + Vector3.up * size.y * 0.5f, size, parent);
            if (solid)
            {
                var collider = item.AddComponent<BoxCollider>();
                Vector3 scale = item.transform.lossyScale;
                collider.size = new Vector3(size.x / Mathf.Abs(scale.x), size.y / Mathf.Abs(scale.y), size.z / Mathf.Abs(scale.z));
            }
            if (size.y > 1.6f) item.AddComponent<MapOccluder>();
        }
        private static GameObject Fit(string path, Vector3 center, Vector3 size, Transform parent, float yaw = 0)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null) throw new InvalidOperationException("Missing downloaded model: " + path);
            var wrapper = new GameObject("3D " + source.name);
            var model = Object.Instantiate(source); model.transform.SetParent(wrapper.transform, false); model.transform.localPosition = Vector3.zero; model.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            foreach (var c in model.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            foreach (var script in model.GetComponentsInChildren<MonoBehaviour>()) Object.DestroyImmediate(script);
            foreach (var animator in model.GetComponentsInChildren<Animator>()) animator.enabled = false;
            var renderers = model.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds; foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            model.transform.localPosition -= bounds.center;
            wrapper.transform.localScale = new Vector3(size.x / Mathf.Max(0.01f, bounds.size.x), size.y / Mathf.Max(0.01f, bounds.size.y), size.z / Mathf.Max(0.01f, bounds.size.z));
            wrapper.transform.SetParent(parent, true); wrapper.transform.position = center;
            foreach (var renderer in renderers) renderer.sharedMaterials = renderer.sharedMaterials.Select(Convert).ToArray();
            count++; return wrapper;
        }
        private static Material Convert(Material source)
        {
            if (source == null) return AssetDatabase.LoadAssetAtPath<Material>(Root + "/Art/Materials/Port/Wet_concrete.mat");
            if (source.shader.name.StartsWith("Universal Render Pipeline/")) return source;
            if (materials.TryGetValue(source, out var cached)) return cached;
            string key = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source));
            if (string.IsNullOrEmpty(key)) key = "default";
            string path = Root + "/Art/Materials/Imported3D/" + key + ".mat";
            var result = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (result == null) { result = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(result, path); }
            result.name = source.name + " URP";
            foreach (var map in new[] { new[] { "_MainTex", "_BaseMap" }, new[] { "_BumpMap", "_BumpMap" }, new[] { "_MetallicGlossMap", "_MetallicGlossMap" }, new[] { "_OcclusionMap", "_OcclusionMap" }, new[] { "_EmissionMap", "_EmissionMap" } })
                if (source.HasProperty(map[0])) { result.SetTexture(map[1], source.GetTexture(map[0])); result.SetTextureScale(map[1], source.GetTextureScale(map[0])); }
            Color color = source.HasProperty("_Color") ? source.GetColor("_Color") : Color.white;
            result.SetColor("_BaseColor", color); result.SetFloat("_Smoothness", 0.32f);
            result.SetFloat("_Metallic", source.HasProperty("_Metallic") ? Mathf.Min(0.65f, source.GetFloat("_Metallic")) : 0.2f);
            if (result.GetTexture("_BumpMap") != null) result.EnableKeyword("_NORMALMAP");
            if (result.GetTexture("_MetallicGlossMap") != null) result.EnableKeyword("_METALLICSPECGLOSSMAP");
            if (result.GetTexture("_OcclusionMap") != null) result.EnableKeyword("_OCCLUSIONMAP");
            if (source.name.Contains("Window") || source.name.Contains("Glass"))
            {
                color.a = 0.25f; result.SetColor("_BaseColor", color); result.SetFloat("_Surface", 1); result.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); result.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha); result.SetFloat("_ZWrite", 0); result.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); result.renderQueue = 3000;
            }
            EditorUtility.SetDirty(result); materials[source] = result; return result;
        }
        private static void Weapons(SandboxPlayer player)
        {
            var old = player.transform.Find("CC0 weapon"); if (old != null) Object.DestroyImmediate(old.gameObject);
            var socket = new GameObject("Weapon socket").transform; socket.SetParent(player.transform, false); socket.localPosition = new Vector3(0.18f, 1.12f, 0.32f);
            var loadout = player.gameObject.GetComponent<WeaponLoadout>(); if (loadout == null) loadout = player.gameObject.AddComponent<WeaponLoadout>();
            loadout.socket = socket; loadout.weapons = new WeaponDefinition[3];
            string[] names = { "Pistola", "Carabina", "Dispersor" };
            for (int i = 0; i < 3; i++)
            {
                string model = "Assets/ThirdParty/Kenney/BlasterKit/Models/FBX format/blaster-" + new[] { "b", "a", "c" }[i] + ".fbx";
                var weapon = Fit(model, new Vector3(0, 0.015f, 0.2f), new Vector3(i == 2 ? 0.18f : 0.14f, 0.24f, i == 0 ? 0.55f : 0.78f), null);
                string materialPath = Root + "/Art/Materials/Imported3D/Weapon_" + i + ".mat";
                var skin = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (skin == null) { skin = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(skin, materialPath); }
                skin.name = names[i] + " gunmetal"; skin.color = new Color(0.17f, 0.22f, 0.24f); skin.SetFloat("_Metallic", 0.7f); skin.SetFloat("_Smoothness", 0.4f);
                EditorUtility.SetDirty(skin); foreach (var r in weapon.GetComponentsInChildren<Renderer>()) r.sharedMaterial = skin;
                // Bake normalization into the model; the instance root is the grip.
                var wrapper = new GameObject("Weapon " + names[i]); weapon.transform.SetParent(wrapper.transform, true);
                var muzzle = new GameObject("Muzzle").transform; muzzle.SetParent(wrapper.transform, false); muzzle.localPosition = new Vector3(0, 0.04f, i == 0 ? 0.5f : 0.62f);
                string prefabPath = Root + "/Prefabs/Weapons/Weapon_" + i + ".prefab";
                var prefab = PrefabUtility.SaveAsPrefabAsset(wrapper, prefabPath); Object.DestroyImmediate(wrapper);
                string dataPath = Root + "/Data/Weapon_" + i + ".asset";
                var definition = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(dataPath);
                if (definition == null) { definition = ScriptableObject.CreateInstance<WeaponDefinition>(); AssetDatabase.CreateAsset(definition, dataPath); }
                definition.displayName = names[i]; definition.visualPrefab = prefab; definition.damage = i == 0 ? 35 : i == 1 ? 23 : 18; definition.pellets = i == 2 ? 5 : 1;
                definition.spread = i == 2 ? 9 : i == 1 ? 1.5f : 0; definition.fireInterval = i == 2 ? 0.85f : i == 1 ? 0.13f : 0.25f; definition.noiseRadius = i == 2 ? 24 : 18;
                EditorUtility.SetDirty(definition); loadout.weapons[i] = definition;
            }
            var pose = player.gameObject.AddComponent<ArmedPose>(); pose.grip = socket;
            pose.support = new GameObject("Support grip").transform; pose.support.SetParent(player.transform, false); pose.support.localPosition = new Vector3(-0.08f, 1.15f, 0.43f);
            Transform Bone(string name) => player.GetComponentsInChildren<Transform>().Single(t => t.name == name);
            pose.rightUpper = Bone("RightArm"); pose.rightLower = Bone("RightForeArm"); pose.rightHand = Bone("RightHand");
            pose.leftUpper = Bone("LeftArm"); pose.leftLower = Bone("LeftForeArm"); pose.leftHand = Bone("LeftHand");
        }
    }
}
