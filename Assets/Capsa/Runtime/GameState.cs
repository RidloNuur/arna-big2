using System;
using UnityEngine;

public static class GameState
{
    public enum State
    {
        PAUSED,
        LOBBY,
        LOADING,
        GAMEPLAY,
        GAMEEND
    }

    public static State CurrentState { get; private set; }

    public static Action<State> onStateChanged;

    public static void ChangeState(State newState)
    {
        if (CurrentState == newState)
            Debug.LogWarning("You're transitioning to the same state: " + newState);

        CurrentState = newState;
        onStateChanged?.Invoke(newState);
    }
}
