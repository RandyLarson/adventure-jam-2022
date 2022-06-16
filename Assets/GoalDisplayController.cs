using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalDisplayController : MonoBehaviour
{
    public GameObject ShowWhenGoalComplete;
    public GameObject ShowWhenGoalInComplete;
    public LevelGoal Goal;
    public Sounds PlayOnSuccess;

    public bool GoalSuccessProcessed = false;
    void Update()
    {
        if ( Goal != null && Goal.IsComplete && !GoalSuccessProcessed)
        {
            GoalSuccessProcessed = true;
            ShowWhenGoalComplete.SetActive(true);
            ShowWhenGoalInComplete.SetActive(false);
            AudioController.Current.PlayRandomSound(PlayOnSuccess);
        }
    }
}
