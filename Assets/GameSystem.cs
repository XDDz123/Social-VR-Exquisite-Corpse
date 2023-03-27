using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameSystem : MonoBehaviour
{
    public enum State
    {
        Prepare,
        InProgress,
        Finished,
    }

    private State _state = State.Prepare;
    private float _gameLength = 10;
    private float _timeRemaining;

    [Serializable]
    public class GameStateChangedEvent : UnityEvent<State> {}
    public GameStateChangedEvent onGameStateChanged = new GameStateChangedEvent();

    [Serializable]
    public class TimerChangedEvent : UnityEvent<float> {}
    public TimerChangedEvent onTimerChanged = new TimerChangedEvent();

    public void ResetGame()
    {
        SwitchState(State.Prepare);
    }

    public void BeginGame()
    {
        SwitchState(State.InProgress);
    }

    public void UpdateGameLength(float length)
    {
        _gameLength = length;
    }

    void Start()
    {
        onGameStateChanged?.Invoke(_state);
    }

    void Update()
    {
        if (_state == State.InProgress) {
            _timeRemaining -= Time.deltaTime;

            onTimerChanged?.Invoke(_timeRemaining);

            if (_timeRemaining <= 0.0f) {
                _timeRemaining = 0.0f;

                Debug.Log("Time run out!");
                SwitchState(State.Finished);
            }
        }
    }

    private void SwitchState(State state) 
    {
        if (_state == state) {
            return;
        }

        switch(state) {
            case State.InProgress:
                _timeRemaining = _gameLength;
                break;
        }

        _state = state;
        onGameStateChanged?.Invoke(state);
    }
}
