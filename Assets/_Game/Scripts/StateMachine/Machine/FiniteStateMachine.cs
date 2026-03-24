using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FiniteStateMachine<Tkey>
{
    StateNode currentNode;
    readonly Dictionary<IState, StateNode> nodes = new();
    readonly HashSet<Transition> anyTransitions = new();
    public IState CurrentState => currentNode.State;

    #region  Unity Life
    
  
    public void Update() {
        var transition = GetTransition();
        if (transition != null) 
        {
            ChangeState(transition.To);
            foreach (var node in nodes.Values) { ResetActionPredicateFlags(node.Transitions); }
            ResetActionPredicateFlags(anyTransitions);
        }
        currentNode.State?.Update();
    }
    public void FixedUpdate() {
        currentNode.State?.FixedUpdate();
    }
    #endregion

    private static void ResetActionPredicateFlags(IEnumerable<Transition> transitions) 
    {
        foreach (var transition in transitions.OfType<Transition<ActionPredicate>>()) transition.condition.flag = false;
    }
    private void SetState(IState state) 
    {
        currentNode = GetOrAddNode(state);
        currentNode.State?.OnEnter();
    }
    private void ChangeState(IState state) 
    {
        if (state == currentNode.State) return;

        var previousState = currentNode.State;
        var nextNode = GetOrAddNode(state);
        
        Debug.Log($"[FSM] {GetType().Name} | {previousState?.GetType().Name ?? "NULL"} -> {nextNode.State.GetType().Name}");
        previousState?.OnExit();
        nextNode.State.OnEnter();
        currentNode = nextNode;
    }
    private void AddState(IState state) => GetOrAddNode(state);
    private void AddTransition<T>(IState from, IState to, T condition) 
    {
        GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
    }

    private void AddAnyTransition<T>(IState to, T condition)
    {
        anyTransitions.Add(new Transition<T>(GetOrAddNode(to).State, condition));
    }

    Transition GetTransition() 
    {
        foreach (var transition in anyTransitions) if (transition.Evaluate()) return transition;

        foreach (var transition in currentNode.Transitions) { if (transition.Evaluate()) return transition; }

        return null;
    }
    StateNode GetOrAddNode(IState state)
    {
        var node = nodes.GetValueOrDefault(state);
        if (node != null) return node;
        node = new StateNode(state);
        nodes[state] = node;
        return node;
    }


    public void TransitionTo(IState state)
    {
       if(nodes.TryGetValue(state, out var node)) ChangeState(node.State);
    }
    private class StateNode 
    {
        public IState State { get; }
        public HashSet<Transition> Transitions { get; }
        public StateNode(IState state) {
            State = state;
            Transitions = new HashSet<Transition>();
        }
        public void AddTransition<T>(IState to, T predicate)
        {
            Transitions.Add(new Transition<T>(to, predicate));
        }
    }
    
    /// <summary>
    /// Fluent builder for constructing and configuring a <see cref="FiniteStateMachine"/>.
    /// Provides a chainable API to register states, define transitions, and build the machine.
    /// </summary>
    public class Builder
    {
        private readonly FiniteStateMachine<Tkey> _machine = new FiniteStateMachine<Tkey>();
        private readonly Dictionary<Tkey, IState> _states = new();

        /// <summary>
        /// Registers a new state instance of type <typeparamref />.
        /// If the state type is already registered, it will not be replaced.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="key"></param>
        /// <param name="instance">
        /// The state instance to register.
        /// </param>
        /// <returns>
        /// The current <see cref="Builder"/> instance for fluent chaining.
        /// </returns>
        public Builder State<TState>(Tkey key, TState instance) where TState : IState
        {
            _states.TryAdd(key, instance);
            return this;
        }
        
        /// <summary>
        /// Gets a previously registered state of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The state type to retrieve.
        /// </typeparam>
        /// <returns>
        /// The registered state instance.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the state has not been registered.
        /// </exception>
        private IState Get(Tkey key)
        {
            return !_states.TryGetValue(key, out var s) ? throw new InvalidOperationException($"State with key {key} not registered.") : s;
        }


        /// <summary>
        /// Adds a transition
        /// that will be evaluated using the given condition.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="condition">
        /// A function that returns true when the transition should occur.
        /// </param>
        /// <param name="from"></param>
        /// <returns>
        /// The current <see cref="Builder"/> instance for fluent chaining.
        /// </returns>
        public Builder At(Tkey from, Tkey to, Func<bool> condition)
        {
            _machine.AddTransition(Get(from), Get(to), condition);
            return this;
        }


        public Builder Bidirectional(Tkey a, Tkey b, Func<bool> forward, Func<bool> backward)
        {
            if (EqualityComparer<Tkey>.Default.Equals(a, b))
                throw new ArgumentException("Keys cannot be the same.");

            _machine.AddTransition(Get(a), Get(b), forward);
            _machine.AddTransition(Get(b), Get(a), backward);
            return this;
        }
        /// <summary>
        /// Adds a global transition to <typeparamref name="TTo"/> that can be triggered
        /// from any state when the condition is met.
        /// </summary>
        /// <typeparam name="TTo">
        /// The target state type.
        /// </typeparam>
        /// <param name="condition">
        /// A function that returns true when the transition should occur.
        /// </param>
        /// <returns>
        /// The current <see cref="Builder"/> instance for fluent chaining.
        public Builder Any(Tkey to, Func<bool> condition)
        {
            _machine.AddAnyTransition(Get(to), condition);
            return this;
        }
        
        /// <summary>
        /// Builds the state machine and sets the initial state.
        /// All registered states will be added to the machine.
        /// </summary>
        /// <typeparam name="TInitial">
        /// The type of the initial state.  
        /// It must be registered via <see cref="State{T}(T)"/> before calling this method.
        /// </typeparam>
        /// <returns>
        /// The fully built <see cref="FiniteStateMachine"/> instance.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the initial state has not been registered.
        /// </exception>
        public FiniteStateMachine<Tkey> Build(Tkey initialKey)
        {
            foreach (var s in _states.Values)
                _machine.AddState(s);

            if (!_states.TryGetValue(initialKey, out var initial))
                throw new InvalidOperationException($"Initial state {initialKey} not registered.");

            Debug.Log("Set State " + initialKey);
            _machine.SetState(initial);

            Debug.Log("Machine Build");
            return _machine;
        }
    }


}