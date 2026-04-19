using UnityEngine;

public class TransformKeyInstance : MonoBehaviour
{
    [SerializeField] private TransformKey key;

    private void OnEnable()
    {
        TransformKeyManager.Instance.Register(key,this);
    }
    private void OnDisable()
    {
       if(TransformKeyManager.HasInstance) TransformKeyManager.Instance.Unregister(key, this);
    }
}