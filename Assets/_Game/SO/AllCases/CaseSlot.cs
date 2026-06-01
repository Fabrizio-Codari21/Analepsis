using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class CaseSlot
{
    [ReadOnly]  public CaseSlotIdentity Identity;
    
    [ShowInInspector, ReadOnly]
    public string SlotTitle => Identity == null ? "(Missing Identity)" : Identity.Description;
    
    [ListDrawerSettings(CustomAddFunction = nameof(AddNewClueElement), ShowIndexLabels = true)]
    [SerializeReference] 
    public List<IClueHolder> requieredClue = new List<IClueHolder>(); 


    private void AddNewClueElement()
    {
#if UNITY_EDITOR
        UnityEditor.GenericMenu menu = new UnityEditor.GenericMenu();
        var allClues = ClueProvider.GetAvailableClues();

        foreach (var clueItem in allClues)
        {
            string menuPath = clueItem.Text;
            IClue clueValue = clueItem.Value;

            menu.AddItem(new GUIContent(menuPath), false, () =>
            {
                requieredClue ??= new List<IClueHolder>();

                
                Type targetType = clueValue.GetType();
                Type holderGenericType = typeof(ClueHolder<>).MakeGenericType(targetType);
                
                IClueHolder wrapperInstance = (IClueHolder)Activator.CreateInstance(holderGenericType, clueValue);
                
                requieredClue.Add(wrapperInstance);
                
                Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
            });
        }

        menu.ShowAsContext();
#endif
    }

    public bool Validate(Whodunnit w, IClue clue)
    {
        if (Identity.ProofTypeNeed != w)
        {
            Debug.Log("The ProofTypeNeed is wrong");
            return false;
        }
        
        if (requieredClue == null || requieredClue.Count == 0) return true;
        
        
        foreach (var holder in requieredClue)
        {
            if (holder == null || holder.GetClue() != clue) continue;
            Debug.Log("Has Neede Clue");
            return true;
        }
        
        Debug.Log("Need Clue");
        return false;
    }
}