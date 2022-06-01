using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ClickDetection : MonoBehaviour
{
    [Tooltip("Invoked if non-empty")]
    public UnityEvent<ClickEvent> OnClick = new UnityEvent<ClickEvent>();
    private int? LastFrameClickSent = null;

    private void Start()
    {
    }

    private void Update()
    {

        bool treatAsClick = Pointer.current.press.isPressed && !LastFrameClickSent.HasValue;
        //bool treatAsClick = Mouse.current.leftButton.isPressed && !LastFrameClickSent.HasValue;

        if (!Pointer.current.press.isPressed)
            LastFrameClickSent = null;

        if (treatAsClick)
        {
            LastFrameClickSent = Time.frameCount;
            var pointerPos = Pointer.current.position.ReadValue();
            Vector3 wposition = Camera.main.ScreenToWorldPoint(pointerPos);
            wposition.z = 0;

            OnClick?.Invoke(new ClickEvent(wposition));
        }
    }
}
