using System;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] private Transform m_handRoot;
    [SerializeField] private EventChannel takeOutChannel;
    [SerializeField] private EventChannel putInChannel;
    [SerializeField] private Camera m_handCamera;
    [SerializeField] private LayerMask m_handLayerMask;
    
    private void Start()
    {
        takeOutChannel.OnEventRaised += TakeOut;
        putInChannel.OnEventRaised += PutIn;
        
        PutIn();
    }


    private void Update()
    {
        if(Input.GetMouseButton(0)) ShootRay();
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

      private void ShootRay()
      {
          Ray ray = m_handCamera.ScreenPointToRay(Input.mousePosition);
          RaycastHit hit;

          if (Physics.Raycast(ray, out hit, Mathf.Infinity, m_handLayerMask))
          {
              Vector2 uv = hit.textureCoord;
              
              Debug.Log(hit.textureCoord);
          }
      }
}
