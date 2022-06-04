using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class HandTool : MonoBehaviour
{
    public string Name;

    public SpriteRenderer HandVisual;

    public GameObject CurrentlyHolding;
    public GameObject PotentialPickup;
    public GameObject AuxHighlightedItem;
    public GameObject PotentialPlacementSpot;
    public GameObject PotentialContextMenuTarget;

    public UnityEvent<PickableItem> OnPickupItem;
    public UnityEvent OnReleaseItem;

    private bool AttemptPickupOrPlacement { get; set; } = false;
    private void Start()
    {
    }

    private void LateUpdate()
    {
        UpdateGameState();
        if (AttemptPickupOrPlacement)
        {
            AttemptPickupOrPlacement = false;
            PickupOrPlaceItem();
        }
    }

    private void OnDestroy()
    {
        CleanupGameStateOnDestory();
    }

    private void OnDisable()
    {
        CleanupGameStateOnDestory();
    }

    void CleanupGameStateOnDestory()
    {
    }

    private void UpdateGameState()
    {
        if (CurrentlyHolding != null)
        {
        }
    }


    public void OnClick()
    {
        AttemptPickupOrPlacement = true;
    }

    private void PickupOrPlaceItem()
    {
        if (TryPlaceCurrentItem())
        {
            return;
        }

        if (PotentialPickup != null && CurrentlyHolding == null)
        {
            AttemptPickup(PotentialPickup, null);
            HighlightItem(PotentialPickup, false);
            PotentialPickup = null;
        }
    }

    private bool TryPlaceCurrentItem()
    {
        if (null == CurrentlyHolding)
            return false;


        RoomItem asRoomitem = CurrentlyHolding.GetComponent<RoomItem>();
        if (asRoomitem != null)
        {

        }

        var res = TryPlaceItem(CurrentlyHolding, PotentialPlacementSpot);

        if (res)
        {
            CurrentlyHolding = null;
            AudioController.Current?.PlayRandomSound(Sounds.ItemPlaced);
        }

        return res;
    }

    private bool TryPlaceItem(GameObject toPlace, GameObject whereToPlace)
    {
        if (toPlace == null)
            return true;

        //var location = whereToPlace.GetComponent<ItemLocation>();
        //if (location == null)
        //    return false

        // Will the item fit
        if (!IsValidPlacementSpot(whereToPlace, toPlace))
            return false;

        // Drop the item.
        toPlace.transform.SetParent(null);

        return false;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        CanPickup(collision.gameObject);
        CanPlaceItem(collision.gameObject);
        CanHighlight(collision.gameObject, true);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        CanPickup(collision.gameObject);
        CanPlaceItem(collision.gameObject);
        CanHighlight(collision.gameObject, true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        ForgetItem(collision.gameObject);
        ForgetPlacement(collision.gameObject);
    }

    private void ForgetPlacement(GameObject gameObject)
    {
        if (null == gameObject || PotentialPlacementSpot == null)
            return;
    }

    private void ForgetItem(GameObject gameObject)
    {
        if (gameObject == PotentialContextMenuTarget)
        {
            HighlightItem(PotentialContextMenuTarget, false);
            PotentialContextMenuTarget = null;
        }

        if (gameObject == PotentialPickup)
        {
            HighlightItem(PotentialPickup, false);
            PotentialPickup = null;
        }

        if (gameObject == AuxHighlightedItem)
        {
            HighlightItem(AuxHighlightedItem, false);
            AuxHighlightedItem = null;
        }
    }


    private void CanPlaceItem(GameObject potentialLocation)
    {
        if (IsValidPlacementSpot(potentialLocation, CurrentlyHolding))
        {
        }
    }

    private void CanPickup(GameObject gameObject)
    {
        if (!IsCompatibleWithTool(gameObject))
        {
            return;
        }

        if (CurrentlyHolding == null && ValidToPickup(gameObject))
        {
            if (PotentialPickup != gameObject)
            {
                HighlightItem(PotentialPickup, false);
                PotentialPickup = gameObject;
                HighlightItem(PotentialPickup, true);
            }
        }
    }

    private void CanHighlight(GameObject gameObject, bool includeContextMenu)
    {
        // Probably don't want to be able to handle items and activate context menus -- give precedence to context menus.
        if (!IsCompatibleWithTool(gameObject))
            return;

        if (PotentialPickup == null && CurrentlyHolding == null && ValidToHighlight(gameObject))
        {
            if (AuxHighlightedItem != gameObject)
            {
                HighlightItem(AuxHighlightedItem, false);
                AuxHighlightedItem = gameObject;
                HighlightItem(AuxHighlightedItem, true);
            }
        }
    }

    private bool IsCompatibleWithTool(GameObject gameObject)
    {
        if (gameObject != null)
        {
            var asRoomItem = GetRoomItem(gameObject, true);
            if (asRoomItem != null)
                return true;
        }
        return false;
    }

    private void HighlightItem(GameObject item, bool applyIt)
    {
        if (item == null)
            return;

        var highlight = GetHighlightableItem(item);
        if (highlight != null)
        {
            if (applyIt)
                highlight.ApplyHighlight();
            else
                highlight.ClearHighlight();
        }
    }

    private bool IsParentOf(GameObject parentItem, GameObject childItem)
    {
        if (parentItem == null || childItem == null)
            return false;

        if (parentItem == childItem)
            return false;

        // Is the item location a child of whatever we are currently holding?
        Transform current = childItem.transform.parent;
        while (current != null)
        {
            if (current.gameObject == parentItem)
            {
                return true;
            }
            current = current.transform.parent;
        }

        return false;
    }

    private bool IsValidPlacementSpot(GameObject potentialLocation, GameObject toPlace)
    {
        return true;
    }

    private PickableItem GetPickableItem(GameObject gameObject, bool includeParent = true) => GetComponent<PickableItem>(gameObject, includeParent);
    private RoomItem GetRoomItem(GameObject gameObject, bool includeParent = false) => GetComponent<RoomItem>(gameObject, includeParent);

    private Highlightable GetHighlightableItem(GameObject gameObject, bool includeParent = true) => GetComponent<Highlightable>(gameObject, includeParent);

    private T GetComponent<T>(GameObject gameObject, bool includeParent = true)
    {
        var asComp = gameObject.GetComponent<T>();
        if (asComp == null && includeParent && gameObject.transform.parent != null)
            asComp = gameObject.transform.parent.GetComponent<T>();

        return asComp;
    }

    private bool ValidToHighlight(GameObject gameObject)
    {
        if (null == gameObject)
            return false;

        var asHighlightable = GetHighlightableItem(gameObject, true);
        var asPickable = GetPickableItem(gameObject, true);
        return asHighlightable != null && asPickable != null && asHighlightable.enabled;
    }

    private bool ValidToPickup(GameObject gameObject)
    {
        if (null == gameObject)
            return false;

        var asPickable = GetPickableItem(gameObject, true);
        return asPickable != null && asPickable.CanBeHandledDirectly;
    }



    private Transform HeldItemOriginalParent { get; set; }

    private void ShowOrHideHandVisual(bool showIt)
    {
        if (HandVisual != null)
        {
            HandVisual.enabled = showIt;
        }
    }

    private void OnEscape()
    {
    }

    private void AttemptPickup(GameObject gameObject, GameObject originalParent)
    {
        if (null == gameObject)
            return;

        var asPickableItem = GetPickableItem(gameObject);
        if (asPickableItem != null && IsCompatibleWithTool(asPickableItem.gameObject))
        {
            if (asPickableItem.CanBeHandledDirectly)
            {
                //asPickableItem.OnCompatibleCpEntered += Target_OnCompatibleCpEntered;
                //asPickableItem.OnCompatibleCpExit += Target_OnCompatibleCpExit;

                if ( gameObject.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb) )
                {
                    rb.velocity = Vector3.zero;
                }

                asPickableItem.OnItemPlaced += HeldItem_OnItemPlaced;
                CurrentlyHolding = gameObject;
                HeldItemOriginalParent = originalParent != null ? originalParent.transform : CurrentlyHolding.transform.parent;
                CurrentlyHolding.transform.SetParent(transform, true);
                CurrentlyHolding.transform.localScale = Vector3.one;
                if (asPickableItem.ReplacesPlacementUi)
                    CurrentlyHolding.transform.SetPositionAndRotation(transform.position, transform.rotation);

                OnPickupItem?.Invoke(asPickableItem);
                ShowOrHideHandVisual(!asPickableItem.ReplacesPlacementUi);
                asPickableItem.ItemWasPickedUp();

                //if (!asPickableItem.IgnoreSortLayerAdjustmentWhenHeld)
                //    gameObject.SetSortLayer(GameConstants.SortingLayerHeldItems);

                // Play Sound Queue, might be a parameter on the item picked up in future
                AudioController.Current?.PlayRandomSound(Sounds.ItemPickedUp);
            }
        }
    }

    private void HeldItem_OnItemPlaced(object sender, ItemPlacementEvent e)
    {
        var asPickableItem = e.TheItem.GetComponent<PickableItem>();
        asPickableItem.OnItemPlaced -= HeldItem_OnItemPlaced;
        e.TheItem.RestoreSortLayer();
    }
}
