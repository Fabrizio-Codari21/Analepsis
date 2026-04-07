using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera _cam;

    private void Start()
    {
        _cam = Camera.main;
    }

    void LateUpdate()
    {
        if (_cam == null) return;
        
        Vector3 forward = transform.position - _cam.transform.position;
        forward.y = 0;
        transform.forward = forward;
       
    }
}