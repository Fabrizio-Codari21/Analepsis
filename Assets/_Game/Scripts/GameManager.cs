using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;
/// <summary>
/// Aca estoy planteando depende estado de juego activar diferente input
/// </summary>
public class GameManager : MonoBehaviour
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


public class ScreenHolder
{
    private readonly Stack<IScreen> _screens = new Stack<IScreen>();

    public void Push(IScreen screen)
    {
        if(_screens.Count > 0) _screens.Peek().Hide(); // si es mayor que 0, oculta el screen actuar
        screen.Show();  // muestra el nuevo pantalla y push a stack
        _screens.Push(screen);
    }


    public void Pop()
    {
        if(_screens.Count <= 1) return; // lo dejo en 1 porque en general hay que dejar un screen de base

        var last = _screens.Pop();
        last.Free(); // libero la pantalla actuar
        
        _screens.Peek().Show(); // y muestro la pantalla lo que estaba bajo de pantalla anterior
    }
    
    
    public void Clear()
    {
        while (_screens.Count > 0)
        {
            var screen = _screens.Pop();
            screen.Free();
        }
    }
}



public enum GameState
{
    Menu,
    Loading,
    Game
}
