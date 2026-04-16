using System;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] private Transform m_handRoot;
    [SerializeField] private EventChannel takeOutChannel;
    [SerializeField] private EventChannel putInChannel;


    
    private void Start()
    {
        takeOutChannel.OnEventRaised += TakeOut;
        putInChannel.OnEventRaised += PutIn;
        
        PutIn();
    }

    private void TakeOut()
      {
         m_handRoot.gameObject.SetActive(true);
      }

      private void PutIn()
      {
          m_handRoot.gameObject.SetActive(false);
      }

      private void OnDestroy()
      {
          takeOutChannel.OnEventRaised -= TakeOut;
          putInChannel.OnEventRaised -= PutIn;  
      }
}
