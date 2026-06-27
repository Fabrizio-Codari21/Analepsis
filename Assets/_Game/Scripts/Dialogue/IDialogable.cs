using UnityEngine;
using UnityEngine.Animations.Rigging;

public interface IDialogable : IInteractable
{
    public string DialoguerName { get; set; }
    public Dialogue Dialogue { get; }
    public bool FirstTimeSpeaking { get; set; }
    public void StartDialogue();
    public void EndDialogue();

    public void SetEmotion(Emotion style);

    public void SetAnimation(Reaction reaction);

    public SerializableGuid Guid();

}