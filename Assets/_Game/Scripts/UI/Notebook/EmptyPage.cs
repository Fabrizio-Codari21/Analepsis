using TMPro;
using UnityEngine;

public class EmptyPage : NotebookPage
{
    [SerializeField] private TMP_Text m_infoText;



    public void SetReason(string reason)
    {
        m_infoText.text = reason;
    }
}