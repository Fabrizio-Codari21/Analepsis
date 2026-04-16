using UnityEngine;

public class Hand : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform m_handRoot;
    [SerializeField] private Camera m_handCamera; 
    [SerializeField] private LayerMask m_handLayerMask;
    [SerializeField] private Vector2EventChannel uvPositionChannel;

    [Header("Channels")]
    [SerializeField] private EventChannel takeOutChannel;
    [SerializeField] private EventChannel putInChannel;

    private void Start()
    {
        takeOutChannel.OnEventRaised += TakeOut;
        putInChannel.OnEventRaised += PutIn;
        PutIn();
    }

    private void Update()
    {
        ShootRay();
    }

private void ShootRay()
{
   
    Ray ray = m_handCamera.ScreenPointToRay(Input.mousePosition);
    
    if (Physics.Raycast(ray, out var hit, 2.0f, m_handLayerMask))
    {
        uvPositionChannel.Raise(hit.textureCoord);
    }
    else
    {
        Debug.DrawRay(ray.origin, ray.direction * 2.0f, Color.red);
        uvPositionChannel.Raise(new Vector2(-1f, -1f));
    }
}

    private void TakeOut() { enabled = true; m_handRoot.gameObject.SetActive(true); }
    private void PutIn() { enabled = false; m_handRoot.gameObject.SetActive(false); }

    private void OnDestroy()
    {
        takeOutChannel.OnEventRaised -= TakeOut;
        putInChannel.OnEventRaised -= PutIn;
    }
}