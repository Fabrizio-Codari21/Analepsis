public interface IDialogable : IInteractable
{
    public string NpcName { get; set; }
    public Dialogue Dialogue { get; }
    public bool FirstTimeSpeaking { get; set; }
    public NpcIdentity ID { get; set; }
    public Dialogue NewDialogue(Dialogue dialogue);
    public void SetFace(Emotion newEmotion = Emotion.Idle);
    public Emotion DefaultEmotion { get; set; }

}