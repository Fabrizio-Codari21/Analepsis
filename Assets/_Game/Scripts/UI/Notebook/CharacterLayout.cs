using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterLayout : NotebookLayout
{
    [Header("Event")]
    [SerializeField] private NpcEvent m_onNpcFound;
    [SerializeField] private NpcEvent m_onNpcAdded;
    [SerializeField] private NpcEvent m_onCharacterSelected;
    
    [Header("Npc")]
    [SerializeField] private List<NpcIdentity> m_characters = new List<NpcIdentity>();
    [SerializeField] private int m_currentIndex;
    private readonly Dictionary<NpcIdentity, int> _indexMap = new Dictionary<NpcIdentity, int>();
    private NpcIdentity _lastSelectedNpc;
    
    [Header("Pages")]
    [SerializeField] private CharacterNotebookPage characterNotebookPage;
    [SerializeField] private TreePage treePage;
    [SerializeField, TextArea] private string leftEmptyReason = "No Character Founded";
    [SerializeField, TextArea] private string rightEmptyReason = "No Character Founded";
    
    [SerializeField] private EmptyPage emptyPagePrefab;
    private EmptyPage _rightEmptyPage;
    private EmptyPage _leftEmptyPage;

    public override void Initialize(Transform leftRoot, Transform rightRoot)
    {
        characterNotebookPage = Instantiate(characterNotebookPage, leftRoot);
        characterNotebookPage.Hide();
        
        _leftEmptyPage = Instantiate(emptyPagePrefab, leftRoot);
        _leftEmptyPage.SetReason(leftEmptyReason);
        _leftEmptyPage.Hide();

        treePage = Instantiate(treePage, rightRoot);
        treePage.Hide();
        
        _rightEmptyPage = Instantiate(emptyPagePrefab, rightRoot);
        _rightEmptyPage.SetReason(rightEmptyReason);
        _rightEmptyPage.Hide();
    }
    
    private void Start()
    {
        m_onNpcFound.OnEventRaised += AddNewCharacter;
        m_onCharacterSelected.OnEventRaised += TrackLastSelected;
    }

    private void OnDestroy()
    {
        m_onNpcFound.OnEventRaised -= AddNewCharacter;
        m_onCharacterSelected.OnEventRaised -= TrackLastSelected;
    }

    private void AddNewCharacter(NpcIdentity newNpc)
    {
        if (!newNpc) return;
        if (_indexMap.ContainsKey(newNpc)) return;
        
        m_characters.Add(newNpc);
        var assignedIndex = m_characters.Count - 1;
        _indexMap.Add(newNpc, assignedIndex);

        if (!_lastSelectedNpc) _lastSelectedNpc = newNpc;
        
     
        if (gameObject.activeInHierarchy) m_onNpcAdded?.Raise(newNpc);
        
    }
    
    private void TrackLastSelected(NpcIdentity selectedNpc)
    {
        _lastSelectedNpc = selectedNpc;
    }

    public override void Show()
    {
      
        if (NotebookManager.Instance != null)
        {
            foreach (var npc in NotebookManager.Instance.FoundCharacters.Where(npc => npc != null && !_indexMap.ContainsKey(npc)))  // este es una defensa por la duda
            {
                m_characters.Add(npc);
                _indexMap.Add(npc, m_characters.Count - 1);
            }
        }

        base.Show();
        
  
        if (m_characters.Count <= 0)
        {
            characterNotebookPage.Hide();
            treePage.Hide();
            _leftEmptyPage.Show();
            _rightEmptyPage.Show();
        }
        else
        {
            _leftEmptyPage.Hide();
            _rightEmptyPage.Hide();
            
          
            characterNotebookPage.Show();
            treePage.Show();
            
            
            characterNotebookPage.SyncAllButtons(m_characters);

            
            if (_lastSelectedNpc == null || !_indexMap.ContainsKey(_lastSelectedNpc)) 
            {
                _lastSelectedNpc = m_characters[^1];
            }
            
            characterNotebookPage.SetupPage(_lastSelectedNpc);
            
            m_onCharacterSelected?.Raise(_lastSelectedNpc);
        }
    }
}