using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterNotebookPage : NotebookPage
{
    [Header("Character Info")]
    [SerializeField] private Image m_characterIcon;
    [SerializeField] private TMP_Text m_text;

    [Header("Button Setting")]
    [SerializeField] private Transform m_buttonRoot;
    [SerializeField] private CharacterSwitchButton m_button;
    
    [Header("Event")]
    [SerializeField] private NpcEvent m_onCharacterSelected;
    [SerializeField] private NpcEvent m_onNpcAdded;
    
    private readonly HashSet<NpcIdentity> _instantiatedButtons = new();

    private void Start()
    {
        m_onCharacterSelected.OnEventRaised += SwitchCharacter;
        m_onNpcAdded.OnEventRaised += AddNpc;
    }

    private void OnDestroy()
    {
        m_onCharacterSelected.OnEventRaised -= SwitchCharacter;
        m_onNpcAdded.OnEventRaised -= AddNpc;
    }

    private void SwitchCharacter(NpcIdentity key)
    {
        if (key == null) return;
        m_characterIcon.gameObject.SetActive(true);
        m_text.gameObject.SetActive(true);
        m_characterIcon.sprite = key.filePhoto;
        m_text.text = key.characterInfo;
    }


    public void SyncAllButtons(List<NpcIdentity> currentCharacters)
    {
        if (currentCharacters == null) return;

        foreach (var npc in currentCharacters)
        {
            if (npc != null && !_instantiatedButtons.Contains(npc))
            {
                CreateButtonInstance(npc);
            }
        }
    }

    public void SetupPage(NpcIdentity currentNpc)
    {
        SwitchCharacter(currentNpc);
    }

    private void AddNpc(NpcIdentity newNpc)
    {
        if (!newNpc) return;
        if (_instantiatedButtons.Contains(newNpc)) return;

        CreateButtonInstance(newNpc);
    }

  
    private void CreateButtonInstance(NpcIdentity npc)
    {
        var buttonInstance = Instantiate(m_button, m_buttonRoot);
        buttonInstance.Init(npc);
        _instantiatedButtons.Add(npc);
    }
}

