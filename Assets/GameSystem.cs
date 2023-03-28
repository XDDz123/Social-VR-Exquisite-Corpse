using System.Collections.Generic;
using System.Collections;
using System;
using Ubiq.Messaging;
using Ubiq.Rooms;
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
        public bool request;
        public State state;
        public float gameLength;
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

        if (args.request)
        {
            SendUpdate();
        }
        else
        {
            // do not switch states when updating game length
            // using the UpdateGameLength fn would send out another update
            // in the next iteration the function goes into SwitchState in the else case
            // if the state is prepare (because of the added != perpare in return)
            // then then program will attempt to call reset and send out another message
            // since the game lenght is not changing here we'd start looping in switch state
            // so silent is added to update game length
            UpdateGameLength(args.gameLength, silent: true);
            SwitchState(args.state, silent: true);
        }
    }

    public void UpdateGameLength(float length, bool silent = false)
    {
        if (_gameLength == length) {
            return;
        }

        _gameLength = length;

        if (!silent)
        {
            SendUpdate();
        }
    }

    void Start()
    {
        _context = NetworkScene.Register(this);
        RoomClient.Find(this).OnJoinedRoom.AddListener(OnRoom);
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

    private void OnRoom(IRoom other)
    {
        SwitchState(State.Prepare, silent: true);
        _context.SendJson(new Message()
        {
            request = true,
            state = _state,
            gameLength = _gameLength,
        });
    }

    private void SwitchState(State state, bool silent = false)
    {
        // resets the game even if state is already prepare
        if (_state == state && _state != State.Prepare) {
            return;
        }

        switch(state) {
            case State.InProgress:
                _timeRemaining = _gameLength;
                break;
        }

        _state = state;
        onGameStateChanged?.Invoke(state);

        if (!silent) {
            SendUpdate();
        }
    }

    private void SendUpdate()
    {
        _context.SendJson(new Message()
        {
            request = false,
            state = _state,
            gameLength = _gameLength,
        });
    }
}
