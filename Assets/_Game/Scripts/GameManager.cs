using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;
/// <summary>
/// Aca estoy planteando depende estado de juego activar diferente input
/// </summary>
public class GameManager : PersistentSingleton<GameManager>
{
    private FiniteStateMachine<GameState> _fsm;
    private readonly Dictionary<GameState, IState> _gameStates =  new();
    
    private void Start()
    {
        BuildStateMachine();
    }
    private void BuildStateMachine()
    { 
        var menu = new EmptyState();
        var game = new EmptyState();
       _fsm = new FiniteStateMachine<GameState>.Builder()
           .State(GameState.Menu,menu)
           .State(GameState.Game,game)
           .Build(GameState.Menu);
       _gameStates.TryAdd(GameState.Menu, menu);
       _gameStates.TryAdd(GameState.Game, game);
    }

    private void ChangeState(GameState newState) => _fsm.TransitionTo(_gameStates[newState]); // aca podria suscribirse por un event channel 
    
}

public class EventChannel : ScriptableObject
{
    private event Action OnEventRaised;
    
    public void RegisterListener(Action listener) => OnEventRaised += listener;
    public void UnregisterListener(Action listener) => OnEventRaised -= listener;
    public void Raise() => OnEventRaised?.Invoke();
}


public abstract class AbstractEventChannel<T> : ScriptableObject
{
    public event Action<T> OnEventRaised;
    public void Raise(T value) => OnEventRaised?.Invoke(value);
}


public enum GameState
{
    Menu,
    Loading,
    Game
}
