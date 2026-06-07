public class DialogueFragmentNote : Evidence
{
    public readonly DialogueNode Node;
    
    public DialogueFragmentNote(string displayName, SerializableGuid guid,Whodunnit proofs, DialogueNode node) : base(displayName, guid, proofs, node) 
    {
        Node = node;
    }
    public override string GetInfo()
    {
        return Node != null ? Node.dialogueText : string.Empty;
    }
}