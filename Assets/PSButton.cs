using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ubiq.Messaging;



public class PSButton : MonoBehaviour
{
    private NetworkContext context;
    public Button button;
    public Button OtherButton;
    public int side;

    void Start()
    {
        context = NetworkScene.Register(this);
        Button btn = button.GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        context.SendJson(new Message(false));
        button.interactable = false;
        OtherButton.enabled = false;

        GameObject GO = GameObject.Find("Board");
        DrawableSurface canvas = GO.GetComponent<DrawableSurface>();

        canvas.side(side);

        GameObject go = GameObject.Find("Player Selector");
        go.SetActive(false);
    }
    /*
    void Update()
    {
        if (!button.interactable && !OtherButton.interactable)
        {
            GameObject go = GameObject.Find("Player Selector");
            go.SetActive(false);
        }
    }
    */

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

        // disable button
        button.interactable = false;
    }

}
