using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Zombtoy.Gameplay;
using Object=UnityEngine.Object;

namespace Zombtoy.Editor
{
    public static class InteriorRebuild
    {
        private const string Warehouse="Assets/SciFi Warehouse Kit/Prefabs/";
        private const string Rusty="Assets/Data/Sci-fi_assets/Sci-Fi Old rusty props PBR/Prefabs/";
        private static Transform root;
        private static Material steel,deck,enamel;
        public static void Build(SandboxSession session,Transform art)
        {
            root=art;
            steel=IndustrialArtRebuild.Material("Interior structural steel",new Color(.21f,.23f,.21f));
            enamel=IndustrialArtRebuild.Material("Interior worn cream",new Color(.67f,.62f,.47f));
            deck=IndustrialArtRebuild.Material("Painted service deck",new Color(.36f,.35f,.28f));
            var tex=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Industrial/Textures/AgedEnamel_v1.png");
            foreach(var mat in new[]{steel,deck,enamel}){mat.SetTexture("_BaseMap",tex);mat.SetTextureScale("_BaseMap",new Vector2(1.2f,1.2f));EditorUtility.SetDirty(mat);}
            // The original outer building remains real collision. Recompose occupied rooms inside it.
            foreach(var t in Object.FindObjectsByType<Transform>().ToArray())
            {
                if(t==null)continue;
                if(t.name.StartsWith("Warehouse ")||t.name.StartsWith("Control north")||t.name.StartsWith("Control east")||t.name.StartsWith("Control cut west")||t.name.StartsWith("Control entry wall")||t.name.StartsWith("Airlock "))
                    foreach(var renderer in t.GetComponentsInChildren<Renderer>())renderer.enabled=false;
                if(t.name=="Storage crate"||t.name=="3D Shelves Empty"||t.name=="Operator console cabinet"||t.name=="Cabinet handle")Object.DestroyImmediate(t.gameObject);
            }
            foreach(var t in art.GetComponentsInChildren<Transform>().ToArray())
            {
                if(t==null)continue;
                if(t.name.StartsWith("Industrial / Shelf Variation")||t.name=="Warehouse upper beam")Object.DestroyImmediate(t.gameObject);
                else if(t.name=="Industrial / Old_computer"&&t.position.z>20)Object.DestroyImmediate(t.gameObject);
            }
            var arrivalNear=new List<Renderer>();
            Wall(new Vector3(-27.6f,1.4f,-15),new Vector3(.24f,2.8f,8),90,arrivalNear);
            Wall(new Vector3(-23.8f,1.4f,-19),new Vector3(7.6f,2.8f,.24f),0,arrivalNear);
            Wall(new Vector3(-26.4f,1.4f,-11),new Vector3(2.4f,2.8f,.24f),0);
            Wall(new Vector3(-21.4f,1.4f,-11),new Vector3(2.8f,2.8f,.24f),0);
            Wall(new Vector3(-20,1.4f,-17),new Vector3(.24f,2.8f,4),90);
            Wall(new Vector3(-20,1.4f,-11.6f),new Vector3(.24f,2.8f,1.2f),90);
            foreach(var renderer in art.GetComponentsInChildren<Renderer>())
                if(renderer.transform.position.z<-18.4f&&renderer.transform.position.z>-19.3f&&renderer.transform.position.x>-28&&renderer.transform.position.x<-19&&renderer.transform.position.y>1.5f)arrivalNear.Add(renderer);
            Cutaway(session,"Quarantine cutaway",new Vector2(-28,-19.4f),new Vector2(-19.4f,-10.5f),arrivalNear);
            Deck("Decontamination deck",new Vector2(-27.4f,-18.8f),new Vector2(-20.2f,-11.2f));
            // North wall and a paired bench make the arrival a functional decontamination station.
            Prop(Rusty+"Electroboxes.prefab",new Vector3(-26.1f,1.15f,-11.25f),new Vector3(1.1f,1.4f,.30f),0,true);
            Prop(Rusty+"Oxy_ballon.prefab",new Vector3(-21.2f,.85f,-12.2f),new Vector3(.42f,1.7f,.42f),0,true);
            Bench(new Vector3(-21.4f,.42f,-17.6f),new Vector3(1.8f,.84f,.70f));
            Prop(Rusty+"Supply_box.prefab",new Vector3(-21.6f,.98f,-17.6f),new Vector3(.65f,.28f,.45f),0,false);
            WorkLight(new Vector3(-24.5f,1.85f,-13),6.5f,8,new Color(1,.80f,.58f));

            var warehouseNear=new List<Renderer>();
            Wall(new Vector3(-25.8f,1.4f,14),new Vector3(.24f,2.8f,9),90,warehouseNear);
            Wall(new Vector3(-24.5f,1.4f,9),new Vector3(3,2.8f,.24f),0,warehouseNear);
            Wall(new Vector3(-15.5f,1.4f,9),new Vector3(3,2.8f,.24f),0,warehouseNear);
            Wall(new Vector3(-20,1.4f,18.7f),new Vector3(12,2.8f,.24f),0);
            Wall(new Vector3(-14,1.4f,14),new Vector3(.24f,2.8f,9),90);
            Cutaway(session,"Stores cutaway",new Vector2(-26,8.6f),new Vector2(-13.5f,19),warehouseNear);
            Deck("Stores deck",new Vector2(-25.7f,9.2f),new Vector2(-14.2f,18.5f));
            Bench(new Vector3(-22,.45f,18.0f),new Vector3(4.1f,.9f,.9f));
            Prop(Warehouse+"Props/Shelves/Shelf Variation 03.prefab",new Vector3(-24.9f,1.2f,15.6f),new Vector3(1.0f,2.4f,3.3f),0,true);
            Prop(Warehouse+"Props/Shelves/Shelf Variation 02.prefab",new Vector3(-17.2f,1.2f,18),new Vector3(3.0f,2.4f,.75f),90,true);
            Prop(Rusty+"Electroboxes.prefab",new Vector3(-20.3f,1.65f,18.45f),new Vector3(.75f,.9f,.2f),0,false);
            Prop(Warehouse+"Props/Crates Barrels Pallets/Crate Short.prefab",new Vector3(-23.6f,1.12f,18),new Vector3(.6f,.35f,.55f),0,false);
            Prop(Rusty+"Supply_box.prefab",new Vector3(-20.7f,1.06f,18),new Vector3(.6f,.22f,.45f),0,false);
            WorkLight(new Vector3(-22.1f,1.9f,17),7.2f,7.5f,new Color(1,.84f,.61f));

            var controlNear=new List<Renderer>();
            Wall(new Vector3(14,1.4f,15.5f),new Vector3(.24f,2.8f,9),90,controlNear);
            Wall(new Vector3(15.5f,1.4f,11),new Vector3(3,2.8f,.24f),0,controlNear);
            Wall(new Vector3(22.5f,1.4f,11),new Vector3(5,2.8f,.24f),0,controlNear);
            Wall(new Vector3(19.5f,1.4f,20),new Vector3(11,2.8f,.24f),0);
            Wall(new Vector3(25,1.4f,15.5f),new Vector3(.24f,2.8f,9),90);
            Cutaway(session,"Operations cutaway",new Vector2(13.7f,10.7f),new Vector2(25.3f,20.3f),controlNear);
            Deck("Operations deck",new Vector2(14.2f,11.2f),new Vector2(24.8f,19.8f));
            Bench(new Vector3(18.2f,.45f,19.2f),new Vector3(6.3f,.9f,.9f));
            for(int i=0;i<2;i++)
            {
                Prop(Rusty+"Old_computer.prefab",new Vector3(16.3f+i*3,.47f,19.1f),new Vector3(.45f,.85f,.6f),180,false);
                ControlConsoleBuilder.Build(root,new Vector3(16.3f+i*3,1.5f,19.1f),45,session);
            }
            foreach(var child in session.terminal.Cast<Transform>().ToArray())if(child.name.StartsWith("3D ")||child.name.StartsWith("Industrial /"))Object.DestroyImmediate(child.gameObject);
            Bench(new Vector3(22,.45f,18.1f),new Vector3(1.7f,.9f,.9f));
            ControlConsoleBuilder.Build(session.terminal,new Vector3(22,1.5f,18.1f),45,session);
            Prop(Rusty+"Electroboxes.prefab",new Vector3(24.7f,1.5f,17),new Vector3(.3f,1.65f,2),90,false);
            Prop(Rusty+"Vents.prefab",new Vector3(21.4f,2.25f,19.8f),new Vector3(1.6f,.7f,.2f),0,false);
            WorkLight(new Vector3(21,1.9f,17.8f),7.0f,8,new Color(.86f,.92f,.79f));
        }
        private static void Deck(string name,Vector2 min,Vector2 max)
        {
            var floor=IndustrialArtRebuild.Box(name,new Vector3((min.x+max.x)*.5f,.042f,(min.y+max.y)*.5f),new Vector3(max.x-min.x,.018f,max.y-min.y),deck,false);
            // Long welded seams instead of the old exposed square platform tiles.
            for(float x=min.x+1.8f;x<max.x;x+=1.8f)IndustrialArtRebuild.Box("Welded deck seam",new Vector3(x,.054f,(min.y+max.y)*.5f),new Vector3(.018f,.006f,max.y-min.y),steel,false);
        }
        private static void Bench(Vector3 center,Vector3 size)
        {
            IndustrialArtRebuild.Box("Service bench surface",center+Vector3.up*size.y*.5f,new Vector3(size.x,.055f,size.z),enamel,true);
            foreach(int sign in new[]{-1,1})IndustrialArtRebuild.Box("Service bench end",center+Vector3.right*sign*(size.x*.5f-.08f),new Vector3(.08f,size.y,size.z),steel,true);
            IndustrialArtRebuild.Box("Service bench lower shelf",center+Vector3.down*size.y*.32f,new Vector3(size.x,.035f,size.z*.9f),steel,false);
        }
        private static void Wall(Vector3 center,Vector3 size,float yaw,List<Renderer> near=null)
        {
            bool x=size.x>size.z;float length=x?size.x:size.z;int count=Mathf.CeilToInt(length/2.2f);
            for(int i=0;i<count;i++)
            {
                var p=center+(x?Vector3.right:Vector3.forward)*(((i+.5f)/count-.5f)*length);
                var go=Prop(Warehouse+"Structures/Walls/Wall Plain.prefab",p,x?new Vector3(length/count,size.y,size.z):new Vector3(size.x,size.y,length/count),x?90:0,true);
                if(near!=null)near.AddRange(go.GetComponentsInChildren<Renderer>());
            }
        }
        private static GameObject Prop(string path,Vector3 center,Vector3 size,float yaw,bool solid)=>IndustrialArtRebuild.Imported(path,center,size,root,yaw,solid);
        private static void Cutaway(SandboxSession session,string name,Vector2 min,Vector2 max,List<Renderer> parts)
        {
            var room=new GameObject(name).AddComponent<InteriorCutaway>();room.transform.SetParent(root);room.actor=session.player.transform;room.min=min;room.max=max;room.nearShell=parts.Distinct().ToArray();
        }
        private static void WorkLight(Vector3 p,float intensity,float range,Color color)
        {
            Prop(Warehouse+"Props/Misc Props/Wall Light.prefab",p+Vector3.up*.6f,new Vector3(.6f,.22f,.25f),0,false);
            var light=new GameObject("Local fluorescent wall bounce").AddComponent<Light>();light.transform.SetParent(root);light.transform.position=p;light.type=LightType.Point;light.range=range;light.intensity=intensity;light.color=color;
        }
    }
}
