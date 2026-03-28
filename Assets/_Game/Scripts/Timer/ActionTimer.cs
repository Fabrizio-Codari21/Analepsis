using Sirenix.OdinInspector;
using UnityEngine;

public class ActionTimer : MonoBehaviour
{
    public static ActionTimer instance;

    public int maxActions;
    [ReadOnly] public int actionsLeft;

    private void Awake()
    {
        if(!instance) instance = this; else Destroy(gameObject);
        actionsLeft = maxActions;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(actionsLeft <= 0) EndOfTimer();
    }

    public void ConsumeActions(int change)
    { 
        if (actionsLeft > 0) actionsLeft -= change;
    }

    public void EndOfTimer()
    {
        print("You consumed all your actions.");
    }
}
