using UnityEngine;

public class Age : MonoBehaviour
{
    public float LifeTime = 60;
    public float BornTime = 0;

    public float CurrentAge { get => Time.time - BornTime; }

    void Start()
    {
        BornTime = Time.time;
    }
}
