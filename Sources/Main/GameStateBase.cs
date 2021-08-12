using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameStateBase
{
    protected GameState _gameStateType;
    public abstract void Init(object value = null);
    public abstract void Release();
    public GameState GetGameState()
    {
        return _gameStateType;
    }
}
