using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class TheorySlot : MonoBehaviour, ISlotData<Evidence>
{

    #region Case

    [ReadOnly,ShowInInspector] private CaseSlotIdentity m_identity;
    
    #endregion
 
    [SerializeField] private RectTransform m_receiveTransform;
    [SerializeField] private TMP_Text m_text;
    [SerializeField] private bool removeOnClear = true;
    private Evidence _currentEvidenceHolder;
    
    [SerializeField] private ButtonSetting m_draggableButton;
    
    
    List<IFlyweight> m_visualButtones = new List<IFlyweight>();
    List<Evidence> m_evidences = new List<Evidence>();

    #region Case
    
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

    #endregion
    
    
    #region ISlotData<Evidence>

    public bool ClearOnRemove => removeOnClear;
    public bool CheckSlotAdapt(Evidence data)
    {
        return data.whodunnits == m_identity.ProofTypeNeed;
    }

    public bool AddOrReplaceData(Evidence data)
    {
       if(!CheckSlotAdapt(data)) return false;
       
       if (m_evidences.Count > 0) ClearSlot();
       m_evidences.Add(data);
       
       EvidenceRepresentButton newButton = FlyweightFactory.Instance.Spawn<EvidenceRepresentButton>(
           m_draggableButton, 
           Vector3.zero, 
           Quaternion.identity, 
           m_receiveTransform
       );
       newButton.SetText(data.displayName);
       newButton.InitData(data,this);
       newButton.MoveToLast();
       m_visualButtones.Add(newButton);

       return true;
    }

    public void ClearSlot()
    {
        throw new System.NotImplementedException();
    }

    public void RemoveData(Evidence data)
    {
       if(!m_evidences.Contains(data)) return;
       
    }
    #endregion
}