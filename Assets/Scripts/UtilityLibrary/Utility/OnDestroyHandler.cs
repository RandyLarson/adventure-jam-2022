using UnityEngine;

public class OnDestroyHandler : MonoBehaviour
{
    public bool DestroyParent;
    public GameObject DestroyTarget;

    private void OnDestroy()
    {
        if (DestroyParent && transform.parent != null)
            Destroy(transform.parent.gameObject);

        if ( DestroyTarget != null )
        {
            Destroy(DestroyTarget);
        }

    }


}
