using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FiniteStateMachine 
{
    StateNode currentNode;
    readonly Dictionary<Type, StateNode> nodes = new();
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
        var node = nodes.GetValueOrDefault(state.GetType());
        if (node != null) return node;
        node = new StateNode(state);
        nodes[state.GetType()] = node;
        return node;
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
        private readonly FiniteStateMachine _machine = new FiniteStateMachine();
        private readonly Dictionary<Type, IState> _states = new();
        
        /// <summary>
        /// Registers a new state instance of type <typeparamref name="T"/>.
        /// If the state type is already registered, it will not be replaced.
        /// </summary>
        /// <typeparam name="T">
        /// The concrete state type, must implement <see cref="IState"/>.
        /// </typeparam>
        /// <param name="instance">
        /// The state instance to register.
        /// </param>
        /// <returns>
        /// The current <see cref="Builder"/> instance for fluent chaining.
        /// </returns>
        public Builder State<T>(T instance) where T : IState
        {
            _states.TryAdd(typeof(T), instance);
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
        private IState Get<T>() where T : IState => !_states.TryGetValue(typeof(T), out var s) 
            ? throw new InvalidOperationException($"State<{typeof(T).Name}> not registered. Call .State(new {typeof(T).Name}()) first.")
            : s;
        
        
        /// <summary>
        /// Adds a transition from <typeparamref name="TFrom"/> to <typeparamref name="TTo"/>
        /// that will be evaluated using the given condition.
        /// </summary>
        /// <typeparam name="TFrom">
        /// The source state type.
        /// </typeparam>
        /// <typeparam name="TTo">
        /// The target state type.
        /// </typeparam>
        /// <param name="condition">
        /// A function that returns true when the transition should occur.
        /// </param>
        /// <returns>
        /// The current <see cref="Builder"/> instance for fluent chaining.
        /// </returns>
        public Builder At<TFrom, TTo>(Func<bool> condition)
            where TFrom : IState
            where TTo : IState
        {
            _machine.AddTransition(Get<TFrom>(), Get<TTo>(), condition);
            return this;
        }


        public Builder Bidirectional<T0, T1>(Func<bool> forward, Func<bool> backward)   where T0 : IState
            where T1 : IState
        {
            if (typeof(T0) == typeof(T1)) throw new ArgumentException("T0 and T1 cannot be the same state.");
            
            _machine.AddTransition(Get<T0>(), Get<T1>(), forward);
            _machine.AddTransition(Get<T1>(), Get<T0>(), backward);
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
        /// </returns>
        public Builder Any<TTo>(Func<bool> condition) where TTo : IState
        {
            _machine.AddAnyTransition(Get<TTo>(), condition);
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
        public FiniteStateMachine Build<TInitial>() where TInitial : IState
        {
            foreach (var s in _states.Values) _machine.AddState(s);
            if (!_states.TryGetValue(typeof(TInitial), out var initial)) throw new InvalidOperationException($"Initial state <{typeof(TInitial).Name}> not registered.");
            
            Debug.Log("Set State " +  typeof(TInitial).Name);
            _machine.SetState(initial);
            
            
            Debug.Log("Machine Build");
            return _machine;
        }
    }


}