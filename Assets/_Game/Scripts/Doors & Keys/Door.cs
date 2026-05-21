using Cysharp.Threading.Tasks;
using UnityEngine;
using PrimeTween;

[RequireComponent(typeof(BoxCollider))]
public class Door : MonoBehaviour
{
    public KeyItem requiredToOpen;
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

    private void OnTriggerEnter(Collider collider)
    {
        _ = ToggleDoor(true, requiredToOpen == null || (requiredToOpen != null /*&& forma de saber que el player tiene el objeto*/));
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
            new Vector3(0, open ? openingDegrees : 0, 0),
            openingDuration,
            ease: Ease.OutCirc));

            doorObject.enabled = !open;
        }
        // Si no, sacude la puerta como si tratara de abrirla pero no pudiera.
        else if (open)
        {
            _ = seq.Group(Tween.PunchLocalRotation(
            doorObject.gameObject.transform, 
            new Vector3(0, closedShakeIntensity, 0),
            openingDuration,
            easeBetweenShakes: Ease.OutCirc));
        }

        await seq;
    }
}
