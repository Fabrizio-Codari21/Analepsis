using System;
using UnityEngine;

public abstract class AbstractEventChannel<T> : ScriptableObject
{
    
    [SerializeField,TextArea] private string description;
    public event Action<T> OnEventRaised;
    public void Raise(T value) => OnEventRaised?.Invoke(value);
}