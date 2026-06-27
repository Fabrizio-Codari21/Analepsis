using UnityEngine;

public class DoorDialoguer : SimpleDialoguer
{
    [SerializeField] private Door _door;
    
    public override void EndDialogue()
    {
        base.EndDialogue();
        _door.TryOpenDoor();
    }
}