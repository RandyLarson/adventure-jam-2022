﻿using Assets.Scripts.Extensions;
using UnityEngine;
using TMPro;

public class FadeOut : MonoBehaviour
{

    [Tooltip("A delay after StartFade has been signaled.")]
    public float BeginFadeAfter = 5;
    [Tooltip("The time the fade/transition portion will take.")]
    public float FadeOutDuration = 4;
    public bool AutoFade = false;
    public bool DestroyWhenFaded = false;
    public bool DisableWhenFaded = true;
    public bool IsFadingOut = true;
    private float FadeStartTime { get; set; } = 0;
    private bool IsActive = false;

    private SpriteRenderer[] Renderers;
    private TextMeshProUGUI[] TextRenderers;
    private CanvasRenderer[] CanvasRenderers;

    public void Start()
    {
        Renderers = GetComponentsInChildren<SpriteRenderer>();
        TextRenderers = GetComponentsInChildren<TextMeshProUGUI>();
        CanvasRenderers = GetComponentsInChildren<CanvasRenderer>();
    }

    public void Initialize()
    {
        if (AutoFade)
            StartFade();
    }
    public void StartFade()
    {
        StartFade(true);
    }

    public void StartFade(bool fadeOut)
    {
        FadeStartTime = Time.time;
        IsActive = true;
        IsFadingOut = fadeOut;
    }

    public void ShowAll()
    {
        if (Renderers != null)
            foreach (var sr in Renderers)
            {
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1);
            }

        if (TextRenderers != null)
            foreach (var tr in TextRenderers)
            {
                tr.alpha = 1;
            }

    }

    public void Reset()
    {
        IsActive = false;
    }

    void Update()
    {
        if (!IsActive)
            return;

        var beginFadeAfterAdj = BeginFadeAfter * Time.timeScale;
        var fadeDurationAdj = FadeOutDuration * Time.timeScale;
        var endFadeAtAdj = FadeStartTime + beginFadeAfterAdj + fadeDurationAdj;

        if (Time.time - FadeStartTime > beginFadeAfterAdj)
        {
            float lerpFrom = IsFadingOut ? 1 : 0;
            float lerpTo = IsFadingOut ? 0 : 1;
            float scale = Mathf.Lerp(lerpFrom, lerpTo, (Time.time - (FadeStartTime + beginFadeAfterAdj)) / fadeDurationAdj);

            foreach (var sr in Renderers)
            {
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, scale);
            }

            foreach (var tr in TextRenderers)
            {
                tr.alpha = scale;
            }

            foreach (var cr in CanvasRenderers)
            {
                cr.SetAlpha(scale);
            }

            if (Time.time > endFadeAtAdj)
            {
                Reset();

                if (IsFadingOut)
                {
                    if (DestroyWhenFaded)
                    {
                        Destroy(gameObject);
                    }
                    else if (DisableWhenFaded)
                    {
                        gameObject.SafeSetActive(false);
                        foreach (var sr in Renderers)
                        {
                            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1);
                        }

                        foreach (var tr in TextRenderers)
                        {
                            tr.alpha = 1;
                        }
                    }
                }
            }
        }
    }

    internal void SetVisibility(bool makeVisible)
    {
        float alpha = makeVisible ? 1.0f : 0.0f;

        foreach (var sr in Renderers)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
        }

        foreach (var tr in TextRenderers)
        {
            tr.alpha = alpha;
        }
    }
}
