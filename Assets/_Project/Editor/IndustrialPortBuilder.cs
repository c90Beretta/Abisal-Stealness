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
using Zombtoy.Gameplay;
using Object = UnityEngine.Object;

namespace Zombtoy.Editor
{
    public static class IndustrialPortBuilder
    {
        private const string Root = "Assets/_Project/Art/Industrial";
        private static Material enamel, dark, steel, rust, ochre, lampWhite, floor;
        private static Transform art;
        private static Font lettering;
        [MenuItem("Abisal/Construir puerto industrial 0.5")]
        public static void Upgrade()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(PortMapBuilder.ScenePath);
            if (GameObject.Find("Industrial art direction") != null) throw new InvalidOperationException("Industrial migration already applied. Preserve the authored scene.");
            Directory.CreateDirectory(Root + "/Materials"); AssetDatabase.Refresh();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "/Textures/AgedEnamel_v1.png");
            var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
            importer.maxTextureSize = 256; importer.filterMode = FilterMode.Point; importer.wrapMode = TextureWrapMode.Repeat; importer.mipmapEnabled = true; importer.anisoLevel = 2; importer.SaveAndReimport();
            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "/Textures/AgedEnamel_v1.png");
            enamel = Mat("Aged enamel", new Color(.84f, .79f, .66f), texture);
            dark = Mat("Service wall lower", new Color(.20f, .24f, .22f), texture);
            steel = Mat("Brushed dull steel", new Color(.24f, .25f, .23f), texture);
            rust = Mat("Oxide pipe", new Color(.34f, .17f, .095f), texture);
            ochre = Mat("Safety paint", new Color(.63f, .39f, .12f), texture);
            floor = Mat("Dirty concrete", new Color(.34f, .32f, .28f), texture); floor.SetTextureScale("_BaseMap", new Vector2(12, 10));
            lampWhite = Mat("Warm fluorescent diffuser", new Color(.87f, .79f, .58f)); lampWhite.EnableKeyword("_EMISSION"); lampWhite.SetColor("_EmissionColor", new Color(2.8f, 2.1f, 1.2f));
            lettering = AssetDatabase.LoadAssetAtPath<Font>("Assets/ThirdParty/Fonts/barlowcondensed/BarlowCondensed-Medium.ttf");
            var session = Object.FindAnyObjectByType<SandboxSession>();
            var faceImporter = (TextureImporter)AssetImporter.GetAtPath(Root + "/Textures/InfectedFace_v1.png"); faceImporter.maxTextureSize = 128; faceImporter.filterMode = FilterMode.Point; faceImporter.wrapMode = TextureWrapMode.Clamp; faceImporter.mipmapEnabled = true; faceImporter.SaveAndReimport();
            var clothImporter=(TextureImporter)AssetImporter.GetAtPath(Root+"/Textures/WornCloth_v1.png");clothImporter.maxTextureSize=128;clothImporter.filterMode=FilterMode.Point;clothImporter.wrapMode=TextureWrapMode.Repeat;clothImporter.mipmapEnabled=true;clothImporter.SaveAndReimport();
            HumanCharacterBuilder.ReplaceAll(session); IndustrialWeaponsBuilder.Build(session);
            session.hudFont = lettering; session.displayFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/ThirdParty/Fonts/crimsontext/CrimsonText-SemiBold.ttf");
            var environment = GameObject.Find("Port_Environment_3D");
            if (PrefabUtility.IsPartOfPrefabInstance(environment)) PrefabUtility.UnpackPrefabInstance(environment, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            art = new GameObject("Industrial art direction").transform; art.SetParent(environment.transform, false);
            // Keep imported structures, remove the glossy toy finish through owned adapters only.
            foreach (string path in AssetDatabase.FindAssets("t:Material", new[] { "Assets/_Project/Art/Materials/Imported3D" }).Select(AssetDatabase.GUIDToAssetPath))
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path); mat.SetFloat("_Smoothness", .06f); mat.SetFloat("_Metallic", .03f); EditorUtility.SetDirty(mat);
            }
            foreach (var renderer in environment.GetComponentsInChildren<Renderer>())
                if (renderer.name == "Quay deck") renderer.sharedMaterial = floor;
            foreach (var text in Object.FindObjectsByType<TextMesh>()) Object.DestroyImmediate(text.gameObject);
            Airlock(); ServiceRoute(); Dressing(session);
            IndustrialArtRebuild.Run(session,art,lettering);
            Lighting(session);IndustrialArtRebuild.FinishLighting();
            var camera = session.player.view; camera.orthographicSize = 2.85f;
            var iso = camera.GetComponent<IsometricView>(); iso.minZoom = 2.6f; iso.maxZoom = 11; iso.pitch = 32; iso.yaw = 45;
            camera.transform.rotation = Quaternion.Euler(iso.pitch, iso.yaw, 0); camera.transform.position = session.player.transform.position + Vector3.up * .9f - camera.transform.forward * 28;
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/_Project/Art/Night/NightScreen.asset");
            profile.TryGet(out Vignette vignette); vignette.intensity.Override(.23f); vignette.smoothness.Override(.62f);
            profile.TryGet(out FilmGrain grain); grain.intensity.Override(.055f);
            profile.TryGet(out Bloom bloom); bloom.intensity.Override(.16f); bloom.threshold.Override(1.6f);
            profile.TryGet(out Tonemapping tone); tone.mode.Override(TonemappingMode.Neutral);
            if (!profile.TryGet(out ColorAdjustments grade)) { grade = profile.Add<ColorAdjustments>(true); AssetDatabase.AddObjectToAsset(grade, profile); }
            grade.postExposure.Override(.15f); grade.contrast.Override(10); grade.saturation.Override(-12);
            EditorUtility.SetDirty(profile);
            var options = session.GetComponent<PresentationOptions>() ?? session.gameObject.AddComponent<PresentationOptions>(); options.volume = Object.FindAnyObjectByType<Volume>();
            const string damagePath = Root + "/Materials/DamageVignette.mat"; var damage = AssetDatabase.LoadAssetAtPath<Material>(damagePath); if (damage == null) { damage = new Material(Shader.Find("Abisal/Damage Vignette")); AssetDatabase.CreateAsset(damage, damagePath); }
            session.damageVignetteMaterial = damage;
            // Art and collision share the same authored parent and baked navigation.
            var surface = environment.GetComponent<NavMeshSurface>(); surface.RemoveData(); surface.navMeshData = null; surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders; surface.BuildNavMesh();
            const string nav = "Assets/_Project/Navigation/Port05_NavMesh.asset"; if (File.Exists(nav)) AssetDatabase.DeleteAsset(nav); AssetDatabase.CreateAsset(surface.navMeshData, nav);
            PrefabUtility.SaveAsPrefabAsset(art.gameObject, "Assets/_Project/Prefabs/Levels/Port_Industrial_Dressing.prefab");
            PlayerSettings.bundleVersion = "0.5.0";
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Directory.CreateDirectory("TestResults"); File.WriteAllText("TestResults/industrial-authoring.txt", "Adult weighted character bodies; 13-body jointed ragdolls; 8 enemy data variants; original grime albedo; enclosed arrival airlock and industrial service route; matte adapted assets; new baked navigation.\n");
        }
        public static void Audit()
        {
            EditorSceneManager.OpenScene(PortMapBuilder.ScenePath);
            var rows = Object.FindObjectsByType<SandboxEnemy>(FindObjectsInactive.Include).Select(e => e.name + " | " + string.Join(";", e.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(s => s.name + " parent=" + s.transform.parent.name + " bones=" + s.bones.Length)) + " | rag=" + e.GetComponent<RagdollController>().poseBones.Length);
            File.WriteAllLines("TestResults/actor-audit.txt", rows);
        }
        private static void Airlock()
        {
            // Two open exits join the unchanged playable route; foreground panels cut away.
            Wall("Airlock west", new Vector3(-27.6f, 1.5f, -15), new Vector3(.22f, 3, 8.2f));
            Wall("Airlock south", new Vector3(-23.8f, 1.5f, -19), new Vector3(7.6f, 3, .22f));
            Wall("Airlock east lower", new Vector3(-20, 1.5f, -17), new Vector3(.22f, 3, 4));
            Wall("Airlock east upper", new Vector3(-20, 1.5f, -11.6f), new Vector3(.22f, 3, 1.2f));
            Wall("Airlock north left", new Vector3(-26.4f, 1.5f, -11), new Vector3(2.4f, 3, .22f));
            Wall("Airlock north right", new Vector3(-21.4f, 1.5f, -11), new Vector3(2.8f, 3, .22f));
            Box("Airlock floor inset", new Vector3(-23.8f, .017f, -15), new Vector3(7.4f, .025f, 7.8f), steel, false);
            for (int i = 0; i < 8; i++)
            {
                Box("Floor seam", new Vector3(-23.8f, .033f, -18.5f + i), new Vector3(7.4f, .006f, .018f), dark, false);
                Box("Floor cross seam", new Vector3(-27.2f + i, .034f, -15), new Vector3(.018f, .006f, 7.7f), dark, false);
            }
            DoorFrame(new Vector3(-24, 0, -11), 0, "ALMACEN 03"); DoorFrame(new Vector3(-20, 0, -13.5f), 90, "PATIO DE CARGA");
            for (int i = 0; i < 4; i++)
            {
                var beam = Box("Airlock roof rib", new Vector3(-23.8f, 2.97f, -18 + i * 2), new Vector3(7.6f, .14f, .11f), dark, false); beam.AddComponent<MapOccluder>();
            }
            Pipe(new Vector3(-27.3f, 2.6f, -18.7f), new Vector3(-27.3f, 2.6f, -11.2f), .09f, rust);
            Pipe(new Vector3(-27.1f, .4f, -18), new Vector3(-27.1f, 2.6f, -18), .075f, steel);
            Box("Decontamination drain", new Vector3(-25.4f, .045f, -16.2f), new Vector3(1, .02f, 1.8f), dark, false);
            for (int i = 0; i < 8; i++) Box("Drain grating", new Vector3(-25.4f, .06f, -16.95f + i * .21f), new Vector3(.92f, .018f, .035f), steel, false);
            Sign("CUARENTENA", new Vector3(-23.8f, 2.32f, -18.82f), 0, 1.25f);
            Sign("CONTROL BIOLOGICO / B-07", new Vector3(-23.8f, 1.91f, -18.8f), 0, .74f);
            Sign("NO RETIRAR LA MASCARA", new Vector3(-26.2f, 1.7f, -11.14f), 180, .58f);
            Box("Departure warning lamp", new Vector3(-24, 2.48f, -11.18f), new Vector3(.52f, .09f, .13f), lampWhite, false);
            Fluorescent(new Vector3(-24, 2.82f, -15.2f), 0, 1.9f, 6.5f, new Color(1, .82f, .61f));
            Fluorescent(new Vector3(-25.8f, 2.72f, -12.2f), 90, 1.3f, 5, new Color(.67f, .78f, .69f));
        }
        private static void ServiceRoute()
        {
            // Low borders and an overhead pipe turn the blank apron into a maintenance route.
            for (int z = -8; z <= 8; z += 4)
            {
                Box("Service kerb", new Vector3(-26.7f, .12f, z), new Vector3(.35f, .24f, 3.6f), dark, true);
                var post = Box("Service pipe support", new Vector3(-26.4f, 1.5f, z), new Vector3(.12f, 3, .12f), steel, true); post.AddComponent<MapOccluder>();
            }
            Pipe(new Vector3(-26.4f, 2.87f, -9.8f), new Vector3(-26.4f, 2.87f, 10), .13f, rust);
            Pipe(new Vector3(-26.05f, 2.65f, -9.8f), new Vector3(-26.05f, 2.65f, 10), .055f, steel);
            Sign("ALMACEN 03", new Vector3(-24.3f, 2.5f, 8.7f), 180, 1.1f);
            Sign("REPARACION / ACCESO DE SERVICIO", new Vector3(5, 2.3f, 13.95f), 180, .7f);
            Sign("CONTROL", new Vector3(18.5f, 2.58f, 9.8f), 180, 1);
            Sign("B-07 / EMBARQUE", new Vector3(30.5f, 1.3f, -10.25f), 0, .75f);
            for (int z = -9; z <= 7; z += 4)
            {
                Box("Route paint left", new Vector3(-25.7f, .043f, z), new Vector3(.1f, .01f, 1.8f), ochre, false);
                Box("Route paint right", new Vector3(-22.1f, .043f, z), new Vector3(.1f, .01f, 1.8f), ochre, false);
            }
        }
        private static void Dressing(SandboxSession session)
        {
            Fluorescent(new Vector3(-22, 2.65f, 16), 0, 2.2f, 7, new Color(1, .84f, .67f));
            Fluorescent(new Vector3(19, 2.65f, 17), 90, 2.0f, 7, new Color(.65f, .79f, .7f));
            Fluorescent(new Vector3(5, 2.3f, 15), 0, 1.4f, 6, new Color(1, .5f, .24f));
            // Practical roof braces and real imported machinery retain space for combat.
            for (int x = -25; x <= -16; x += 3)
            {
                var brace = Box("Warehouse upper beam", new Vector3(x, 3, 16), new Vector3(.12f, .18f, 11.6f), dark, false); brace.AddComponent<MapOccluder>();
            }
            Pipe(new Vector3(12.2f, 2.45f, 10.5f), new Vector3(12.2f, 2.45f, 21.5f), .08f, rust);
            for (int i = 0; i < 5; i++)
            {
                Box("Warehouse warning paint", new Vector3(-25 + i * .3f, .04f, 10), new Vector3(.13f, .01f, .7f), ochre, false);
                Box("Generator warning paint", new Vector3(3.7f + i * .5f, .04f, 12.4f), new Vector3(.2f, .01f, .7f), ochre, false);
            }
            Sign("FUGA O2 / MASCARA OBLIGATORIA", new Vector3(7.9f, 1.9f, 17.3f), 180, .6f);
            // The imported supply boxes remain physical props; their UI appears only nearby.
            foreach (var pickup in session.GetComponent<NightSurvival>().GetComponents<WorldPickup>()) EditorUtility.SetDirty(pickup);
        }
        private static void Lighting(SandboxSession session)
        {
            foreach (var old in Object.FindObjectsByType<Light>()) if (old.name == "Emergency red") { old.color = new Color(1, .56f, .26f); old.intensity = .45f; old.range = 4; }
            RenderSettings.ambientMode = AmbientMode.Trilight; RenderSettings.ambientSkyColor = new Color(.055f, .052f, .044f); RenderSettings.ambientEquatorColor = new Color(.027f, .028f, .025f); RenderSettings.ambientGroundColor = new Color(.012f, .012f, .011f);
            RenderSettings.fog = true; RenderSettings.fogDensity = .022f; RenderSettings.fogColor = new Color(.016f, .021f, .019f);
            foreach (var light in Object.FindObjectsByType<Light>())
                if (light.type == LightType.Directional) { light.intensity = .09f; light.color = new Color(.58f, .65f, .63f); light.shadows = LightShadows.Soft; }
            var night = session.GetComponent<NightSurvival>(); night.personalLight.color = new Color(.65f, .66f, .57f); night.personalLight.intensity = 2.6f; night.personalLight.range = 4.8f;
            night.flashlight.intensity = 10; night.flashlight.color = new Color(1, .91f, .74f); night.flashlight.spotAngle = 69; night.flashlight.innerSpotAngle = 40;
            night.flashlight.transform.localPosition = new Vector3(.19f, 1.4f, .3f); night.flashlight.transform.localRotation = Quaternion.Euler(18, 0, 0);
        }
        private static void Wall(string name, Vector3 pos, Vector3 size)
        {
            var wall = Box(name, pos, size, enamel, true); wall.AddComponent<MapOccluder>();
            var lower = Box("Dark wainscot", pos + Vector3.down * 1.02f, new Vector3(size.x + .01f, .9f, size.z + .01f), dark, false); lower.transform.SetParent(wall.transform, true);
            var stripe = Box("Panel waist seam", pos + Vector3.down * .53f, new Vector3(size.x + .02f, .035f, size.z + .02f), steel, false); stripe.transform.SetParent(wall.transform, true);
            bool horizontal = size.x > size.z; int count = Mathf.CeilToInt((horizontal ? size.x : size.z) / 1.25f);
            for (int i = 0; i <= count; i++)
            {
                Vector3 offset = (horizontal ? Vector3.right : Vector3.forward) * ((i / (float)Mathf.Max(1, count) - .5f) * (horizontal ? size.x : size.z));
                var seam = Box("Bulkhead panel joint", pos + offset, new Vector3(horizontal ? .023f : size.x + .025f, size.y, horizontal ? size.z + .025f : .023f), steel, false); seam.transform.SetParent(wall.transform, true);
            }
        }
        private static void DoorFrame(Vector3 foot, float yaw, string label)
        {
            var root = new GameObject("Open service hatch").transform; root.SetParent(art, false); root.position = foot; root.rotation = Quaternion.Euler(0, yaw, 0);
            foreach (int sign in new[] { -1, 1 })
            { var jamb = Box("Hatch jamb", Vector3.zero, new Vector3(.19f, 2.5f, .32f), steel, true); jamb.transform.SetParent(root, false); jamb.transform.localPosition = new Vector3(sign * 1.05f, 1.25f, 0); }
            var lintel = Box("Hatch lintel", Vector3.zero, new Vector3(2.3f, .23f, .32f), steel, true); lintel.transform.SetParent(root, false); lintel.transform.localPosition = Vector3.up * 2.61f;
            Sign(label, foot + Vector3.up * 2.95f, yaw, .58f);
        }
        private static void Fluorescent(Vector3 pos, float yaw, float intensity, float range, Color color)
        {
            var fixture = Box("Fluorescent housing", pos, new Vector3(1.25f, .12f, .24f), dark, false); fixture.transform.rotation = Quaternion.Euler(0, yaw, 0);
            var diffuser = Box("Fluorescent diffuser", pos + Vector3.down * .066f, new Vector3(1.08f, .02f, .16f), lampWhite, false); diffuser.transform.rotation = fixture.transform.rotation;
            var lamp = new GameObject("Practical fluorescent").AddComponent<Light>(); lamp.transform.SetParent(art); lamp.transform.position = pos + Vector3.down * .22f; lamp.color = color; lamp.range = range; lamp.intensity = intensity; lamp.type = LightType.Point;
            var subtle = lamp.gameObject.AddComponent<IndustrialLight>(); subtle.strength = intensity; subtle.seed = Mathf.Abs(pos.x + pos.z);
        }
        private static void Sign(string label, Vector3 pos, float yaw, float size)
        {
            var backing = Box("Sign / " + label, pos, new Vector3(size * 3.6f, size * .56f, .035f), dark, false); backing.transform.rotation = Quaternion.Euler(0, yaw, 0);
            var text = new GameObject(label).AddComponent<TextMesh>(); text.transform.SetParent(art, false); text.transform.position = pos + new Vector3(-.035f, 0, -.035f); float readableYaw = yaw % 180; if (readableYaw < 0) readableYaw += 180; text.transform.rotation = Quaternion.Euler(0, readableYaw, 0);
            text.text = label; text.font = lettering; text.GetComponent<MeshRenderer>().sharedMaterial = lettering.material; text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center; text.fontSize = 64; text.characterSize = size * .043f; text.color = new Color(.85f, .8f, .65f);
        }
        private static void Pipe(Vector3 a, Vector3 b, float radius, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); go.name = "Service pipe"; Object.DestroyImmediate(go.GetComponent<Collider>()); go.transform.SetParent(art);
            go.transform.position = (a + b) * .5f; go.transform.up = (b - a).normalized; go.transform.localScale = new Vector3(radius * 2, Vector3.Distance(a, b) * .5f, radius * 2); go.GetComponent<Renderer>().sharedMaterial = material;
        }
        private static GameObject Box(string name, Vector3 pos, Vector3 size, Material material, bool solid)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(art); go.transform.position = pos; go.transform.localScale = size; go.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }
        private static Material Mat(string name, Color color, Texture texture = null)
        {
            string path = Root + "/Materials/" + name.Replace(" ", "_") + ".mat"; var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, path); }
            mat.SetColor("_BaseColor", color); mat.SetFloat("_Smoothness", .035f); mat.SetFloat("_Metallic", 0); mat.enableInstancing = true;
            if (texture != null) mat.SetTexture("_BaseMap", texture); EditorUtility.SetDirty(mat); return mat;
        }
    }
}
