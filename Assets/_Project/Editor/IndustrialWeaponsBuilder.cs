using System.IO;
using UnityEditor;
using UnityEngine;
using Zombtoy.Gameplay;
using Object = UnityEngine.Object;

namespace Zombtoy.Editor
{
    public static class IndustrialWeaponsBuilder
    {
        private const string Root = "Assets/_Project/Art/Industrial/Weapons";
        private static Material metal, polymer, wood;
        public static void Build(SandboxSession session)
        {
            Directory.CreateDirectory(Root); AssetDatabase.Refresh();
            metal = Mat("Parkerized steel", new Color(.085f, .09f, .085f), .14f, .25f); polymer = Mat("Black polymer", new Color(.035f, .038f, .034f), .03f, 0); wood = Mat("Worn dark stock", new Color(.16f, .095f, .05f), .04f, 0);
            for (int index = 0; index < 3; index++)
            {
                var gun = new GameObject(new[] { "DP-9 pistol", "C3 service carbine", "S12 pump shotgun" }[index]);
                if (index == 0)
                {
                    Box(gun.transform, "Slide", new Vector3(0, .055f, .12f), new Vector3(.065f, .075f, .29f), metal);
                    var grip = Box(gun.transform, "Grip", new Vector3(0, -.05f, -.008f), new Vector3(.055f, .14f, .075f), polymer); grip.transform.localRotation = Quaternion.Euler(-14, 0, 0);
                    Tube(gun.transform, "Barrel", new Vector3(0, .06f, .23f), .014f, .17f, metal);
                    Box(gun.transform, "Rear sight", new Vector3(0, .104f, -.01f), new Vector3(.045f, .02f, .014f), metal);
                    Box(gun.transform, "Front sight", new Vector3(0, .103f, .24f), new Vector3(.014f, .016f, .015f), metal);
                    Guard(gun.transform, .025f, -.045f, .072f);
                }
                else
                {
                    Box(gun.transform, "Receiver", new Vector3(0, .045f, .12f), new Vector3(.08f, .10f, .30f), metal);
                    Box(gun.transform, "Pistol grip", new Vector3(0, -.07f, -.005f), new Vector3(.065f, .16f, .085f), polymer);
                    Box(gun.transform, "Butt stock", new Vector3(0, .01f, -.19f), new Vector3(.065f, .13f, .32f), index == 2 ? wood : polymer);
                    Box(gun.transform, "Stock shoulder pad", new Vector3(0, .0f, -.36f), new Vector3(.08f, .17f, .035f), polymer);
                    Tube(gun.transform, "Barrel", new Vector3(0, .07f, .49f), index == 2 ? .024f : .015f, .59f, metal);
                    if (index == 1)
                    {
                        Box(gun.transform, "Hand guard", new Vector3(0, .048f, .38f), new Vector3(.075f, .083f, .26f), polymer);
                        var mag = Box(gun.transform, "Magazine", new Vector3(0, -.105f, .16f), new Vector3(.05f, .22f, .09f), metal); mag.transform.localRotation = Quaternion.Euler(11, 0, 0);
                        Box(gun.transform, "Top rail", new Vector3(0, .11f, .20f), new Vector3(.065f, .022f, .43f), metal);
                        for (int j = 0; j < 9; j++) Box(gun.transform, "Rail tooth", new Vector3(0, .129f, .04f + j * .04f), new Vector3(.07f, .012f, .012f), polymer);
                    }
                    else
                    {
                        Tube(gun.transform, "Magazine tube", new Vector3(0, .017f, .42f), .021f, .46f, metal);
                        Box(gun.transform, "Pump", new Vector3(0, -.007f, .39f), new Vector3(.086f, .065f, .23f), wood);
                        for (int j = 0; j < 6; j++) Box(gun.transform, "Pump rib", new Vector3(0, -.02f, .30f + j * .033f), new Vector3(.09f, .055f, .01f), polymer);
                    }
                    Guard(gun.transform, .04f, -.045f, .085f);
                }
                float muzzle = index == 0 ? .323f : .795f;
                var tip = new GameObject("Muzzle").transform; tip.SetParent(gun.transform, false); tip.localPosition = new Vector3(0, index == 0 ? .06f : .07f, muzzle);
                string path = Root + "/Firearm_" + index + ".prefab"; var prefab = PrefabUtility.SaveAsPrefabAsset(gun, path); Object.DestroyImmediate(gun);
                var weapon = session.player.GetComponent<WeaponLoadout>().weapons[index]; weapon.visualPrefab = prefab; EditorUtility.SetDirty(weapon);
            }
            var art = AssetDatabase.LoadAssetAtPath<CombatArt>(Root + "/CombatArt.asset"); if (art == null) { art = ScriptableObject.CreateInstance<CombatArt>(); AssetDatabase.CreateAsset(art, Root + "/CombatArt.asset"); }
            for (int i = 0; i < 2; i++)
            {
                var tool = new GameObject(i == 0 ? "Combat knife" : "Boarding axe");
                Box(tool.transform, "Handle", new Vector3(0, -.11f, 0), new Vector3(.035f, i == 0 ? .18f : .48f, .04f), i == 0 ? polymer : wood);
                Box(tool.transform, "Steel blade", new Vector3(i == 0 ? 0 : .075f, i == 0 ? .13f : .2f, 0), new Vector3(i == 0 ? .045f : .27f, i == 0 ? .28f : .16f, .018f), metal);
                Box(tool.transform, "Blade bevel", new Vector3(i == 0 ? .026f : .21f, i == 0 ? .15f : .2f, 0), new Vector3(.01f, i == 0 ? .23f : .13f, .008f), Mat("Blade edge", new Color(.52f, .54f, .49f), .22f, .5f));
                var prefab = PrefabUtility.SaveAsPrefabAsset(tool, Root + (i == 0 ? "/CombatKnife.prefab" : "/BoardingAxe.prefab")); Object.DestroyImmediate(tool);
                if (i == 0) art.knife = prefab; else art.axe = prefab;
            }
            var particleSource = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesUnlit.mat");
            string particlePath = Root + "/Soft_particles.mat"; art.particles = AssetDatabase.LoadAssetAtPath<Material>(particlePath);
            if (art.particles == null) { art.particles = new Material(particleSource); AssetDatabase.CreateAsset(art.particles, particlePath); }
            art.blood = Mat("Dark clotted blood", new Color(.105f, .008f, .011f), .12f, 0); art.brass = Mat("Tarnished brass", new Color(.43f, .31f, .105f), .15f, .4f);
            var pool = new Mesh { name = "Irregular blood pool" }; var v = new Vector3[17]; var t = new int[48];
            for (int i = 0; i < 16; i++) { float angle = i * Mathf.PI / 8; float r = .35f * (1 + Mathf.Sin(i * 7.3f) * .2f); v[i + 1] = new Vector3(Mathf.Cos(angle) * r, 0, Mathf.Sin(angle) * r * .8f); t[i * 3] = 0; t[i * 3 + 1] = (i + 1) % 16 + 1; t[i * 3 + 2] = i + 1; }
            pool.vertices = v; pool.triangles = t; pool.RecalculateNormals(); pool.RecalculateBounds(); art.bloodPool = SaveMesh(pool, Root + "/BloodPool.asset");
            var casing = GameObject.CreatePrimitive(PrimitiveType.Cylinder); var casingMesh = Object.Instantiate(casing.GetComponent<MeshFilter>().sharedMesh); Object.DestroyImmediate(casing);
            var verts = casingMesh.vertices; for (int i = 0; i < verts.Length; i++) verts[i] = Vector3.Scale(verts[i], new Vector3(.014f, .033f, .014f)); casingMesh.vertices = verts; casingMesh.RecalculateBounds(); casingMesh.name = "Brass cartridge"; art.casing = SaveMesh(casingMesh, Root + "/Casing.asset");
            session.combatArt = art; EditorUtility.SetDirty(art);
        }
        private static Mesh SaveMesh(Mesh mesh, string path)
        { var old = AssetDatabase.LoadAssetAtPath<Mesh>(path); if (old == null) { AssetDatabase.CreateAsset(mesh, path); return mesh; } EditorUtility.CopySerialized(mesh, old); Object.DestroyImmediate(mesh); return old; }
        private static void Guard(Transform parent, float y, float bottom, float z)
        {
            Box(parent, "Trigger guard bottom", new Vector3(0, bottom, z), new Vector3(.04f, .012f, .08f), metal);
            Box(parent, "Trigger guard front", new Vector3(0, (y + bottom) * .5f, z + .038f), new Vector3(.04f, y - bottom, .012f), metal);
        }
        private static GameObject Box(Transform parent, string name, Vector3 point, Vector3 size, Material mat)
        { var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; Object.DestroyImmediate(go.GetComponent<Collider>()); go.transform.SetParent(parent, false); go.transform.localPosition = point; go.transform.localScale = size; go.GetComponent<Renderer>().sharedMaterial = mat; return go; }
        private static void Tube(Transform parent, string name, Vector3 point, float radius, float length, Material mat)
        { var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); go.name = name; Object.DestroyImmediate(go.GetComponent<Collider>()); go.transform.SetParent(parent, false); go.transform.localPosition = point; go.transform.localRotation = Quaternion.Euler(90, 0, 0); go.transform.localScale = new Vector3(radius * 2, length * .5f, radius * 2); go.GetComponent<Renderer>().sharedMaterial = mat; }
        private static Material Mat(string name, Color color, float smooth, float metallic)
        { string path = Root + "/" + name.Replace(" ", "_") + ".mat"; var mat = AssetDatabase.LoadAssetAtPath<Material>(path); if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, path); } mat.SetColor("_BaseColor", color); mat.SetFloat("_Smoothness", smooth); mat.SetFloat("_Metallic", metallic); EditorUtility.SetDirty(mat); return mat; }
    }
}
