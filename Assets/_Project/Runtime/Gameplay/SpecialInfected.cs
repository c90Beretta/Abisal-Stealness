using UnityEngine;
namespace Zombtoy.Gameplay
{
    public enum SpecialSpecies { Hound, Bloater, Colossus }
    [DefaultExecutionOrder(160)]
    public sealed class SpecialInfected : MonoBehaviour
    {
        public SpecialSpecies Species {get;private set;}
        private SandboxSession session;
        private SandboxEnemy enemy;
        private bool dead;
        private float nextAbility, nextVoice, chargeUntil;
        private Transform dog;
        private Transform[] legs;
        private Renderer[] humanParts;
        public void Initialize(SandboxSession value,SpecialSpecies species)
        {
            session=value;enemy=GetComponent<SandboxEnemy>();Species=species;
            var definition=Instantiate(enemy.definition); enemy.definition=definition;
            definition.displayName=species==SpecialSpecies.Hound?"Perro infectado":species==SpecialSpecies.Bloater?"Hinchado":"Coloso";
            definition.health=species==SpecialSpecies.Hound?55:species==SpecialSpecies.Bloater?110:380;
            definition.speed=species==SpecialSpecies.Hound?3.3f:species==SpecialSpecies.Bloater?.8f:1.05f;
            definition.damage=species==SpecialSpecies.Hound?15:species==SpecialSpecies.Bloater?18:42;
            definition.attackWindup=species==SpecialSpecies.Colossus?.9f:.45f;
            enemy.health=definition.health;enemy.speed=definition.speed;
            if(species==SpecialSpecies.Hound)BuildDog();
            else {var visual=transform.Find("Character visual");if(visual!=null)visual.localScale=species==SpecialSpecies.Colossus?new Vector3(1.55f,1.35f,1.5f):new Vector3(1.5f,1,1.45f);}
            nextAbility=Time.time+5;
        }
        private void BuildDog()
        {
            humanParts=GetComponentsInChildren<Renderer>();
            foreach(var renderer in humanParts)renderer.enabled=false;
            var controller=GetComponent<CharacterController>();controller.height=1;controller.center=Vector3.up*.5f;controller.radius=.3f;
            dog=new GameObject("Original infected hound").transform;dog.SetParent(transform,false);
            var material=session.tracerMaterial;
            Part("Ribcage",new Vector3(0,.58f,0),new Vector3(.38f,.38f,.85f),material);
            Part("Skull",new Vector3(0,.64f,.53f),new Vector3(.3f,.3f,.36f),material);
            Part("Jaw",new Vector3(0,.51f,.72f),new Vector3(.22f,.09f,.28f),material);
            legs=new Transform[4];for(int i=0;i<4;i++)legs[i]=Part("Leg",new Vector3(i%2==0?-.18f:.18f,.25f,i<2?.3f:-.3f),new Vector3(.09f,.5f,.12f),material);
            Part("Tail",new Vector3(0,.62f,-.6f),new Vector3(.06f,.07f,.42f),material);
        }
        private Transform Part(string name,Vector3 pos,Vector3 scale,Material material)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Capsule);obj.name=name;obj.transform.SetParent(dog,false);obj.transform.localPosition=pos;obj.transform.localScale=scale;
            obj.GetComponent<Collider>().enabled=false;Destroy(obj.GetComponent<Collider>());
            var renderer=obj.GetComponent<Renderer>();renderer.sharedMaterial=material;var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",new Color(.23f,.16f,.13f));block.SetColor("_Color",new Color(.23f,.16f,.13f));renderer.SetPropertyBlock(block);return obj.transform;
        }
        private void LateUpdate()
        {
            if(dog==null)return;
            foreach(var part in humanParts)if(part!=null)part.forceRenderingOff=true;
            bool visible=session.Night==null||session.Night.Perceives(enemy.IsDead?enemy.CorpseAnchor:transform.position+Vector3.up*.6f);
            foreach(var part in dog.GetComponentsInChildren<Renderer>())part.enabled=visible;
        }
        private bool ClearToPlayer()
        {
            return Physics.Linecast(transform.position+Vector3.up,session.player.transform.position+Vector3.up,out var hit,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore)&&hit.collider.GetComponentInParent<SandboxPlayer>()==session.player;
        }
        private void Update()
        {
            if(session==null||!session.Running)return;
            if(enemy.IsDead)
            {
                if(dog!=null){dog.position=enemy.CorpseAnchor;dog.rotation=Quaternion.Euler(0,transform.eulerAngles.y,85);}
                if(!dead){dead=true;if(Species==SpecialSpecies.Bloater){session.EmitNoise(transform.position,28);CombatEffects.Blood(transform.position+Vector3.up,session.tracerMaterial);TacticalAudio.Play(transform.position,TacticalCue.Burst);if(Vector3.Distance(transform.position,session.player.transform.position)<4&&ClearToPlayer())session.TakeDamage(24,false);}}
                return;
            }
            dead=false;
            if(legs!=null)for(int i=0;i<4;i++)legs[i].localRotation=Quaternion.Euler(enemy.Alerted?Mathf.Sin(Time.time*14+(i%3)*Mathf.PI)*32:0,0,0);
            if(enemy.Alerted&&Time.time>nextVoice){nextVoice=Time.time+Random.Range(4,7);TacticalAudio.Play(transform.position,TacticalCue.Growl,Species==SpecialSpecies.Hound?.7f:1.3f);}
            if(Species==SpecialSpecies.Colossus)
            {
                enemy.speed=Time.time<chargeUntil?2.8f:1.05f;
                if(enemy.Alerted&&Time.time>=nextAbility&&ClearToPlayer()) {nextAbility=Time.time+12;chargeUntil=Time.time+2.5f;session.EmitNoise(transform.position,22);TacticalAudio.Play(transform.position,TacticalCue.Growl,1.4f);}
                return;
            }
            if(Species!=SpecialSpecies.Bloater||Time.time<nextAbility||!enemy.Alerted)return;
            float distance=Vector3.Distance(transform.position,session.player.transform.position);
            if(distance>6||distance<2||!ClearToPlayer())return;
            nextAbility=Time.time+8;enemy.Stagger(1);session.EmitNoise(transform.position,18);
            CombatEffects.Blood(transform.position+transform.forward+Vector3.up,session.tracerMaterial);TacticalAudio.Play(transform.position,TacticalCue.Burst);
            session.TakeDamage(10,false);session.SetMessage("Salpicadura del hinchado: rompe su linea de vision.");
        }
    }
}
