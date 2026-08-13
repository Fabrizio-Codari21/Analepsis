using System;
using UnityEngine;

public abstract class LocalizedComponents : MonoBehaviour
{
    [SerializeField] protected CSVKey m_key;
    [SerializeField] protected string m_id;

 

    private void Start()
    {
        if (!Validate()) return;
        LcaManager.Instance.OnLanguageChanged += Translate;
       Translate();
    }

    
    

    private void OnDestroy()
    {
        
        if (LcaManager.HasInstance) LcaManager.Instance.OnLanguageChanged -= Translate;
    }

    
    
    private bool Validate()
    {
        if (m_key == null) {Debug.LogError($"[Localization] CSVKey is missing on '{name}'.", this); return false;
        }

        if (string.IsNullOrWhiteSpace(m_id))
        {
            Debug.LogError($"[Localization] Localization ID is empty on '{name}'.", this);

            return false;
        }

        return true;
    }
  

    protected abstract void Translate();
}