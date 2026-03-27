using System;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/Event/Base",fileName = "VoidEvent")]
public class EventChannel : ScriptableObject
{
    private event Action OnEventRaised;
    
    public void RegisterListener(Action listener) => OnEventRaised += listener;
    public void UnregisterListener(Action listener) => OnEventRaised -= listener;
    public void Raise() => OnEventRaised?.Invoke();
}