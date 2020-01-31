using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PKTools.Timer;

public class TimerExample : MonoBehaviour
{
    TimerEvent e;

    // Start is called before the first frame update
    void Start()
    {
        e = TimerEvent.Create("Simple", 5.0f)
            .SetAutoKill(true)
            .SetCountingWay(Counting.CountUp)
            .OnUpdate((float obj) => Debug.Log("Time elapsed: " + obj.ToString() + "s."))
            .OnPause(() => Debug.Log(e.Tag + " is paused."))
            .OnResume(() => Debug.Log(e.Tag + " is resumed."))
            .OnCancel(() => Debug.Log(e.Tag + " is canceled"))
            .OnComplete(() => Debug.Log("Time's up"));
    }

    // Update is called once per frame
    void Update()
    {
        // Two ways to get the timer event.
        if (Input.GetKeyDown(KeyCode.Space)) { TimerEvent.GetTimer("Simple").Play(); }
        if (Input.GetKeyDown(KeyCode.P))     { e.Pause(); }

        if (Input.GetKeyDown(KeyCode.R))     { TimerEvent.GetTimer("Simple").Resume(); }
        if (Input.GetKeyDown(KeyCode.C))     { TimerEvent.GetTimer("Simple").Cancel(); }
        if (Input.GetKeyDown(KeyCode.X))     { TimerEvent.Destroy("Simple"); }
    }
}
