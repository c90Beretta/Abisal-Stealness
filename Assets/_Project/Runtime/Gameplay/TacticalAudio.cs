using UnityEngine;
namespace Zombtoy.Gameplay
{
    public enum TacticalCue { Swing, Impact, Gunshot, Growl, Burst }
    public static class TacticalAudio
    {
        private static readonly AudioClip[] clips=new AudioClip[5];
        public static void Ambience(GameObject owner)
        {
            const int rate=22050;var data=new float[rate*4];
            for(int i=0;i<data.Length;i++){float t=(float)i/rate;data[i]=(.5f*Mathf.Sin(t*50*Mathf.PI*2)+.2f*Mathf.Sin(t*75*Mathf.PI*2))*.08f;}
            var clip=AudioClip.Create("Original ventilation loop",data.Length,1,rate,false);clip.SetData(data,0);
            var source=owner.AddComponent<AudioSource>();source.clip=clip;source.loop=true;source.volume=.22f;source.spatialBlend=0;source.Play();
        }
        public static void Play(Vector3 position,TacticalCue cue,float tone=1)
        {
            int index=(int)cue;
            if(clips[index]==null)
            {
                const int rate=22050;float duration=cue==TacticalCue.Growl?.65f:.22f;var samples=new float[(int)(rate*duration)];
                var random=new System.Random(790+index);float filtered=0;
                for(int i=0;i<samples.Length;i++)
                {
                    float t=(float)i/rate;filtered=Mathf.Lerp(filtered,(float)random.NextDouble()*2-1,cue==TacticalCue.Growl?.08f:.65f);
                    float wave=Mathf.Sin(t*(cue==TacticalCue.Growl?85:cue==TacticalCue.Impact?150:65)*Mathf.PI*2);
                    samples[i]=(filtered*.7f+wave*.3f)*Mathf.Exp(-t*(cue==TacticalCue.Growl?5:25))*.55f;
                }
                clips[index]=AudioClip.Create("Original synthesized "+cue,samples.Length,1,rate,false);clips[index].SetData(samples,0);
            }
            var obj=new GameObject("Spatial "+cue);obj.transform.position=position;var source=obj.AddComponent<AudioSource>();
            source.clip=clips[index];source.spatialBlend=.85f;source.minDistance=2;source.maxDistance=22;source.rolloffMode=AudioRolloffMode.Linear;source.dopplerLevel=0;source.volume=.55f;
            var listener=Object.FindAnyObjectByType<AudioListener>();
            if(listener!=null && Physics.Linecast(position+Vector3.up*.5f,listener.transform.position,out var hit,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore) && hit.collider.GetComponentInParent<SandboxPlayer>()==null)source.volume*=.4f;source.pitch=Mathf.Clamp(1/tone,.65f,1.5f);source.Play();Object.Destroy(obj,2);
        }
    }
}
