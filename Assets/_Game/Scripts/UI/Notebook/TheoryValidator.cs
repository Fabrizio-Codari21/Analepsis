using System.Collections.Generic;
using UnityEngine;

public class TheoryValidator : MonoBehaviour
{
    [SerializeField] private List<TheorySlot> m_allSlot = new();
    [SerializeField] private CaseResolution m_currentCaseResolution;
    private bool Validate()
    {
        var result = m_currentCaseResolution.ValidateCase(m_allSlot);
        return result != null;
    }
}