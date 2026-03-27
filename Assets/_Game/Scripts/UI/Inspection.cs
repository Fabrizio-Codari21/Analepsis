using System;
using UnityEngine;
public class Inspection : MonoBehaviour
{
    [SerializeField] private InspectionInputReader m_inputReader;
    [SerializeField] private GameObject m_inspectObject;
    [SerializeField] private Camera m_camera;
    private void Start()
    {
        m_inputReader.Rotate += Rotate;
        m_inputReader.DragPressed += RotateStart;
    }
    private void RotateStart(bool enable)
    {
        m_camera.enabled = enable;
    }
    private void Rotate(Vector2 rotation)
    {
        m_inspectObject.transform.Rotate(Vector3.up, -rotation.x , Space.World);
        m_inspectObject.transform.Rotate(Vector3.right, rotation.y, Space.World);
    }
    
}