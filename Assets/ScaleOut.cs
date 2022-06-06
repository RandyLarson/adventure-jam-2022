using Assets.Scripts.Extensions;
using UnityEngine;
using TMPro;

public class ScaleOut : MonoBehaviour
{

    [Tooltip("A delay after activated.")]
    public float BeginAfter = 5;
    [Tooltip("The time the fade/transition portion will take.")]
    public float Duration = 4;
    public bool AutoStart = false;
    public bool DestroyWhenDone = false;
    public bool DisableWhenDone = true;
    private float StartTime { get; set; } = 0;
    public bool IsActive = false;

    public void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (AutoStart)
            Activate();
    }
    public void Activate()
    {
        StartTime = Time.time;
        IsActive = true;
    }

    void Update()
    {
        if (!IsActive)
            return;

        var endAt = StartTime + BeginAfter + Duration;

        if (Time.time - StartTime > BeginAfter)
        {
            float lerpFrom = 1;
            float lerpTo = 0;
            float scale = Mathf.Lerp(lerpFrom, lerpTo, (Time.time - (StartTime + BeginAfter)) / Duration);

            transform.localScale = new Vector3(scale, scale, 0);

            if (Time.time > endAt)
            {
                IsActive = false;
                if (DestroyWhenDone)
                {
                    Destroy(gameObject);
                }
                else if (DisableWhenDone)
                {
                    gameObject.SafeSetActive(false);
                }
            }
        }
    }
}

