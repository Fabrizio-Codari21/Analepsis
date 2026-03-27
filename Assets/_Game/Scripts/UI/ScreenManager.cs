using Sirenix.OdinInspector;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    [ShowInInspector, ReadOnly] private StackManager<IActivity> _activities = new StackManager<IActivity>();


    [SerializeField] private IActivityEvent m_pushEvent;
    [SerializeField] private EventChannel m_popInputEvent;

    private void Awake()
    {
        m_pushEvent.OnEventRaised += Push;
        m_popInputEvent.RegisterListener(Pop);
    }
    private void Push(IActivity activity)
    {
        _activities.Push(activity);
    }

    private void Pop()
    {
        _activities.PopSaveRoot();
    }
}