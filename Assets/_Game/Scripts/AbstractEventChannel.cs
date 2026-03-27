using System;
using UnityEngine;

public abstract class AbstractEventChannel<T> : ScriptableObject
{
    
    [TextArea] private string _description;
    public event Action<T> OnEventRaised;
    public void Raise(T value) => OnEventRaised?.Invoke(value);
}