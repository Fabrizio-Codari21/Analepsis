using TMPro;
using UnityEngine;

public class ResponseDialogueButton : ButtonFactoryObject
{
    [SerializeField] private TMP_Text isNewText;

    public void SetTag(string tip)
    {
        if (tip == string.Empty)
        {
            isNewText.gameObject.SetActive(false);
            return;
        }
        isNewText.gameObject.SetActive(true);
        isNewText.text = tip;
    }
}