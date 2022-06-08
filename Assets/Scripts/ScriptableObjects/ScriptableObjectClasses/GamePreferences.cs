using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GamePreferences", menuName = "ScriptableObjects/GamePreferences", order = 1)]
public class GamePreferences : ScriptableObject
{
    public string[] LevelProgression;

    public GameSettings GameSettings;
    public ColorAndThemeParameters ColorAndTheme;
}


[Serializable]
public class GameSettings
{
    public bool MuteAll = false;
    public float MusicVolume = .5f;
    public float EffectVolume = .8f;
}

[Serializable]
public class ColorAndThemeParameters
{
    [Tooltip("The standard highlight color for items, overlayed atop their sprite.")]
    public Color HighlightColor = new Color(152, 255, 0);
}
