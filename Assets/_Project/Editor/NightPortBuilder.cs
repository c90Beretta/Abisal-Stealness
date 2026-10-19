using System;
using System.IO;
using System.Linq;
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
    public static class NightPortBuilder
    {
        private const string Root = "Assets/_Project";
        private static Material black, steel, glass, red;
        [MenuItem("Abisal/Actualizar puerto nocturno")]
        public static void Upgrade()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(PortMapBuilder.ScenePath);
            var s = Object.FindAnyObjectByType<SandboxSession>();
            if (s.GetComponent<NightSurvival>() != null) throw new InvalidOperationException("Night migration already applied; edit the authored scene instead.");
            Directory.CreateDirectory(Root + "/Art/Night"); Directory.CreateDirectory(Root + "/Prefabs/Infected"); AssetDatabase.Refresh();
            var importer = (ModelImporter)AssetImporter.GetAtPath("Assets/ThirdParty/Kenney/Survivors/Model/characterMedium.fbx");
            if (!importer.isReadable) { importer.isReadable = true; importer.SaveAndReimport(); }
            black = Material("Tactical_black", new Color(.025f, .032f, .04f), .22f, .2f);
            steel = Material("Tank_gunmetal", new Color(.17f, .2f, .22f), .4f, .6f);
            glass = Material("Mask_lenses", new Color(.08f, .23f, .22f), .8f, .5f);
            red = Material("Infected_wounds", new Color(.24f, .014f, .018f), .35f, 0);
            Soldier(s.player);
            var night = s.gameObject.AddComponent<NightSurvival>(); night.session = s;
            foreach (var lamp in Object.FindObjectsByType<Light>()) lamp.enabled = false;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.032f, .048f, .065f);
            RenderSettings.ambientEquatorColor = new Color(.014f, .024f, .032f); RenderSettings.ambientGroundColor = new Color(.006f, .01f, .014f);
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(.008f, .018f, .024f); RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogDensity = .018f;
            var moon = Object.FindObjectsByType<Light>().First(l => l.type == LightType.Directional);
            moon.enabled = true; moon.intensity = .08f; moon.color = new Color(.38f, .58f, .8f); moon.shadows = LightShadows.Soft;
            night.flashlight = s.player.GetComponentsInChildren<Light>().First();
            var flash = night.flashlight; flash.name = "Tactical flashlight"; flash.enabled = true; flash.type = LightType.Spot; flash.range = 18; flash.intensity = 12;
            flash.spotAngle = 76; flash.innerSpotAngle = 40; flash.color = new Color(.82f, .94f, 1); flash.shadows = LightShadows.Soft; flash.shadowBias = .025f;
            flash.transform.localPosition = new Vector3(.18f, 1.25f, .36f); flash.transform.localRotation = Quaternion.Euler(18, 0, 0);
            night.personalLight = Lamp("Near perception light", s.player.transform, new Vector3(0, 1.8f, 0), new Color(.32f, .53f, .62f), 1.4f, 4.5f);
            var lightsRoot = new GameObject("Night_port_lighting").transform;
            foreach (var position in new[] { new Vector3(-25, 1.8f, -16), new Vector3(-22, 2.2f, 15), new Vector3(4, 2, 14), new Vector3(18, 2.1f, 11), new Vector3(30, 1.3f, -8) })
                Lamp("Emergency red", lightsRoot, position, new Color(1, .13f, .04f), 2.1f, 5);
            night.poweredLights = new[] {
                Lamp("Warehouse power", lightsRoot, new Vector3(-21, 2.4f, 17), new Color(.5f, .8f, .7f), 3, 7),
                Lamp("Control power", lightsRoot, new Vector3(19, 2.5f, 17), new Color(.55f, .78f, .84f), 3, 8),
                Lamp("Pier power", lightsRoot, new Vector3(32, 2.5f, -8), new Color(.55f, .75f, .7f), 3, 7) };
            foreach (var light in night.poweredLights) light.enabled = false;
            s.player.view.backgroundColor = new Color(.005f, .012f, .018f);
            var cameraData = s.player.view.GetUniversalAdditionalCameraData(); cameraData.renderPostProcessing = true;
            var volume = new GameObject("Night screen effects").AddComponent<Volume>(); volume.isGlobal = true;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(profile, Root + "/Art/Night/NightScreen.asset"); volume.sharedProfile = profile;
            var vignette = profile.Add<Vignette>(true); vignette.intensity.Override(.36f); vignette.smoothness.Override(.65f);
            var bloom = profile.Add<Bloom>(true); bloom.intensity.Override(.35f); bloom.threshold.Override(1.2f);
            var grain = profile.Add<FilmGrain>(true); grain.intensity.Override(.18f); grain.response.Override(.7f);
            var tone = profile.Add<Tonemapping>(true); tone.mode.Override(TonemappingMode.ACES);
            foreach (var component in profile.components) AssetDatabase.AddObjectToAsset(component, profile);
            var enemies = Object.FindObjectsByType<SandboxEnemy>().OrderBy(e => e.name).ToArray();
            GameObject[] templates = new GameObject[4];
            for (int i = 0; i < enemies.Length; i++)
            {
                int kind = i % 4; Configure(enemies[i], (InfectedKind)kind, i);
                if (templates[kind] == null)
                {
                    var savedSession = enemies[i].session; enemies[i].session = null;
                    templates[kind] = PrefabUtility.SaveAsPrefabAsset(enemies[i].gameObject, Root + "/Prefabs/Infected/Night_" + (InfectedKind)kind + ".prefab");
                    enemies[i].session = savedSession;
                }
            }
            var director = s.gameObject.AddComponent<HordeDirector>(); director.session = s; night.hordes = director;
            var reserves = new SandboxEnemy[16]; var reserveRoot = new GameObject("Horde_reserves_finite").transform;
            Vector3[] points = { new Vector3(-26,0,-1), new Vector3(-25,0,1), new Vector3(-26,0,6), new Vector3(-23,0,7),
                new Vector3(-12,0,20), new Vector3(-10,0,19), new Vector3(-8,0,21), new Vector3(-5,0,20),
                new Vector3(3,0,20), new Vector3(4,0,21), new Vector3(6,0,21), new Vector3(11,0,7),
                new Vector3(24,0,2), new Vector3(25,0,4), new Vector3(25,0,7), new Vector3(22,0,7) };
            for (int i = 0; i < reserves.Length; i++)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(templates[i % 4]); go.transform.SetParent(reserveRoot); go.name = "Reserve_" + i.ToString("00") + "_" + (InfectedKind)(i % 4);
                if (!NavMesh.SamplePosition(points[i], out var nav, 3, NavMesh.AllAreas)) throw new InvalidOperationException("Horde position outside navmesh: " + points[i]);
                go.transform.position = nav.position + Vector3.up * .1f; reserves[i] = go.GetComponent<SandboxEnemy>(); reserves[i].session = s;
                go.GetComponent<LootContainer>().id = "corpse-reserve-" + i; go.SetActive(false);
            }
            director.reserves = reserves; director.released = new bool[reserves.Length];
            Container("dock-locker", "Taquilla de desembarco", new Vector3(-23, .1f, -12), new LootStock { cells = 2, oxygen = 1, ammo = 24 });
            Container("warehouse-cache", "Caja del almacen", new Vector3(-19, .1f, 18), new LootStock { plates = 2, medkits = 1, ammo = 30 });
            Container("generator-spares", "Repuestos de servicio", new Vector3(0, .1f, 12), new LootStock { cells = 2, oxygen = 2, bandages = 3 });
            Container("control-armory", "Armeria de control", new Vector3(14.5f, .1f, 20), new LootStock { plates = 1, medkits = 2, ammo = 36, doses = 2 });
            FinalizeNight();
            PlayerSettings.bundleVersion = "0.4.0";
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Directory.CreateDirectory("TestResults"); File.WriteAllText("TestResults/night-authoring.txt", "Night port: 6 patrols + 16 finite reserves; 4 derived weighted meshes; tactical soldier with Oxy_ballon model; 4 loot containers; URP vignette/bloom/grain/ACES.\n");
        }
        public static void FinalizeNight()
        {
            var s = Object.FindAnyObjectByType<SandboxSession>();
            if (s == null || s.GetComponent<NightSurvival>() == null) { EditorSceneManager.OpenScene(PortMapBuilder.ScenePath); s = Object.FindAnyObjectByType<SandboxSession>(); }
            var n = s.GetComponent<NightSurvival>();
            string signalPath = Root + "/Art/Night/NightSignal.mat";
            var signal = AssetDatabase.LoadAssetAtPath<Material>(signalPath);
            if (signal == null) { signal = new Material(Shader.Find("Abisal/Night Signal")); AssetDatabase.CreateAsset(signal, signalPath); }
            s.tracerMaterial = signal;
            var all = Object.FindObjectsByType<LootContainer>(FindObjectsInactive.Include);
            all.Single(c => c.id == "generator-spares").transform.position = new Vector3(0, .45f, 12);
            all.Single(c => c.id == "control-armory").transform.position = new Vector3(14.5f, .45f, 20);
            n.breathing = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/Night/Breathing.wav");
            s.player.meleeSound = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/Night/Melee.wav");
            foreach (var gore in Object.FindObjectsByType<Dismemberment>(FindObjectsInactive.Include)) gore.woundMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Art/Night/Infected_wounds.mat");
            foreach (var enemy in Object.FindObjectsByType<InfectedVariant>(FindObjectsInactive.Include))
            {
                enemy.groan = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/Night/Groan.wav");
                enemy.scream = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/Night/Scream.wav");
            }
            for (int i = 0; i < 3; i++)
            {
                var weapon = s.player.GetComponent<WeaponLoadout>().weapons[i];
                weapon.shotClip = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/Night/" + new[] { "Pistol", "Carbine", "Scatter" }[i] + ".wav"); EditorUtility.SetDirty(weapon);
            }
            foreach (var path in AssetDatabase.FindAssets("t:Prefab", new[] { Root + "/Prefabs/Infected" }).Select(AssetDatabase.GUIDToAssetPath))
            {
                var go = PrefabUtility.LoadPrefabContents(path); var v = go.GetComponent<InfectedVariant>();
                go.GetComponent<Dismemberment>().woundMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Art/Night/Infected_wounds.mat");
                v.groan = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/Night/Groan.wav"); v.scream = AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/Night/Scream.wav");
                PrefabUtility.SaveAsPrefabAsset(go, path); PrefabUtility.UnloadPrefabContents(go);
            }
            EditorSceneManager.MarkSceneDirty(s.gameObject.scene); EditorSceneManager.SaveScene(s.gameObject.scene); AssetDatabase.SaveAssets();
        }
        private static void Soldier(SandboxPlayer player)
        {
            var visual = player.transform.Find("CC0 visual");
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = black;
            Attach(player.transform, "Chest", "Armoured vest", new Vector3(0, 1.17f, 0), new Vector3(.53f, .47f, .31f), black, PrimitiveType.Cube);
            Attach(player.transform, "Head", "Black helmet", new Vector3(0, 1.73f, 0), new Vector3(.38f, .28f, .37f), black, PrimitiveType.Sphere);
            Attach(player.transform, "Head", "Respirator mask", new Vector3(0, 1.55f, .18f), new Vector3(.29f, .19f, .13f), steel, PrimitiveType.Cube);
            for (int sign = -1; sign <= 1; sign += 2) Attach(player.transform, "Head", "Mask lens", new Vector3(sign * .08f, 1.66f, .19f), new Vector3(.1f, .075f, .055f), glass, PrimitiveType.Sphere);
            Attach(player.transform, "Chest", "Harness left", new Vector3(-.18f, 1.2f, -.16f), new Vector3(.045f, .5f, .08f), steel, PrimitiveType.Cube);
            Attach(player.transform, "Chest", "Harness right", new Vector3(.18f, 1.2f, -.16f), new Vector3(.045f, .5f, .08f), steel, PrimitiveType.Cube);
            var tank = Imported("Assets/Data/Sci-fi_assets/Sci-Fi Old rusty props PBR/Prefabs/Oxy_ballon.prefab", "Soldier oxygen tank", player.transform, new Vector3(0, 1.14f, -.31f), new Vector3(.28f, .74f, .28f));
            foreach (var renderer in tank.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = steel;
            var hose = new GameObject("Oxygen breathing hose").AddComponent<LineRenderer>(); hose.transform.SetParent(visual, false);
            hose.useWorldSpace = false; hose.positionCount = 5; hose.startWidth = hose.endWidth = .038f; hose.sharedMaterial = black;
            hose.SetPositions(new[] { new Vector3(.1f,1.4f,-.32f), new Vector3(.28f,1.45f,-.16f), new Vector3(.25f,1.42f,.08f), new Vector3(.15f,1.48f,.23f), new Vector3(0,1.54f,.25f) });
            tank.transform.SetParent(visual, true);
        }
        private static void Configure(SandboxEnemy enemy, InfectedKind kind, int index)
        {
            var variant = enemy.gameObject.AddComponent<InfectedVariant>(); variant.kind = kind;
            enemy.gameObject.AddComponent<Dismemberment>(); enemy.gameObject.AddComponent<InfectedVisibility>();
            float hp = kind == InfectedKind.Brute ? 180 : kind == InfectedKind.Runner ? 55 : kind == InfectedKind.Screamer ? 85 : 70;
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>(); definition.displayName = variant.Label; definition.health = hp;
            definition.speed = kind == InfectedKind.Runner ? 2.8f : kind == InfectedKind.Brute ? .9f : 1.2f; definition.sightRange = kind == InfectedKind.Screamer ? 10 : 7;
            definition.damage = 12; definition.attackWindup = kind == InfectedKind.Brute ? .9f : .6f; definition.attackRecovery = kind == InfectedKind.Brute ? 1.6f : 1.2f;
            string path = Root + "/Data/Enemy_Night_" + kind + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if (existing == null) AssetDatabase.CreateAsset(definition, path); else { Object.DestroyImmediate(definition); definition = existing; }
            enemy.definition = definition; enemy.health = hp; enemy.speed = definition.speed; enemy.sightRange = definition.sightRange;
            var skin = enemy.GetComponentInChildren<SkinnedMeshRenderer>(); string meshPath = Root + "/Art/Night/Infected_" + kind + ".asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (mesh == null)
            {
                mesh = Object.Instantiate(skin.sharedMesh); mesh.name = "Infected " + kind + " weighted body"; var vertices = mesh.vertices;
                float width = kind == InfectedKind.Brute ? 1.5f : kind == InfectedKind.Runner ? .72f : kind == InfectedKind.Screamer ? .85f : 1;
                for (int i = 0; i < vertices.Length; i++) { vertices[i].x *= width; vertices[i].z *= kind == InfectedKind.Brute ? 1.4f : 1; }
                mesh.vertices = vertices; mesh.RecalculateBounds(); mesh.RecalculateNormals(); AssetDatabase.CreateAsset(mesh, meshPath);
            }
            skin.sharedMesh = mesh;
            if (kind == InfectedKind.Brute)
            {
                Attach(enemy.transform, "Chest", "Broken riot chest armour", new Vector3(0,1.2f,.02f), new Vector3(.65f,.5f,.4f), black, PrimitiveType.Cube);
                Attach(enemy.transform, "Head", "Riot helmet", new Vector3(0,1.72f,0), new Vector3(.4f,.27f,.4f), steel, PrimitiveType.Sphere);
            }
            if (kind == InfectedKind.Screamer)
            {
                Attach(enemy.transform, "Neck", "Exposed throat sac", new Vector3(0,1.47f,.18f), new Vector3(.23f,.28f,.18f), red, PrimitiveType.Sphere);
                Attach(enemy.transform, "Chest", "Infected dorsal growth", new Vector3(0,1.3f,-.22f), new Vector3(.3f,.42f,.23f), red, PrimitiveType.Sphere);
            }
            var loot = enemy.gameObject.AddComponent<LootContainer>(); loot.id = "corpse-patrol-" + index; loot.label = "Cuerpo: " + variant.Label; loot.corpse = enemy;
            loot.stock = new LootStock { ammo = kind == InfectedKind.Brute ? 12 : 5, bandages = index % 2, cells = kind == InfectedKind.Screamer ? 1 : 0 };
        }
        private static void Container(string id, string label, Vector3 position, LootStock stock)
        {
            var go = Imported("Assets/Data/Sci-fi_assets/Sci-Fi Old rusty props PBR/Prefabs/Supply_box.prefab", label, null, position + Vector3.up * .35f, new Vector3(.9f,.7f,.7f));
            var loot = go.AddComponent<LootContainer>(); loot.id = id; loot.label = label; loot.stock = stock;
        }
        private static GameObject Imported(string path, string name, Transform parent, Vector3 center, Vector3 size)
        {
            var root = new GameObject(name); var model = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path)); model.transform.SetParent(root.transform, false);
            foreach (var c in model.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            foreach (var b in model.GetComponentsInChildren<MonoBehaviour>()) Object.DestroyImmediate(b);
            var renderers = model.GetComponentsInChildren<Renderer>(); var bounds = renderers[0].bounds; foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            model.transform.localPosition -= bounds.center;
            root.transform.localScale = new Vector3(size.x / bounds.size.x, size.y / bounds.size.y, size.z / bounds.size.z); root.transform.SetParent(parent, false); root.transform.localPosition = center;
            foreach (var r in renderers) r.sharedMaterials = r.sharedMaterials.Select(m => AssetDatabase.LoadAssetAtPath<Material>(Root + "/Art/Materials/Imported3D/" + AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m)) + ".mat") ?? steel).ToArray();
            return root;
        }
        private static void Attach(Transform actor, string bone, string name, Vector3 center, Vector3 size, Material mat, PrimitiveType shape)
        {
            var target = actor.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == bone) ?? actor;
            var go = GameObject.CreatePrimitive(shape); go.name = name; Object.DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = mat; go.transform.SetPositionAndRotation(actor.TransformPoint(center), actor.rotation); go.transform.localScale = size;
            go.transform.SetParent(target, true);
        }
        private static Material Material(string name, Color color, float smooth, float metallic)
        {
            string path = Root + "/Art/Night/" + name + ".mat"; var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, path); }
            mat.SetColor("_BaseColor", color); mat.SetFloat("_Smoothness", smooth); mat.SetFloat("_Metallic", metallic); EditorUtility.SetDirty(mat); return mat;
        }
        private static Light Lamp(string name, Transform parent, Vector3 position, Color color, float intensity, float range)
        {
            var light = new GameObject(name).AddComponent<Light>(); light.transform.SetParent(parent, false); light.transform.localPosition = position;
            light.type = LightType.Point; light.color = color; light.intensity = intensity; light.range = range; return light;
        }
    }
}
