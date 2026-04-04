using System;
using UnityEngine;

public abstract class AbstractEventChannel<T> : ScriptableObject
{
    
    [SerializeField,TextArea] private string description;
    public event Action<T> OnEventRaised;
    public void Raise(T value) => OnEventRaised?.Invoke(value);
}

public abstract class AbstractFuncChannel<TParam, TResult> : ScriptableObject
{
    [SerializeField, TextArea] private string description;
    public Func<TParam, TResult> OnRequest;
    public TResult Request(TParam param)
    {
        return OnRequest == null ? default(TResult) : OnRequest.Invoke(param);
    }
}