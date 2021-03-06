using Assets.Scripts.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HandTool : MonoBehaviour
{
    public string Name;

    public SpriteRenderer HandVisual;

    public GameObject HeldItemParentLocation;
    public SpriteRenderer HandImageForSortOrder;

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

    internal List<RoomItem> LookForRoomItemsAtLocation(Vector3 atPosition)
    {
        int numHits = Physics2D.OverlapCircle(atPosition, GameData.Current.GamePrefs.Environment.PickableItemDetectionRadius, ColliderFilter, ColliderHits);

        List<RoomItem> hits = new List<RoomItem>();

        for (int i = 0; i < numHits; i++)
        {
            Collider2D item = ColliderHits[i];
            var asRoomItem = item.GetComponentInParent<RoomItem>();
            if (asRoomItem != null)
            {
                hits.Add(asRoomItem);
            }
        }
        return hits;
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


    //internal GameObject LookForPickableItemAtLocation(Vector3 atPosition)
    //{
    //    int numHits = Physics2D.OverlapCircle(atPosition, GameData.Current.GamePrefs.Environment.PickableItemDetectionRadius, ColliderFilter, ColliderHits);

    //    for (int i = 0; i < numHits; i++)
    //    {
    //        Collider2D item = ColliderHits[i];
    //        if (IsValidToPickup(item.gameObject))
    //            return item.gameObject;
    //    }
    //    return null;
    //}

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
                        if (!CurrentlyHolding.IsValidGameobject() || CurrentlyHolding.transform.parent != gameObject)
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
                if (asRoomItem.TestForInteractionWith(whatToUse, true))
                {
                    itemWasUsed = true;

                    // An item may self-destruct at use time.
                    // Forget we are holding it, if so.
                    if (!CurrentlyHolding.IsValidGameobject())
                    {
                        CurrentlyHolding = null;
                    }
                    else if (CurrentlyHolding.transform.parent != gameObject && CurrentlyHolding.transform.parent != HeldItemParentLocation.transform)
                    {
                        ClearHoldingLocationChildren();
                        CurrentlyHolding = null;
                    }
                }
            }

        }
        return itemWasUsed;
    }


    public void ClearHoldingLocationChildren()
    {
        if (null == HeldItemParentLocation)
            return;

        HeldItemParentLocation.transform.DetachChildren();
    }

    public bool TryPlaceCurrentItem()
    {
        if (null == CurrentlyHolding)
        {
            ClearHoldingLocationChildren();
            return false;
        }

        bool res = TryPlaceItem(CurrentlyHolding, PotentialPlacementSpot);

        if (res)
        {
            CurrentlyHolding = null;
            AudioController.Current?.PlayRandomSound(Sounds.ItemPlaced);
            ClearHoldingLocationChildren();
        }

        return res;
    }

    static public (bool foundGround, float distance, Vector3 atPos, Surface surfaceFound) DetectGround(Vector3 fromPosition)
    {
        var dyMax = GameController.TheGameData.GamePrefs.Environment.Gravity * Time.deltaTime;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(fromPosition, 1, Vector3.down, 10 * dyMax, GameConstants.LayerMaskDefault);
        Debug.DrawLine(fromPosition - new Vector3(5, 0, 0), fromPosition - new Vector3(5, dyMax, 0), Color.blue);

        bool groundDetected = false;
        float dyActual = dyMax;

        Vector3 closestPoint = Vector3.zero;
        Surface closestSurface = null;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.CompareTag(GameConstants.Surface))
            {
                // Looking for the shortest distance to 'fall' before we find a surface.

                if (groundDetected == false)
                {
                    closestPoint = hits[i].point;
                }

                groundDetected = true;

                if ((hits[i].distance) < dyActual)
                {
                    dyActual = hits[i].distance;

                    if (Mathf.Abs(dyActual) < GameController.TheGameData.GamePrefs.Environment.TreatAsZero)
                        dyActual = 0;

                    closestPoint = hits[i].point;
                    closestSurface = hits[i].collider.gameObject.GetComponent<Surface>();
                }
            }
        }

        Debug.DrawLine(fromPosition, fromPosition - new Vector3(0, dyActual, 0), Color.red);
        return (groundDetected, -dyActual, closestPoint, closestSurface);
    }


    //public IEnumerator DropItemToNearestSurface(object itemToDrop)
    //{
    //    if (itemToDrop == null)
    //        yield break;

    //    GameObject asGameObject = (GameObject)itemToDrop;
    //    if (null == asGameObject)
    //        yield break;

    //    var res = DetectGround(asGameObject.transform.position);
    //    while (!res.foundGround || res.distance > 0)
    //    {
    //        var dy = GameController.TheGameData.GamePrefs.Environment.Gravity * Time.deltaTime;
    //        if (res.foundGround)
    //        {
    //            dy = Mathf.Min(res.distance, dy);
    //        }

    //        asGameObject.transform.position = new Vector3(asGameObject.transform.position.x, asGameObject.transform.position.y - dy, asGameObject.transform.position.z);
    //        yield return new WaitForEndOfFrame();

    //        res = DetectGround(asGameObject.transform.position);
    //    }

    //    yield break;
    //}

    private bool TryPlaceItem(GameObject toPlace, GameObject whereToPlace)
    {
        if (toPlace == null)
            return false;

        // Will the item fit
        if (!IsValidPlacementSpot(whereToPlace, toPlace))
            return false;

        // Drop the item.
        //StartCoroutine("DropItemToNearestSurface", toPlace);

        if (HandImageForSortOrder != null)
        {
            AdjustLayerOrdersBy(toPlace, -(1 + HandImageForSortOrder.sortingOrder));
        }

        Vector3? surfaceAt = null;

        if (TryGetComponent<MeepleController>(out var mc))
        {
            var res = mc.DetectGround(transform.position);
            if (res.foundGround)
            {
                surfaceAt = res.atPos;
            }
        }


        float newY = surfaceAt.HasValue ?
            surfaceAt.Value.y + Random.Range(0, GameData.Current.GamePrefs.Environment.DropItemRangeFromSurfaceMax) :
            toPlace.transform.position.y + GameData.Current.GamePrefs.Environment.DropItemVerticalDisplacement;

        toPlace.transform.SetParent(null);
        toPlace.transform.rotation = Quaternion.identity;
        toPlace.transform.position = new Vector3(toPlace.transform.position.x, newY, toPlace.transform.position.z);

        GameObject obstruction = LookForPotObstruction(toPlace.transform.position);
        if ( obstruction != null )
        {
            toPlace.transform.position += new Vector3(GameData.Current.GamePrefs.Environment.PotObstructionDisplacement, 0);
        }

        OnReleaseItem?.Invoke(GetRoomItem(toPlace));

        return true;
    }


    internal GameObject LookForPotObstruction(Vector3 atPosition)
    {
        int numHits = Physics2D.OverlapCircle(atPosition, GameData.Current.GamePrefs.Environment.PotObstructionRadius, ColliderFilter, ColliderHits);

        for (int i = 0; i < numHits; i++)
        {
            Collider2D item = ColliderHits[i];
            var asRoomItem = item.GetComponentInParent<RoomItem>();
            if (asRoomItem != null && asRoomItem.CompareTag("Pot"))
                return asRoomItem.gameObject;
        }

        return null;
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

    public void AdjustLayerOrdersBy(GameObject itemToAdjust, int adjustBy)
    {
        var rs = itemToAdjust.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (var sr in rs)
        {
            sr.sortingOrder = sr.sortingOrder + adjustBy;
        }
    }

    public void AttemptPickup(GameObject givenToPickup, GameObject originalParent)
    {
        if (null == givenToPickup)
            return;

        var asRoomItem = GetRoomItem(givenToPickup);
        if (asRoomItem != null && IsCompatibleWithTool(asRoomItem.gameObject))
        {
            if (IsValidToPickup(givenToPickup) && asRoomItem.CanBeHandledDirectly)
            {
                if (givenToPickup.TryGetComponent(out Rigidbody2D rb))
                {
                    rb.velocity = Vector3.zero;
                }

                GameObject toPickup = asRoomItem.ParentObjectToPickUpInLieuOfSelf != null ? asRoomItem.ParentObjectToPickUpInLieuOfSelf : givenToPickup;

                CurrentlyHolding = toPickup;
                if (HandImageForSortOrder != null)
                {
                    AdjustLayerOrdersBy(CurrentlyHolding, 1 + HandImageForSortOrder.sortingOrder);
                }

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
