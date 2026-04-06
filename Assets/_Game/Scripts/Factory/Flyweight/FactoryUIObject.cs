using UnityEngine;

public abstract class FactoryUIObject : FactoryObject
{
    [SerializeField] protected RectTransform m_rectTransform;
    public override void SetPositionAndRotation(Vector3 pos, Quaternion rot, Transform parent = null)
    {
        m_rectTransform.SetParent(parent ,false); 
        m_rectTransform.localScale = Vector3.one;
    }
}