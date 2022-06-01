using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GamePreferences", menuName = "ScriptableObjects/GamePreferences", order = 1)]
public class GamePreferences : ScriptableObject
{
    public bool PreferenceA = true;
    public bool PreferenceB = false;
    public int CountA = 12;

    public string[] LevelProgression;
}
