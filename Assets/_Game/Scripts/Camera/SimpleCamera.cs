using Unity.Cinemachine;
using UnityEngine;

public class SimpleCamera : MonoBehaviour, ICamera
{
    [SerializeField] private GameObject m_target;
    
    [SerializeField] private CinemachineCamera m_camera;
    public CinemachineCamera Camera
    {
        get => m_camera;
        set => m_camera = value;
    }

    public GameObject Target
    {
        get => m_target;
        set => m_target = value;
    }

    
    public virtual Quaternion HorizontalPlane => _horizontalPlane;
    protected virtual Quaternion _horizontalPlane => Quaternion.Euler(0, m_target.transform.rotation.eulerAngles.y, 0);
}