
using System;
using UnityEngine;


public class InputsManager : MonoBehaviour
{
    [SerializeField] public InputReader[] m_inputReader;
    [SerializeField] private InputReaderEvent m_inputReaderEvent;
    private InputActions _inputActions;

    public static InputsManager instance;
    

    private void Awake()
    {
        if (!instance) instance = this; else Destroy(gameObject);
        
        _inputActions = new InputActions();
        foreach (var inputReader in m_inputReader)
        {
            inputReader.SetCallback(_inputActions);
        }
        m_inputReaderEvent.OnEventRaised += EnableInputReader;
    }


    // private void Update()
    // {
    //     if(Input.GetKeyDown(KeyCode.Space)) EnableInputReader((m_inputReader[0],false));
    // }


    public void EnableInputReader((InputReader reader, bool enable) provider)
    {
        provider.reader.SetEnable(_inputActions,provider.enable);
    }

 
}


