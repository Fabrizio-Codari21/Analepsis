using Unity.Cinemachine;
using UnityEngine;

public interface ICamera 
{
    public CinemachineCamera Camera { get; set; }
    
    public Quaternion HorizontalPlane{ get;  }
    
    public GameObject Target{ get; set; }
    
}