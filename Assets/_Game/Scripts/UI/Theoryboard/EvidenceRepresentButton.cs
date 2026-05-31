using UnityEngine;

public class EvidenceRepresentButton : DraggableButton
{
    private Evidence m_evidenceData;
    
    private ISlotAcceptor _currentSlot; 

    public void SetEvidence(Evidence evidence) => m_evidenceData = evidence;


    protected override bool HandlePlacement(ISlotAcceptor slot)
    {
        throw new System.NotImplementedException();
    }
}