using Assets.Scripts.Extensions;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Highlightable : MonoBehaviour
{
    [Tooltip("Object will be enabled when highlight is applied, if any")]
    public GameObject EnableOnHighlight;

    [Tooltip("Can be toggled on or off independent of the auto-highlight.")]
    public GameObject ActiveHighlightMarker;

    [Tooltip("Apply the HighlightColor to the item when the highlight is triggered?")]
    public bool ApplyHighlightColor = true;
    public bool UseColorOverride = false;
    public Color HighlightColorOverride = new Color(0x98,0xFF,0x00); //  new Color(1, 0, 0);

    [Tooltip("Will use if populated, otherwise it'll find the SpriteRenderer on the component.")]
    public SpriteRenderer HighlightRenderer;

    [Tooltip("Will apply the highlight when the primary pointer tool enters a collider on this item.")]
    public bool AutoHighlightOnPointer;
    public bool UseSystemCursorLocation = true;

    private Color? NormalColor;

    public Collider2D OurCollider;
    private void Start()
    {
        if ( OurCollider == null )
            OurCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if ( AutoHighlightOnPointer )
        {
            CheckForPointerHighlight();
        }
    }

    private void CheckForPointerHighlight()
    {
        var pointerPos = Pointer.current.position.ReadValue();
        Vector3 wposition = Camera.main.ScreenToWorldPoint(pointerPos);
        wposition.z = 0;

        if (!OurCollider.OverlapPoint(wposition))
        {
            ClearHighlight();
        }
        else
        {
            ApplyHighlight();
        }
    }


    private SpriteRenderer GetRendererToHighlight()
    {
        if (HighlightRenderer != null)
            return HighlightRenderer;
        return GetComponent<SpriteRenderer>();
    }

    public void SetHighlightMarkerActive(bool toBeVisible)
    {
        ActiveHighlightMarker.SafeSetActive(toBeVisible);
    }

    public void ToggleHighlightMarker()
    {
        if (ActiveHighlightMarker != null)
            ActiveHighlightMarker.SafeSetActive(!ActiveHighlightMarker.gameObject.activeSelf);
    }

    public virtual void ApplyHighlight()
    {
        EnableOnHighlight.SafeSetActive(true);
        if (ApplyHighlightColor)
        {
            var sr = GetRendererToHighlight();
            if (sr != null)
            {
                if (NormalColor == null)
                    NormalColor = sr.color;

                if (UseColorOverride)
                    sr.color = HighlightColorOverride;
                else
                    sr.color = GameController.TheGameData.GamePrefs.ColorAndTheme.HighlightColor;
            }
        }
    }

    public virtual void ClearHighlight()
    {
        EnableOnHighlight.SafeSetActive(false);
        if (ApplyHighlightColor)
        {
            var sr = GetRendererToHighlight();
            if (sr != null && NormalColor.HasValue)
            {
                sr.color = NormalColor.Value;
                NormalColor = null;
            }
        }

    }
}