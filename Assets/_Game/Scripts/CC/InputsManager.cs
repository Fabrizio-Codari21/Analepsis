using UnityEngine;

public class InputsManager : MonoBehaviour
{
    [SerializeField] private InputReader[] m_inputReader;
    
    private InputActions _inputActions;

    private void Start()
    {
        _inputActions = new InputActions();
        foreach (var inputReader in m_inputReader)
        {
            inputReader.SetCallback(_inputActions);
            inputReader.SetEnable(_inputActions);
        }
    }
}