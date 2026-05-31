using UnityEngine;

public class TheorySlot : MonoBehaviour
{
    [SerializeField] private CaseSlotIdentity m_identity;
    [SerializeField] private string m_displayName;
    private Evidence _currentEvidenceHolder;
    public bool Check(CaseSlot slotRule)
    {
        if (slotRule == null) return false;
        if (_currentEvidenceHolder != null && slotRule.requiredWhodunnit != _currentEvidenceHolder.whodunnits)
        {
            return false;
        }
        Clue playerPlacedClue = _currentEvidenceHolder != null ? _currentEvidenceHolder.representerClue : null;
        return slotRule.Validate(slotRule.requiredWhodunnit, playerPlacedClue);
        
    }
    public bool IsIdentity(CaseSlotIdentity identityToCompare)
    {
        return m_identity == identityToCompare;
    }
    public void SetEvidence(Evidence evidence)
    {
        _currentEvidenceHolder = evidence;
    }
}