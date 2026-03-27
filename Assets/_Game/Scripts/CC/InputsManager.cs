using UnityEngine;
public class InputsManager : PersistentSingleton<InputsManager>
{
    [SerializeField] public InputReader[] m_inputReader;

    private InputActions _inputActions;


    [SerializeField] private IActivityEvent m_inputStackEvent;
   private readonly StackManager<IActivity> _inputStackActivity = new StackManager<IActivity>();

    protected override void Awake()
    {
        
        base.Awake();
        _inputActions = new InputActions();
        foreach (var inputReader in m_inputReader)
        {
            inputReader.SetCallback(_inputActions);
            inputReader.SetEnable(inputReader.isAutoEnable);
        }
        m_inputStackEvent.OnEventRaised += Push;
        
    }


  

    private void Push(IActivity activity)
    {
        _inputStackActivity.Push(activity);
    }

    private void Pop()
    {
        _inputStackActivity.PopSaveRoot();
    }
    

    public void PushInput(InputReader inputReader)
    {
       
    }

    public void PopInput()
    {
     
    }
    
    
}