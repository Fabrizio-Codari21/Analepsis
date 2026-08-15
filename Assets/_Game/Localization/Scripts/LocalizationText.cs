using System;
using TMPro;
using UnityEngine;

public class LocalizationText : LocalizedComponents
{
    [SerializeField] private TMP_Text m_text;
    protected override void Translate()
    {
        m_text.text = LcaManager.Instance.TranslateText(m_key, m_id);
    }
}