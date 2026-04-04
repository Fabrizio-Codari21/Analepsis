using System;
using UnityEngine;

public class Npc : MonoBehaviour,INpc
{
   [SerializeField] private NpcIdentity m_npcIdentity;
   [SerializeField] private Dialogue m_defaultDialogue;
   [SerializeField] private DialoguerEvent m_dialogueEvent;
   private void Start()
   {
       NewDialogue(m_defaultDialogue);
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
      OnStart?.Invoke();
   }

   public void InteractEnd()
   {
       OnEnd?.Invoke();
      Speck();
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
      return "Talk";
   }
}