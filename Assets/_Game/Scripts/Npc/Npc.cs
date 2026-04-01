using UnityEngine;

public class Npc : MonoBehaviour,INpc
{
   [SerializeField] private NpcIdentity m_npcIdentity;

   
   
   #region Interface
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
   #endregion
}
