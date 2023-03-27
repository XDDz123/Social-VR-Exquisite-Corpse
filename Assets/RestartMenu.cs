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
    private NetworkContext _context;

    public Slider gameLengthSlider;
    public GameObject PS;
    public Button pen_tp;
    public GameObject pen;

    private bool _silent = false;

    public struct Message
    {
        public bool request;
        public float gameLength;
    };

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

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var args = JsonUtility.FromJson<Message>(msg.ToString());

        if (args.request)
        {
            SendUpdate();
        }
        else
        {
            if (gameLengthSlider.value != args.gameLength) {
                _silent = true;
                gameLengthSlider.value = args.gameLength;
                _silent = false;
            }
        }
    }

    void Start()
    {
        _context = NetworkScene.Register(this);
        RoomClient.Find(this).OnJoinedRoom.AddListener(OnRoom);

        gameLengthSlider.onValueChanged.AddListener(delegate { SendUpdate(); });
        pen_tp.onClick.AddListener(PenListener);

        gameLengthSlider.minValue = 10;
        gameLengthSlider.maxValue = 300;
        gameLengthSlider.value = 10;
    }

    public void PenListener()
    {
        Pen pen_obj = pen.GetComponent<Pen>();
        pen_obj.ResetPosition();
    }

    private void OnRoom(IRoom other)
    {
        _context.SendJson(new Message()
        {
            request = true,
            gameLength = gameLengthSlider.value,
        });
    }

    private void SendUpdate()
    {
        if (_silent) {
            return;
        }

        _context.SendJson(new Message()
        {
            request = false,
            gameLength = gameLengthSlider.value,
        });
    }
}
