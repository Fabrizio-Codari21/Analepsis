using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[Serializable]
[CreateAssetMenu(fileName = "New Case", menuName = "Game/CaseResolution/NewCase")]
public class CaseResolution : ScriptableObject  // Recipe
{
    /// <summary>
    /// La lista tiene structs que contienen un caso (con clues asignadas a roles) 
    /// mas su nombre y descripci�n (un string).
    /// </summary>
    [InfoBox("Make a new case by creating <b>ways to solve it</b>: " +
        "\n\n<b>1)</b> Give your theory a <b>name</b> to identify it. " +
        "\n<b>2)</b> Add a <b>description</b> of what the case would be." +
        "\n<b>3)</b> Select which <b>clues</b> prove each aspect of the case (never use NoProof). " +
        "\n\nRemember that you still have to assign <b>roles</b> to each NPC and <b>proof</b> to each clue." +
        "\n\n(IMPORTANT: make sure the first answer on the list is your '<b>true</b>' answer.)", 
        Icon = SdfIconType.Newspaper), Space(15)]
    public List<CaseAnswer> AllValidAnswers = new();
    
    
    public List<CaseSlotIdentity> allSlots = new();
    
    public List<CaseAnswer> validAnswers = new();
    
    private void OnValidate()
    {
        if (validAnswers == null || allSlots == null) return;
        HashSet<CaseSlotIdentity> uniqueSlots = new HashSet<CaseSlotIdentity>();  // para que no hay dos slot igual
        for (int i = allSlots.Count - 1; i >= 0; i--)
        {
            if (allSlots[i] != null && uniqueSlots.Add(allSlots[i])) continue;
            allSlots.RemoveAt(i); 
        }
        // en toodas las respuesta tiene que tener misma cantidad de slot osea quien mato a quient, con que etc, y cada uno de eso se manajar por un key por SO , y dentro de ese SO esta para escribir para que sirve el slot que tiene
        // que poner ... 
        foreach (var answer in validAnswers)
        {
            answer.AnswerRequirements ??= new SerializedDictionary<CaseSlotIdentity, CaseSlot>();

            var currentKeys = new List<CaseSlotIdentity>(answer.AnswerRequirements.Keys);
           
            foreach (var oldKey in currentKeys.Where(oldKey => !uniqueSlots.Contains(oldKey))) answer.AnswerRequirements.Remove(oldKey);
            
            foreach (var globalIdentity in uniqueSlots.Where(globalIdentity => !answer.AnswerRequirements.ContainsKey(globalIdentity))) answer.AnswerRequirements.Add(globalIdentity, new CaseSlot());
            
            foreach (var globalIdentity in uniqueSlots)
            {
                if (!answer.AnswerRequirements.TryGetValue(globalIdentity, out var slot)) continue;
                slot.Identity = globalIdentity; 
            }
        }
    }
    

    public CaseAnswer ValidateCase(List<TheorySlot> allRuntimeSlots)
    {
        
        if (allRuntimeSlots == null || validAnswers == null || validAnswers.Count == 0) return null;

        return validAnswers.Where(possibleAnswer => possibleAnswer != null).FirstOrDefault(possibleAnswer => IsAnswerMatched(possibleAnswer, allRuntimeSlots));
    }
    
    private bool IsAnswerMatched(CaseAnswer answer, List<TheorySlot> allRuntimeSlots)
    {
        if (answer.AnswerRequirements.Count != allRuntimeSlots.Count) return false;
        
        foreach (var runtimeSlot in allRuntimeSlots)
        {
            if (runtimeSlot == null) return false;
            
            CaseSlot matchedRule = (from kvp in answer.AnswerRequirements where runtimeSlot.IsIdentity(kvp.Key) select kvp.Value).FirstOrDefault();

            if (!runtimeSlot.Check(matchedRule)) return false; 
            
        }

        return true;
    }

   
}

[Serializable]
public class CaseAnswer
{
    [Title("--- CREATE AN ANSWER ---", TitleAlignment = TitleAlignments.Centered)]
    public string Name;
    [TextArea(0,30)] public string Description;
    
    [ShowInInspector, DictionaryDrawerSettings(KeyLabel = "Role", ValueLabel = "Clues"), PropertySpace(10,15)] 
    public SerializedDictionary<Whodunnit, SerializedList<IClue>> Answer;
    
    [ShowInInspector]
    [DictionaryDrawerSettings(KeyLabel = "ID (Asset)", ValueLabel = "Case Slot", IsReadOnly = true)]
    [PropertySpace(10, 15)]
    public SerializedDictionary<CaseSlotIdentity, CaseSlot> AnswerRequirements = new();


    public bool ValidateAnswer(Dictionary<CaseSlotIdentity, IClue> playerAnswer)
    {
        if (AnswerRequirements.Count != playerAnswer.Count) return false;

        foreach (var kvp in AnswerRequirements)
        {
            CaseSlotIdentity target = kvp.Key;
            CaseSlot rule =  kvp.Value;
            if (!playerAnswer.TryGetValue(target, out IClue playerPlacedClue)) return false;
            if (!rule.Validate(rule.Identity.ProofTypeNeed, playerPlacedClue)) return false; 
        }

        return true;
    }
    
    
}


public interface IClueHolder
{
    IClue GetClue();
}

[Serializable]
public class ClueHolder<T> : IClueHolder where T : class, IClue
{

    [SerializeReference] 
    private T m_clueTarget;
    
    public ClueHolder(T target)
    {
        m_clueTarget = target;
    }

    public IClue GetClue()
    {
        return m_clueTarget;
    }
}