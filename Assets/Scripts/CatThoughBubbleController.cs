using Assets.Scripts.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatThoughBubbleController : MonoBehaviour
{
    public LevelGoal AssociatedGoal;

    public float InitialDelayBeforeFirstHint = 6;
    public float IntervalBetweenHints = 2;
    public float HintDisplayDuration = 5;

    public GameObject TheThoughtBubble;
    public GameObject[] HintContent;
    public GameObject DisplayOnGoalAchieved;

    public GameObject CurrentHint;
    public float NextHintTime = 0;
    public int NextHintIndex;

    void Start()
    {
        NextHintTime = Time.time + InitialDelayBeforeFirstHint;
        CalcNextIndex();
    }

    private void Awake()
    {
        NextHintTime = Time.time + InitialDelayBeforeFirstHint;
        CalcNextIndex();
    }

    void CalcNextIndex()
    {
        int lastHint = NextHintIndex;
        do
        {
            NextHintIndex = Random.Range(0, HintContent.Length);
        }
        while (HintContent.Length > 1 && lastHint == NextHintIndex);
    }

    void Update()
    {
        if ( Time.time > NextHintTime)
        {
            if ( !TheThoughtBubble.SafeIsActive() )
            {
                FadeIn(TheThoughtBubble);
            }

            NextHintTime = Time.time + IntervalBetweenHints + HintDisplayDuration;

            FadeOut(CurrentHint);
            CurrentHint = HintContent[NextHintIndex];
            CalcNextIndex();
            FadeIn(CurrentHint);
        }
    }

    void FadeOut(GameObject toFade)
    {
        if (toFade == null)
            return;

        if ( toFade.TryGetComponent<FadeOut>(out var fader))
        {
            fader.StartFade(true);
        }
    }

    void FadeIn(GameObject toFade)
    {
        if (toFade == null)
            return;

        toFade.SafeSetActive(true);

        if (toFade.TryGetComponent<FadeOut>(out var fader))
        {
            fader.SetFadeTo(0);
            fader.StartFade(false);
        }
    }

}
