using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

namespace Zombtoy.Gameplay
{
    public sealed class RetroRenderFeature : ScriptableRendererFeature
    {
        public Material material;
        private RetroPass pass;
        public override void Create() => pass=new RetroPass {renderPassEvent=RenderPassEvent.AfterRenderingPostProcessing,requiresIntermediateTexture=true};
        public override void AddRenderPasses(ScriptableRenderer renderer,ref RenderingData data)
        {
            if(material==null||PresentationOptions.ReducedEffects||data.cameraData.camera.GetComponent<IsometricView>()==null)return;
            pass.material=material;renderer.EnqueuePass(pass);
        }
        private sealed class RetroPass : ScriptableRenderPass
        {
            public Material material;
            public override void RecordRenderGraph(RenderGraph graph,ContextContainer frame)
            {
                var resources=frame.Get<UniversalResourceData>();if(resources.isActiveTargetBackBuffer)return;
                var source=resources.activeColorTexture;
                var full=graph.GetTextureDesc(source);full.name="Retro world output";full.clearBuffer=false;
                var low=full;low.width=Mathf.Max(1,Mathf.RoundToInt(270f*full.width/full.height));low.height=270;low.name="PS1 world 270p";low.filterMode=FilterMode.Point;
                var lowTexture=graph.CreateTexture(low);var destination=graph.CreateTexture(full);
                graph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(source,lowTexture,material,0),passName:"PS1 ordered dither and colour depth");
                graph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(lowTexture,destination,material,1),passName:"PS1 point upscale (HUD excluded)");
                resources.cameraColor=destination;
            }
        }
    }
}
