using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Aca estoy planteando depende estado de juego activar diferente input
/// </summary>
public class GameManager : MonoBehaviour
{
    private FiniteStateMachine<GameState> _fsm;
    private readonly Dictionary<GameState, IState> _gameStates =  new Dictionary<GameState, IState>();
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

    private void ChangeState(GameState newState)
    {
        _fsm.TransitionTo(_gameStates[newState]);
    }
}





public enum GameState
{
    Menu,
    Loading,
    Game
}
