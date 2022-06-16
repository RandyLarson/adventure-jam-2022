using Assets.Scripts.Extensions;
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
        FindRenderers();
        Initialize();
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

    private void Awake()
    {
        FindRenderers();
    }

    private void FindRenderers()
    {
        Renderers = GetComponentsInChildren<SpriteRenderer>();
        TextRenderers = GetComponentsInChildren<TextMeshProUGUI>();
        CanvasRenderers = GetComponentsInChildren<CanvasRenderer>();
    }

    public void SetFadeTo(float amt)
    {
        if (Renderers != null)
        {
            foreach (var sr in Renderers)
            {
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, amt);
            }
        }

        if (TextRenderers != null)
        {
            foreach (var tr in TextRenderers)
            {
                tr.alpha = amt;
            }
        }

        if (CanvasRenderers != null)
        {
            foreach (var cr in CanvasRenderers)
            {
                cr.SetAlpha(amt);
            }
        }
    }

    public void StartFade(bool fadeOut)
    {
        FadeStartTime = Time.time;
        IsActive = true;
        IsFadingOut = fadeOut;

        if (IsFadingOut)
        {
            if (Renderers.Length > 0)
                LerpFrom = Renderers[0].color.a;
            else
                LerpFrom = 1;
        }
        else
        {
            LerpFrom = 0;
            SetFadeTo(LerpFrom);
        }
    }

    public void ShowAll()
    {
        SetFadeTo(1);
    }

    public void Reset()
    {
        IsActive = false;
    }

    float LerpFrom = 1;

    void Update()
    {
        if (!IsActive)
            return;

        var beginFadeAfterAdj = BeginFadeAfter * Time.timeScale;
        var fadeDurationAdj = FadeOutDuration * Time.timeScale;
        var endFadeAtAdj = FadeStartTime + beginFadeAfterAdj + fadeDurationAdj;

        if (Time.time - FadeStartTime > beginFadeAfterAdj)
        {
            float lerpTo = IsFadingOut ? 0 : 1;
            float scale = Mathf.Lerp(LerpFrom, lerpTo, (Time.time - (FadeStartTime + beginFadeAfterAdj)) / fadeDurationAdj);

            SetFadeTo(scale);

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
