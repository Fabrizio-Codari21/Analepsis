using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Npc : MonoBehaviour,INpc, IConditionCheck
{
   [SerializeField] private NpcIdentity m_npcIdentity;
   public DecalProjector faceProjector;
   public Emotion defaultEmotion = Emotion.Idle;
   [SerializeField] private Dialogue m_defaultDialogue;
    private bool _firstTimeSpeaking = true;
   [SerializeField] private DialoguerEvent m_dialogueEvent;
   
   [SerializeField] private DynamicTextSetting m_nameTextSetting;
   [SerializeField] private Vector3 m_textPositionOffset;

   [SerializeField] private Tip m_tip; //Por ahora no uso el texto que le asignamos en el inspector
   
   public List<ICondition> Conditions { get; } = new();
   private DynamicText _text;
   private List<Tip> tips = new();
   private void Start()
   {
       NewDialogue(m_defaultDialogue);
       OnFocus += SpawnName;
       OnUnfocus += DespawnName;
       OnStart += DespawnName;
       m_tip.tip = $"Should I talk to {m_npcIdentity.npcName}? ";
       AddTip(m_tip);

       SetFace(DefaultEmotion);
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
      FirstTimeSpeaking = false;
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
    public Emotion DefaultEmotion { get => defaultEmotion; set => defaultEmotion = value; }
    public bool FirstTimeSpeaking { get => _firstTimeSpeaking; set => _firstTimeSpeaking = value; }

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

    public void SetFace(Emotion newEmotion = Emotion.Idle)
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

}


