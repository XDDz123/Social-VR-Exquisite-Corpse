using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Ubiq.Messaging;
using Ubiq.Rooms;


public class PlayerSelector : MonoBehaviour
{
    private NetworkContext _context;

    public Button left;
    public Button right;
    public GameObject menu;

    private struct Message
    {
        public bool request;
        public bool left;
        public bool right;
    }

    [Serializable]
    public class SideChangedEvent : UnityEvent<int> {}
    public SideChangedEvent onSideChanged = new SideChangedEvent();

    public void OnGameStateChanged(GameSystem.State state)
    {
        switch(state)
        {
            case GameSystem.State.Prepare:
                Reset();
                break;

            case GameSystem.State.InProgress:
                menu.SetActive(false);
                break;

            case GameSystem.State.Finished:
                menu.SetActive(false);
                break;
        }
    }

    public void Start()
    {
        _context = NetworkScene.Register(this);

        RoomClient.Find(this).OnJoinedRoom.AddListener(OnRoom);
        Reset();
    }

    public void OnClickLeft()
    {
        onSideChanged?.Invoke(2);

        right.enabled = false;
        left.interactable = false;

        SendUpdate();
    }

    public void OnClickRight()
    {
        onSideChanged?.Invoke(1);

        left.enabled = false;
        right.interactable = false;

        SendUpdate();
    }

    void OnRoom(IRoom other)
    {
        Reset();

        // send message to request the state of the obj from other players
        _context.SendJson(new Message()
        {
            request = true,
            left = left.enabled,
            right = right.enabled
        });
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();

        if (data.request)
        {
            SendUpdate();
        }
        else
        {
            left.interactable = data.left;
            right.interactable = data.right;
        }
    }

    private void Reset()
    {
        menu.SetActive(true);
        left.interactable = true;
        right.interactable = true;
        left.enabled = true;
        right.enabled = true;
    }

    private void SendUpdate()
    {
        _context.SendJson(new Message()
        {
            request = false,
            left = left.interactable,
            right = right.interactable
        });
    }
}
