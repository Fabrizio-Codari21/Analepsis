using System;
using UnityEngine;

public class RenderTextureCamera : MonoBehaviour
{

    [SerializeField] private Camera m_camera;
    [SerializeField] private LayerMask m_layerMask;
    [SerializeField] private Vector2EventChannel uvPositionChannel;

    private void Update()
    {
        ShootRay();
    }

    private void ShootRay()
    {

        Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit, 500f, m_layerMask))
        {
            uvPositionChannel.Raise(hit.textureCoord);
        }
    }
}