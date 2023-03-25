using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ubiq.Messaging;
using Ubiq.Rooms;


public class PSButton : MonoBehaviour
{
    private NetworkContext context;
    public Button button;
    public Button OtherButton;
    public int side;
    private bool clicked = false;

    void Start()
    {
        context = NetworkScene.Register(this);
        RoomClient.Find(this).OnJoinedRoom.AddListener(OnRoom);

        Button btn = button.GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        clicked = true;
        // disable the current button for the remote user
        context.SendJson(new Message(false));
        // disable the current button for the current user
        button.interactable = false;
        // disable the other button for the current user
        OtherButton.enabled = false;

        GameObject GO = GameObject.Find("Board");
        DrawableSurface canvas = GO.GetComponent<DrawableSurface>();

        canvas.side(side);

        //GameObject go = GameObject.Find("Player Selector");
        //go.SetActive(false);
    }

    
    void Update()
    {
        if (!button.interactable && !OtherButton.interactable)
        {
            GameObject go = GameObject.Find("Player Selector");
            go.SetActive(false);
        }
    }

    private struct Message
    {
        public bool flag;

        public Message(bool flag)
        {
            this.flag = flag;
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();

        // flag for state request 
        if (data.flag)
        {
            // send update if current button has been clicked
            if (clicked)
            {
                context.SendJson(new Message(false));
            }
        }
        else
        {
            // disable button
            button.interactable = false;
        }
    }

    void OnRoom(IRoom other)
    {   
        // send message to request the state of the obj from other players
        context.SendJson(new Message(true));
    }

}