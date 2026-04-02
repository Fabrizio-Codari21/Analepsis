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
   public void Focus()
   {
     
   }

   public void Unfocus()
   {
     
   }

   public void InteractStart()
   {
      
   }

   public void InteractEnd()
   {
      Debug.Log("InteractEnd");
      Speck();
   }
   #endregion

   private void Speck()
   {
      m_dialogueEvent.Raise(this);
   }
   public string Name
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

}


