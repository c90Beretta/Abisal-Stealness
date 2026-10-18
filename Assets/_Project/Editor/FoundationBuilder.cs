using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Zombtoy.Core;
using Zombtoy.Gameplay;

namespace Zombtoy.Editor
{
    public static class FoundationBuilder
    {
        public const string Root = "Assets/_Project";
        public const string ScenePath = Root + "/Scenes/MVP_Sandbox.unity";
        private static Material steel, floor, red, green, black, skin;

        [MenuItem("Zombtoy/Generate foundation sandbox")]
        public static void Generate()
        {
            if (File.Exists(ScenePath))
                throw new InvalidOperationException("Sandbox already exists. Preserve edits; regenerate in a separate validation copy.");
            Scene previous = SceneManager.GetActiveScene();
            if (!Application.isBatchMode && string.IsNullOrEmpty(previous.path))
                throw new InvalidOperationException("Save the current untitled scene before generating the sandbox.");
            foreach (string dir in new[] { "/Scenes", "/Art/Materials", "/Data", "/Prefabs" })
                Directory.CreateDirectory(Root + dir);
            AssetDatabase.Refresh();
            steel = Material("Oxidized steel", new Color(0.22f, 0.37f, 0.31f));
            floor = Material("Grated floor", new Color(0.15f, 0.19f, 0.18f));
            red = Material("Emergency red", new Color(0.55f, 0.12f, 0.1f));
            green = Material("Clinical green", new Color(0.3f, 0.8f, 0.5f));
            black = Material("Soldier charcoal", new Color(0.06f, 0.09f, 0.08f));
            skin = Material("Infected", new Color(0.5f, 0.38f, 0.3f));
            var config = AssetDatabase.LoadAssetAtPath<SurvivalConfig>(Root + "/Data/Survival_Default.asset");
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<SurvivalConfig>();
                AssetDatabase.CreateAsset(config, Root + "/Data/Survival_Default.asset");
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
                Application.isBatchMode ? NewSceneMode.Single : NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.23f, 0.29f, 0.26f);
                var session = new GameObject("Sandbox systems").AddComponent<SandboxSession>();
                session.config = config;
                session.tracerMaterial = green;
                var level = new GameObject("Level - control room and dock").transform;
                Cube("Deck", new Vector3(0, -0.25f, 0), new Vector3(22, 0.5f, 18), floor, level);
                Cube("Rear bulkhead", new Vector3(0, 1.5f, 9), new Vector3(22, 3, 0.35f), steel, level);
                Cube("Side bulkhead", new Vector3(-11, 1.5f, 0), new Vector3(0.35f, 3, 18), steel, level);
                // Low front walls keep the sandbox readable; dynamic cutaway is a production task.
                Cube("Front cut wall", new Vector3(0, 0.3f, -9), new Vector3(22, 0.6f, 0.35f), steel, level);
                Cube("Right cut wall", new Vector3(11, 0.3f, 0), new Vector3(0.35f, 0.6f, 18), steel, level);
                for (int x = -10; x <= 10; x += 2)
                    Cube("Deck seam", new Vector3(x, 0.007f, 0), new Vector3(0.035f, 0.01f, 18), black, level, false);
                for (int z = -8; z <= 8; z += 2)
                    Cube("Deck seam", new Vector3(0, 0.009f, z), new Vector3(22, 0.01f, 0.035f), black, level, false);
                foreach (Vector3 point in new[] { new Vector3(-4, 0.7f, 1), new Vector3(3, 0.7f, 4), new Vector3(6, 0.7f, -2) })
                {
                    Cube("Cargo container", point, new Vector3(2.4f, 1.4f, 1.6f), steel, level);
                    for (int i = -2; i <= 2; i++)
                        Cube("Container rib", point + new Vector3(i * 0.4f, 0, -0.83f), new Vector3(0.05f, 1.3f, 0.05f), black, level, false);
                }
                var terminal = Cube("TERMINAL - E", new Vector3(-7, 0.6f, 6), new Vector3(1.2f, 1.2f, 0.7f), steel, level);
                Cube("Terminal screen", new Vector3(-7, 1f, 5.6f), new Vector3(0.8f, 0.5f, 0.04f), red, level, false);
                session.terminal = terminal.transform;
                session.extraction = Cube("EXTRACTION - E", new Vector3(8, 0.03f, 7), new Vector3(2.5f, 0.06f, 2.5f), green, level, false).transform;
                Label("TERMINAL", new Vector3(-7, 2f, 6), red, level);
                Label("EXTRACCION", new Vector3(8, 1.3f, 7), green, level);
                Light("Extraction beacon", new Vector3(8, 2, 7), Color.green, 4, 6);
                Light("Red emergency", new Vector3(-7, 2.4f, 5), new Color(1, 0.13f, 0.05f), 4, 8);
                var sun = new GameObject("Hard key light").AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.intensity = 1.2f;
                sun.shadows = LightShadows.Hard;
                sun.transform.rotation = Quaternion.Euler(55, -30, 0);

                var player = Actor("Soldier", new Vector3(5, 0, -6), black, 1.7f);
                var controller = player.AddComponent<SandboxPlayer>();
                controller.session = session;
                session.player = controller;
                var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
                camera.tag = "MainCamera";
                camera.orthographic = true;
                camera.orthographicSize = 7.5f;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 80;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.015f, 0.022f, 0.02f);
                camera.transform.rotation = Quaternion.Euler(30, 45, 0);
                camera.transform.position = player.transform.position + Vector3.up - camera.transform.forward * 24;
                camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
                var iso = camera.gameObject.AddComponent<IsometricView>();
                iso.target = player.transform;
                controller.view = camera;

                // Selected imported models are normalized into project-owned prefab wrappers.
                var imports = AssetDatabase.FindAssets("t:Model", new[] { "Assets/ThirdParty/Kenney" })
                    .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();
                if (imports.Length == 0) throw new InvalidOperationException("Install the licensed asset subset before generation.");
                string character = imports.FirstOrDefault(p => p.EndsWith("/Model/characterMedium.fbx"));
                string weapon = imports.FirstOrDefault(p => p.EndsWith("/blaster-a.fbx"));
                string environment = imports.FirstOrDefault(p => p.EndsWith("/corridor-corner.fbx"));
                if (character == null || weapon == null || environment == null)
                    throw new InvalidOperationException("Missing a required model pack.");
                ReplaceVisual(player, character, black, 1.7f);
                var gun = Imported("CC0 weapon", weapon, black, 0.3f);
                gun.transform.SetParent(player.transform, false);
                gun.transform.localPosition = new Vector3(0.25f, 0.95f, 0.4f);
                PrefabUtility.SaveAsPrefabAsset(player.transform.Find("CC0 visual").gameObject, Root + "/Prefabs/Soldier_Visual.prefab");
                PrefabUtility.SaveAsPrefabAsset(gun, Root + "/Prefabs/Weapon_Visual.prefab");
                int index = 0;
                foreach (Vector3 position in new[] { new Vector3(-7, 0, 0), new Vector3(0, 0, 5), new Vector3(7, 0, 1), new Vector3(-4, 0, 6) })
                {
                    bool runner = index % 2 == 1;
                    var actor = Actor((runner ? "Runner" : "Shambler") + "_" + index, position, skin, runner ? 1.5f : 1.7f);
                    ReplaceVisual(actor, character, index < 2 ? skin : red, runner ? 1.5f : 1.7f);
                    PrefabUtility.SaveAsPrefabAsset(actor, Root + "/Prefabs/" + actor.name + "_Visual.prefab");
                    var enemy = actor.AddComponent<SandboxEnemy>();
                    enemy.session = session;
                    enemy.speed = runner ? 2.5f : 1.1f;
                    enemy.health = index < 2 ? 70 : 105;
                    index++;
                }
                var exhibit = Imported("CC0 modular asset exhibit", environment, steel, 2f);
                exhibit.transform.position = new Vector3(-8, 0, -5);
                PrefabUtility.SaveAsPrefabAsset(exhibit, Root + "/Prefabs/ModularAsset_Sandbox.prefab");
                string audio = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/ThirdParty/Kenney" })
                    .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).FirstOrDefault();
                if (audio == null) throw new InvalidOperationException("No licensed audio imported.");
                controller.stepSound = AssetDatabase.LoadAssetAtPath<AudioClip>(audio);
                controller.shotSound = controller.stepSound; // Temporary impact, not final firearm audio.
                EditorSceneManager.SaveScene(scene, ScenePath);
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
                EditorBuildSettings.RemoveConfigObject("com.unity.dt.app-ui");
                EditorSettings.serializationMode = SerializationMode.ForceText;
                PlayerSettings.productName = "Abisal Stealness - Foundation";
                PlayerSettings.companyName = "Zombtoy Team";
                AssetDatabase.SaveAssets();
                WriteImportReport(imports);
                Debug.Log("ZOMBTOY_FOUNDATION_GENERATED " + ScenePath);
            }
            finally
            {
                if (!Application.isBatchMode)
                {
                    if (previous.IsValid()) SceneManager.SetActiveScene(previous);
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        [MenuItem("Zombtoy/Build Windows foundation")]
        public static void BuildWindows()
        {
            if (!File.Exists(ScenePath)) throw new InvalidOperationException("Generate sandbox first.");
            Directory.CreateDirectory("Builds/Windows");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = EditorBuildSettings.scenes.Where(item => item.enabled).Select(item => item.path).ToArray(),
                locationPathName = "Builds/Windows/AbisalStealness.exe",
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
            Directory.CreateDirectory("TestResults");
            File.WriteAllText("TestResults/build-summary.txt", $"Result: {report.summary.result}\nErrors: {report.summary.totalErrors}\nWarnings: {report.summary.totalWarnings}\nBytes: {report.summary.totalSize}\n");
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows build failed. See build-summary.txt and Unity log.");
            foreach (string file in new[] { "CREDITS.md", "LICENSES.md" })
                if (File.Exists(file)) File.Copy(file, "Builds/Windows/" + file, true);
            foreach (string file in Directory.GetFiles("Assets/ThirdParty/Kenney", "License.txt", SearchOption.AllDirectories))
            {
                string pack = new DirectoryInfo(Path.GetDirectoryName(file)).Name;
                string destination = "Builds/Windows/Licenses/Kenney/" + pack;
                Directory.CreateDirectory(destination);
                File.Copy(file, destination + "/License.txt", true);
            }
        }

        public static void CaptureScene()
        {
            EditorSceneManager.OpenScene(ScenePath);
            var camera = Camera.main;
            var texture = new RenderTexture(960, 540, 24) { filterMode = FilterMode.Point };
            camera.targetTexture = texture;
            camera.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = texture;
            var image = new Texture2D(960, 540, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
            image.Apply();
            Directory.CreateDirectory("TestResults");
            File.WriteAllBytes("TestResults/foundation-scene.png", image.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(texture);
        }

        private static Material Material(string name, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader missing.");
            string path = Root + "/Art/Materials/" + name.Replace(' ', '_') + ".mat";
            var result = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool create = result == null;
            if (create) result = new Material(shader) { name = name };
            result.SetColor("_BaseColor", color);
            result.SetFloat("_Smoothness", 0.05f);
            if (create) AssetDatabase.CreateAsset(result, path);
            return result;
        }

        private static GameObject Cube(string name, Vector3 position, Vector3 scale, Material material, Transform parent, bool collider = true)
        {
            var result = GameObject.CreatePrimitive(PrimitiveType.Cube);
            result.name = name;
            result.transform.SetParent(parent);
            result.transform.position = position;
            result.transform.localScale = scale;
            result.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider) UnityEngine.Object.DestroyImmediate(result.GetComponent<Collider>());
            return result;
        }

        private static GameObject Actor(string name, Vector3 position, Material material, float height)
        {
            var result = new GameObject(name);
            result.transform.position = position;
            var cc = result.AddComponent<CharacterController>();
            cc.height = height;
            cc.radius = 0.35f;
            cc.center = Vector3.up * height * 0.5f;
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Placeholder visual";
            visual.transform.SetParent(result.transform, false);
            visual.transform.localPosition = Vector3.up * height * 0.5f;
            visual.transform.localScale = new Vector3(0.65f, height * 0.5f, 0.65f);
            visual.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
            return result;
        }

        private static void ReplaceVisual(GameObject actor, string path, Material material, float height)
        {
            UnityEngine.Object.DestroyImmediate(actor.transform.Find("Placeholder visual").gameObject);
            var model = Imported("CC0 visual", path, material, height);
            model.transform.SetParent(actor.transform, false);
        }

        private static GameObject Imported(string name, string path, Material material, float targetHeight)
        {
            var wrapper = new GameObject(name);
            var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            model.transform.SetParent(wrapper.transform, false);
            foreach (var animator in model.GetComponentsInChildren<Animator>()) animator.enabled = false;
            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) throw new InvalidOperationException("Model has no renderer: " + path);
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
                renderer.sharedMaterials = renderer.sharedMaterials.Select(_ => material).ToArray();
            }
            float factor = targetHeight / Mathf.Max(0.01f, bounds.size.y);
            model.transform.localScale *= factor;
            model.transform.localPosition = new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z) * factor;
            return wrapper;
        }

        private static void Light(string name, Vector3 position, Color color, float intensity, float range)
        {
            var light = new GameObject(name).AddComponent<Light>();
            light.transform.position = position;
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
        }

        private static void Label(string value, Vector3 position, Material material, Transform parent)
        {
            var go = new GameObject(value);
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(30, 45, 0);
            var label = go.AddComponent<TextMesh>();
            label.text = value;
            label.fontSize = 40;
            label.characterSize = 0.04f;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = material.color;
        }

        private static void WriteImportReport(string[] imports)
        {
            Directory.CreateDirectory("TestResults");
            var lines = new List<string> { "path,meshes,renderers,clips,avatar_valid,avatar_human" };
            foreach (string path in imports)
            {
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                var avatar = assets.OfType<Avatar>().FirstOrDefault();
                lines.Add($"{path},{assets.OfType<Mesh>().Count()},{root.GetComponentsInChildren<Renderer>().Length},{assets.OfType<AnimationClip>().Count()},{avatar != null && avatar.isValid},{avatar != null && avatar.isHuman}");
            }
            File.WriteAllLines("TestResults/asset-import.csv", lines);
        }
    }
}
