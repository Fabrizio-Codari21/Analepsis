using Cysharp.Threading.Tasks;
using UnityEngine;
using PrimeTween;
using System.Linq;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class Door : MonoBehaviour, IInteractable, IConditionCheck
{
    public Clue requiredToOpen;
    public Collider doorObject;
    public float openingDegrees, openingDuration, closedShakeIntensity;
    public Vector2 proximityRange;
    public LockState overrideLock;
    public Tip openTip { get; private set; } = new Tip($"Open?", TipOrder.InteractionType);

    BoxCollider _col, _doorCol;

    void Start()
    {
        _col = GetComponent<BoxCollider>();
        _col.isTrigger = true;
        _doorCol = doorObject as BoxCollider;
        _col.size = _doorCol.size.ZToY().Times(_doorCol.gameObject.transform.localScale * 1.1f);

        OnEnd += Open;
        if (TryGetComponent<ITipProvider>(out var tp)) tp.AddTip(openTip);

    }

    void Update()
    {
        
    }

    // Por ahora, si no tiene llave, si llegaste a ver el flashback de la llave (es decir que
    // analizaste el objeto por completo) o si llegaste a X dialogo te deja desbloquear la puerta.
    private void Open()
    {
        _ = ToggleDoor(true, CheckKey(requiredToOpen));
    }
    //private void OnTriggerEnter(Collider other)
    //{
    //    _ = ToggleDoor(true, CheckKey(requiredToOpen));
    //}
    private void OnTriggerExit(Collider other)
    {
        _ = ToggleDoor(false);
    }

    public bool CheckKey(Clue clue)
    {
        if(clue == null) return true;

        if (clue is Item)
        {
            var c = (Item)clue;
            return c.keyInfo.isKey &&
            NotebookManager.Instance.GetItemFlashbackInfo(c) != string.Empty;
        }
        else if (clue is Dialogue)
            return NotebookManager.Instance.StartedDialogues.Any(x => x.GetFullDialogue() == clue && x.IsKey());
        else return false;
    }

    public async UniTask ToggleDoor(bool open = true, bool unlocked = true) 
    {

        var seq = Sequence.Create();

        // Si esta desbloqueada, se abre y cierra rotándose y desactiva la colisión de la puerta.
        if((unlocked && overrideLock is not LockState.Lock) || overrideLock is LockState.Unlock)
        {
            _ = seq.Group(Tween.LocalRotation(
            doorObject.gameObject.transform,
            new Vector3(-90, 0, open ? openingDegrees : 0),
            openingDuration,
            ease: Ease.OutCirc));

            _col.size = open
                ? new Vector3(proximityRange.x, doorObject.transform.localScale.y, proximityRange.y)
                : _doorCol.size.ZToY().Times(_doorCol.gameObject.transform.localScale * 1.1f);

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

    #region Interact

    public event Action OnStart;
    public event Action OnEnd;
    public event Action OnFocus;
    public event Action OnUnfocus;
    public event Action<float> OnUpdateDistance;
    private List<Tip> tips = new();
    private DynamicText _text;

    public List<ICondition> Conditions { get; } = new();

    public virtual void InteractStart()
    {
        var state = GetCurrentState();
        if (!state.canInteract) return;
        OnStart?.Invoke();
    }
    public virtual void InteractEnd()
    {
        var state = GetCurrentState();
        if (!state.canInteract) return;
        OnEnd?.Invoke();
    }
    public virtual void Focus()
    {
        OnFocus?.Invoke();
    }

    public virtual void Unfocus()
    {
        OnUnfocus?.Invoke();
    }

    public string GetTip()
    {
        if (tips.Count == 0) return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var t in tips) sb.Append(t.tip + " ");

        return sb.ToString();
    }

    public void AddTip(Tip tip)
    {
        int insertIndex = tips.Count;
        for (int i = 0; i < tips.Count; i++)
        {
            if (tip.order >= tips[i].order) continue;
            insertIndex = i;
            break;
        }

        tips.Insert(insertIndex, tip);
    }

    public void RemoveTip(Tip tip)
    {
        tips.Remove(tip);
    }

    public void ClearTip()
    {
        tips.Clear();
    }
    public InteractionState GetCurrentState() // este para hacer un override de tip si no se puede interactuar
    {
        foreach (var condition in Conditions)
        {
            if (!condition.Check())
                return new InteractionState
                {
                    canInteract = false,
                    tipOverride = condition.GetFailureTip(),
                    tipColor = Color.red
                };
        }
        return new InteractionState
        {
            canInteract = true,
            tipOverride = GetTip(),
            tipColor = Color.white
        };
    }

    public void UpdateDistance(float dist)
    {
        OnUpdateDistance?.Invoke(dist);
    }

    //private void SpawnName()
    //{
    //    _text = FlyweightFactory.Instance.Spawn<DynamicText>(m_nameTextSetting, m_textPositionOffset + transform.position, Quaternion.identity, transform);
    //    _text.SetText(m_itemReference.Name, 2, m_nameTextSetting.color);
    //    _ = _text.PlayTypeWriterEffect();
    //}

    //private void DespawnName()
    //{
    //    if (_text) FlyweightFactory.Instance.Return(_text);
    //    _text = null;
    //}


    //public void UpdateOffset(float dist)
    //{
    //    if (_text != null)
    //        _text.transform.position = transform.position + Mathf.Lerp(0.5f, m_textPositionOffset.y, dist).AsY();
    //}

    #endregion
}

public enum LockState
{
    None,
    Lock,
    Unlock,
}