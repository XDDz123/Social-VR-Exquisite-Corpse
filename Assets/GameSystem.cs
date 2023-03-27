using System.Collections.Generic;
using System.Collections;
using System;
using Ubiq.Messaging;
using UnityEngine.Events;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    private NetworkContext _context;

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

    [Serializable]
    private struct Message
    {
        public State state;
    }

    public void ResetGame()
    {
        SwitchState(State.Prepare);
    }

    public void BeginGame()
    {
        SwitchState(State.InProgress);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var args = JsonUtility.FromJson<Message>(msg.ToString());

        SwitchState(args.state);
    }

    public void UpdateGameLength(float length)
    {
        _gameLength = length;
    }

    void Start()
    {
        _context = NetworkScene.Register(this);
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
        _context.SendJson(new Message() { state = _state });
        onGameStateChanged?.Invoke(state);
    }
}
