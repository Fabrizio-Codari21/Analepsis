using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering.Universal;

public class Npc : MonoBehaviour,INpc, IConditionCheck
{
   [SerializeField] private NpcIdentity m_npcIdentity;
   public DecalProjector faceProjector;
   public Emotion defaultEmotion = Emotion.Idle;
   public Animator animator;
   [SerializeField] private Dialogue m_defaultDialogue;
   [SerializeField] private DialoguerEvent m_dialogueEvent;
   
   [SerializeField] private DynamicTextSetting m_nameTextSetting;
   [SerializeField] private Vector3 m_textPositionOffset;
   [SerializeField] private MultiAimConstraint m_lookAt;

   [SerializeField] private Tip m_tip; //Por ahora no uso el texto que le asignamos en el inspector
   
   public List<ICondition> Conditions { get; } = new();
   private DynamicText _text;
   private List<Tip> tips = new();
   [SerializeField] private MultiAimConstraint m_player;
   private void Start()
   {
       OnFocus += SpawnName;
       OnUpdateDistance += UpdateOffset;
       OnUnfocus += DespawnName;
       OnStart += DespawnName;
       m_tip.tip = $"Should I talk to {m_npcIdentity.npcName}? ";
       AddTip(m_tip);

       SetEmotion(DefaultEmotion);


       m_lookAt.weight = 0;

   }

    #region IInteract
    public event Action OnFocus;
   public event Action OnUnfocus;

   public void Focus()
   {
        OnFocus?.Invoke();
   }

   public void Unfocus()
   {
        OnUnfocus?.Invoke();
   }

   public void UpdateDistance(float dist)
   {
       OnUpdateDistance?.Invoke(dist);
   }

   public event Action OnStart;
   public event Action OnEnd;
   public event Action<float> OnUpdateDistance;

   public void InteractStart()
   {
      var state = GetCurrentState();
      if(!state.canInteract) return;
      OnStart?.Invoke();
   }

   public void InteractEnd()
   {
      var state = GetCurrentState();
      if(!state.canInteract) return;
       OnEnd?.Invoke();
      Speck();
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

    public void UpdateOffset(float dist)
    {
        if(_text != null)
            _text.transform.position = transform.position + Mathf.Lerp(0.5f, m_textPositionOffset.y, dist).AsY();
    }
    
    #endregion

    private void Speck()
   {
      m_dialogueEvent.Raise(this);
      StartDialogue();
    }
   public string DialoguerName
   {
      get => m_npcIdentity.npcName;
      set => m_npcIdentity.npcName = value;
   }
   public void StartDialogue()
   {
       Debug.Log("Npc.StartDialogue");
       if (m_lookAt != null)
       {
           Debug.Log("View To Player");
           _= ViewToPlayer(m_lookAt,1f,view: true);
       }
       else
       {
           Debug.LogError("Error");
       }
   }

   public void EndDialogue()
   { 
       if (m_lookAt != null)
       {
           ViewToPlayer(m_lookAt,1f,view: false).Forget();
       }
       SetEmotion(DefaultEmotion);
      ResetAnimation();
      
       if (FirstTimeSpeaking)
       {
           NotebookManager.Instance.AddCharacter(m_npcIdentity);           
          FirstTimeSpeaking = false;
       }

   }


   public Dialogue Dialogue => m_defaultDialogue;

    public Emotion DefaultEmotion { get => defaultEmotion; set => defaultEmotion = value; }
    public bool FirstTimeSpeaking { get; set; } = true;

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
   
   private void SpawnName()
   {
      _text = FlyweightFactory.Instance.Spawn<DynamicText>(m_nameTextSetting, m_textPositionOffset+transform.position,Quaternion.identity,transform);
      _text.SetText(m_npcIdentity.npcName,2,Color.white);
      _ = _text.PlayTypeWriterEffect();
   }

   private void DespawnName()
   {
      if(!_text ) return;
      FlyweightFactory.Instance.Return(_text);
      _text =  null;
   }

    public void SetEmotion(Emotion newEmotion = Emotion.Idle)
    {
        if(!m_npcIdentity.allFaces.ContainsKey(newEmotion))
        {
            print($"No {newEmotion} sprite assigned to {m_npcIdentity.npcName}.");
            return;
        }
        if(faceProjector) faceProjector.material.SetTexture(
            "Base_Map",
            m_npcIdentity.allFaces[newEmotion].texture);
    }

    public void SetAnimation(Reaction newReaction = Reaction.Idle)
    {
        if (!m_npcIdentity.allReactions.ContainsKey(newReaction))
        {
            print($"No {newReaction} parameter assigned to {m_npcIdentity.npcName}.");
            return;
        }
        if (animator)
        {
            var parameters = animator.parameters.ToList();
            foreach (var item in animator.parameters)
            {
                int index = parameters.IndexOf(item);
                if (item.name == m_npcIdentity.allReactions[newReaction])
                {
                    SetAnimParameter(index, true);
                    //print("Current animation: " + item.name);
                    //return;
                }
                else
                {
                    SetAnimParameter(index,false);
                }
            }
        }
    }

    public SerializableGuid Guid()
    {
        return m_npcIdentity.npcGuid;
    }

    public void ResetAnimation()
    {
        if (animator)
        {
            var parameters = animator.parameters.ToList();
            foreach (var item in animator.parameters)
            {
                SetAnimParameter(parameters.IndexOf(item), false);
            }
        }
    }

    public void SetAnimParameter(int index, bool value)
    {
        var parameter = animator.parameters[index];
        switch (parameter.type)
        {
            case AnimatorControllerParameterType.Bool:
                if(animator.GetBool(parameter.name) != value) animator.SetBool(parameter.name, value); break;
            case AnimatorControllerParameterType.Trigger: if (value) animator.SetTrigger(parameter.name); 
                else animator.ResetTrigger(parameter.name); break;
            default: print("Wrong type of Parameter."); break;
        }
        //print("Set anim parameter " + parameter.name + " to " + value);
    }



    private async UniTask ViewToPlayer(MultiAimConstraint constraint, float duration, float minWeight = 0.3f, float maxWeight = 0.9f, bool view = true)
    {
        Debug.Log($"Start Weight = {constraint.weight}");

        float targetWeight = view ? maxWeight : minWeight;

        await Tween.Custom(constraint.weight, targetWeight, duration,
            x =>
            {
                constraint.weight = x;
                Debug.Log($"Tween: {x}");
            },
            Ease.OutCirc);
        
        
        Debug.Log($"End Weight = {constraint.weight}");
    }
    public void ClearTip()
    {
        tips.Clear();
    }
}


