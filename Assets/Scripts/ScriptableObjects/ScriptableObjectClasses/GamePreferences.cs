using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GamePreferences", menuName = "ScriptableObjects/GamePreferences", order = 1)]
public class GamePreferences : ScriptableObject
{
    public string[] LevelProgression;

    public GameSettings GameSettings;
    public ColorAndThemeParameters ColorAndTheme;
    public Environment Environment;
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

[Serializable]
public class Environment
{
    [Tooltip("The multiplier to use for falling items.")]
    public float Gravity = 500f;

    [Tooltip("A vertical offset to apply to a transform when it jumps onto something. This is added after the tranform's " +
        "destination has been set. It provides a bit of a hoping action on screen.")]
    public float JumpLandingPointYOffset = -150;
    public float JumpSpeed = 100;
    public float MaxJumpDistance = 100;
    public float JumpDurationSeconds = 2;


    public float MinimumDistanceOfInteraction = 350;

    [Tooltip("Distances smaller than this are treated as zero.")]
    public float TreatAsZero = .01f;

    [Tooltip("The radius around a given point to look for items that may be picked up or interacted " +
        "with. Generally used when a single world position is given (via click) and we want to locate " +
        "things arond there that we can pick up or interact with.")]
    public float PickableItemDetectionRadius = 1f;
    public float PotObstructionRadius = 5f;
    public float PotObstructionDisplacement = 100f;
    public float StoppingDistanceFromTarget = 70;
    public float DropItemVerticalDisplacement = -10;
    public float DropItemRangeFromSurfaceMax = 50;
}
