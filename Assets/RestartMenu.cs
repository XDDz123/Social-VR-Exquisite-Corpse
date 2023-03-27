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
    public Slider gameLengthSlider;
    public GameObject PS;
    public Button pen_tp;
    public GameObject pen;

    [Serializable]
    public class GameLengthChangedEvent : UnityEvent<float> {}
    public GameLengthChangedEvent onGameLengthChanged = new GameLengthChangedEvent();

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

    public void OnGameLengthChanged(float time)
    {
        gameLengthSlider.value = time;
    }

    void Start()
    {
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
}
