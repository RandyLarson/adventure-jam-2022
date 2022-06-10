using System;
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
        InputActionJump = InputActions.actions["Jump"];
    }

    Vector3? CurrentWalkingDestination = null;

    [Tooltip("We measure from this item's position to another item's position to figure out if we " +
        "are close enough to interact. This works for ")]
    public GameObject ItemInteractionMeasurementLocation;

    [ReadOnly]
    public GameObject ItemToInteractWithAtDestination;
    public Vector3 InputVector = Vector3.zero;
    public Vector3 CurrentVector = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        CheckInputs();
        //IsOnSurface = IsStandingOnSurface();
        HandleMovement();
    }

    private void CheckInputs()
    {
        TryJumping = InputActionJump != null && InputActionJump.IsPressed();
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
            CurrentWalkingDestination = null;

            var moveVal = value.Get<Vector2>().normalized;
            InputVector = moveVal;

            moveVal.y = 0;

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

        // Could be a destination to walk to
        // Could be somthing to interact with (we may need to walk there first).
        // If we are not holding something, we'll probably pick it up or push it.
        // If we are holding something, we'll try to use what we are holding on it.

        AudioController.Current.PlayRandomSound(Sounds.MeepleMoves);
        CurrentWalkingDestination = new Vector3(wposition.x, wposition.y, 0);
        RoomItem itemAtLocation = HandTool.LookForRoomItemAtLocation(wposition);

        if (itemAtLocation != null)
        {
            ItemToInteractWithAtDestination = itemAtLocation.gameObject;
        }
        else
        {
            ItemToInteractWithAtDestination = null;
        }

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
        if (CurrentWalkingDestination.HasValue)
        {
            // We've arrived:
            if (Mathf.Abs(transform.position.x - CurrentWalkingDestination.Value.x) < StoppingDistanceFromTarget)
            {
                if (ItemToInteractWithAtDestination != null)
                {
                    HandleItemInteraction(ItemToInteractWithAtDestination, CurrentWalkingDestination, HandTool.CurrentlyHolding);
                    ItemToInteractWithAtDestination = null;
                }

                CurrentVector = Vector3.zero;
                CurrentSpeed = 0;
                CurrentWalkingDestination = null;
            }
            else
            {
                CurrentVector = (CurrentWalkingDestination.Value - transform.position).normalized;
                CurrentVector.y = 0;
                CurrentSpeed = WalkingSpeed;
            }
        }


        Vector3 mvTo = transform.position + new Vector3(CurrentSpeed * CurrentVector.x, 0);
        MoveToward(mvTo);
        HandleJumping(CurrentVector);

        OurAnimator.SetBool(GameConstants.IsWalking, CurrentSpeed != 0);
        OurAnimator.SetBool(GameConstants.IsJumping, IsJumping);
    }


    private void HandleItemInteraction(GameObject itemToInteractWith, Vector3? pointOfInteraction, GameObject heldItem)
    {
        if (null == itemToInteractWith)
            return;

        // Are we close enough to interact with the item?
        float dxToItem = Vector2.Distance(itemToInteractWith.transform.position, ItemInteractionMeasurementLocation.transform.position);
        if (dxToItem > GameController.TheGameData.GamePrefs.Environment.MinimumDistanceOfInteraction)
        {
            AudioController.Current.PlayRandomSound(Sounds.ItemTooFarAway);
            return;
        }

        RoomItem asRoomItem = itemToInteractWith.GetComponent<RoomItem>();

        if (asRoomItem == null)
            return;


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
            if (asRoomItem.CanClimbOnto && asRoomItem.CanBeHandledDirectly)
            {
                pickupItem = true;

                Surface itsSurface = asRoomItem.GetComponentInChildren<Surface>();
                if (itsSurface != null && itsSurface.SurfaceCollider != null)
                {
                    if (pointOfInteraction.HasValue && pointOfInteraction.Value.y > itsSurface.SurfaceCollider.bounds.max.y)
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
        else if (asRoomItem.PickingUpActivatesAction)
        {
            asRoomItem.PerformSelfActivationActions();
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

        transform.position = new Vector3(transform.position.x, itsSurface.transform.position.y - GameController.TheGameData.GamePrefs.Environment.JumpLandingPointYOffset, transform.position.z);
    }

    public bool IsOnSurface = false;
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
            IsOnSurface = IsStandingOnSurface();

            if (!IsOnSurface)
                return;

            // Turn on. Next frame we'll do the jump.
            IsJumping = true;
            JumpTimeStart = Time.time;
            JumpTimeEnd = JumpTimeStart + GameController.TheGameData.GamePrefs.Environment.JumpDurationSeconds;
        }
    }

    private bool IsStandingOnSurface()
    {
        var res = DetectGround(transform.position);
        return res.foundGround && res.distance == 0;
    }

    public virtual void MoveToward(Vector3 destination)
    {
        Vector3 mvVector = Vector3.zero;

        if (destination != transform.position)
        {
            // Flip the villager depending upon the orientation of it to the target object
            Face(destination.x - transform.position.x);

            // Max move delta, contrained by obstructions
            CurrentSpeed = WalkingSpeed;

            mvVector.x = Time.deltaTime * CurrentSpeed;

            if (transform.position.x > destination.x)
                mvVector *= -1;
        }

        //mvVector.y = DetectGroundAndVerticalMovement(transform.position);
        // If not on the way up --
        if (!IsJumping)
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

            var bufferPos = Vector2.MoveTowards(CurrentWalkingDestination.Value, transform.position, StoppingDistanceFromTarget);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(bufferPos, 20);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(ItemInteractionMeasurementLocation.transform.position, GameController.TheGameData.GamePrefs.Environment.MinimumDistanceOfInteraction);
    }

    Vector3 ClosestSurfacePoint = Vector3.zero;
    Vector3 CastOffset = new Vector3(0, 20, 0);


    public (bool foundGround, float distance, Vector3 atPos) DetectGround(Vector3 fromPosition)
    {
        ClosestSurfacePoint = Vector3.zero;
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
                    ClosestSurfacePoint = hits[i].point;

                groundDetected = true;

                if ((hits[i].distance - CastOffset.y) < dyActual)
                {
                    dyActual = hits[i].distance - CastOffset.y;
                    ClosestSurfacePoint = hits[i].point;
                }
            }
        }

        Debug.DrawLine(fromPosition + CastOffset, fromPosition - CastOffset - new Vector3(0, dyActual, 0), Color.red);
        return (groundDetected, -dyActual, ClosestSurfacePoint);
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


    //private float CalculateMovedDistance(Vector3 destination, float wishedDistance)
    //{
    //    //// No movement if the animator is mid jump or wave or some action that prevents horizontal movement.
    //    //if (OurAnimator.GetBool(GameConstants.AnimatorHashMidNonMovingAction))
    //    //    return 0;

    //    //if (IsFalling)
    //    //    return 0;

    //    if (wishedDistance == 0)
    //        return 0;

    //    destination.x = Mathf.Clamp(destination.x, LevelBounds.xMin, LevelBounds.xMax);
    //    destination.y = Mathf.Clamp(destination.y, LevelBounds.yMin, LevelBounds.yMax);

    //    var distanceFromTarget = destination - transform.position;
    //    if (Mathf.Abs(distanceFromTarget.x) < StoppingDistanceFromTarget)
    //        return 0;

    //    return wishedDistance;
    //}


}
