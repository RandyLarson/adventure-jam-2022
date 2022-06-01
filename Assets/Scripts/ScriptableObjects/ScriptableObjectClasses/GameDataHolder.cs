using UnityEngine;

[CreateAssetMenu(fileName = "GameDataHolder", menuName = "ScriptableObjects/GameDataHolder", order = 1)]
public class GameDataHolder : ScriptableObject
{
    public GamePreferences GamePrefs;
    public CurrentGameData CurentGameData;
}
