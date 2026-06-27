using NUnit.Framework;
using UnityEngine;

public class CursorScript : MonoBehaviour
{
    public Texture2D cursorArrow;
    public Texture2D cursorArrowUpdate;
    private Camera _cam;
    [SerializeField]
    private AudioClip _clickClip;
    [SerializeField]
    private AudioClip _unclickClip;
    void Start()
    {
        Cursor.SetCursor(cursorArrow, Vector2.zero, CursorMode.ForceSoftware);
        _cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            AudioSource.PlayClipAtPoint(_clickClip, _cam.transform.position);
            //Cursor.SetCursor(cursorArrowUpdate, Vector2.zero, CursorMode.ForceSoftware);
        }
        /*
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            AudioSource.PlayClipAtPoint(_unclickClip, _cam.transform.position);
            Cursor.SetCursor(cursorArrow, Vector2.zero, CursorMode.ForceSoftware);
        }
            */
    }
}
