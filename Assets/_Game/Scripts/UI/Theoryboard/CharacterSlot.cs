using System.Collections.Generic;
using UnityEngine;

public class CharacterSlot : MonoBehaviour, ISlotData<Evidence>
{
    [SerializeField] private RectTransform m_contentRoot;
    [SerializeField] private ButtonSetting m_draggableButton;

    [SerializeField] private GameObject m_emptyCharacter;
    private readonly Dictionary<SerializableGuid, EvidenceRepresentButton> m_buttonMap = new Dictionary<SerializableGuid, EvidenceRepresentButton>();
    public bool CheckSlotAdapt(Evidence data)
    {
        return false;
    }
    
    public void DisplayCharacter(Evidence npcEvidence)
    {
        ClearSlot();
        if (npcEvidence == null)
        {
            m_emptyCharacter.SetActive(true);
            return;
        }

        m_emptyCharacter.SetActive(false);
      
        EvidenceRepresentButton newButton = FlyweightFactory.Instance.Spawn<EvidenceRepresentButton>(
            m_draggableButton, 
            Vector3.zero, 
            Quaternion.identity, 
            m_contentRoot != null ? m_contentRoot : (RectTransform)transform
        );

        newButton.SetText(npcEvidence.displayName);
        newButton.InitData(npcEvidence, this); 
        newButton.MoveToLast();
        m_buttonMap.Add(npcEvidence.guid, newButton);
    }


    public void ClearSlot()
    {
        foreach (var button in m_buttonMap.Values)
        {
            if (button != null) FlyweightFactory.Instance.Return(button);
        }
        m_buttonMap.Clear();
    }

    public bool ClearOnRemove => false;
    public void RemoveData(Evidence data)
    {
       
    }

    public bool ReplaceData(Evidence data)
    {
        return false; 
    }
}