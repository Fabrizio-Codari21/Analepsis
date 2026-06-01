using Cysharp.Threading.Tasks;
using UnityEngine;
using PrimeTween;
using System.Linq;

[RequireComponent(typeof(BoxCollider))]
public class Door : MonoBehaviour
{
    public IClue requiredToOpen;
    public Collider doorObject;
    public float openingDegrees, openingDuration, closedShakeIntensity;
    public Vector2 interactRange;
    public LockState overrideLock;


    BoxCollider _col;
    void Start()
    {
        _col = GetComponent<BoxCollider>();
        _col.isTrigger = true;
        _col.size = new Vector3(interactRange.x, doorObject.transform.localScale.y, interactRange.y);
    }

    void Update()
    {
        
    }

    // Por ahora, si no tiene llave o si llegaste a ver el flashback de la llave (es decir que
    // analizaste el objeto por completo) te deja desbloquear la puerta.
    private void OnTriggerEnter(Collider collider)
    {
        _ = ToggleDoor(true, CheckKey(requiredToOpen));
    }
    private void OnTriggerExit(Collider other)
    {
        _ = ToggleDoor(false);
    }

    public bool CheckKey(IClue clue)
    {
        if(clue == null) return true;

        if (clue is Item)
        {
            var c = (Item)clue;
            return c.keyInfo.isKey &&
            NotebookManager.Instance.GetItemFlashbackInfo(c) != string.Empty;
        }            
        // else if (clue is Dialogue)
        //     return NotebookManager.Instance.StartedDialogues.Any(x => x.GetFullDialogue() == clue && x.IsKey());
        // else return false;

        return true;
    }

    public async UniTask ToggleDoor(bool open = true, bool unlocked = true) 
    {

        var seq = Sequence.Create();

        // Si esta desbloqueada, se abre y cierra rot�ndose y desactiva la colisi�n de la puerta.
        if((unlocked && overrideLock is not LockState.Lock) || overrideLock is LockState.Unlock)
        {
            _ = seq.Group(Tween.LocalRotation(
            doorObject.gameObject.transform,
            new Vector3(-90, 0, open ? openingDegrees : 0),
            openingDuration,
            ease: Ease.OutCirc));

            doorObject.enabled = !open;
        }
        // Si no, sacude la puerta como si tratara de abrirla pero no pudiera.
        else if (open)
        {
            _ = seq.Group(Tween.PunchLocalRotation(
            doorObject.gameObject.transform, 
            new Vector3(0, 0, closedShakeIntensity),
            openingDuration,
            easeBetweenShakes: Ease.OutCirc));
        }

        await seq;
    }
}

public enum LockState
{
    None,
    Lock,
    Unlock,
}