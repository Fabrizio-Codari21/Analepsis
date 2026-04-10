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
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("UV_Pass", out var passData))
            {
                // Get the data needed to create the list of objects to draw
                UniversalRenderingData renderingData = frameContext.Get<UniversalRenderingData>();
                UniversalCameraData cameraData = frameContext.Get<UniversalCameraData>();
                UniversalLightData lightData = frameContext.Get<UniversalLightData>();
                SortingCriteria sortFlags = cameraData.defaultOpaqueSortFlags;
                RenderQueueRange renderQueueRange = RenderQueueRange.opaque;
                FilteringSettings filterSettings = new FilteringSettings(renderQueueRange, ~0);

                // Redraw only objects that have their LightMode tag set to UniversalForward 
                ShaderTagId shadersToOverride = new ShaderTagId("UniversalForward");

                // Create drawing settings
                DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(shadersToOverride, renderingData, cameraData, lightData, sortFlags);

                // Add the override material to the drawing settings
                drawSettings.overrideMaterial =  m_UVMaterial;

                RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
                desc.msaaSamples = 1;
                desc.depthBufferBits = 0;

                passData.sceneUv = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "uvTex", false);

                // Create the list of objects to draw
                var rendererListParameters = new RendererListParams(renderingData.cullResults, drawSettings, filterSettings);

                // Convert the list to a list handle that the render graph system can use
                passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParameters);

                // Set the render target as the color and depth textures of the active camera texture
                UniversalResourceData resourceData = frameContext.Get<UniversalResourceData>();
                builder.UseRendererList(passData.rendererListHandle);
                builder.SetRenderAttachment(passData.sceneUv, 0);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);
                builder.AllowGlobalStateModification(true);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
                builder.SetGlobalTextureAfterPass(passData.sceneUv, passData.sceneUvId);
            }
        }

    static void ExecutePass(PassData data, RasterGraphContext context)
    {
        // Clear the render target to black
        context.cmd.ClearRenderTarget(true, true, Color.black);

        // Draw the objects in the list
        context.cmd.DrawRendererList(data.rendererListHandle);
    }
}