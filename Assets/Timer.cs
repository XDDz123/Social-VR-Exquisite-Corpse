using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private TextMesh textMesh;
    private DrawableSurface canvas;

    private void Start()
    {
        textMesh = GetComponent<TextMesh>();
        GameObject go = GameObject.Find("Board");
        canvas = go.GetComponent<DrawableSurface>();
    }

    void Update()
    {
        if (canvas.count_down_start)
        {
            DisplayTime(canvas.time_remaining);
        }
        
        if (canvas.player_remaining == 0)
        {
            textMesh.text = "--:--";
        }
    }
    void DisplayTime(float timeToDisplay)
    {
        // display time function from
        // https://gamedevbeginner.com/how-to-make-countdown-timer-in-unity-minutes-seconds/
        timeToDisplay += 1;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        textMesh.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}