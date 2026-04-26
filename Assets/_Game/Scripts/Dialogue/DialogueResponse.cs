using System;
using System.Collections.Generic;
using UnityEngine;
// Lo que decimos en respuesta a un NPC y que dialogo le sigue a esa respuesta.
[Serializable]
public class DialogueResponse
{
    [TextArea] public string responseText;
    [SerializeReference] public DialogueNode nextNode;
    [SerializeReference] public string dialogueTopic;
    [SerializeReference] public List<DialogueCondition> m_conditions= new List<DialogueCondition>();
    
    public bool IsAvailable()
    {
        if (m_conditions == null || m_conditions.Count == 0)
        {
            return true;
        }

        foreach (DialogueCondition condition in m_conditions)
        {
            if (condition == null || condition.Evaluate() == condition.unlockIfTrue) continue;
            return false;
        }
        return true;
    }
    public bool HasTopic(out string topic) 
    {
        if (nextNode == null && dialogueTopic != "")
        {
            topic = dialogueTopic; return true;
        }
        else
        {
            topic = default; return false;
        }
    }
    
    public bool HasConditions() => m_conditions is { Count: > 0 };
    public bool IsNewResponse()
    {
        if (nextNode == null) return false;
        if (!HasConditions()) return false; 
        
        return !DialogueManager.Instance.CheckDialogue(nextNode.guid);
    }

    public bool ShouldShowNewPath()
    {
        return HasConditions() && ScanForUnreadNodes(new HashSet<DialogueNode>());
    }
    // dfs busqueda
    private bool ScanForUnreadNodes(HashSet<DialogueNode> visited)
    {
        if (nextNode == null) return false;

     
        if (!visited.Add(nextNode)) return false;


        if (!DialogueManager.Instance.CheckDialogue(nextNode.guid)) return true;

        if (nextNode.responses == null) return false;
        foreach (var res in nextNode.responses)
        {
            if (!res.IsAvailable()) continue;
            if (res.ScanForUnreadNodes(visited)) return true;
        }

        return false;
    }
    #if UNITY_EDITOR
    [HideInInspector]public Vector2 editorPosition;
    #endif
}

[Serializable]
public abstract class DialogueCondition
{
    [SerializeField] public string conditionName;
    [SerializeField] public bool unlockIfTrue = true;
    public abstract bool Evaluate();
}

public class DialogueNodeCondition : DialogueCondition
{
    [SerializeField] public Dialogue targetDialogue;
    [SerializeField] public DialogueNode isTalkDialogueNode;
    public override bool Evaluate()
    {
        return targetDialogue && isTalkDialogueNode != null && 
               DialogueManager.Instance.CheckDialogue(isTalkDialogueNode.guid);
    }
}

public class ItemNodeCondition : DialogueCondition
{
    [SerializeField] public Item item;
    public override bool Evaluate()
    {
        return !item || NotebookManager.Instance.CheckNote(item.guid);
    }
}

