using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterNotebookPage : NotebookPage
{
    [Header("Character Info")]
    [SerializeField] private Image m_characterIcon;
    [SerializeField] private TMP_Text m_characterIntroduceText;

    [Header("Button Setting ")]
    [SerializeField] private Transform m_buttonRoot;
    

    [Header("Npc")]
    [SerializeField] private List<NpcIdentity> m_characters = new List<NpcIdentity>();
    [SerializeField] private int m_currentIndex;
    private readonly Dictionary<NpcIdentity,int> _indexMap = new Dictionary<NpcIdentity,int>();
    
    
    
    private void Start()
    {
        m_characters.Clear();
    }
    private void AddNewCharacter(NpcIdentity newNpc)
    {
        if(!newNpc) return;
        if(_indexMap.ContainsKey(newNpc)) return;
        m_characters.Add(newNpc);
        var assignedIndex = m_characters.Count - 1;
        _indexMap.Add(newNpc, assignedIndex);
    }
    private void SwitchCharacter(NpcIdentity key)
    {
        m_characterIcon.sprite = key.filePhoto;
        m_characterIntroduceText.text = key.characterInfo;
    }
    private void SwitchCharacter(int direction)
    {
        if (m_characters.Count == 0) return;
        var finalIndex = (m_currentIndex + direction + m_characters.Count) % m_characters.Count;
        m_currentIndex = finalIndex;
        SwitchCharacter(m_characters[finalIndex]);
    }
    private void Next() => SwitchCharacter(1);
    private void Previous() => SwitchCharacter(-1);
}