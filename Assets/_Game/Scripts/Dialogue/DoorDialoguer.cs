using UnityEngine;

public class DoorDialoguer : SimpleDialoguer
{
    [SerializeField] private Door _door;
    
    public override void EndDialogue()
    {
        base.EndDialogue();
        _door.TryOpenDoor();
    }

  
    public override void StartDialogue()
    {
       
        if (_door.CheckKey())
        {
            Debug.Log("Door Open");
            _door.TryOpenDoor();
            return;
        }

        base.StartDialogue();
    }
}