using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ubiq.Messaging;
using Ubiq.Rooms;


public class RestartMenu : MonoBehaviour
{
    private NetworkContext context;
    public Button button;
    public Slider time;
    private bool updating;
    public GameObject PS;
    public Button pen_tp;
    public GameObject pen;

    private struct Message
    {
        public bool request;
        public bool flag;
        public float time;

        public Message(bool request, bool flag, float time)
        {
            this.request = request;
            this.flag = flag;
            this.time = time;
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();

        if (!data.request)
        {
            if (data.flag)
            {
                Reset();
            }
            else
            {
                updating = true;
                UpdateTime(data.time);
                UpdateSlider(data.time);
                updating = false;
            }
        } 
        else
        {
            context.SendJson(new Message(false, false, time.value));
        }
    }

    void Update()
    {
        if (PS.activeSelf)
        {
            time.gameObject.SetActive(true);
        } 
        else
        {
            time.gameObject.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        context = NetworkScene.Register(this);
        button.onClick.AddListener(ResetListener);
        pen_tp.onClick.AddListener(PenListener);
        RoomClient.Find(this).OnJoinedRoom.AddListener(OnRoom);

        time.onValueChanged.AddListener(delegate { TimerListener(); });
        time.minValue = 10;
        time.maxValue = 300;
        time.value = 10;

        updating = false;
    }

    void TimerListener() 
    {
        if (!updating)
        {
            UpdateTime(time.value);
            context.SendJson(new Message(false, false, time.value));
        }
    }

    void UpdateTime(float value)
    {
        GameObject GO = GameObject.Find("Board");
        DrawableSurface canvas = GO.GetComponent<DrawableSurface>();
        canvas.timer = Mathf.Round(value);
    }

    void UpdateSlider(float value)
    {
        time.value = value;
    }

    private void Reset()
    {
        GameObject GO = GameObject.Find("Board");
        DrawableSurface canvas = GO.GetComponent<DrawableSurface>();
        canvas.StartHelper();

        GameObject TimerGO = GameObject.Find("Timer");
        Timer timer = TimerGO.GetComponent<Timer>();
        timer.Start();

        // might become null when obj set to inactive
        if (PS != null)
        {
            PlayerSelector ps = PS.GetComponent<PlayerSelector>();
            ps.StartHelper();
        }
    }


    public void ResetListener()
    {
        Reset();
        context.SendJson(new Message(false, true, 0));
    }

    void OnRoom(IRoom other)
    {
        context.SendJson(new Message(true, true, 0));
    }

    public void PenListener()
    {
        Pen pen_obj = pen.GetComponent<Pen>();
        pen_obj.ResetPosition();
    }
}
