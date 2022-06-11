using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Surface : MonoBehaviour
{
    [Tooltip("Elevated surfaces can be moved off of by pressing 'down/s' on the input. " +
        "The floor isn't elevated, so you can't move off of it.")]
    public bool IsElevated = true;
    public Collider2D SurfaceCollider;

    private void Start()
    {
        SurfaceCollider = GetComponent<Collider2D>();
    }
}
