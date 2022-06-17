using Assets.Scripts.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WipeController : MonoBehaviour
{
    public GameObject WipeObject;
    public float StartScale = .001f;
    public float EndScale = 10f;
    public bool ReturnToStart = true;
    public UnityEvent OnBegin;
    public UnityEvent OnBeginningOfReturn;
    public UnityEvent OnComplete;
    public float SpinRevolutions = 3;

    public float BeginAfter = 5;
    public float PauseBeforeReturn = 1;
    public float Duration = 4;
    public bool AutoStart = false;
    public bool DestroyWhenDone = false;
    public bool DisableWhenDone = true;

    private float StartTime { get; set; } = 0;
    public bool IsActive = false;

    public bool DbgActivate = false;

    private bool HaveStartedReturn = false;
    private float AngleLerpFrom = 1;
    private float AngleLerpTo = 1;
    private int AngleModifier = 1;
    private bool CalledOnBegin = false;
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
        LerpFrom = StartScale;
        AngleLerpFrom = 0;
        AngleLerpTo = SpinRevolutions * 360;
        WipeObject.SafeSetActive(true);
        transform.localScale = new Vector3(StartScale, StartScale, 1);
        AudioController.Current.PlayRandomSound(Sounds.Spiral);
    }

    float LerpFrom = 1;

    void Update()
    {
        if (DbgActivate)
        {
            DbgActivate = false;
            Activate();
            return;
        }

        if (!IsActive)
            return;

        var endAt = StartTime + BeginAfter + Duration;

        if (Time.time - StartTime > BeginAfter)
        {
            if ( CalledOnBegin == false )
            {
                CalledOnBegin = true;
                OnBegin?.Invoke();
            }

            float lerpSegmentDuration = ReturnToStart ? Duration / 2 : Duration;

            float lerpProgression = (Time.time - (StartTime + BeginAfter)) / lerpSegmentDuration;
            float lerpTo = EndScale;
            float scale = Mathf.Lerp(LerpFrom, lerpTo, lerpProgression);

            if (SpinRevolutions > 0)
            {
                float rotation = Mathf.Lerp(AngleLerpFrom, AngleLerpTo, (Time.time - (StartTime + BeginAfter)) / Duration);
                rotation *= AngleModifier;
                transform.rotation = Quaternion.AngleAxis(rotation, Vector3.forward);
            }

            transform.localScale = new Vector3(scale, scale, 1);

            if (ReturnToStart && !HaveStartedReturn && (Time.time - StartTime) > Duration / 2)
            {
                HaveStartedReturn = true;
                EndScale = StartScale;
                LerpFrom = scale;

                AngleModifier *= -1;

                StartTime = Time.time;
                BeginAfter = PauseBeforeReturn;
                AudioController.Current.PlayRandomSound(Sounds.Spiral);
                OnBeginningOfReturn?.Invoke();
            }

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

                OnComplete?.Invoke();
            }
        }
    }
}
