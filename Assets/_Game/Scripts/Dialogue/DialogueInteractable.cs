using UnityEngine;

// Los NPC con los que se puede dialogar tendrian este script.
public class DialogueInteractable : Interactable
{
    public string characterName;
    public Dialogue dialogue;
    
    public void SpeakTo()
    {
        //DialogueManager.instance.StartDialogue(characterName, dialogue.RootNode);
    }
}
