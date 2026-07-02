using System.Collections.Generic;
using UnityEngine;

public class EvidenceDataSlots : MonoBehaviour, ISlotData<Evidence>
{
    [SerializeField] private RectTransform m_contentRoot;
    [SerializeField] private ButtonSetting m_draggableButton;
    [SerializeField] private bool m_clearOnRemove = false;
    
    private readonly Dictionary<SerializableGuid, EvidenceRepresentButton> m_buttonMap = new Dictionary<SerializableGuid, EvidenceRepresentButton>();

    public bool ClearOnRemove => m_clearOnRemove;

    public bool CheckSlotAdapt(Evidence data)
    {
        if (data == null) return false;
      
        if (m_buttonMap.ContainsKey(data.guid)) return false;
        return true;
    }

    public bool ReplaceData(Evidence data, float scaleMultiplier = 1f)
    {
        if (!CheckSlotAdapt(data)) return false;

        EvidenceRepresentButton newButton = FlyweightFactory.Instance.Spawn<EvidenceRepresentButton>(
            m_draggableButton, 
            Vector3.zero, 
            Quaternion.identity, 
            m_contentRoot != null ? m_contentRoot : (RectTransform)transform
        );
       
        newButton.SetText(data.displayName);
        newButton.InitData(data, this); 
        newButton.transform.localScale *= scaleMultiplier;
        newButton.MoveToLast();

        m_buttonMap.Add(data.guid, newButton);
        return true;
    }

    public void RemoveData(Evidence data)
    {
        if (data == null) return;

        if (m_buttonMap.TryGetValue(data.guid, out var targetButton))
        {
            if (targetButton != null) FlyweightFactory.Instance.Return(targetButton);
            m_buttonMap.Remove(data.guid);
        }
    }


 
    public void ClearSlot()
    {
        foreach (var button in m_buttonMap.Values)
        {
            if (button != null) FlyweightFactory.Instance.Return(button);
        }
        m_buttonMap.Clear();
    }

    private void OnDestroy()
    {
        if(FlyweightFactory.HasInstance) ClearSlot();
    }
}