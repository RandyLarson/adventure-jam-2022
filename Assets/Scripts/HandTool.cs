using Assets.Scripts.Extensions;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class HandTool : MonoBehaviour
{
    public string Name;

    public SpriteRenderer HandVisual;

    [ReadOnly]
    public Collider2D OurCollider;

    public GameObject CurrentlyHolding;
    public GameObject PotentialPickup;
    public GameObject PotentialPlacementSpot;

    public UnityEvent<PickableItem> OnPickupItem;
    public UnityEvent OnReleaseItem;

    Collider2D[] ColliderHits = new Collider2D[30];
    ContactFilter2D ColliderFilter;

    private void Start()
    {
        Init();
        OurCollider = GetComponentInChildren<Collider2D>();
    }

    private void Init()
    {
        var lm = 1 << LayerMask.NameToLayer(GameConstants.Default);
        var clm = GameConstants.LayerMaskDefault;

        ColliderFilter = new ContactFilter2D()
        {
            layerMask = lm,
            useLayerMask = true,
            useTriggers = true
        };
    }

    private void LateUpdate()
    {
        UpdateGameState();
    }

    private void UpdateGameState()
    {
        if (CurrentlyHolding != null)
        {
        }
    }


    public void OnClick()
    {
        PickupOrPlaceItem();
    }


    private void PickupOrPlaceItem()
    {

        // Try to let the held item interact with 
        bool itemWasUsed = TryToUseHeldItem();

        // Don't drop or pickup anything if we used what we were holding.
        if (itemWasUsed)
            return;

        if (TryPlaceCurrentItem())
        {
            return;
        }

        if ( TryToPickupItem() )
        {

        }
    }

    private bool TryToPickupItem()
    {
        if (CurrentlyHolding != null)
            return false;

        int numHits = OurCollider.OverlapCollider(ColliderFilter, ColliderHits);
        for (int i = 0; i < numHits && CurrentlyHolding == null; i++)
        {
            Collider2D item = ColliderHits[i];
            var asRoomItem = item.GetComponentInParent<RoomItem>();
            if (asRoomItem != null)
            {
                if (ValidToPickup(asRoomItem.gameObject))
                {
                    AttemptPickup(asRoomItem.gameObject, null);
                }
            }
        }

        return CurrentlyHolding != null;
    }

    private bool TryToUseHeldItem()
    {
        bool itemWasUsed = false;
        if ( CurrentlyHolding != null && OurCollider != null)
        {
            int numHits = OurCollider.OverlapCollider(ColliderFilter, ColliderHits);
            
            for (int i=0; i<numHits; i++)
            {
                Collider2D item = ColliderHits[i];

                var asRoomItem = item.GetComponentInParent<RoomItem>();
                if ( asRoomItem != null )
                {
                    if ( asRoomItem.TestForInteractionWith(CurrentlyHolding, true) )
                    {
                        itemWasUsed = true;

                        // An item may self-destruct at use time.
                        // Forget we are holding it, if so.
                        if (!CurrentlyHolding.IsValidGameobject())
                            CurrentlyHolding = null;
                    }
                }
            }
        }
        return itemWasUsed;
    }

    private bool TryPlaceCurrentItem()
    {
        if (null == CurrentlyHolding)
            return false;


        if (CurrentlyHolding.TryGetComponent(out RoomItem asRoomitem))
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
            return false;

        //var location = whereToPlace.GetComponent<ItemLocation>();
        //if (location == null)
        //    return false

        // Will the item fit
        if (!IsValidPlacementSpot(whereToPlace, toPlace))
            return false;

        // Drop the item.
        toPlace.transform.SetParent(null);

        return true;
    }


    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    CanPickup(collision.gameObject);
    //    CanPlaceItem(collision.gameObject);
    //}

    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    CanPickup(collision.gameObject);
    //    CanPlaceItem(collision.gameObject);
    //}

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    ForgetItem(collision.gameObject);
    //    ForgetPlacement(collision.gameObject);
    //}

    private void ForgetPlacement(GameObject gameObject)
    {
        if (null == gameObject || PotentialPlacementSpot == null)
            return;
    }

    private void ForgetItem(GameObject gameObject)
    {
        var asRoomItem = GetRoomItem(gameObject, true);
        if (asRoomItem.gameObject == PotentialPickup)
        {
            HighlightItem(PotentialPickup, false);
            PotentialPickup = null;
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

        var asRoomItem = GetRoomItem(gameObject, true);
        if (CurrentlyHolding == null && asRoomItem != null && ValidToPickup(asRoomItem.gameObject))
        {
            if (PotentialPickup != asRoomItem.gameObject)
            {
                HighlightItem(PotentialPickup, false);
                PotentialPickup = asRoomItem.gameObject;
                HighlightItem(PotentialPickup, true);
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

                if (gameObject.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
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
