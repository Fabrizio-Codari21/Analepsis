using UnityEngine;

public  abstract class NotebookLayout : MonoBehaviour
{
    
    public int index;

    public abstract void Initialize(Transform leftRoot, Transform rightRoot);
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }
    public virtual void Hide()
    {
        gameObject.SetActive(false);
        
        
    }

    
}