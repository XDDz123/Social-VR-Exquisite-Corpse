using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private TextMesh _textMesh;

    public void OnGameStateChanged(GameSystem.State state)
    {
        switch (state) {
            case GameSystem.State.Prepare:
                Reset();
                break;

            case GameSystem.State.Finished:
                if (_textMesh != null) {
                    _textMesh.text = "Game Over";
                }

                break;
        }
    }

    public void OnTimerChanged(float time)
    {
        // display time function from
        // https://gamedevbeginner.com/how-to-make-countdown-timer-in-unity-minutes-seconds/
        time += 1;
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);
        _textMesh.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void Start()
    {
        _textMesh = GetComponent<TextMesh>();
        Reset();
    }

    private void Reset()
    {
        if (_textMesh != null) {
            _textMesh.text = "--:--";
        }
    }
}