public interface IDialogable : IInteractable
{
    public string NpcName { get; set; }
    public Dialogue Dialogue { get; }
    public Dialogue NewDialogue(Dialogue dialogue);

}