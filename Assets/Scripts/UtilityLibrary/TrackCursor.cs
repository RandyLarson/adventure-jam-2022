using UnityEngine;
using UnityEngine.InputSystem;
public class TrackCursor : MonoBehaviour
{
    public bool TrackPointerLocation = true;

    void Update()
    {
        if (TrackPointerLocation)
        {
            var pointerPos = CursorToWorldCoordinates;
            pointerPos.z = 0;
            transform.position = pointerPos;
        }
    }

    public Vector3 CursorToWorldCoordinates
    {
        get
        {
            var pointerPos = Pointer.current.position.ReadValue();

            return Camera.main.ScreenToWorldPoint(pointerPos);
        }
    }
}
