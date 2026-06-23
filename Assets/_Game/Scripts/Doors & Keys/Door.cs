using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using PrimeTween;
using System.Linq;
using Sirenix.OdinInspector;

[RequireComponent(typeof(BoxCollider))]
public class Door : MonoBehaviour
{
    public Collider doorObject;
    public float openingDegrees, openingDuration, closedShakeIntensity;
    public Vector2 interactRange;
    public LockState overrideLock;

    

    [ValueDropdown(nameof(GetClueDropdownOptions))]
    [SerializeReference] 
    public IClueHolder requiredClue; 
    
#if UNITY_EDITOR
    
    private IEnumerable<ValueDropdownItem<IClueHolder>> GetClueDropdownOptions()
    {
        var dropdownList = new List<ValueDropdownItem<IClueHolder>>();
        
        var allClues = ClueProvider.GetAvailableClues();

        foreach (var clueItem in allClues)
        {
            string menuPath = clueItem.Text;
            IClue clueValue = clueItem.Value;
            
            Type targetType = clueValue.GetType();
            Type holderGenericType = typeof(ClueHolder<>).MakeGenericType(targetType);
            IClueHolder wrapperInstance = (IClueHolder)Activator.CreateInstance(holderGenericType, clueValue);
            
            dropdownList.Add(new ValueDropdownItem<IClueHolder>(menuPath, wrapperInstance));
        }

        return dropdownList;
    }
#endif


    BoxCollider _col;
    void Start()
    {
        _col = GetComponent<BoxCollider>();
        _col.isTrigger = true;
        _col.size = new Vector3(interactRange.x, doorObject.transform.localScale.y, interactRange.y);
    }


    // Por ahora, si no tiene llave o si llegaste a ver el flashback de la llave (es decir que
    // analizaste el objeto por completo) te deja desbloquear la puerta.
    private void OnTriggerEnter(Collider collider)
    {
        _ = ToggleDoor(true, CheckKey(requiredClue.GetClue()));
    }
    private void OnTriggerExit(Collider other)
    {
        _ = ToggleDoor(false);
    }

    private bool CheckKey(IClue clue)
    {

        if (clue is Item)
        {
            return NotebookManager.Instance.CheckNote(clue.CompareGuid());
        }

        if (clue is DialogueNode)
        {
            return DialogueManager.Instance.CheckDialogue(clue.CompareGuid());
        }

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