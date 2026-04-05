using System;
using System.Collections.Generic;
using UnityEngine;
// Lo que decimos en respuesta a un NPC y que dialogo le sigue a esa respuesta.
[Serializable]
public class DialogueResponse
{
    [TextArea] public string responseText;
    [SerializeReference] public DialogueNode nextNode;
    [SerializeReference] public List<DialogueCondition> m_conditions= new List<DialogueCondition>();
    
    public bool IsAvailable()
    {
        if(m_conditions == null || m_conditions.Count == 0) return true;
        foreach(DialogueCondition condition in m_conditions) if(condition!= null && !condition.Evaluate()) return false;
        return true;
    }
    #if UNITY_EDITOR
    [HideInInspector]public Vector2 editorPosition;
    #endif
}

[Serializable]
public abstract class DialogueCondition
{
    [SerializeField] public string conditionName;
    public abstract bool Evaluate();
}

public class DialogueNodeCondition : DialogueCondition
{
    [SerializeField] public Dialogue targetDialogue;
    [SerializeField] public DialogueNode targetNode;
    public override bool Evaluate()
    {
        return targetDialogue != null && targetNode != null && 
               DialogueManager.Instance.CheckDialogue(targetNode.guid);
    }
}