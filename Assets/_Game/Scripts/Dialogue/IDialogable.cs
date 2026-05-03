using UnityEngine;
using UnityEngine.Animations.Rigging;

public interface IDialogable : IInteractable
{
    public string NpcName { get; set; }
    public Dialogue Dialogue { get; }
    public bool FirstTimeSpeaking { get; set; }
    public NpcIdentity ID { get; set; }
    public Dialogue NewDialogue(Dialogue dialogue);
    public void SetFace(Emotion newEmotion = Emotion.Idle);
    public void SetAnimation(Reaction newReaction = Reaction.None);
    public void ResetAnimation();
    public Emotion DefaultEmotion { get; set; }
    public MultiAimConstraint LookAt { get; set; }
    public MultiAimConstraint Player { get; set; }

}