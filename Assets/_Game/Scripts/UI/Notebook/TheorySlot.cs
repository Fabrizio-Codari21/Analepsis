using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class TheorySlot : MonoBehaviour, ISlotData<Evidence>
{
    #region Case
    [ReadOnly, ShowInInspector] private CaseSlotIdentity m_identity;
    #endregion
 
    [SerializeField] private RectTransform m_receiveTransform;
    [SerializeField] private TMP_Text m_text;
    [SerializeField] private bool removeOnClear = true;
    
    private Evidence _currentEvidenceHolder;
    [SerializeField] private ButtonSetting m_draggableButton;
 
    private readonly Dictionary<SerializableGuid, EvidenceRepresentButton> _buttonMap = new Dictionary<SerializableGuid, EvidenceRepresentButton>();

    #region Case
    
    public bool Check(CaseSlot slotRule) // Solution Check
    {
        if (slotRule == null)
        {
            Debug.Log(slotRule.Identity.Description);
            return false;
        }

        if (_currentEvidenceHolder == null)
        {
            Debug.Log("2");
            Debug.Log(slotRule.Identity.Description);
            return false;
        }
        if (!_currentEvidenceHolder.whodunnits.HasFlag(m_identity.ProofTypeNeed)) {
        {
            Debug.Log("3");
            Debug.Log(slotRule.Identity.Description);
            return false;
        } }
    
        IClue playerPlacedClue = _currentEvidenceHolder.representerClue;
        
        Debug.Log("4");
        Debug.Log(slotRule.Identity.Description);
        return slotRule.Validate(slotRule.Identity.ProofTypeNeed, playerPlacedClue);
    }
    
    public bool IsIdentity(CaseSlotIdentity identityToCompare)
    {
        return m_identity == identityToCompare;
    }

    public void SetIdentity(CaseSlotIdentity identity)
    {
        m_identity = identity;
        m_text.text = m_identity.Description;
    }
    #endregion
    
    #region ISlotData<Evidence>
    public bool ClearOnRemove => removeOnClear;

    public bool CheckSlotAdapt(Evidence data)
    {
        if (data == null || m_identity == null) return false;

        if (data.whodunnits == Whodunnit.NoProof) return true;
        if (!data.whodunnits.HasFlag(m_identity.ProofTypeNeed)) return false;
    
        return !_buttonMap.ContainsKey(data.guid);
    }

    public bool ReplaceData(Evidence data)
    {
        if (!CheckSlotAdapt(data)) return false;

        if (_buttonMap.Count > 0)
        {
            Debug.Log("Contain Data Clear Now");
            ClearSlot();
        }
        
        Debug.Log("Remplace Data");
        _currentEvidenceHolder = data; 
        EvidenceRepresentButton newButton = FlyweightFactory.Instance.Spawn<EvidenceRepresentButton>(m_draggableButton, Vector3.zero, Quaternion.identity, m_receiveTransform);
        newButton.SetText(data.displayName);
        newButton.InitData(data, this); 
        newButton.MoveToLast();
        newButton.Center();
        newButton.transform.SetParent(m_receiveTransform,false);
        _buttonMap.Add(data.guid, newButton);

        
        return true;
    }

    public void RemoveData(Evidence data)
    {
        if (data == null)
        {
            Debug.Log("No Has Data");
            return;
        }
        
        if (_buttonMap.TryGetValue(data.guid, out var targetButton))
        {
            if (targetButton != null) FlyweightFactory.Instance.Return(targetButton);
            _buttonMap.Remove(data.guid);
            
            Debug.Log("Remove Data");
        }
        else
        {
            Debug.Log("No Has Data nI bUTTON");
        }
       
        if (_currentEvidenceHolder != null && _currentEvidenceHolder.guid == data.guid)
        {
            _currentEvidenceHolder = null;
        }
    }

    public void ClearSlot()
    {
        foreach (var button in _buttonMap.Values) if (button != null) FlyweightFactory.Instance.Return(button);
        _buttonMap.Clear();
        _currentEvidenceHolder = null;
        
        
        Debug.Log("Clear Data");
    }
    
   
    #endregion

    private void OnDestroy()
    {
       if(FlyweightFactory.HasInstance) ClearSlot();
    }
}