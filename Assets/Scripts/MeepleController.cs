using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeepleController : MonoBehaviour
{
    public GameObject HeldObjectMountPoint;

    public float WalkingSpeed = 25f;

    [ReadOnly]
    public float CurrentSpeed = 0f;

    [ReadOnly]
    public bool IsJumping = false;

    public bool TryJumping = false;

    public bool IsOnSurface = false;
    public Surface CurrentSurface = null;

    [ReadOnly]
    float JumpTimeStart = 0;

    [ReadOnly]
    float JumpTimeEnd = 0;


    [ReadOnly]
    public int Facing = 1;

    [ReadOnly]
    public Animator OurAnimator;

    [ReadOnly]
    public Rect LevelBounds;

    HandTool HandTool;

    public float StoppingDistanceFromTarget = 10;

    PlayerInput InputActions;
    InputAction InputActionJump;

    // Start is called before the first frame update
    void Start()
    {
        OurAnimator = GetComponentInChildren<Animator>();
        HandTool = GetComponentInChildren<HandTool>();
        InputActions = GetComponentInChildren<PlayerInput>();
        InputActionJump = InputActions.actions[GameConstants.Jump];
    }

    Vector3? InnerCurrentWalkingDestination = null;

    Vector3? CurrentWalkingDestination => InnerCurrentWalkingDestination;

    void SetCurrentWalkingDestination(Vector3? to)
    {
        InnerCurrentWalkingDestination = to;
        DiagnosticController.Current.AddContent("Destination", InnerCurrentWalkingDestination?.ToString() ?? "null");
    }

    [Tooltip("We measure from this item's position to another item's position to figure out if we " +
        "are close enough to interact. This works for ")]
    public GameObject ItemInteractionMeasurementLocation;

    [ReadOnly]
    public RoomItem ItemToInteractWithAtDestination;

    [ReadOnly]
    public RoomItem ItemAtDestination;

    public Vector3 InputVector = Vector3.zero;
    public Vector3 CurrentVector = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        CheckInputs();
        DetectCurrentSurfaceStandingStatus();
        HandleMovement();
    }

    private void CheckInputs()
    {
        TryJumping = InputActionJump != null && InputActionJump.IsPressed();
    }


    void OnPause(InputValue value)
    {
        GameController.TheGameController.ShowPauseMenu();
    }

    void OnJump(InputValue value)
    {
        if (value != null)
            TryJumping = value.isPressed;
    }

    void OnMove(InputValue value)
    {
        if (value != null)
        {
            SetCurrentWalkingDestination(null);

            var moveVal = value.Get<Vector2>().normalized;
            InputVector = moveVal;

            CurrentVector = moveVal;
            CurrentSpeed = WalkingSpeed;
        }
        else
        {
            InputVector = Vector3.zero;
            CurrentVector = Vector3.zero;
            CurrentSpeed = 0;
        }
    }

    void OnFire(InputValue value)
    {
    }

    void OnDropItem(InputValue value)
    {
        HandTool.TryPlaceCurrentItem();
    }

    public void OnClick()
    {
        var pointerPos = Pointer.current.position.ReadValue();
        Vector3 wposition = Camera.main.ScreenToWorldPoint(pointerPos);
        wposition.z = 0;

        DiagnosticController.Current.Clear();
        DiagnosticController.Current.AddContent("Cursor", wposition.ToString());

        // Could be a destination to walk to
        // Could be somthing to interact with (we may need to walk there first).
        // If we are not holding something, we'll probably pick it up or push it.
        // If we are holding something, we'll try to use what we are holding on it.

        AudioController.Current.PlayRandomSound(Sounds.MeepleMoves);
        SetCurrentWalkingDestination(new Vector3(wposition.x, wposition.y, 0));
        List<RoomItem> itemsAtLocation = HandTool.LookForRoomItemsAtLocation(wposition);

        DiagnosticController.Current.AddContent("Items at click", itemsAtLocation.Count);

        ItemToInteractWithAtDestination = null;
        int sortOrderTieBreaker = -10;

        for (int i = 0; i < itemsAtLocation.Count; i++)
        {
            RoomItem item = itemsAtLocation[i];

            DiagnosticController.Current.AddContent($"Item-{i}", item.gameObject.name);

            var res = CanInteractWith(i, item, wposition, HandTool.CurrentlyHolding);

            if (res.answer == true)
            {
                int itemSortOrder = 0;

                if (item.PrimaryCollider.TryGetComponent<SpriteRenderer>(out var sr))
                {
                    itemSortOrder = sr.sortingOrder;
                }

                if (itemSortOrder > sortOrderTieBreaker)
                {
                    sortOrderTieBreaker = sr.sortingOrder;
                    DiagnosticController.Current.AddContent($"{i} Candidate item", $"{item.gameObject.name}, so: {itemSortOrder}");
                    ItemToInteractWithAtDestination = item;
                    ItemAtDestination = item;
                }
            }
            else
            {
                ItemAtDestination = item;
            }
        }
        DiagnosticController.Current.AddContent("Interaction item", ItemToInteractWithAtDestination == null ? "null":ItemToInteractWithAtDestination.name);

    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        Facing *= -1;
        transform.Rotate(Vector3.up, 180);
    }

    /// <summary>
    /// Right is positive.
    /// </summary>
    /// <param name="leftOrRight"></param>
    private void Face(float leftOrRight)
    {
        // Right is positive
        if (leftOrRight >= 0 && Facing < 0)
            Flip();
        else if (leftOrRight < 0 && Facing > 0)
            Flip();
    }

    private void HandleMovement()
    {
        bool doItemInteraction = false;
        bool haveArrived = false;

        if (CurrentWalkingDestination.HasValue)
        {
            //CurrentVector.y = 0;

            if (ItemToInteractWithAtDestination != null)
            {
                // If we are not yet moving, and the item that is clicked on is within reach, just do it.
                if (CurrentSpeed == 0 && ItemToInteractWithAtDestination.AllowActivationWithoutMovingToClickedPoint && IsWithinInteractionDistance(ItemToInteractWithAtDestination))
                {
                    doItemInteraction = true;
                    haveArrived = true;
                }
            }

            if (!doItemInteraction)
            {
                var dxToDst = Mathf.Abs(transform.position.x - CurrentWalkingDestination.Value.x);
                DiagnosticController.Current.AddContent("DxToDst", dxToDst);
                DiagnosticController.Current.AddContent("Stop Distance", GameData.Current.GamePrefs.Environment.StoppingDistanceFromTarget);

                // Have we arrived?
                if (dxToDst < GameData.Current.GamePrefs.Environment.StoppingDistanceFromTarget)
                {
                    haveArrived = true;
                    doItemInteraction = ItemToInteractWithAtDestination != null;
                }
                else
                {
                    CurrentVector = (CurrentWalkingDestination.Value - transform.position).normalized;
                    if (CurrentWalkingDestination.Value.y > transform.position.y)
                        CurrentVector.y = 0;
                    CurrentSpeed = WalkingSpeed;
                }
            }

            if (doItemInteraction)
            {
                HandleItemInteraction(ItemToInteractWithAtDestination.gameObject, CurrentWalkingDestination, HandTool.CurrentlyHolding);
                ItemToInteractWithAtDestination = null;
            }

            if (haveArrived)
            {
                CurrentVector = Vector3.zero;
                CurrentSpeed = 0;
                SetCurrentWalkingDestination(null);
            }
        }
        DiagnosticController.Current.AddContent("Have arrived", haveArrived);

        if (!IsOnSurface || !CurrentSurface.IsElevated)
            CurrentVector.y = 0;

        Vector3 mvTo = transform.position + new Vector3(CurrentSpeed * CurrentVector.x, CurrentSpeed * CurrentVector.y);
        MoveToward(mvTo);
        HandleJumping(CurrentVector);

        OurAnimator.SetBool(GameConstants.IsWalking, CurrentSpeed != 0);
        OurAnimator.SetBool(GameConstants.IsJumping, IsJumping);
    }

    public bool IsWithinInteractionDistance(RoomItem asRoomItem)
    {
        if (asRoomItem == null)
            return false;

        // Are we close enough to interact with the item
        // Find the collider, and if not then use the transform position.
        Vector3 measureTo = asRoomItem.transform.position;
        float dxToItem = 0;

        if (asRoomItem.PrimaryCollider != null)
        {
            Vector3 pt = asRoomItem.PrimaryCollider.ClosestPoint(ItemInteractionMeasurementLocation.transform.position);
            dxToItem = Vector2.Distance(pt, ItemInteractionMeasurementLocation.transform.position);
        }
        else
        {
            dxToItem = Vector2.Distance(measureTo, ItemInteractionMeasurementLocation.transform.position);

            var itsCollider = asRoomItem.gameObject.GetComponentsInChildren<Collider2D>(false);
            for (int i = 0; i < itsCollider.Length; i++)
            {
                if (itsCollider[i].enabled)
                {
                    Vector3 pt = itsCollider[i].ClosestPoint(ItemInteractionMeasurementLocation.transform.position);
                    float d = Vector2.Distance(pt, ItemInteractionMeasurementLocation.transform.position);
                    if (d < dxToItem)
                        dxToItem = d;
                }
            }
        }

        DiagnosticController.Current.AddContent("Dx to item", dxToItem);
        DiagnosticController.Current.AddContent("Dx item", asRoomItem.name);
        DiagnosticController.Current.AddContent("Min Dx", GameController.TheGameData.GamePrefs.Environment.MinimumDistanceOfInteraction);

        if (dxToItem > GameController.TheGameData.GamePrefs.Environment.MinimumDistanceOfInteraction)
        {
            return false;
        }

        return true;
    }

    struct Interaction
    {
        public bool generalAnswer;
        public bool canUseHeldItem;
        public bool answer;
        public bool isWithinDistance;
    }

    private Interaction CanInteractWith(int diagId, RoomItem roomItem, Vector3? pointOfInteraction, GameObject heldItem)
    {
        Interaction res = new Interaction();

        if (null != roomItem)
        {
            res.isWithinDistance = IsWithinInteractionDistance(roomItem);
            // Need to tie-break between picking up and getting atop something (like a stool).
            // Also need to factor in if the thing we are holding can interact with the thing given.
            //

            // Precedence:
            //  1. Held item interacts with item-to-interact-with
            //  2. Climb-onto if the point of interaction is above the surface of the item-to-interact-with (and it can be climbed onto)
            //  3. Pick up if able


            res.generalAnswer = roomItem.CanClimbOnto || roomItem.CanBeHandledDirectly || roomItem.PickingUpActivatesAction;
            res.canUseHeldItem = (heldItem == null) ? false : roomItem.TestForInteractionWith(heldItem, false);

            res.answer = res.generalAnswer;

            if (heldItem != null)
                res.answer = res.canUseHeldItem;
        }

        DiagnosticController.Current.AddContent($"{diagId}- Interaction with", roomItem != null ? roomItem.name : "null");
        DiagnosticController.Current.AddContent($"{diagId}- Can Interact      (dx)", res.isWithinDistance);
        DiagnosticController.Current.AddContent($"{diagId}- Can Interact (general)", res.generalAnswer);
        DiagnosticController.Current.AddContent($"{diagId}- Can Interact (useheld)", res.canUseHeldItem);
        DiagnosticController.Current.AddContent($"{diagId}- Can Interact (overall)", res.answer);

        return res;
    }


    private void HandleItemInteraction(GameObject itemToInteractWith, Vector3? pointOfInteraction, GameObject heldItem)
    {
        if (null == itemToInteractWith)
            return;

        RoomItem asRoomItem = itemToInteractWith.GetComponent<RoomItem>();

        bool canInteractWith = IsWithinInteractionDistance(asRoomItem);
        if (!canInteractWith)
        {
            AudioController.Current.PlayRandomSound(Sounds.ItemTooFarAway);
            return;
        }



        bool isHoldingItem = heldItem != null;
        bool tryUseHeldItem = false;
        bool usedHeldItem = false;
        bool climbOntoItem = false;
        bool pickupItem = false;
        bool pickedUpItem = false;


        // Need to tie-break between picking up and getting atop something (like a stool).
        // Also need to factor in if the thing we are holding can interact with the thing given.
        //

        // Precedence:
        //  1. Held item interacts with item-to-interact-with
        //  2. Climb-onto if the point of interaction is above the surface of the item-to-interact-with (and it can be climbed onto)
        //  3. Pick up if able


        if (asRoomItem.CanClimbOnto || asRoomItem.CanBeHandledDirectly)
        {
            if (asRoomItem.CanClimbOnto && (asRoomItem.CanBeHandledDirectly || asRoomItem.PickingUpActivatesAction))
            {
                pickupItem = true;

                Surface itsSurface = asRoomItem.GetComponentInChildren<Surface>();
                if (itsSurface != null && itsSurface.SurfaceCollider != null)
                {
                    if (pointOfInteraction.HasValue &&
                        pointOfInteraction.Value.y > itsSurface.SurfaceCollider.bounds.max.y &&
                        Vector3.Distance(transform.position, itsSurface.SurfaceCollider.bounds.ClosestPoint(transform.position)) < GamePrefs.Current.Environment.MaxJumpDistance)
                    {
                        climbOntoItem = true;
                        pickupItem = false;
                    }
                }
            }
            else
            {
                climbOntoItem = asRoomItem.CanClimbOnto;
                pickupItem = asRoomItem.CanBeHandledDirectly;
            }
        }

        if (HandTool.CurrentlyHolding != null)
        {
            tryUseHeldItem = true;
            pickupItem = false;
        }

        if (tryUseHeldItem)
        {
            usedHeldItem = HandTool.TryToUseHeldItemOn(itemToInteractWith);
        }

        if (!usedHeldItem && climbOntoItem)
        {
            ClimbOnToItem(asRoomItem);
        }
        else if (!usedHeldItem && pickupItem)
        {
            pickedUpItem = HandTool.AttemptPickup(itemToInteractWith);
        }

        if (asRoomItem.PickingUpActivatesAction && !climbOntoItem && !usedHeldItem)
        {
            GameObject replacementItem = asRoomItem.PerformSelfActivationActions();
            if (replacementItem != null)
            {
                HandTool.AttemptPickup(replacementItem.gameObject);
            }
        }


        // Audio
        // For item use or not.
        if (tryUseHeldItem && !climbOntoItem && !pickupItem)
        {
            if (usedHeldItem)
            {
                AudioController.Current.PlayRandomSound(Sounds.ItemsInteract);
            }
            else
            {
                AudioController.Current.PlayRandomSound(Sounds.ItemsDontInteract);
            }
        }
    }

    private void ClimbOnToItem(RoomItem asRoomItem)
    {
        if (null == asRoomItem)
            return;

        Surface itsSurface = asRoomItem.GetComponentInChildren<Surface>();
        if (itsSurface == null)
            return;

        transform.position = new Vector3(transform.position.x, itsSurface.transform.position.y + GameController.TheGameData.GamePrefs.Environment.JumpLandingPointYOffset, transform.position.z);
    }

    private void HandleJumping(Vector3 mvVector)
    {
        if (IsJumping)
        {
            TryJumping = false;

            // Adjust the vertical position here.
            Vector3 newPos = transform.position;
            newPos.y += GameController.TheGameData.GamePrefs.Environment.JumpSpeed * Time.deltaTime;
            transform.position = newPos;

            // If we've reached the height of the jump, turn off so we can fall to a surface.
            if (Time.time > JumpTimeEnd)
            {
                IsJumping = false;
            }
        }
        else if (TryJumping)
        {
            TryJumping = false;
            //var surfaceRes = IsStandingOnSurface();
            //IsOnSurface = surfaceRes.isOnGround;

            if (!IsOnSurface)
                return;

            // Turn on. Next frame we'll do the jump.
            IsJumping = true;
            JumpTimeStart = Time.time;
            JumpTimeEnd = JumpTimeStart + GameController.TheGameData.GamePrefs.Environment.JumpDurationSeconds;
            AudioController.Current.PlayRandomSound(Sounds.MeepleJumps);
        }
    }

    private void DetectCurrentSurfaceStandingStatus()
    {
        var res = DetectGround(transform.position);

        IsOnSurface = res.foundGround && res.distance == 0;
        CurrentSurface = res.surfaceFound;
    }

    public virtual void MoveToward(Vector3 destination)
    {
        Vector3 mvVector = Vector3.zero;

        if (destination.x != transform.position.x)
        {
            // Flip the villager depending upon the orientation of it to the target object
            Face(destination.x - transform.position.x);

            // Max move delta, contrained by obstructions
            CurrentSpeed = WalkingSpeed;

            mvVector.x = Time.deltaTime * CurrentSpeed;

            if (transform.position.x > destination.x)
                mvVector *= -1;
        }

        if (destination.y != transform.position.y)
        {
            mvVector.y = -(Time.deltaTime * WalkingSpeed);
        }
        else if (!IsJumping)
        {
            var res = DetectGround(transform.position);
            mvVector.y = res.distance;
        }

        Vector3 actualDestination = ClampMoveDistance(transform.position, destination, mvVector);
        if (actualDestination != transform.position)
        {
            //var newPos = Vector2.MoveTowards(transform.position, destination, actualDestination);
            //var posDelta = newPos - (Vector2)transform.position;

            //transform.position = new Vector2(newPos.x, transform.position.y);

            transform.position = actualDestination;
        }
        else
        {
            CurrentSpeed = 0;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(ClosestSurfacePoint, 20);

        if (CurrentWalkingDestination.HasValue)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(CurrentWalkingDestination.Value, 20);

            var bufferPos = Vector2.MoveTowards(CurrentWalkingDestination.Value, transform.position, GameData.Current.GamePrefs.Environment.StoppingDistanceFromTarget);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(bufferPos, 20);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(ItemInteractionMeasurementLocation.transform.position, GameController.TheGameData.GamePrefs.Environment.MinimumDistanceOfInteraction);
    }

    Surface ClosestSurface = null;
    Vector3 ClosestSurfacePoint = Vector3.zero;
    Vector3 CastOffset = new Vector3(0, 0, 0);


    public (bool foundGround, float distance, Vector3 atPos, Surface surfaceFound) DetectGround(Vector3 fromPosition)
    {
        ClosestSurfacePoint = Vector3.zero;
        ClosestSurface = null;

        var dyMax = GameController.TheGameData.GamePrefs.Environment.Gravity * Time.deltaTime;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(fromPosition + CastOffset, 1, Vector3.down, 10 * dyMax + CastOffset.y, GameConstants.LayerMaskDefault);
        Debug.DrawLine(fromPosition + CastOffset - new Vector3(5, 0, 0), fromPosition - new Vector3(5, dyMax + CastOffset.y, 0), Color.blue);

        bool groundDetected = false;
        float dyActual = dyMax;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.CompareTag(GameConstants.Surface))
            {
                // Looking for the shortest distance to 'fall' before we find a surface.

                if (groundDetected == false)
                {
                    ClosestSurfacePoint = hits[i].point;
                }

                groundDetected = true;

                if ((hits[i].distance - CastOffset.y) < dyActual)
                {
                    dyActual = hits[i].distance - CastOffset.y;

                    if (MathF.Abs(dyActual) < GameController.TheGameData.GamePrefs.Environment.TreatAsZero)
                        dyActual = 0;

                    ClosestSurfacePoint = hits[i].point;
                    ClosestSurface = hits[i].collider.gameObject.GetComponent<Surface>();
                }
            }
        }

        Debug.DrawLine(fromPosition + CastOffset, fromPosition - CastOffset - new Vector3(0, dyActual, 0), Color.red);
        return (groundDetected, -dyActual, ClosestSurfacePoint, ClosestSurface);
    }


    private Vector3 ClampMoveDistance(Vector3 source, Vector3 destination, Vector3 wishedDistance)
    {
        //// No movement if the animator is mid jump or wave or some action that prevents horizontal movement.
        //if (OurAnimator.GetBool(GameConstants.AnimatorHashMidNonMovingAction))
        //    return 0;

        //if (IsFalling)
        //    return 0;

        if (wishedDistance == Vector3.zero)
            return source;

        var optimalDestination = source + wishedDistance;

        optimalDestination.x = Mathf.Clamp(optimalDestination.x, LevelBounds.xMin, LevelBounds.xMax);
        optimalDestination.y = Mathf.Clamp(optimalDestination.y, LevelBounds.yMin, LevelBounds.yMax);

        return optimalDestination;
    }

}
