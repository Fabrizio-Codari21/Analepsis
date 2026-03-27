using UnityEngine;
public class InputsManager : PersistentSingleton<InputsManager>
{
    [SerializeField] public InputReader[] m_inputReader;

    private InputActions _inputActions;
  

    protected override void Awake()
    {
        
        base.Awake();
        _inputActions = new InputActions();
        foreach (var inputReader in m_inputReader)
        {
            inputReader.SetCallback(_inputActions);
            inputReader.SetEnable(inputReader.isAutoEnable);
        }
       
        
    }


    

    public void PushInput(InputReader inputReader)
    {
       
    }

    public void PopInput()
    {
     
    }
    
    
}