using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CurrentGameData", menuName = "ScriptableObjects/CurrentGameData", order = 1)]
public class CurrentGameData : ScriptableObject
{
    public string PlayerName;
    public string[] Achievements;

    public void AddAchievement(string named)
    {
        if (string.IsNullOrEmpty(named))
            return;

        var tmp = new List<string>();
        if (Achievements != null)
            tmp.AddRange(Achievements);
        tmp.Add(named);
        Achievements = tmp.ToArray();
    }
}
