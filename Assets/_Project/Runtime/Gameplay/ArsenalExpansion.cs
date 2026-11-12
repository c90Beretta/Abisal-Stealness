using UnityEngine;
using UnityEngine.UI;
using Zombtoy.Core;

namespace Zombtoy.Gameplay
{
    // Extends the existing scene without duplicating encounter or checkpoint identities.
    public sealed class ArsenalExpansion : MonoBehaviour
    {
        private SandboxSession session;
        private GameObject panel;
        private Text summary;
        private int role, upgrade, selected;
        private bool applied;
        private readonly System.Collections.Generic.Dictionary<Button,Text> captions=new System.Collections.Generic.Dictionary<Button,Text>();
        public void UseCheckpoint() { applied=true; }
        private readonly Button[] roleButtons=new Button[3],weaponButtons=new Button[6],kitButtons=new Button[3];
        private readonly string[] roles = { "Asalto", "Explorador", "Sanitario" };
        private readonly string[] kits = { "Placa reforzada", "Suministros medicos", "Reserva de municion" };
        public void Initialize(SandboxSession value)
        {
            session = value;
            var loadout = session.player.GetComponent<WeaponLoadout>();
            if (loadout == null || loadout.weapons.Length < 3) return;
            var originals = loadout.weapons;
            loadout.weapons = new WeaponDefinition[6];
            for (int i = 0; i < 3; i++) loadout.weapons[i] = originals[i];
            CreateWeapon(loadout, 3, originals[1], "Fusil de precision", 110, 1.2f, .15f, 1);
            CreateWeapon(loadout, 4, originals[1], "Subfusil", 22, .085f, 3, 1);
            CreateWeapon(loadout, 5, originals[1], "Ametralladora pesada", 48, .13f, 4, 1);
            int special=0;
            var encounters=(SandboxEnemy[])session.Enemies.Clone();
            System.Array.Sort(encounters,(a,b)=>string.CompareOrdinal(a.name,b.name));
            foreach(var enemy in encounters)
            {
                enemy.gameObject.AddComponent<AttackPose>();
                if(special<3 && enemy.gameObject.activeInHierarchy) enemy.gameObject.AddComponent<SpecialInfected>().Initialize(session,(SpecialSpecies)special++);
            }
            session.player.gameObject.AddComponent<AttackPose>();
            BuildMenu();
            TacticalAudio.Ambience(gameObject);
        }
        private void CreateWeapon(WeaponLoadout loadout, int index, WeaponDefinition basis, string title, float damage, float interval, float spread, int pellets)
        {
            var weapon = Instantiate(basis); weapon.displayName = title; weapon.damage = damage;
            weapon.range = index == 3 ? 65 : 25; weapon.fireInterval = interval; weapon.spread = spread; weapon.pellets = pellets; weapon.noiseRadius = index == 4 ? 16 : 30;
            var model = Instantiate(basis.visualPrefab); model.name = title + " template"; model.SetActive(false); model.transform.SetParent(transform);
            // Individual silhouettes, using the project's original weapon geometry.
            model.transform.localScale = index == 3 ? new Vector3(.85f, .85f, 1.6f) : index == 4 ? Vector3.one * .72f : new Vector3(1.25f, 1.2f, 1.3f);
            weapon.visualPrefab = model; loadout.weapons[index] = weapon;
        }
        private void BuildMenu()
        {
            var canvas = new GameObject("Deployment armory", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.transform.SetParent(transform); canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.GetComponent<Canvas>().sortingOrder = 101;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280,720); scaler.matchWidthOrHeight = .5f;
            panel = new GameObject("Weapon locker", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(canvas.transform,false);
            var r = panel.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = new Vector2(1,.5f); r.pivot = new Vector2(1,.5f); r.anchoredPosition = new Vector2(-35,0); r.sizeDelta = new Vector2(650,640);
            panel.GetComponent<Image>().color = new Color(.025f,.023f,.019f,.98f);
            Label("PREPARAR DESPLIEGUE", 28, 24, 18, 600, 42);
            Label("PERSONAL / MISMO SOLDADO, DISTINTA ESPECIALIDAD", 17,24,70,600,28);
            for (int i=0;i<3;i++) { int n=i; roleButtons[i]=Button(roles[i],24+i*202,106,194,42,()=>role=n); }
            Label("ARMARIO / ARMA INICIAL",20,24,168,600,32);
            var names = new[]{"1  Arma corta","2  Fusil de asalto","3  Escopeta","4  Precision","5  Subfusil","6  Arma pesada"};
            for (int i=0;i<6;i++) { int n=i; weaponButtons[i]=Button(names[i],24+(i%2)*302,210+(i/2)*52,290,44,()=>selected=n); }
            Label("EQUIPO / UN KIT POR SALIDA",20,24,382,600,30);
            for(int i=0;i<3;i++) {int n=i;kitButtons[i]=Button(kits[i],24+i*202,424,194,46,()=>upgrade=n);}
            summary=Label("",20,24,490,600,90);
            Label("ENTER / COMENZAR EN EL PANEL IZQUIERDO",17,24,592,600,28);
        }
        private Text Label(string text,int size,float x,float y,float w,float h)
        {
            var obj=new GameObject(text,typeof(RectTransform),typeof(Text));obj.transform.SetParent(panel.transform,false);
            var r=obj.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);
            var t=obj.GetComponent<Text>();t.text=text;t.font=session.hudFont;t.fontSize=size;t.color=new Color(.88f,.82f,.64f);t.raycastTarget=false;return t;
        }
        private Button Button(string text,float x,float y,float w,float h,UnityEngine.Events.UnityAction click)
        {
            var obj=new GameObject(text,typeof(RectTransform),typeof(Image),typeof(Button));obj.transform.SetParent(panel.transform,false);
            var r=obj.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);
            var image=obj.GetComponent<Image>();image.color=new Color(.12f,.115f,.09f);
            var child=Label(text,19,x,y,w,h);child.alignment=TextAnchor.MiddleCenter;
            var button=obj.GetComponent<Button>();button.targetGraphic=image;button.onClick.AddListener(click);captions[button]=child;var outline=obj.AddComponent<Outline>();outline.effectColor=new Color(.95f,.9f,.73f);outline.effectDistance=new Vector2(2,2);return button;
        }
        private void Mark(Button[] buttons,int selected)
        {
            for(int i=0;i<buttons.Length;i++)
            {
                buttons[i].GetComponent<Image>().color=i==selected?new Color(.38f,.32f,.18f):new Color(.12f,.115f,.09f);
                captions[buttons[i]].text=(i==selected?"[X] ":"[ ] ")+buttons[i].name;
                buttons[i].GetComponent<Outline>().enabled=UnityEngine.EventSystems.EventSystem.current!=null&&UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject==buttons[i].gameObject;
            }
        }
        private void Update()
        {
            if(session==null || panel==null)return;
            panel.SetActive(session.Mode==SessionMode.Title && !session.Hud.ShowingHelp);
            if(panel.activeSelf)
            {
                Mark(roleButtons,role);Mark(weaponButtons,selected);Mark(kitButtons,upgrade);
                var benefit=new[]{"Dotacion base equilibrada","+1 pila de linterna","+1 botiquin"};
                var kitBenefit=new[]{"+1 placa de repuesto","+2 vendas","+24 cartuchos de reserva"};
                summary.text=$"{roles[role]}: {benefit[role]}\n{session.player.GetComponent<WeaponLoadout>().weapons[selected].displayName} / municion compartida\n{kits[upgrade]}: {kitBenefit[upgrade]}\n1-6 cambian arma. Espacio corta / V empuja.";
            }
            if(!applied && session.Running)
            {
                applied=true;
                // Loading a checkpoint keeps its resources and weapon; only a new sortie gets a kit.
                if(session.State.elapsed>0.2f)return;
                var loadout=session.player.GetComponent<WeaponLoadout>(); loadout.Equip(selected,true);
                if(role==2)session.State.medkits=Mathf.Min(8,session.State.medkits+1);
                if(role==1 && session.Night!=null)session.Night.Inventory.cells=Mathf.Min(8,session.Night.Inventory.cells+1);
                if(upgrade==1)session.State.bandages=Mathf.Min(12,session.State.bandages+2);
                if(upgrade==2)session.reserve=Mathf.Min(240,session.reserve+24);
                if(upgrade==0 && session.Night!=null)session.Night.Inventory.plates=Mathf.Min(4,session.Night.Inventory.plates+1);
                session.SetMessage(roles[role]+" equipado. Espacio: corte / V: empujon / 1-6: arsenal.");
            }
        }
    }
}
