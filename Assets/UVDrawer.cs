using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UVDrawer : MonoBehaviour
{
    [SerializeField] private Material m_uvDrawerMaterial;
    private UVDrawerPass m_pass;


    private void Start()
    {
        RenderPipelineManager.beginCameraRendering += OnCameraRendering;
        m_pass = new UVDrawerPass( m_uvDrawerMaterial);
    }

    private void OnCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(m_pass);
    }

    private void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= OnCameraRendering;
    }
}