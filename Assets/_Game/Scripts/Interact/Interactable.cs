using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable , IConditionCheck
{
    public List<ICondition> Conditions { get; } = new();

    public event Action OnStart;
    public event Action OnEnd;
    public event Action OnFocus;
    public event Action OnUnfocus;
    
    private List<Tip> tips = new();

    //FlashbackManager.Instance.AddInteractable(gameObject);
    //public void Update()
    //{
    //    FlashbackManager.Instance.ToggleByFlashback(gameObject);
    //}

    #region Emergency Teleport

    public Transform teleportIfOverlapping;
    bool _canTeleport = true;
    private void OnTriggerEnter(Collider collision)
    {
        if (teleportIfOverlapping && _canTeleport 
        && collision.gameObject.TryGetComponent<MoveEngine>(out var move)
        && collision.gameObject.TryGetComponent<Controller>(out var cont))
        {
            print(cont.gameObject.name + " is Emergency Teleporting");

            cont.enabled = false;
            move.RelativeParent.Reset();
            move.enabled = false;
            cont.gameObject.transform.position = teleportIfOverlapping.position;

            // La idea era hacer que rotara hacia el ultimo lugar que estaba viendo: esta
            // rotacion no va a terminar de funcionar por el tema de que la cinemachine
            // nunca deja de actualizarse (lo mismo que pasaba con el board).
            var cam = cont.gameObject.GetComponentInChildren<CinemachineCamera>();
            Physics.Raycast(cam.transform.position, cam.transform.forward, out var r);
            cam.gameObject.transform.LookAt(r.transform);
            //cam.ResolveLookAt(r.transform); //?

            this.WaitAndThen(timeToWait: 0.1f, () =>
            {
                _canTeleport = false;
                cont.enabled = true;
                move.enabled = true;
            },
            cancelCondition: () => this.ExecuteIfCancelled(!enabled, () =>
            {
                _canTeleport = true;
                cont.enabled = true;
                move.enabled = true;
            }));
        }
    }

    private void OnEnable()
    {
        _canTeleport = true;

        this.WaitAndThen(timeToWait: 0.1f, () =>
        {
            _canTeleport = false;
        },
        cancelCondition: () => !enabled);
    }
    private void OnDisable()
    {
        _canTeleport = true;
    }

    #endregion

    public virtual void InteractStart()
    {
        var state = GetCurrentState();
        if(!state.canInteract ) return;
        OnStart?.Invoke();
    }
    public virtual void InteractEnd()
    {
        var state = GetCurrentState();
        if(!state.canInteract) return;
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
}



public struct InteractionState
{
    public bool canInteract;  
    public string tipOverride; 
    public Color tipColor;      
}



public interface IInspectable
{
    public ItemReference GetItemReference();
}

