using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputsManager : PersistentSingleton<InputsManager>
{
    [SerializeField] public InputReader[] m_inputReader;

    private InputActions _inputActions;
   
    private readonly PointerEventData _pointerEventData = new PointerEventData(EventSystem.current);
    private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();
    public Vector2 MousePosition => Mouse.current.position.ReadValue();
    protected override void Awake()
    {
        base.Awake();
        _inputActions = new InputActions();
        foreach (var inputReader in m_inputReader)
        {
            inputReader.Initialize(_inputActions);
            inputReader.SetEnable(inputReader.isAutoEnable);
        }
        
    }
    
    public bool IsPointerOverUI()
    {
        _pointerEventData.position = MousePosition;   
        _raycastResults.Clear();
        EventSystem.current.RaycastAll(_pointerEventData, _raycastResults);
        return _raycastResults.Count > 0;
    }
    
    
}