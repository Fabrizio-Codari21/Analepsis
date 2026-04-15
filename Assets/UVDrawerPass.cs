using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class UVDrawerPass : ScriptableRenderPass
{
    private readonly Material m_UVMaterial;
    public UVDrawerPass(Material overrideMaterial)
    {
        // Set the pass's local copy of the override material 
        m_UVMaterial= overrideMaterial;
    }
    private class PassData
    {
        // Create a field to store the list of objects to draw
        public RendererListHandle rendererListHandle;
        public TextureHandle sceneUv;
        public int sceneUvId = Shader.PropertyToID("_SceneUV");
    }

 
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
{
   
    UniversalResourceData resourceData = frameContext.Get<UniversalResourceData>();
    UniversalCameraData cameraData = frameContext.Get<UniversalCameraData>();
    UniversalRenderingData renderingData = frameContext.Get<UniversalRenderingData>();
    UniversalLightData lightData = frameContext.Get<UniversalLightData>();


    if (resourceData.activeDepthTexture.IsValid() == false) return;

    using (var builder = renderGraph.AddRasterRenderPass<PassData>("UV_Pass", out var passData))
    {
      
        RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0; 
        
   
        passData.sceneUv = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "uvTex", false);
        passData.sceneUvId = Shader.PropertyToID("_SceneUV");

     
        var sortFlags = cameraData.defaultOpaqueSortFlags;
        var filterSettings = new FilteringSettings(RenderQueueRange.opaque, ~0);
        var shadersToOverride = new ShaderTagId("UniversalForward");
        var drawSettings = RenderingUtils.CreateDrawingSettings(shadersToOverride, renderingData, cameraData, lightData, sortFlags);
        drawSettings.overrideMaterial = m_UVMaterial;

        var rendererListParameters = new RendererListParams(renderingData.cullResults, drawSettings, filterSettings);
        passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParameters);

        builder.UseRendererList(passData.rendererListHandle);
        builder.SetRenderAttachment(passData.sceneUv, 0);
        
        builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.ReadWrite);
        
        builder.AllowGlobalStateModification(true);

        
        builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
        
    
        builder.SetGlobalTextureAfterPass(passData.sceneUv, passData.sceneUvId);
    }
}


static void ExecutePass(PassData data, RasterGraphContext context)
{
   
    context.cmd.ClearRenderTarget(false, true, Color.black);
    context.cmd.DrawRendererList(data.rendererListHandle);
}
}