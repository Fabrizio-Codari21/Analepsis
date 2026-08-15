using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public abstract class LocalizedComponents : MonoBehaviour
{
    [SerializeField] protected CSVKey m_key;
    [SerializeField]
    [ValueDropdown(nameof(GetLocalizationKeys))]
    protected string m_id;

 

    private void Start()
    {
        if (!Validate()) return;
        LcaManager.Instance.OnLanguageChanged += Translate;
       Translate();
    }
    
    private IEnumerable<string> GetLocalizationKeys()
    {
        if (m_key == null || m_key.table == null) yield break;

        var rows = CSVUtility.ReadCSV(m_key.table.text);

        if (rows.Count == 0) yield break;

        var keyColumn = FindKeyColumn(rows[0]);

        if (keyColumn < 0) yield break;

        for (var i = 1; i < rows.Count; i++)
        {
            var row = rows[i];

            if (keyColumn >= row.Length) continue;

            var id = row[keyColumn].Trim();

            if (string.IsNullOrWhiteSpace(id)) continue;

            yield return id;
        }
    }

    private static int FindKeyColumn(string[] header)
    {
        for (var i = 0; i < header.Length; i++)
        {
            if (header[i].Trim().Equals(
                    "Key",
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
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