using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;


[Serializable]
public class StackManager<T> where T : IActivity 
{
    [ShowInInspector,ReadOnly]private readonly Stack<T> _stack = new Stack<T>();

    public void Push(T item)
    {
     
        if (_stack.TryPeek(out var current)) current.Pause();
        
        _stack.Push(item);
        item.Resume();
    }

    public void Pop() => InternalPop(0);

    public void PopSaveRoot() => InternalPop(1);


    private void InternalPop(int minCount)
    {
        if (_stack.Count <= minCount) return;
        
        var top = _stack.Pop();
        top.Stop();
        
        if (_stack.TryPeek(out var next)) next.Resume();
        
    }

    public void Clear()
    {
        while (_stack.Count > 0) _stack.Pop().Stop();
        
    }
}