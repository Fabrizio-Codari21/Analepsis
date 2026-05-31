using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class TheorySlot : MonoBehaviour, ISlotData<Evidence>
{
    [ReadOnly,ShowInInspector] private CaseSlotIdentity m_identity;
 
    [SerializeField] private RectTransform m_receiveTransform;
    [SerializeField] private TMP_Text m_text;
    private Evidence _currentEvidenceHolder;
    
    [SerializeField] private ButtonSetting m_draggableButton;
    public bool Check(CaseSlot slotRule)
    {
        if (slotRule == null) return false;
        if (_currentEvidenceHolder != null && slotRule.Identity.ProofTypeNeed!= _currentEvidenceHolder.whodunnits)
        {
            return false;
        }
        IClue playerPlacedClue = _currentEvidenceHolder?.representerClue;
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

    public void SetEvidence(Evidence evidence)
    {
        _currentEvidenceHolder = evidence;
    }
    

    public Transform SlotTransform => m_receiveTransform;
    
    public bool ReplaceData(Evidence data)
    {
        if (data.whodunnits != m_identity.ProofTypeNeed) return false;
        
        if (_currentEvidenceHolder != null)
        {
            if (data.representerClue == _currentEvidenceHolder.representerClue) return false;
            
        }
        ClearSlot();
        _currentEvidenceHolder = data;
        return true;
    }

    public bool CheckSlotAdapt(Evidence data)
    {
        Debug.Log(data.whodunnits.ToString());
        Debug.Log(m_identity.ProofTypeNeed.ToString());
        return data.whodunnits == m_identity.ProofTypeNeed;
    }

    public bool CanAccept()
    {
        return true;
    }

    public void ClearSlot()
    {
        _currentEvidenceHolder = null;
    }
}