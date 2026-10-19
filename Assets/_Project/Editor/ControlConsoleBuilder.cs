using UnityEngine;
using Zombtoy.Gameplay;

namespace Zombtoy.Editor
{
    public static class ControlConsoleBuilder
    {
        public static void Build(Transform parent,Vector3 position,float yaw,SandboxSession session)
        {
            var root=new GameObject("Operating CRT and keyboard").transform;root.SetParent(parent);root.position=position;root.rotation=Quaternion.Euler(0,yaw,0);
            var body=IndustrialArtRebuild.Material("CRT worn case",new Color(.24f,.25f,.20f));
            var bezel=IndustrialArtRebuild.Material("CRT dark bezel",new Color(.045f,.06f,.045f));
            var keys=IndustrialArtRebuild.Material("Keyboard raised keys",new Color(.22f,.24f,.20f));
            Part(root,"Monitor shell",new Vector3(0,0,.08f),new Vector3(1.35f,.85f,.58f),body);
            Part(root,"Rear tube housing",new Vector3(0,.02f,.41f),new Vector3(1.08f,.65f,.22f),body);
            foreach(int side in new[]{-1,1})
            {
                Part(root,"Glass frame vertical",new Vector3(side*.627f,0,-.32f),new Vector3(.09f,.81f,.10f),bezel);
                Part(root,"Glass frame horizontal",new Vector3(0,side*.37f,-.32f),new Vector3(1.22f,.085f,.10f),bezel);
            }
            Part(root,"Monitor neck",new Vector3(0,-.47f,.08f),new Vector3(.18f,.25f,.20f),bezel);
            Part(root,"Monitor foot",new Vector3(0,-.58f,.08f),new Vector3(.60f,.045f,.45f),body);
            for(int i=0;i<3;i++)Part(root,"Bezel switch",new Vector3(.33f+i*.08f,-.385f,-.385f),new Vector3(.045f,.022f,.025f),keys);
            Part(root,"Keyboard case",new Vector3(0,-.51f,-.77f),new Vector3(1.05f,.045f,.34f),body);
            for(int row=0;row<4;row++)for(int key=0;key<11;key++)Part(root,"Mechanical key",new Vector3(-.45f+key*.085f,-.475f,-.86f+row*.055f),new Vector3(.058f,.022f,.036f),keys);
            var display=root.gameObject.AddComponent<TerminalDisplay>();display.session=session;
        }
        private static void Part(Transform parent,string name,Vector3 center,Vector3 size,Material material)
        {
            var go=IndustrialArtRebuild.Box(name,parent.TransformPoint(center),size,material,false);go.transform.SetParent(parent,true);go.transform.localRotation=Quaternion.identity;
        }
    }
}
