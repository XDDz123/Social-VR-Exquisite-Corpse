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
    private NetworkContext context;
    public Button left;
    public Button right;
    public GameObject menu;

    private struct Message
    {
        public bool flag;
        public bool left;
        public bool right;

        public Message(bool flag, bool left, bool right)
        {
            this.flag = flag;
            this.left = left;
            this.right = right;
        }
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
        }
    }

    public void Start()
    {
        context = NetworkScene.Register(this);

        RoomClient.Find(this).OnJoinedRoom.AddListener(OnRoom);
        Reset();
    }

    public void OnClickLeft()
    {
        onSideChanged?.Invoke(2);

        right.enabled = false;
        left.interactable = false;

        context.SendJson(new Message(false, left.interactable, right.interactable));
    }

    public void OnClickRight()
    {
        onSideChanged?.Invoke(1);

        left.enabled = false;
        right.interactable = false;

        context.SendJson(new Message(false, left.interactable, right.interactable));
    }

    void OnRoom(IRoom other)
    {
        Reset();

        // send message to request the state of the obj from other players
        context.SendJson(new Message(true, left.enabled, right.enabled));
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();

        // flag for state request 
        if (data.flag)
        {
            context.SendJson(new Message(false, left.interactable, right.interactable));
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

        context.SendJson(new Message(false, left.interactable, right.interactable));
    }
}
