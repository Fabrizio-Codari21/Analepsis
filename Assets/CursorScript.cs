using NUnit.Framework;
using UnityEngine;

public class CursorScript : MonoBehaviour
{
    public Texture2D cursorArrow;
    public Texture2D cursorArrowUpdate;
    public Vector2 skewedVector = new Vector2(0, 1);
    private Camera _cam;
    bool _clicked = false;
    //[SerializeField]
    //private AudioClip _clickClip;
    //[SerializeField]
    //private AudioClip _unclickClip;
    void Start()
    {
        Cursor.SetCursor(cursorArrow, Vector2.zero, CursorMode.ForceSoftware);
        _cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && Cursor.visible)
        {
            AudioManager.Instance.SelectSfx(SFXType.Player, _clicked ? "PenUnClick" : "PenClick");
            _clicked = !_clicked;
           
            Cursor.SetCursor(cursorArrowUpdate, skewedVector, CursorMode.ForceSoftware);
        }
        
        if (Input.GetKeyUp(KeyCode.Mouse0) && Cursor.visible)
        {            
            Cursor.SetCursor(cursorArrow, Vector2.zero, CursorMode.ForceSoftware);
        }
            
    }
}
