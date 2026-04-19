using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class AsyncFiniteStateMachine<Tkey>
{
    StateNode currentNode;
    readonly Dictionary<Tkey, StateNode> nodes = new();
    readonly HashSet<AsyncTransition> anyTransitions = new();
    readonly Dictionary<IAsyncState, StateNode> stateLookup = new();
    bool _isTransitioning;

    public IAsyncState CurrentState => currentNode?.State;

    #region Unity Life

    public void Update()
    {
        if (_isTransitioning || currentNode == null)
            return;

        var transition = GetTransition();

        if (transition != null)
        {
            _ = HandleTransitionAsync(transition);
            return;
        }

        currentNode.State?.Update();
    }
    
    private async Task HandleTransitionAsync(AsyncTransition transition)
    {
        if (_isTransitioning) return;

        _isTransitioning = true;

        try
        {
            var nextNode = stateLookup[transition.To];

            await ChangeState(nextNode);

            foreach (var node in nodes.Values) ResetActionPredicateFlags(node.Transitions);

            ResetActionPredicateFlags(anyTransitions);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            _isTransitioning = false;
        }
    }
    

    #endregion

    
    


    private class StateNode
    {
        public Tkey Key { get; }
        public IAsyncState State { get; }
        public HashSet<AsyncTransition> Transitions { get; }

        public StateNode(Tkey key, IAsyncState state)
        {
            Key = key;
            State = state;
            Transitions = new HashSet<AsyncTransition>();
        }

        public void AddTransition<T>(Tkey toKey, T predicate, Func<Tkey, StateNode> getNode)
            where T : IPredicate
        {
            var toNode = getNode(toKey);
            Transitions.Add(new AsyncTransition<T>(toNode.State, predicate));
        }
    }



    private async Task ChangeState(StateNode nextNode)
    {
        if (currentNode == nextNode) return;
        

        var previous = currentNode;

        if (previous?.State != null) await previous.State.OnExit();

        currentNode = nextNode;

        if (currentNode.State != null) await currentNode.State.OnEnter();
        
    }

    private void SetState(Tkey key)
    {
        currentNode = GetNode(key);
        _ = currentNode.State?.OnEnter(); 
    }

    private void AddState(Tkey key, IAsyncState state)
    {
        if (nodes.ContainsKey(key)) return;
        var node = new StateNode(key, state);

        nodes[key] = node;
        stateLookup[state] = node;
    }

    private void AddTransition<T>(Tkey from, Tkey to, T condition) where T : IPredicate
    {
        GetNode(from).AddTransition(to, condition, GetNode);
    }

    private void AddAnyTransition(Tkey to, IPredicate condition)
    {
        var toNode = GetNode(to);
        anyTransitions.Add(new AsyncTransition<IPredicate>(toNode.State, condition));
    }

    private AsyncTransition GetTransition()
    {
        foreach (var t in anyTransitions) if (t.Evaluate()) return t;

        foreach (var t in currentNode.Transitions) if (t.Evaluate()) return t;

        return null;
    }

    private StateNode GetNode(Tkey key)
    {
        return !nodes.TryGetValue(key, out var node) ? throw new Exception($"State {key} not registered") : node;
    }

    public async Task TransitionTo(Tkey key)
    {
        if (_isTransitioning) return;

        var nextNode = GetNode(key);
        await ChangeState(nextNode);
    }

    private static void ResetActionPredicateFlags(IEnumerable<AsyncTransition> transitions)
    {
        foreach (var t in transitions.OfType<AsyncTransition<ActionPredicate>>())
            t.condition.flag = false;
    }
    

    public class Builder
    {
        private readonly AsyncFiniteStateMachine<Tkey> _machine = new();
        private readonly Dictionary<Tkey, IAsyncState> _states = new();

        public Builder State<TState>(Tkey key, TState instance)
            where TState : IAsyncState
        {
            _states.TryAdd(key, instance);
            return this;
        }

        private IAsyncState Get(Tkey key)
        {
            return !_states.TryGetValue(key, out var s) ? throw new InvalidOperationException($"State {key} not registered.") : s;
        }

        public Builder At(Tkey from, Tkey to, IPredicate condition)
        {
            _machine.AddTransition(from, to, condition);
            return this;
        }

        public Builder Bidirectional(Tkey a, Tkey b, IPredicate forward, IPredicate backward)
        {
            if (EqualityComparer<Tkey>.Default.Equals(a, b)) throw new ArgumentException("Keys cannot be the same.");

            _machine.AddTransition(a, b, forward);
            _machine.AddTransition(b, a, backward);

            return this;
        }

        public Builder Any(Tkey to, IPredicate condition)
        {
            _machine.AddAnyTransition(to, condition);
            return this;
        }

        public AsyncFiniteStateMachine<Tkey> Build(Tkey initialKey)
        {
            foreach (var kv in _states) _machine.AddState(kv.Key, kv.Value);

            if (!_states.ContainsKey(initialKey)) throw new InvalidOperationException($"Initial state {initialKey} not registered.");
            _machine.SetState(initialKey);
            return _machine;
        }
    }
}
public abstract class AsyncTransition 
{
    public IAsyncState To { get; protected set; }
    public abstract bool Evaluate();
    
}



public class AsyncTransition<T>  : AsyncTransition where T : IPredicate
{
    public readonly T condition;

    public AsyncTransition(IAsyncState to, T condition) {
        To = to;
        this.condition = condition;
    }
    public override bool Evaluate()  => condition.Evaluate();
}


public interface IAsyncState
{
    Task OnEnter();
    void Update(); Task OnExit();
}