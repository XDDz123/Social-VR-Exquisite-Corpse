using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Ubiq.Messaging;
using Ubiq.Rooms;


public class RestartMenu : MonoBehaviour
{
    private NetworkContext context;
    private bool updating = false;

    public Slider gameLengthSlider;
    public GameObject PS;
    public Button pen_tp;
    public GameObject pen;

    [Serializable]
    public class GameLengthChangedEvent : UnityEvent<float> {}
    public GameLengthChangedEvent onGameLengthChanged = new GameLengthChangedEvent();

    private struct Message
    {
        public bool request;
        public float time;

        public Message(bool request, float time)
        {
            this.request = request;
            this.time = time;
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();

        if (!data.request)
        {
            updating = true;
            gameLengthSlider.value = data.time;
            updating = false;
        }
        else
        {
            context.SendJson(new Message(false, gameLengthSlider.value));
        }
    }

    public void OnGameStateChanged(GameSystem.State state)
    {
        switch(state) {
            case GameSystem.State.Prepare:
                goto case GameSystem.State.Finished;

            case GameSystem.State.InProgress:
                gameLengthSlider.interactable = false;
                break;

            case GameSystem.State.Finished:
                gameLengthSlider.interactable = true;
                break;
        }
    }

    void Start()
    {
        context = NetworkScene.Register(this);
        pen_tp.onClick.AddListener(PenListener);
        RoomClient.Find(this).OnJoinedRoom.AddListener(OnRoom);

        gameLengthSlider.onValueChanged.AddListener(delegate { UpdateGameLength(); });
        gameLengthSlider.minValue = 10;
        gameLengthSlider.maxValue = 300;
        gameLengthSlider.value = 10;
    }

    void UpdateGameLength()
    {
        if (!updating)
        {
            onGameLengthChanged?.Invoke(gameLengthSlider.value);
            context.SendJson(new Message(false, gameLengthSlider.value));
        }
    }

    void OnRoom(IRoom other)
    {
        context.SendJson(new Message(true, 0));
    }

    public void PenListener()
    {
        Pen pen_obj = pen.GetComponent<Pen>();
        pen_obj.ResetPosition();
    }
}
