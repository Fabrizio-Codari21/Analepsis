using System;
using System.Collections.Generic;
using UnityEngine;
// Lo que decimos en respuesta a un NPC y que dialogo le sigue a esa respuesta.
[Serializable]
public class DialogueResponse
{
    [TextArea] public string responseText;
    [SerializeReference] public DialogueNode nextNode;
    [SerializeReference] private List<DialogueCondition> m_conditions;
    
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
    public abstract bool Evaluate();
}


public class DialogueNodeCondition : DialogueCondition
{
    [SerializeField] private SerializableGuid guid;
    [SerializeField] private Check m_check;
    public override bool Evaluate()
    {
        return m_check.Request(guid);
    }
}