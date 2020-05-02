using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PKTools.Timer;


public class TimerDemo : MonoBehaviour
{
    private TimerEvent timerEx0;

    void Start()
    {

        timerEx0 = TimerEvent.Create(Counting.Up, 10.0f, "Ex. 00")
                             .OnUpdate(TimerUpdate)
                             .OnPause(TimerPause)
                             .OnResume(TimerResume)
                             .OnCancel(TimerCancel)
                             .OnComplete(TimerComplete)
                             .Play();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) { TimerEvent.Get("Ex. 00").Play(); }
        if (Input.GetKeyDown(KeyCode.Escape)) { TimerEvent.Get("Ex. 00").Cancel(); }
        if (Input.GetKeyDown(KeyCode.P)) { timerEx0.Pause(); }
        if (Input.GetKeyDown(KeyCode.R)) { timerEx0.Resume(); }
    }

    private void TimerComplete()
    {
        Debug.Log("[COMPLETE] - " + timerEx0.Tag + " : Times up!");
    }

    private void TimerUpdate(float time)
    {
        Debug.Log("[UPDATE] - Current pass time: " + time.ToString() + ".");
    }

    private void TimerPause()
    {
        Debug.Log("[PAUSE] - Remaining time: " + timerEx0.RemainingTime + ", Elasped time: " + timerEx0.ElaspedTime);
    }

    private void TimerResume()
    {
        Debug.Log("[RESUME] - Timer ticking. Start at time: " + timerEx0.RemainingTime + "s.");
    }

    private void TimerCancel()
    {
        Debug.Log("[Cancel] - Cancel timer. Remaining time is: " + timerEx0.RemainingTime);
    }
}
