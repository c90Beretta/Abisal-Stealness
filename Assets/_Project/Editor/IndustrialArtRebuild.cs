using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Zombtoy.Gameplay;
using Object=UnityEngine.Object;

namespace Zombtoy.Editor
{
    public static class IndustrialArtRebuild
    {
        private const string Root="Assets/_Project/Art/Industrial";
        private const string Warehouse="Assets/SciFi Warehouse Kit/Prefabs/Props/";
        private const string Rusty="Assets/Data/Sci-fi_assets/Sci-Fi Old rusty props PBR/Prefabs/";
        private static Transform art;

        private static readonly Dictionary<Material,Material> converted=new Dictionary<Material,Material>();
        public static void Run(SandboxSession session,Transform parent,Font lettering)
        {
            art=parent;converted.Clear();InstallRetro();Envelope();
            // Replace flat bulkhead faces with the user's detailed modular wall meshes.
            foreach(var wall in parent.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Airlock ")&&(t.name.Contains("west")||t.name.Contains("south")||t.name.Contains("east")||t.name.Contains("north"))).ToArray())
            {
                var renderer=wall.GetComponent<MeshRenderer>();if(renderer==null)continue;
                Bounds b=renderer.bounds;foreach(var old in wall.GetComponentsInChildren<Renderer>())old.enabled=false;
                bool alongX=b.size.x>b.size.z;float length=alongX?b.size.x:b.size.z;int count=Mathf.CeilToInt(length/1.9f);
                for(int i=0;i<count;i++)
                {
                    Vector3 center=b.center+(alongX?Vector3.right:Vector3.forward)*((i+.5f)/count-.5f)*length;
                    Imported(Rusty+"BulkHead.prefab",center,alongX?new Vector3(length/count,3,.23f):new Vector3(.23f,3,length/count),wall,alongX?0:90,false);
                }
            }
            Imported(Warehouse+"Misc Props/Cart.prefab",new Vector3(-26.5f,.48f,-17.2f),new Vector3(.8f,.95f,1.25f),art,0,true);
            Imported(Warehouse+"Misc Props/Fire Extinguisher.prefab",new Vector3(-27.29f,1.25f,-15.2f),new Vector3(.2f,.65f,.2f),art,90,false);
            Imported(Rusty+"Electroboxes.prefab",new Vector3(-26.95f,1.35f,-12),new Vector3(.85f,1.2f,.35f),art,90,true);
            Imported(Rusty+"Vents.prefab",new Vector3(-26.95f,2.2f,-17),new Vector3(.75f,.55f,.27f),art,90,false);
            Imported(Warehouse+"Ducts/Duct Straight.prefab",new Vector3(-26.6f,2.6f,-14.8f),new Vector3(.45f,.4f,4.5f),art,90,false);
            Imported(Warehouse+"Misc Props/Wall Light.prefab",new Vector3(-26.9f,2.25f,-13.1f),new Vector3(.22f,.45f,.25f),art,90,false);
            Imported(Warehouse+"Misc Props/Garbage Bin.prefab",new Vector3(-21.1f,.55f,-17.9f),new Vector3(.65f,1.1f,.65f),art,0,true);
            BodyBag(new Vector3(-26.4f,.38f,-14.8f),0);
            Imported(Rusty+"Oxy_agregate.prefab",new Vector3(-21.2f,.72f,-17.2f),new Vector3(1.1f,1.45f,.7f),art,0,true);
            Imported(Rusty+"Vents.prefab",new Vector3(-20.2f,2.15f,-16.8f),new Vector3(.2f,.65f,1.1f),art,90,false);
            Imported(Warehouse+"Shelves/Shelf Variation 03.prefab",new Vector3(-24.8f,1.2f,18.6f),new Vector3(1.35f,2.4f,3.3f),art,90,true);
            Imported(Warehouse+"Shelves/Shelf Variation 02.prefab",new Vector3(-15.1f,1.1f,18.4f),new Vector3(1.3f,2.2f,3.2f),art,90,true);
            Imported(Rusty+"Electroboxes.prefab",new Vector3(-25.65f,1.35f,12.5f),new Vector3(.25f,1.5f,1.2f),art,90,false);
            Imported(Warehouse+"Misc Props/Cart.prefab",new Vector3(-18.5f,.48f,19.4f),new Vector3(1.5f,.95f,.7f),art,90,true);
            Imported(Warehouse+"Ducts/Duct Elbow 01.prefab",new Vector3(-24.8f,2.45f,20),new Vector3(1,.65f,1),art,90,false);
            Imported(Rusty+"Tubes.prefab",new Vector3(12.8f,2.15f,19),new Vector3(.55f,.65f,3.5f),art,90,false);
            ReplaceMonitors(session);
            var camera=session.player.view;camera.orthographicSize=2.85f;camera.GetComponent<IsometricView>().minZoom=2.6f;
            var fill=new GameObject("Equipment inspection bounce").AddComponent<Light>();fill.transform.SetParent(session.player.transform,false);fill.transform.localPosition=new Vector3(-.5f,.9f,-.7f);fill.type=LightType.Point;fill.range=2.7f;fill.intensity=.75f;fill.color=new Color(.78f,.82f,.75f);
            var lowerFill=new GameObject("Low equipment bounce").AddComponent<Light>();lowerFill.transform.SetParent(session.player.transform,false);lowerFill.transform.localPosition=new Vector3(-.35f,.4f,-.55f);lowerFill.type=LightType.Point;lowerFill.range=1.8f;lowerFill.intensity=.55f;lowerFill.color=new Color(.72f,.79f,.76f);
            foreach(var material in AssetDatabase.FindAssets("t:Material",new[]{Root,"Assets/_Project/Art/Materials/Imported3D"}).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Material>))
            {
                if(material.shader.name=="Universal Render Pipeline/Lit") { material.shader=Shader.Find("Abisal/Retro Surface");EditorUtility.SetDirty(material); }
            }
        }
        private static void Envelope()
        {
            foreach(string name in new[]{"South perimeter","West perimeter","North perimeter"}){var old=GameObject.Find(name);if(old!=null)Object.DestroyImmediate(old);}
            var rail=Material("Quay guard steel",new Color(.21f,.24f,.22f));
            Box("South perimeter rebuilt",new Vector3(5,.45f,-20),new Vector3(46,.9f,.3f),rail,true);
            Box("West perimeter rebuilt",new Vector3(-28,.45f,0),new Vector3(.3f,.9f,16),rail,true);
            Box("North perimeter rebuilt",new Vector3(-1,.45f,24),new Vector3(24,.9f,.3f),rail,true);
            Building("Quarantine receiving",new Vector2(-32,-24),new Vector2(-18,-8),-24);
            Building("Warehouse service annex",new Vector2(-32,8),new Vector2(-13,27),-20);
            Building("Control service annex",new Vector2(11,8),new Vector2(27,27),18.5f);
        }
        private static void Building(string name,Vector2 min,Vector2 max,float doorwayX)
        {
            var floorMat=Material("Service deck",new Color(.43f,.42f,.34f));floorMat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/AgedEnamel_v1.png"));floorMat.SetTextureScale("_BaseMap",new Vector2(9,9));
            Box(name+" structural deck",new Vector3((min.x+max.x)*.5f,-.08f,(min.y+max.y)*.5f),new Vector3(max.x-min.x,.16f,max.y-min.y),floorMat,true);
            Segment(new Vector3(min.x,1.7f,min.y),new Vector3(min.x,1.7f,max.y));
            Segment(new Vector3(min.x,1.7f,max.y),new Vector3(doorwayX-1.5f,1.7f,max.y));
            Segment(new Vector3(doorwayX+1.5f,1.7f,max.y),new Vector3(max.x,1.7f,max.y));
            Segment(new Vector3(min.x,1.7f,min.y),new Vector3(doorwayX-1.5f,1.7f,min.y));
            Segment(new Vector3(doorwayX+1.5f,1.7f,min.y),new Vector3(max.x,1.7f,min.y));
            if(name.StartsWith("Quarantine"))
            {
                Segment(new Vector3(max.x,1.7f,min.y),new Vector3(max.x,1.7f,-15));
                Segment(new Vector3(max.x,1.7f,-12),new Vector3(max.x,1.7f,max.y));
            }
            else Segment(new Vector3(max.x,1.7f,min.y),new Vector3(max.x,1.7f,max.y));
        }
        private static void Segment(Vector3 a,Vector3 b)
        {
            float length=Vector3.Distance(a,b);if(length<.1f)return;bool x=Mathf.Abs(a.x-b.x)>Mathf.Abs(a.z-b.z);int count=Mathf.CeilToInt(length/2);
            for(int i=0;i<count;i++)Imported(Rusty+"BulkHead.prefab",Vector3.Lerp(a,b,(i+.5f)/count),x?new Vector3(length/count,3.4f,.3f):new Vector3(.3f,3.4f,length/count),art,x?0:90,true);
        }
        public static void InstallRetro()
        {
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/PC_Renderer.asset");
            renderer.rendererFeatures.RemoveAll(f=>f==null);
            var feature=renderer.rendererFeatures.OfType<RetroRenderFeature>().FirstOrDefault();
            if(feature==null){feature=ScriptableObject.CreateInstance<RetroRenderFeature>();feature.name="ABISAL PS1 world";AssetDatabase.AddObjectToAsset(feature,renderer);renderer.rendererFeatures.Add(feature);}
            feature.material=Material("PS1 colour depth",Color.white,"Abisal/PS1 Screen");feature.SetActive(true);feature.Create();EditorUtility.SetDirty(feature);EditorUtility.SetDirty(renderer);
        }
        public static void FinishLighting()
        {
            RenderSettings.ambientSkyColor=new Color(.09f,.085f,.07f);RenderSettings.ambientEquatorColor=new Color(.04f,.045f,.035f);RenderSettings.ambientGroundColor=new Color(.025f,.024f,.021f);
            RenderSettings.fogDensity=.018f;
            Spot(new Vector3(-24.8f,2.8f,-15),7,7,new Color(1,.84f,.63f));
            Spot(new Vector3(-22.5f,2.8f,15.5f),8,8,new Color(1,.82f,.61f));
            Spot(new Vector3(19.5f,2.9f,17),7,8,new Color(.83f,.88f,.76f));
        }
        private static void Spot(Vector3 position,float intensity,float range,Color color)
        {
            var light=new GameObject("Recessed work light").AddComponent<Light>();light.transform.SetParent(art);light.transform.position=position;light.transform.rotation=Quaternion.Euler(90,0,0);light.type=LightType.Spot;light.spotAngle=120;light.innerSpotAngle=75;light.range=range;light.intensity=intensity;light.color=color;light.shadows=LightShadows.Soft;light.shadowResolution=UnityEngine.Rendering.LightShadowResolution.Low;light.shadowBias=.035f;light.shadowNormalBias=.08f;
        }
        private static void ReplaceMonitors(SandboxSession session)
        {
            foreach(var screen in Object.FindObjectsByType<Transform>().Where(t=>t.name=="Control wall display"||t.name=="Terminal screen").ToArray())Object.DestroyImmediate(screen.gameObject);
            var terminal=session.terminal.GetComponent<Renderer>();if(terminal!=null)terminal.enabled=false;
            Imported(Rusty+"Old_computer.prefab",new Vector3(22,.8f,18),new Vector3(1.7f,1.55f,.95f),session.terminal,0,false);
            for(int i=0;i<3;i++)
            {
                float x=15+i*3;
                Imported(Rusty+"Old_computer.prefab",new Vector3(x,1.15f,21.1f),new Vector3(1.35f,1.4f,.8f),art,0,false);
                var body=Box("Operator console cabinet",new Vector3(x,.4f,21.1f),new Vector3(1.7f,.8f,1.0f),Material("Console case",new Color(.22f,.25f,.21f)),true);
                for(int drawer=0;drawer<2;drawer++)Box("Cabinet handle",new Vector3(x,.26f+drawer*.3f,20.58f),new Vector3(.45f,.025f,.04f),Material("Old aluminium",new Color(.37f,.36f,.3f)),false);
            }
        }
        private static void BodyBag(Vector3 center,float yaw)
        {
            var root=new GameObject("Quarantine stretcher").transform;root.SetParent(art);root.position=center;root.rotation=Quaternion.Euler(0,yaw,0);
            var mesh=new Mesh {name="Sewn quarantine body bag"};var vertices=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();
            float[] z={-.95f,-.82f,-.62f,-.32f,.12f,.48f,.68f,.82f,.9f};float[] width={.06f,.16f,.20f,.29f,.28f,.23f,.15f,.15f,.04f};float[] height={.025f,.075f,.085f,.14f,.16f,.13f,.11f,.1f,.025f};
            for(int ring=0;ring<z.Length;ring++)for(int side=0;side<=12;side++){float a=side*Mathf.PI*2/12;vertices.Add(new Vector3(Mathf.Cos(a)*width[ring],Mathf.Sin(a)*height[ring],z[ring]));uv.Add(new Vector2(side/12f,ring/(float)(z.Length-1)));}
            for(int ring=0;ring<z.Length-1;ring++)for(int side=0;side<12;side++){int a=ring*13+side,b=a+13;indices.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
            mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();string path=Root+"/QuarantineBag.asset";var stored=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(stored==null){AssetDatabase.CreateAsset(mesh,path);stored=mesh;}else{EditorUtility.CopySerialized(mesh,stored);Object.DestroyImmediate(mesh);}
            var bag=new GameObject("Sealed textile bag",typeof(MeshFilter),typeof(MeshRenderer));bag.transform.SetParent(root,false);bag.GetComponent<MeshFilter>().sharedMesh=stored;
            var cloth=Material("Body bag canvas",new Color(.24f,.24f,.17f));cloth.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/WornCloth_v1.png"));bag.GetComponent<MeshRenderer>().sharedMaterial=cloth;
            var frame=Box("Stretcher tray",center+Vector3.down*.17f,new Vector3(.69f,.055f,2.1f),Material("Stretcher steel",new Color(.24f,.25f,.23f)),true);frame.transform.SetParent(root,true);
            foreach(int x in new[]{-1,1})foreach(int zed in new[]{-1,1}){var leg=Box("Stretcher folding leg",center+new Vector3(x*.26f,-.28f,zed*.65f),new Vector3(.035f,.28f,.035f),Material("Stretcher steel",new Color(.24f,.25f,.23f)),false);leg.transform.SetParent(root,true);}
            for(int sign=-1;sign<=1;sign+=2){var rail=Box("Carrying rail",center+Vector3.right*sign*.36f+Vector3.down*.12f,new Vector3(.04f,.04f,2.35f),Material("Stretcher steel",new Color(.24f,.25f,.23f)),false);rail.transform.SetParent(root,true);}
        }
        internal static GameObject Box(string name,Vector3 center,Vector3 size,Material material,bool solid)
        {
            var box=GameObject.CreatePrimitive(PrimitiveType.Cube);box.name=name;box.transform.SetParent(art);box.transform.position=center;box.transform.localScale=size;box.GetComponent<Renderer>().sharedMaterial=material;if(!solid)Object.DestroyImmediate(box.GetComponent<Collider>());return box;
        }
        internal static Material Material(string name,Color tint,string shader="Abisal/Retro Surface")
        {
            string path=Root+"/Materials/"+name.Replace(' ','_')+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null){mat=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(mat,path);}mat.SetColor("_BaseColor",tint);mat.enableInstancing=true;EditorUtility.SetDirty(mat);return mat;
        }
        internal static GameObject Imported(string path,Vector3 center,Vector3 size,Transform parent,float yaw,bool solid)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(source==null)throw new InvalidOperationException("Missing licensed source: "+path);
            var wrapper=new GameObject("Industrial / "+source.name);var model=Object.Instantiate(source);model.transform.SetParent(wrapper.transform,false);model.transform.localRotation=Quaternion.Euler(0,yaw,0);
            foreach(var c in model.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);foreach(var script in model.GetComponentsInChildren<MonoBehaviour>())Object.DestroyImmediate(script);
            var renderers=model.GetComponentsInChildren<Renderer>();Bounds bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);model.transform.localPosition-=bounds.center;
            wrapper.transform.localScale=new Vector3(size.x/Mathf.Max(.01f,bounds.size.x),size.y/Mathf.Max(.01f,bounds.size.y),size.z/Mathf.Max(.01f,bounds.size.z));wrapper.transform.SetParent(parent,true);wrapper.transform.position=center;
            foreach(var r in renderers)r.sharedMaterials=r.sharedMaterials.Select(Convert).ToArray();
            if(solid){var c=wrapper.AddComponent<BoxCollider>();var scale=wrapper.transform.lossyScale;c.size=new Vector3(size.x/Mathf.Abs(scale.x),size.y/Mathf.Abs(scale.y),size.z/Mathf.Abs(scale.z));}
            if(size.y>1.6f)wrapper.AddComponent<MapOccluder>();return wrapper;
        }
        private static Material Convert(Material source)
        {
            if(source==null)return Material("Missing source steel",Color.gray);if(converted.TryGetValue(source,out var found))return found;
            string guid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source));var result=Material("Industrial source "+guid,Color.white);
            Texture texture=source.HasProperty("_BaseMap")?source.GetTexture("_BaseMap"):source.HasProperty("_MainTex")?source.GetTexture("_MainTex"):null;
            if(texture!=null)result.SetTexture("_BaseMap",texture);
            Color tint=source.HasProperty("_BaseColor")?source.GetColor("_BaseColor"):source.HasProperty("_Color")?source.GetColor("_Color"):Color.white;
            result.SetColor("_BaseColor",tint*new Color(.74f,.72f,.64f,1));converted[source]=result;EditorUtility.SetDirty(result);return result;
        }
    }
}
