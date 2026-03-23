using System;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private void Awake()
    {
       EnableCursor();
    }

    private void Update()
    { 
        if(Input.GetKeyDown(KeyCode.F12))EnableCursor();// solamente para debug  acutalmente
    }

    private bool _enable = true;

    private void EnableCursor()
    {
        if (_enable)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        
        _enable = !_enable;
    }
}
