using System;
using System.Collections.Generic;
using UnityEngine;

public class Npc : MonoBehaviour,INpc, IConditionCheck
{
   [SerializeField] private NpcIdentity m_npcIdentity;
   [SerializeField] private Dialogue m_defaultDialogue;
   [SerializeField] private DialoguerEvent m_dialogueEvent;
   
   [SerializeField] private DynamicTextSetting m_nameTextSetting;
   [SerializeField] private Vector3 m_textPositionOffset;

   [SerializeField] private Tip m_tip;
   
   public List<ICondition> Conditions { get; } = new();
   private DynamicText _text;
   private List<Tip> tips = new();
   private void Start()
   {
       NewDialogue(m_defaultDialogue);
       OnFocus += SpawnName;
       OnUnfocus += DespawnName;
       OnStart += DespawnName;
       AddTip(m_tip);
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
   public event Action OnStart;
   public event Action OnEnd;
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

   #endregion

   private void Speck()
   {
      m_dialogueEvent.Raise(this);
   }
   public string NpcName
   {
      get => m_npcIdentity.npcName;
      set => m_npcIdentity.npcName = value;
   }
   public Dialogue Dialogue { get; private set; }
   public Dialogue NewDialogue(Dialogue dialogue) => Dialogue = dialogue;
   public NpcIdentity ID  {
      get => m_npcIdentity;
      set => m_npcIdentity = value;
   }

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
      _text.SetText(m_npcIdentity.npcName,1,Color.white);
      _ = _text.PlayTypeWriterEffect();
   }

   private void DespawnName()
   {
      if(!_text ) return;
      FlyweightFactory.Instance.Return(_text);
      _text =  null;
   }

}

