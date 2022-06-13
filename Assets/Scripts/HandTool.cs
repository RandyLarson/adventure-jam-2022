using Assets.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Events;

public class HandTool : MonoBehaviour
{
    public string Name;
    [Tooltip("The radius around a given point to look for items that may be picked up or interacted " +
        "with. Generally used when a single world position is given (via click) and we want to locate " +
        "things arond there that we can pick up or interact with.")]
    public float PickableItemDetectionRadius = 10f;

    public SpriteRenderer HandVisual;

    public GameObject HeldItemParentLocation;

    [ReadOnly]
    public Collider2D OurCollider;

    public GameObject CurrentlyHolding;
    public GameObject PotentialPickup;
    public GameObject PotentialPlacementSpot;

    public UnityEvent<RoomItem> OnPickupItem;
    public UnityEvent<RoomItem> OnReleaseItem;

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

        if (TryToPickupItem())
        {

        }
    }

    public bool AttemptPickup(GameObject toPickup)
    {
        AttemptPickup(toPickup, null);
        return CurrentlyHolding != null;
    }

    public bool TryToPickupItem()
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

    internal RoomItem LookForRoomItemAtLocation(Vector3 atPosition)
    {
        int numHits = Physics2D.OverlapCircle(atPosition, PickableItemDetectionRadius, ColliderFilter, ColliderHits);

        for (int i = 0; i < numHits; i++)
        {
            Collider2D item = ColliderHits[i];
            var asRoomItem = item.GetComponentInParent<RoomItem>();
            if (asRoomItem != null)
            {
                return asRoomItem;
            }
        }
        return null;
    }


    internal bool IsValidToPickup(GameObject item)
    {
        var asRoomItem = GetRoomItem(item, true);
        if (asRoomItem != null)
        {
            return asRoomItem.CanBeHandledDirectly;
            //return true;
        }

        return false;
    }


    internal GameObject LookForPickableItemAtLocation(Vector3 atPosition)
    {
        int numHits = Physics2D.OverlapCircle(atPosition, PickableItemDetectionRadius, ColliderFilter, ColliderHits);

        for (int i = 0; i < numHits; i++)
        {
            Collider2D item = ColliderHits[i];
            if (IsValidToPickup(item.gameObject))
                return item.gameObject;
        }
        return null;
    }

    private bool TryToUseHeldItem()
    {
        bool itemWasUsed = false;
        if (CurrentlyHolding != null && OurCollider != null)
        {
            int numHits = OurCollider.OverlapCollider(ColliderFilter, ColliderHits);

            for (int i = 0; i < numHits; i++)
            {
                Collider2D item = ColliderHits[i];

                var asRoomItem = item.GetComponentInParent<RoomItem>();
                if (asRoomItem != null)
                {
                    if (asRoomItem.TestForInteractionWith(CurrentlyHolding, true))
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

    public bool TryToUseHeldItemOn(GameObject whatToUseItOn)
    {
        return TryToUseItem(CurrentlyHolding, whatToUseItOn);
    }

    public bool TryToUseItem(GameObject whatToUse, GameObject whatToUseItOn)
    {
        bool itemWasUsed = false;
        if (whatToUse != null && whatToUseItOn != null)
        {
            var asRoomItem = whatToUseItOn.GetComponentInParent<RoomItem>();
            if (asRoomItem != null)
            {
                if (asRoomItem.TestForInteractionWith(CurrentlyHolding, true))
                {
                    itemWasUsed = true;

                    // An item may self-destruct at use time.
                    // Forget we are holding it, if so.
                    if (!CurrentlyHolding.IsValidGameobject())
                        CurrentlyHolding = null;
                }
            }

        }
        return itemWasUsed;
    }

    public bool TryPlaceCurrentItem()
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

        // Will the item fit
        if (!IsValidPlacementSpot(whereToPlace, toPlace))
            return false;

        // Drop the item.
        toPlace.transform.SetParent(null);
        toPlace.transform.rotation = Quaternion.identity;
        OnReleaseItem?.Invoke(GetRoomItem(toPlace));

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

    //private PickableItem GetPickableItem(GameObject gameObject, bool includeParent = true) => GetComponent<PickableItem>(gameObject, includeParent);
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
        var asRoomItem = GetRoomItem(gameObject, true);
        return asHighlightable != null && asRoomItem != null && asHighlightable.enabled;
    }

    private bool ValidToPickup(GameObject gameObject)
    {
        if (null == gameObject)
            return false;

        var asRoomItem = GetRoomItem(gameObject, true);
        return asRoomItem != null && asRoomItem.CanBeHandledDirectly;
    }



    private Transform HeldItemOriginalParent { get; set; }

    private void ShowOrHideHandVisual(bool showIt)
    {
        if (HandVisual != null)
        {
            HandVisual.enabled = showIt;
        }
    }


    public void AttemptPickup(GameObject gameObject, GameObject originalParent)
    {
        if (null == gameObject)
            return;

        var asRoomItem = GetRoomItem(gameObject);
        if (asRoomItem != null && IsCompatibleWithTool(asRoomItem.gameObject))
        {
            if (IsValidToPickup(gameObject) && asRoomItem.CanBeHandledDirectly)
            {
                if (gameObject.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
                {
                    rb.velocity = Vector3.zero;
                }

                CurrentlyHolding = gameObject;
                HeldItemOriginalParent = originalParent != null ? originalParent.transform : CurrentlyHolding.transform.parent;

                if (HeldItemParentLocation != null)
                {
                    CurrentlyHolding.transform.SetParent(HeldItemParentLocation.transform);
                    CurrentlyHolding.transform.SetPositionAndRotation(HeldItemParentLocation.transform.position, HeldItemParentLocation.transform.rotation);
                }
                else
                {
                    CurrentlyHolding.transform.SetParent(transform, true);
                }

                //CurrentlyHolding.transform.localScale = Vector3.one;

                OnPickupItem?.Invoke(asRoomItem);
                AudioController.Current?.PlayRandomSound(Sounds.ItemPickedUp);
            }
        }
    }
}
