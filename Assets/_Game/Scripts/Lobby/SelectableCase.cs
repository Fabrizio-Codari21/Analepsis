using System;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SelectableCase : MonoBehaviour
{
    public SelectableCaseInfo caseInfo;

    public Image caseIcon;
    public TextMeshProUGUI textName;
    public Button playButton;
    public GameObject lockPanel;

    public void Assign(SelectableCaseInfo info, int order = -1)
    {
        caseInfo.icon = info.icon;
        caseInfo.levelToLoad = info.levelToLoad;
        caseInfo.isUnlocked = info.isUnlocked;
        caseInfo.caseName = info.caseName;
        caseInfo.year = info.year;

        caseIcon.sprite = caseInfo.icon;
        //textName.text = $"Case {(order != -1 ? order.ToString() : "")} - '{caseInfo.caseName}'";
        textName.text = $"{caseInfo.year.AsAYear()} - '{caseInfo.caseName}'";
        if (caseInfo.isUnlocked)
        {
            playButton.onClick.AddListener(() =>
            {
                if (caseInfo.levelToLoad != "") _ = this.AsyncLoader(caseInfo.levelToLoad);
            });
            lockPanel.SetActive(false);
        }
        else
        {
            playButton.GetComponentInChildren<TextMeshProUGUI>().text = "LOCKED";
            playButton.enabled = false;
            lockPanel.SetActive(true);
        }

    }
}

[Serializable]
public struct SelectableCaseInfo
{
    [Header("CASE INFO")]
    public Sprite icon;

    public string caseName;
    public int year;

    [DefaultValue("Menu")] public string levelToLoad;

    public bool isUnlocked;

}
