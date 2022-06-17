using UnityEngine.InputSystem;
using UnityEngine;

[RequireComponent(typeof(MeepleController))]
public class RoomItemHighlighter : MonoBehaviour
{
    MeepleController Controller;

    Collider2D[] ColliderHits = new Collider2D[30];
    ContactFilter2D ColliderFilter;

    Highlightable LastHighlightedItem;

    private void Start()
    {
        ColliderFilter = new ContactFilter2D()
        {
            layerMask = GameConstants.LayerMaskDefault,
            useLayerMask = true,
            useTriggers = true
        };
    }

    void Update()
    {
        if (Controller == null)
        {
            Controller = GetComponent<MeepleController>();
        }

        DetectAndHighlightRoomItems();
    }

    Vector3 LastTestPos = Vector3.one;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(LastTestPos, 5);
    }

    private void DetectAndHighlightRoomItems()
    {
        if (Controller == null)
            return;

        var pointerPos = Pointer.current.position.ReadValue();
        Vector3 wposition = Camera.main.ScreenToWorldPoint(pointerPos);
        wposition.z = 0;
        LastTestPos = wposition;

        int numHits = Physics2D.OverlapPoint(wposition, ColliderFilter, ColliderHits);

        RoomItem closestItem = null;

        for (int i = 0; i < numHits; i++)
        {
            Collider2D item = ColliderHits[i];
            var asRoomItem = item.GetComponentInParent<RoomItem>();
            if (asRoomItem != null)
            {
                closestItem = asRoomItem;
            }
        }

        if (LastHighlightedItem != null && LastHighlightedItem != closestItem)
        {
            LastHighlightedItem.ClearHighlight();
        }

        if (closestItem != null)
        {
            LastHighlightedItem = closestItem.GetComponent<Highlightable>();
            if (LastHighlightedItem != null)
            {
                LastHighlightedItem.UseColorOverride = false;
                if (!Controller.IsWithinInteractionDistance(closestItem))
                {
                    LastHighlightedItem.HighlightColorOverride = Color.red;
                    LastHighlightedItem.UseColorOverride = true;
                }

                LastHighlightedItem.ApplyHighlight();
            }
        }
    }
}
