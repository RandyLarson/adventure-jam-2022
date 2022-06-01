using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ClickEvent
{
    public ClickEvent()
    {
        WorldPosition = Vector3.zero;
    }
    public ClickEvent(Vector3 pos)
    {
        WorldPosition = pos;
    }

    public Vector3 WorldPosition { get; }
}

[RequireComponent(typeof(Collider2D))]
public class ClickedDetection : MonoBehaviour
{
    public Collider2D OurCollider;
    public bool PressActsAsClick = false;
    public bool UseTrigger = false;
    public bool UseUpdate = true;
    public bool RequireMouseToBePressed = true;
    private int LastFrameActedOn = 0;
    public class ButtonClickedEvent : UnityEvent { }

    [Tooltip("Invoked if non-empty")]
    public UnityEvent<ClickEvent> OnClick = new UnityEvent<ClickEvent>();

    private void Start()
    {
        if (OurCollider == null)
            OurCollider = GetComponent<Collider2D>();
    }
    private void Update()
    {
        if (!UseUpdate)
            return;

        bool treatAsClick = !RequireMouseToBePressed || Pointer.current.press.wasPressedThisFrame; //  Mouse.current.leftButton.wasPressedThisFrame;

        if (!treatAsClick && PressActsAsClick && LastFrameActedOn < Time.frameCount)
            treatAsClick = Pointer.current.press.isPressed;

        if (treatAsClick)
        {
            LastFrameActedOn = Time.frameCount;
            var pointerPos = Pointer.current.position.ReadValue();
            Vector3 wposition = Camera.main.ScreenToWorldPoint(pointerPos);
            wposition.z = 0;

            if (OurCollider.OverlapPoint(wposition))
            {
                //Debug.Log($"Mouse press detected. {Time.time}; {Time.frameCount}");
                OnClick?.Invoke(new ClickEvent());
            }

        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        InspectCollision(collision);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        InspectCollision(collision);
    }

    private void InspectCollision(Collider2D collision)
    {
        if (!UseTrigger)
            return;

        if (LastFrameActedOn == Time.frameCount)
            return;

        if (OnClick == null)
            return;

        if (!collision.CompareTag(GameConstants.GamePointer))
            return;

        LastFrameActedOn = Time.frameCount;
        bool treatAsClick = !RequireMouseToBePressed || Mouse.current.leftButton.wasPressedThisFrame;

        if (!treatAsClick && PressActsAsClick)
            treatAsClick = Mouse.current.leftButton.isPressed;

        if (treatAsClick)
        {
            OnClick?.Invoke(new ClickEvent());
        }
    }
}

