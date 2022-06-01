using UnityEngine;

[CreateAssetMenu(fileName = "CurrentGameData", menuName = "ScriptableObjects/CurrentGameData", order = 1)]
public class CurrentGameData : ScriptableObject
{
    public string PlayerName;
    public float Hp;

}
