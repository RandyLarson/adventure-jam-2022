using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActivateAfterTime : MonoBehaviour
{
    public float TimerLength = 5f;
    public UnityEvent OnTimerStart;
    public UnityEvent OnTimerEnd;

    public bool DbgStartTimer = false;
    private float TimerEnd;
    private bool TimerStarted = false;
    public void StartTimer()
    {
        if (TimerStarted)
            return;

        TimerStarted = true;
        TimerEnd = Time.time + TimerLength;
        OnTimerStart?.Invoke();
    }

    private void Update()
    {
        if (DbgStartTimer)
        {
            StartTimer();
            DbgStartTimer = false;
        }

        if (TimerStarted && Time.time > TimerEnd)
        {
            TimerStarted = false;
            OnTimerEnd?.Invoke();
        }
    }
}
