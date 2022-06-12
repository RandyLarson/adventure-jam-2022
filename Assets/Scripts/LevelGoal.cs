using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class LevelGoal : MonoBehaviour
{
    public string GoalName = "Goal Name";
    public bool IsComplete = false;

    [Tooltip("Once complete, can the goal become un-complete?")]
    public bool CanGoalRegress = true;
    public LevelGoal[] DependentGoals;
    public UnityEvent OnGoalSuccess;
    public UnityEvent OnGoalRegression;

    public void SetComplete()
    {
        SetCompletionStatus(true, false);
    }
    public void SetInComplete()
    {
        SetCompletionStatus(false, false);
    }
    
    public void SetCompletionStatus(bool to, bool force=false)
    {
        bool priorVal = IsComplete;

        // Only go to false if forced to or theis goal can regress from true.
        if (to == true)
        {
            IsComplete = true;
        }
        else
        {
            if (force || CanGoalRegress)
                IsComplete = to;
        }

        if ( priorVal != IsComplete )
        {
            if (IsComplete)
                OnGoalSuccess?.Invoke();
            else
                OnGoalRegression?.Invoke();
        }
    }

    private void Update()
    {
        if ( (IsComplete == false || CanGoalRegress) && DependentGoals.Length > 0)
        {
            bool isComplete = DependentGoals.All(g => g.IsComplete);
            SetCompletionStatus(isComplete, false);
        }
    }
}


