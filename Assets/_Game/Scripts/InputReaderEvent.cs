using UnityEngine;

[CreateAssetMenu(menuName = "_Game/Event/Input",fileName = "InputReaderEvent")]
public class InputReaderEvent : AbstractEventChannel<(InputReader,bool)>
{
    
}