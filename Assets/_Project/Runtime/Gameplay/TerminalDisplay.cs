using UnityEngine;
using UnityEngine.UI;

namespace Zombtoy.Gameplay
{
    // A diegetic readout of the actual mission state, rendered with the 3D world.
    public sealed class TerminalDisplay : MonoBehaviour
    {
        public SandboxSession session;
        private Text state;
        private Image lamp;
        private string previous;
        private void Start()
        {
            var go=new GameObject("Phosphor operating surface",typeof(RectTransform),typeof(Canvas));
            go.transform.SetParent(transform,false);go.transform.localPosition=new Vector3(0,0,-.352f);go.transform.localScale=Vector3.one*.0036f;
            var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=session.player.view;
            go.GetComponent<RectTransform>().sizeDelta=new Vector2(320,180);
            var background=Node("Worn phosphor glass",go.transform,Vector2.zero,new Vector2(320,180));background.gameObject.AddComponent<Image>().color=new Color(.012f,.032f,.019f);
            var label=Node("Boarding control state",go.transform,new Vector2(-12,0),new Vector2(280,160));state=label.gameObject.AddComponent<Text>();state.font=session.hudFont;state.fontSize=36;state.lineSpacing=.9f;state.alignment=TextAnchor.MiddleCenter;
            lamp=Node("Network status lamp",go.transform,new Vector2(145,64),new Vector2(8,8)).gameObject.AddComponent<Image>();
            for(int i=0;i<5;i++)Node("Signal trace",go.transform,new Vector2(144,-20+i*12),new Vector2(3,5+i*3)).gameObject.AddComponent<Image>().color=new Color(.12f,.29f,.17f);
            Refresh();
        }
        private static RectTransform Node(string name,Transform parent,Vector2 position,Vector2 size)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);rect.anchoredPosition=position;rect.sizeDelta=size;return rect;
        }
        private void Update(){if(state!=null)Refresh();}
        private void Refresh()
        {
            string value=!session.portMission.PowerOn?"B-07\nSIN RED\nBLOQUEO":session.portMission.BoardingAuthorized?"B-07\nACCESO\nABIERTO":"B-07\nRED OK\nCONTROL";
            if(value==previous)return;previous=value;state.text=value;state.color=session.portMission.PowerOn?new Color(.69f,.88f,.57f):new Color(.88f,.59f,.24f);lamp.color=state.color;
        }
    }
}
