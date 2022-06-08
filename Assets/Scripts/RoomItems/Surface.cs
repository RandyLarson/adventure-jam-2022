using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Surface : MonoBehaviour
{
    public Collider2D SurfaceCollider;

    private void Start()
    {
        SurfaceCollider = GetComponent<Collider2D>();
    }
}
