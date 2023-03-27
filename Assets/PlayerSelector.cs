using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    // Start is called before the first frame update
    public void Start()
    {
        context = NetworkScene.Register(this);
        RoomClient.Find(this).OnJoinedRoom.AddListener(OnRoom);

        Button left_btn = left.GetComponent<Button>();
        left_btn.onClick.AddListener(OnClickLeft);

        Button right_btn = right.GetComponent<Button>();
        right_btn.onClick.AddListener(OnClickRight);

        menu.SetActive(true);

        left.interactable = true;
        right.interactable = true;
        left.enabled = true;
        right.enabled = true;
    }

    void OnClickLeft()
    {
        GameObject GO = GameObject.Find("Board");
        DrawableSurface canvas = GO.GetComponent<DrawableSurface>();
        canvas.side(2);

        right.enabled = false;
        left.interactable = false;

        context.SendJson(new Message(false, left.interactable, right.interactable));
    }

    void OnClickRight()
    {
        GameObject GO = GameObject.Find("Board");
        DrawableSurface canvas = GO.GetComponent<DrawableSurface>();
        canvas.side(1);

        left.enabled = false;
        right.interactable = false;

        context.SendJson(new Message(false, left.interactable, right.interactable));
    }

    // Update is called once per frame
    void Update()
    {
        if (!left.interactable && !right.interactable)
        {
            menu.SetActive(false);
        }
    }

    void OnRoom(IRoom other)
    {
        // reset state
        menu.SetActive(true);
        left.interactable = true;
        right.interactable = true;
        left.enabled = true;
        right.enabled = true;

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
}
