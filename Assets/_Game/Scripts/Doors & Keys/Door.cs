using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using PrimeTween;
using Sirenix.OdinInspector;

[RequireComponent(typeof(BoxCollider))]
public class Door : MonoBehaviour
{
    public Collider doorObject;
    public float openingDegrees, openingDuration, closedShakeIntensity;
    public Vector2 interactRange;
    public LockState overrideLock;

    [InlineButton(nameof(ClearRequiredClue), "Clear")]
    [ValueDropdown(nameof(GetClueDropdownOptions))]
    [SerializeReference] 
    public IClueHolder requiredClue; 
    
    private void ClearRequiredClue()
    {
        
#if UNITY_EDITOR
        
        UnityEditor.Undo.RecordObject(this, "Clear Required Clue");
        
        requiredClue = null;
       
        UnityEditor.EditorUtility.SetDirty(this);
        
        UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }
        else if (gameObject.scene.IsValid())
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif

    }

    private IEnumerable<ValueDropdownItem<IClueHolder>> GetClueDropdownOptions()
    {
        var dropdownList = new List<ValueDropdownItem<IClueHolder>>();
#if UNITY_EDITOR
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

#endif
        return dropdownList;
        
    }



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
       TryOpenDoor();
    }
    private void OnTriggerExit(Collider other)
    {
        _ = ToggleDoor(false);
    }

    private bool CheckKey(IClueHolder clue)
    {

        if (clue == null) return true;
        
        var c = clue.GetClue();
        if (c is Item)
        {
            return NotebookManager.Instance.CheckNote(c.CompareGuid());
        }

        if (c is DialogueNode)
        {
            return DialogueManager.Instance.CheckDialogue(c.CompareGuid());
        }

        return true;
    }

    public void TryOpenDoor() => _ = ToggleDoor(true, CheckKey(requiredClue));
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