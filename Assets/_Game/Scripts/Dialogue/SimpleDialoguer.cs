using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SimpleDialoguer : MonoBehaviour, IDialogable, IConditionCheck
{
    public string npcName;
    public string NpcName { get => npcName; set => npcName = value; }

    public Dialogue localDialogue;
    public Dialogue Dialogue => localDialogue;
    [SerializeField] private DialoguerEvent m_dialogueEvent;

    public string interactText;
    public DynamicTextSetting textSetting;
    public Vector3 textOffset;
    public bool canBeDisabled;

    private List<Tip> tips = new();
    private DynamicText _text;

    public event Action OnStart;
    public event Action OnEnd;
    public event Action OnFocus;
    public event Action OnUnfocus;
    public event Action<float> OnUpdateDistance;

    private void Start()
    {
        var interact = GetComponents<IInteractable>().Where(i => i != this);
        if (interact.Any())
            foreach (var i in interact)
            {
                i.OnFocus += SpawnName;
                i.OnUnfocus += DespawnName;
                i.OnUpdateDistance += UpdateOffset;
                i.OnStart += DespawnName;
                i.OnStart += StartDialogue;

                i.AddTip(new Tip(interactText, TipOrder.InteractionType));
            }

        //OnFocus += SpawnName;
        //OnUnfocus += DespawnName;
        //OnUpdateDistance += UpdateOffset;
        //OnStart += DespawnName;
        //OnStart += StartDialogue;

        //AddTip(new Tip(interactText, TipOrder.InteractionType));
    }

    public void Focus()
    {
        OnFocus?.Invoke();
    }

    public void Unfocus()
    {
        OnUnfocus?.Invoke();
    }

    public void InteractEnd()
    {
        var state = GetCurrentState();
        if (!state.canInteract) return;
        OnEnd?.Invoke();
    }

    public void InteractStart()
    {
        var state = GetCurrentState();
        if (!state.canInteract) return;
        OnStart?.Invoke();
    }

    public Dialogue NewDialogue(Dialogue dialogue) => localDialogue = dialogue;

    public string GetTip()
    {
        if (tips.Count == 0) return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var t in tips) sb.Append(t.tip);


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

    public void UpdateDistance(float dist)
    {
        OnUpdateDistance?.Invoke(dist);
    }

    private void SpawnName()
    {
        _text = FlyweightFactory.Instance.Spawn<DynamicText>(textSetting, textOffset + transform.position, Quaternion.identity, transform);
        _text.SetText(npcName, 2, Color.white);
        _ = _text.PlayTypeWriterEffect();
    }

    private void DespawnName()
    {
        if (!_text) return;
        FlyweightFactory.Instance.Return(_text);
        _text = null;
    }

    public void UpdateOffset(float dist)
    {
        if (_text != null)
            _text.transform.position = transform.position + new Vector3(
            0,
            Mathf.Lerp(0.5f, textOffset.y, dist),
            0);
    }


    public List<ICondition> Conditions { get; } = new();

    public bool DisableDialogue { get; set; } = false;
    public InteractionState GetCurrentState()
    {
        if (DisableDialogue) return new InteractionState{ canInteract = false, tipOverride = "", };
        
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

    public void StartDialogue() => m_dialogueEvent.Raise(this);

    #region Unused

    //no habria motivo para usarlos por ahora
    public bool FirstTimeSpeaking { get => false; set => throw new NotImplementedException(); }
    public NpcIdentity ID { get => null; set => throw new NotImplementedException(); }
    public Emotion DefaultEmotion { get => Emotion.None; set => throw new NotImplementedException(); }
    public MultiAimConstraint LookAt { get => null; set => throw new NotImplementedException(); }
    public MultiAimConstraint Player { get => null; set => throw new NotImplementedException(); }

    public void ResetAnimation()
    {
       
    }

    public void SetAnimation(Reaction newReaction = Reaction.None)
    {
        
    }

    public void SetFace(Emotion newEmotion = Emotion.Idle)
    {
        
    }

    #endregion

}
