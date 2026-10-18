using System;
using System.IO;
using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Zombtoy.Gameplay;

namespace Zombtoy.Editor
{
    public static class PortMapBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/Level01_Port.unity";
        private const string Root = "Assets/_Project";
        private static Transform geometry;
        private static Material concrete, teal, rust, yellow, white, dark, water, green, red;

        [MenuItem("Zombtoy/Generate first port map")]
        public static void Generate()
        {
            if (File.Exists(ScenePath)) throw new InvalidOperationException("Port scene exists. Preserve edits; generate only in a fresh validation copy.");
            var previous = SceneManager.GetActiveScene();
            if (!Application.isBatchMode && string.IsNullOrEmpty(previous.path))
                throw new InvalidOperationException("Save the current untitled scene before generating the port.");
            var scene = EditorSceneManager.OpenScene(FoundationBuilder.ScenePath,
                Application.isBatchMode ? OpenSceneMode.Single : OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                foreach (string folder in new[] { "/Art/Materials/Port", "/Prefabs/Levels", "/Navigation" })
                    Directory.CreateDirectory(Root + folder);
                AssetDatabase.Refresh();
                concrete = Mat("Wet concrete", new Color(0.31f, 0.36f, 0.37f));
                teal = Mat("Container teal", new Color(0.19f, 0.42f, 0.39f));
                rust = Mat("Container oxide", new Color(0.48f, 0.23f, 0.16f));
                yellow = Mat("Safety ochre", new Color(0.9f, 0.62f, 0.16f));
                white = Mat("Paint ivory", new Color(0.82f, 0.82f, 0.68f));
                dark = Mat("Hull charcoal", new Color(0.10f, 0.17f, 0.19f));
                water = Mat("Harbour water", new Color(0.045f, 0.15f, 0.19f), true);
                green = Mat("Signal green", new Color(0.25f, 0.95f, 0.65f), true);
                red = Mat("Signal red", new Color(0.95f, 0.2f, 0.11f), true);
                var session = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<SandboxSession>()).Single();
                var player = session.player;
                var camera = player.view;
                foreach (var root in scene.GetRootGameObjects())
                    if (root != session.gameObject && root != player.gameObject && root != camera.gameObject)
                        UnityEngine.Object.DestroyImmediate(root);
                session.name = "Port mission systems";
                session.missionTitle = "ABISAL / 01 - PUERTO CARGUERO";
                session.rounds = 12;
                session.reserve = 24;
                session.State.serum = 65;
                session.State.doses = 1;
                var mission = session.gameObject.AddComponent<PortMission>();
                mission.session = session;
                session.portMission = mission;
                geometry = new GameObject("Port_Environment").transform;
                var interactables = new GameObject("Port_Interactables").transform;
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.57f, 0.64f, 0.68f);
                RenderSettings.fog = false;
                var sun = new GameObject("Overcast moonlight").AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.color = new Color(0.77f, 0.86f, 1);
                sun.intensity = 1.65f;
                sun.shadows = LightShadows.Hard;
                sun.transform.rotation = Quaternion.Euler(55, -25, 0);

                Box("Water", new Vector3(7, -1.3f, 2), new Vector3(120, 0.15f, 90), water, false);
                Box("Quay deck", new Vector3(0, -0.5f, 2), new Vector3(56, 1, 44), concrete);
                Box("Boarding pier", new Vector3(32.5f, -0.25f, -8), new Vector3(9, 0.5f, 5), concrete);
                Box("Submarine boarding deck", new Vector3(39.5f, -0.25f, -8), new Vector3(7, 0.5f, 4.6f), dark);
                Rail("South perimeter", new Vector3(0, 0.45f, -20), new Vector3(56, 0.9f, 0.3f));
                Rail("North perimeter", new Vector3(0, 0.45f, 24), new Vector3(56, 0.9f, 0.3f));
                Rail("West perimeter", new Vector3(-28, 0.45f, 2), new Vector3(0.3f, 0.9f, 44));
                Rail("Quay edge south", new Vector3(28, 0.45f, -15.3f), new Vector3(0.3f, 0.9f, 9.4f));
                Rail("Quay edge north", new Vector3(28, 0.45f, 9.3f), new Vector3(0.3f, 0.9f, 29.4f));
                foreach (float z in new[] { -10.5f, -5.5f })
                    Rail("Pier guardrail", new Vector3(32.5f, 0.45f, z), new Vector3(9, 0.9f, 0.2f));
                foreach (float z in new[] { -10.3f, -5.7f })
                    Rail("Sub deck rail", new Vector3(40, 0.45f, z), new Vector3(6, 0.9f, 0.2f));
                Rail("Sub deck end", new Vector3(43, 0.45f, -8), new Vector3(0.2f, 0.9f, 4.6f));
                for (int z = -18; z <= 22; z += 4)
                    Box("Quay safety stripe", new Vector3(27.5f, 0.012f, z), new Vector3(0.35f, 0.02f, 2.2f), yellow, false);
                foreach (float x in new[] { -18f, -9f, 0f }) Container(new Vector3(x, 0, -2), x == -9 ? rust : teal);
                foreach (float x in new[] { -16f, -6f, 3f }) Container(new Vector3(x, 0, 5), x == -6 ? teal : rust);
                for (int i = 0; i < 5; i++)
                    Box("Road centre marking", new Vector3(-24, 0.012f, -13 + i * 4), new Vector3(0.16f, 0.02f, 1.8f), yellow, false);
                for (int i = 0; i < 5; i++)
                    Box("Control approach marking", new Vector3(-11 + i * 6, 0.012f, 9), new Vector3(2, 0.02f, 0.16f), white, false);

                Warehouse();
                ControlRoom();
                Crane();
                Submarine();
                for (int i = 0; i < 5; i++) Bollard(new Vector3(26.5f, 0, -17 + i * 9));
                var arrival = new GameObject("01_Arrival").transform;
                arrival.position = new Vector3(-24, 0.1f, -15);
                mission.arrival = arrival;
                player.transform.position = arrival.position;
                player.transform.rotation = Quaternion.Euler(0, 0, 0);
                var heroMaterial = Skin("Soldier skin", "survivorMaleB.png");
                foreach (var renderer in player.transform.Find("CC0 visual").GetComponentsInChildren<Renderer>())
                    renderer.sharedMaterial = heroMaterial;
                var locator = Box("Player locator", arrival.position + Vector3.up * 0.03f,
                    new Vector3(0.75f, 0.025f, 0.75f), green, false, player.transform);
                locator.transform.localPosition = Vector3.up * 0.03f;
                camera.orthographicSize = 8.2f;
                camera.farClipPlane = 160;
                camera.backgroundColor = new Color(0.025f, 0.06f, 0.075f);
                camera.transform.rotation = Quaternion.Euler(30, 45, 0);
                camera.transform.position = arrival.position + Vector3.up - camera.transform.forward * 24;
                camera.gameObject.AddComponent<PortVisibility>().target = player.transform;

                mission.fuse = Marker("Repuesto", new Vector3(-22, 0.8f, 17), yellow, interactables);
                mission.generator = Box("Generator - E", new Vector3(5, 0.8f, 15), new Vector3(2.4f, 1.6f, 1.7f), teal, true, interactables).transform;
                for (int i = 0; i < 4; i++)
                    Box("Generator vents", new Vector3(4.3f + i * 0.45f, 0.9f, 14.12f), new Vector3(0.12f, 0.7f, 0.04f), dark, false)
                        .transform.SetParent(mission.generator, true);
                mission.supplies = Marker("Suministros +12 / +1", new Vector3(-8, 0.5f, -10), green, interactables);
                mission.controlDoor = Gate("Control rolling door", new Vector3(18.5f, 1.3f, 10), new Vector3(3, 2.6f, 0.35f), interactables);
                mission.boardingGate = Gate("Boarding checkpoint", new Vector3(28, 1.1f, -8), new Vector3(0.35f, 2.2f, 5), interactables);
                session.terminal = Box("Control terminal - E", new Vector3(22, 0.8f, 18), new Vector3(1.8f, 1.6f, 0.8f), teal, true, interactables).transform;
                Box("Terminal screen", new Vector3(22, 1.25f, 17.57f), new Vector3(1.3f, 0.5f, 0.04f), green, false)
                    .transform.SetParent(session.terminal, true);
                session.extraction = Marker("Embarcar", new Vector3(39, 0.15f, -8), green, interactables);
                Label("01 / DESEMBARCO", new Vector3(-23, 0.04f, -18), white, true);
                Label("02 / PATIO DE CARGA", new Vector3(-10, 0.04f, -6), white, true);
                Label("03 / ALMACEN", new Vector3(-20, 3.4f, 20), yellow);
                Label("04 / GENERADOR", new Vector3(5, 2.5f, 15), yellow);
                Label("05 / CONTROL", new Vector3(19, 3.4f, 22), green);
                Label("06 / EMBARQUE", new Vector3(32.5f, 0.04f, -8), yellow, true);
                Label("ABISAL // B-07", new Vector3(40, 3f, -3), white);
                Lamp(new Vector3(-24, 0, -12), yellow);
                Lamp(new Vector3(-12, 0, 11), yellow);
                Lamp(new Vector3(10, 0, 9), yellow);
                Lamp(new Vector3(25, 0, -5), green);
                SpawnEnemies(session);

                var surface = geometry.gameObject.AddComponent<NavMeshSurface>();
                surface.collectObjects = CollectObjects.Children;
                surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
                surface.BuildNavMesh();
                if (surface.navMeshData == null) throw new InvalidOperationException("Port NavMesh bake failed.");
                string navPath = Root + "/Navigation/Port01_NavMesh.asset";
                if (AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath) != null)
                    throw new InvalidOperationException("Port navigation asset exists; preserve it before regeneration.");
                AssetDatabase.CreateAsset(surface.navMeshData, navPath);
                PrefabUtility.SaveAsPrefabAssetAndConnect(geometry.gameObject, Root + "/Prefabs/Levels/Port_Environment.prefab", InteractionMode.AutomatedAction);
                EditorSceneManager.SaveScene(scene, ScenePath);
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true),
                    new EditorBuildSettingsScene(FoundationBuilder.ScenePath, true) };
                PlayerSettings.productName = "Abisal Stealness";
                AssetDatabase.SaveAssets();
                Debug.Log("ZOMBTOY_PORT_GENERATED " + ScenePath);
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

        private static void Warehouse()
        {
            Box("Warehouse floor", new Vector3(-20, 0.015f, 15), new Vector3(12, 0.03f, 12), dark, false);
            Wall("Warehouse west", new Vector3(-26, 1.5f, 15), new Vector3(0.3f, 3, 12));
            Wall("Warehouse north", new Vector3(-20, 1.5f, 21), new Vector3(12, 3, 0.3f));
            Wall("Warehouse cut east", new Vector3(-14, 0.45f, 15), new Vector3(0.3f, 0.9f, 12));
            foreach (float x in new[] { -24.5f, -15.5f })
                Wall("Warehouse entrance", new Vector3(x, 0.45f, 9), new Vector3(3, 0.9f, 0.3f));
            foreach (Vector3 p in new[] { new Vector3(-24, 0.65f, 19), new Vector3(-16, 0.65f, 18), new Vector3(-16, 0.65f, 13) })
                Box("Storage crate", p, new Vector3(1.8f, 1.3f, 1.6f), rust);
        }

        private static void ControlRoom()
        {
            Box("Control floor", new Vector3(18.5f, 0.016f, 16), new Vector3(13, 0.03f, 12), teal, false);
            Wall("Control north", new Vector3(18.5f, 1.5f, 22), new Vector3(13, 3, 0.3f));
            Wall("Control east", new Vector3(25, 1.5f, 16), new Vector3(0.3f, 3, 12));
            Wall("Control cut west", new Vector3(12, 0.45f, 16), new Vector3(0.3f, 0.9f, 12));
            foreach (float x in new[] { 14.5f, 22.5f })
                Wall("Control entry wall", new Vector3(x, 0.45f, 10), new Vector3(5, 0.9f, 0.3f));
            Box("Control desk", new Vector3(15, 0.55f, 19), new Vector3(3, 1.1f, 1), dark);
            Box("Radio console", new Vector3(15, 1.3f, 19), new Vector3(2.4f, 0.4f, 0.7f), white, false);
            for (int i = 0; i < 3; i++)
                Box("Control wall display", new Vector3(15 + i * 3, 2, 21.8f), new Vector3(1.7f, 0.7f, 0.05f), green, false);
        }

        private static void Container(Vector3 p, Material material)
        {
            var group = new GameObject("Cargo container").transform;
            group.SetParent(geometry);
            Box("Container body", p + Vector3.up * 1.2f, new Vector3(6, 2.4f, 3), material, true, group);
            for (int i = -5; i <= 5; i++)
                Box("Corrugation", p + new Vector3(i * 0.5f, 1.2f, -1.53f), new Vector3(0.055f, 2.25f, 0.055f), dark, false, group);
            Box("Container serial panel", p + new Vector3(2, 1.5f, -1.56f), new Vector3(0.65f, 0.4f, 0.03f), white, false, group);
            group.gameObject.AddComponent<MapOccluder>();
        }

        private static void Crane()
        {
            foreach (float x in new[] { -5f, 0f })
                Box("Crane upright", new Vector3(x, 6, 20), new Vector3(0.6f, 12, 0.6f), yellow);
            Box("Crane gantry", new Vector3(-2.5f, 12, 20), new Vector3(8, 0.8f, 1), yellow);
            Box("Crane boom", new Vector3(8, 12, 20), new Vector3(22, 0.5f, 0.55f), yellow, false);
            Box("Crane cable", new Vector3(15, 8, 20), new Vector3(0.06f, 8, 0.06f), dark, false);
            Box("Crane hook", new Vector3(15, 4, 20), new Vector3(0.5f, 0.6f, 0.25f), yellow, false);
        }

        private static void Submarine()
        {
            var hull = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hull.name = "Submarine hull B-07";
            hull.transform.SetParent(geometry);
            hull.transform.position = new Vector3(40, -0.8f, -3);
            hull.transform.localScale = new Vector3(8, 4, 28);
            hull.GetComponent<Renderer>().sharedMaterial = dark;
            UnityEngine.Object.DestroyImmediate(hull.GetComponent<Collider>());
            Box("Submarine sail", new Vector3(40, 2.1f, -1), new Vector3(2.6f, 3, 5), dark, false);
            Box("Periscope", new Vector3(40, 5, -1), new Vector3(0.2f, 3, 0.2f), teal, false);
            Box("Periscope head", new Vector3(40.4f, 6.5f, -1), new Vector3(0.9f, 0.2f, 0.2f), teal, false);
        }

        private static void SpawnEnemies(SandboxSession session)
        {
            Vector3[] points = { new Vector3(-13, 0.1f, -8), new Vector3(-6, 0.1f, 1),
                new Vector3(-23, 0.1f, 4), new Vector3(-9, 0.1f, 13),
                new Vector3(7, 0.1f, 3), new Vector3(21, 0.1f, -1) };
            var root = new GameObject("Port_Encounters").transform;
            for (int i = 0; i < points.Length; i++)
            {
                bool runner = i == 1 || i == 5;
                var actor = new GameObject((runner ? "Runner" : "Shambler") + "_Port_" + i);
                actor.transform.SetParent(root);
                actor.transform.position = points[i];
                var cc = actor.AddComponent<CharacterController>();
                cc.height = 1.7f; cc.center = Vector3.up * 0.85f; cc.radius = 0.35f;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/Soldier_Visual.prefab");
                var visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                visual.transform.SetParent(actor.transform, false);
                var material = Skin(i % 2 == 0 ? "Zombie pale" : "Zombie oxide", i % 2 == 0 ? "zombieA.png" : "zombieC.png");
                foreach (var renderer in visual.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = material;
                var enemy = actor.AddComponent<SandboxEnemy>();
                enemy.session = session;
                enemy.speed = runner ? 2.35f : 1.15f;
                enemy.health = i == 3 ? 105 : 70;
                enemy.sightRange = runner ? 8 : 6;
                enemy.useNavigation = true;
            }
        }

        private static GameObject Gate(string name, Vector3 point, Vector3 size, Transform parent)
        {
            var gate = Box(name, point, size, red, true, parent);
            var obstacle = gate.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.size = Vector3.one;
            obstacle.carving = true;
            return gate;
        }

        private static Transform Marker(string name, Vector3 point, Material material, Transform parent)
        {
            var result = Box(name, point, new Vector3(0.8f, 0.5f, 0.65f), material, true, parent);
            Label(name.ToUpperInvariant(), point + Vector3.up * 1.1f, material);
            return result.transform;
        }

        private static void Rail(string name, Vector3 point, Vector3 size)
        {
            Box(name, point, size, dark);
            Box(name + " yellow cap", point + Vector3.up * (size.y * 0.5f),
                new Vector3(size.x, 0.07f, size.z), yellow, false);
        }

        private static void Wall(string name, Vector3 point, Vector3 size) =>
            Box(name, point, size, concrete).AddComponent<MapOccluder>();

        private static void Bollard(Vector3 point)
        {
            Box("Mooring bollard", point + Vector3.up * 0.35f, new Vector3(0.35f, 0.7f, 0.35f), dark);
            Box("Bollard cap", point + Vector3.up * 0.7f, new Vector3(0.65f, 0.12f, 0.35f), yellow, false);
        }

        private static void Lamp(Vector3 point, Material signal)
        {
            Box("Dock light pole", point + Vector3.up * 2.5f, new Vector3(0.16f, 5, 0.16f), dark);
            Box("Dock lantern", point + Vector3.up * 5, new Vector3(0.8f, 0.15f, 0.5f), signal, false);
            var light = new GameObject("Dock local light").AddComponent<Light>();
            light.transform.SetParent(geometry);
            light.transform.position = point + Vector3.up * 4.8f;
            light.type = LightType.Point;
            light.range = 10; light.intensity = 2;
            light.color = new Color(0.9f, 0.73f, 0.39f);
        }

        private static Material Skin(string name, string texture)
        {
            var result = Mat(name, Color.white);
            result.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/ThirdParty/Kenney/Survivors/Skins/" + texture));
            return result;
        }

        private static Material Mat(string name, Color color, bool unlit = false)
        {
            string path = Root + "/Art/Materials/Port/" + name.Replace(' ', '_') + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit"));
                material.name = name;
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            if (!unlit) material.SetFloat("_Smoothness", 0.12f);
            return material;
        }

        private static GameObject Box(string name, Vector3 point, Vector3 size, Material material, bool collider = true, Transform parent = null)
        {
            var item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = name;
            item.transform.SetParent(parent != null ? parent : geometry);
            item.transform.position = point;
            item.transform.localScale = size;
            item.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider) UnityEngine.Object.DestroyImmediate(item.GetComponent<Collider>());
            return item;
        }

        private static void Label(string text, Vector3 point, Material material, bool floor = false)
        {
            var item = new GameObject(text).AddComponent<TextMesh>();
            item.transform.SetParent(geometry);
            item.transform.position = point;
            item.transform.rotation = floor ? Quaternion.Euler(90, 0, 0) : Quaternion.Euler(30, 45, 0);
            item.text = text; item.fontSize = 48; item.characterSize = floor ? 0.075f : 0.045f;
            item.anchor = TextAnchor.MiddleCenter; item.color = material.color;
        }

        public static void Capture()
        {
            EditorSceneManager.OpenScene(ScenePath);
            var camera = Camera.main;
            Directory.CreateDirectory("TestResults");
            Capture(camera, "port-player.png", 1280, 720);
            camera.orthographicSize = 35;
            camera.transform.rotation = Quaternion.Euler(55, 0, 0);
            camera.transform.position = new Vector3(6, 0, 2) - camera.transform.forward * 85;
            Capture(camera, "port-overview.png", 1600, 1100);
        }

        private static void Capture(Camera camera, string file, int width, int height)
        {
            var texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            camera.targetTexture = texture;
            var request = new UniversalRenderPipeline.SingleCameraRequest { destination = texture };
            RenderPipeline.SubmitRenderRequest(camera, request);
            // Warm the per-camera material state before reading the offscreen image.
            RenderPipeline.SubmitRenderRequest(camera, request);
            var previous = RenderTexture.active;
            RenderTexture.active = texture;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
            File.WriteAllBytes("TestResults/" + file, image.EncodeToPNG());
            camera.targetTexture = null; RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(texture);
        }
    }
}
