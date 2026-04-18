using UnityEngine;

public class FlashbackToggled : MonoBehaviour
{
    public BoolEventChannel flashbackEnable;

    private void Start()
    {
        flashbackEnable.OnEventRaised += SetEnabled;
    }

    private void OnDestroy()
    {
        flashbackEnable.OnEventRaised -= SetEnabled;
    }

    public virtual void SetEnabled(bool enabled)
    {
        gameObject.SetActive(enabled);
    }
}
