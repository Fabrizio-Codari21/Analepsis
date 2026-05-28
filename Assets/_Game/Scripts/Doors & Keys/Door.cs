using Cysharp.Threading.Tasks;
using UnityEngine;
using PrimeTween;

[RequireComponent(typeof(BoxCollider))]
public class Door : MonoBehaviour
{
    public Item requiredToOpen;
    public Collider doorObject;
    public float openingDegrees, openingDuration, closedShakeIntensity;
    public Vector2 interactRange;
    public bool overrideLock;

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
        _ = ToggleDoor(
            true, 
            requiredToOpen == null || 
            (requiredToOpen != null && requiredToOpen.keyInfo.isKey &&
            NotebookManager.Instance.GetItemFlashbackInfo(requiredToOpen) != string.Empty));
    }
    private void OnTriggerExit(Collider other)
    {
        _ = ToggleDoor(false);
    }

    public async UniTask ToggleDoor(bool open = true, bool unlocked = true) 
    {

        var seq = Sequence.Create();

        // Si esta desbloqueada, se abre y cierra rotándose y desactiva la colisión de la puerta.
        if(unlocked && !overrideLock)
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
