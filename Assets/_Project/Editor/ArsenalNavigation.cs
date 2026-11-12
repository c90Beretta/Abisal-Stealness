using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
namespace Zombtoy.Editor
{
    public static class ArsenalNavigation
    {
        public static void Rebuild()
        {
            var scene=EditorSceneManager.OpenScene("Assets/_Project/Scenes/Level01_Port.unity");
            int index=0;
            foreach(var surface in Object.FindObjectsByType<NavMeshSurface>())
            {
                surface.RemoveData();surface.navMeshData=null;surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;surface.BuildNavMesh();
                string path="Assets/_Project/Navigation/Port06_"+(index++)+".asset";
                var old=AssetDatabase.LoadAssetAtPath<UnityEngine.AI.NavMeshData>(path);
                if(old!=null){EditorUtility.CopySerialized(surface.navMeshData,old);surface.RemoveData();surface.navMeshData=old;surface.AddData();EditorUtility.SetDirty(old);}
                else AssetDatabase.CreateAsset(surface.navMeshData,path);
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
    }
}
