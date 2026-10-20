using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Zombtoy.Editor
{
    public static class Asset3DInspection
    {
        public static void Inspect()
        {
            Directory.CreateDirectory("TestResults");
            using (var output = new StreamWriter("TestResults/asset-3d-inspection.txt"))
            {
                foreach (string folder in new[] { "Assets/SciFi Warehouse Kit/Prefabs", "Assets/Sci Fi Modular Pack/Prefabs", "Assets/Data/Sci-fi_assets/Sci-Fi Old rusty props PBR/Prefabs", "Assets/ThirdParty/Kenney/BlasterKit/Models/FBX format" })
                    foreach (string guid in AssetDatabase.FindAssets("t:GameObject", new[] { folder }))
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        var copy = Object.Instantiate(asset);
                        var renderers = copy.GetComponentsInChildren<Renderer>();
                        if (renderers.Length == 0) { Object.DestroyImmediate(copy); continue; }
                        var bounds = renderers[0].bounds; foreach (var r in renderers) bounds.Encapsulate(r.bounds);
                        output.WriteLine(path + " | size " + bounds.size.ToString("F3") + " center " + bounds.center.ToString("F3") + " mats " + string.Join(",", renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null).Select(m => m.name + ":" + m.shader.name).Distinct()));
                        Object.DestroyImmediate(copy);
                    }
            }
        }
    }
}
