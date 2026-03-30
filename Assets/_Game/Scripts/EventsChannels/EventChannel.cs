using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/Event/Base",fileName = "VoidEvent")]
public class EventChannel : ScriptableObject
{
   public event Action OnEventRaised;
   
    public void Raise() => OnEventRaised?.Invoke();
}