using Assets.Scripts.Extensions;
using UnityEngine;

public class EnableChildren : MonoBehaviour
{
    public float DelayUntilEnabling = 3f;
    public GameObject[] ToEnable;
    private float EnableAt = 0;

    private void OnEnable()
    {
        DoEnabling(false);
        EnableAt = Time.time + DelayUntilEnabling;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > EnableAt)
            DoEnabling(true);
    }

    void DoEnabling(bool toState)
    {
        for (int i = 0; i < ToEnable.Length; i++)
            ToEnable[i].SafeSetActive(toState);
    }
}
