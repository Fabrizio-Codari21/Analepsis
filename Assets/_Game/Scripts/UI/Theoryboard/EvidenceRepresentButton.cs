using System;
using UnityEngine;

public class EvidenceRepresentButton : DraggableButton<Evidence>
{
    private Evidence m_evidenceData;
    
    private ISlotAcceptor _currentSlot; 

    public void SetEvidence(Evidence evidence) => m_evidenceData = evidence;


    protected override Evidence GetButtonData() => m_evidenceData;
    
    private Action _onDragEndedCallback;
    public void InitializeCallback(Action onDragEnded)
    {
        _onDragEndedCallback = onDragEnded;
    }
    
    
    
  
    
    public override void Despawn()
    {
        base.Despawn();
        
        _onDragEndedCallback = null; 
        m_evidenceData = null;
        
    }
}