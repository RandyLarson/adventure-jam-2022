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
    [ReadOnly]
    public int Facing = 1;

    [ReadOnly]
    public Animator OurAnimator;

    [ReadOnly]
    public Rect LevelBounds;

    HandTool HandTool;

    public float StoppingDistanceFromTarget = 10;

    // Start is called before the first frame update
    void Start()
    {
        OurAnimator = GetComponentInChildren<Animator>();
        HandTool = GetComponentInChildren<HandTool>();
    }

    Vector3? CurrentWalkingDestination = null;
    [ReadOnly]
    public GameObject ItemToInteractWithAtDestination;
    Vector3 CurrentVector = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
    }


    void OnMove(InputValue value)
    {
        if (value != null)
        {
            var moveVal = value.Get<Vector2>().normalized;

            CurrentVector = moveVal;
        }
        else
        {
            CurrentVector = Vector3.zero;
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
        CurrentWalkingDestination = new Vector3(wposition.x, transform.position.y, 0);
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
        CurrentSpeed = 0;

        if (CurrentWalkingDestination.HasValue)
        {
            // We've arrived:
            if (Mathf.Abs(transform.position.x - CurrentWalkingDestination.Value.x) < StoppingDistanceFromTarget)
            {
                CurrentVector = Vector3.zero;
                CurrentWalkingDestination = null;

                if (ItemToInteractWithAtDestination != null)
                {
                    RoomItem asRoomItem = ItemToInteractWithAtDestination.GetComponent<RoomItem>();

                    bool isHoldingItem = HandTool.CurrentlyHolding != null;
                    bool usedHeldItem = false;
                    bool didClimb = false;

                    if (HandTool.CurrentlyHolding != null)
                    {
                        usedHeldItem = HandTool.TryToUseHeldItemOn(ItemToInteractWithAtDestination);
                    }
                    else if (HandTool.IsValidToPickup(ItemToInteractWithAtDestination))
                    {
                        HandTool.AttemptPickup(ItemToInteractWithAtDestination);
                    }


                    if (asRoomItem != null)
                    {
                        if (!usedHeldItem && asRoomItem.CanClimbOnto)
                        {
                            ClimbOnToItem(asRoomItem);
                            didClimb = true;
                        }
                    }

                    if (isHoldingItem)
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

                    ItemToInteractWithAtDestination = null;
                }
            }
            else
            {
                CurrentVector = (CurrentWalkingDestination.Value - transform.position).normalized;
                CurrentSpeed = WalkingSpeed;
            }
        }


        Vector3 mvTo = transform.position + new Vector3(CurrentSpeed * CurrentVector.x, 0);
        MoveToward(mvTo);

        OurAnimator.SetBool(GameConstants.IsWalking, CurrentSpeed != 0);
        OurAnimator.SetBool(GameConstants.IsJumping, IsJumping);

        //if (CurrentWalkingDestination != Vector3.zero)
        //{
        //    MoveToward(CurrentWalkingDestination);
        //}
    }

    private void ClimbOnToItem(RoomItem asRoomItem)
    {
        if (null == asRoomItem)
            return;

        Surface itsSurface = asRoomItem.GetComponentInChildren<Surface>();
        if (itsSurface == null)
            return;

        transform.position = new Vector3(transform.position.x, itsSurface.transform.position.y, transform.position.z);
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

        mvVector.y = DetectGroundAndVerticalMovement(transform.position);

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
    }

    Vector3 ClosestSurfacePoint = Vector3.zero;
    Vector3 CastOffset = new Vector3(0, 20, 0);
    public float DetectGroundAndVerticalMovement(Vector3 fromPosition)
    {
        ClosestSurfacePoint = Vector3.zero;
        var dyMax = GameController.TheGameData.GamePrefs.Environment.Gravity * Time.deltaTime;

        RaycastHit2D[] hits = Physics2D.RaycastAll(fromPosition+CastOffset, Vector3.down, 10*dyMax + CastOffset.y, GameConstants.LayerMaskDefault);
        Debug.DrawLine(fromPosition + CastOffset - new Vector3(5,0,0), fromPosition - new Vector3(5, dyMax + CastOffset.y, 0), Color.blue);

        float dyActual = dyMax;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.CompareTag(GameConstants.Surface))
            {
                if (ClosestSurfacePoint == Vector3.zero)
                    ClosestSurfacePoint = hits[i].point;

                // Looking for the shortest distance to 'fall' before we find a surface.
                if ((hits[i].distance - CastOffset.y) < dyActual)
                {
                    dyActual = hits[i].distance - CastOffset.y;
                    ClosestSurfacePoint = hits[i].point;
                }
            }
        }

        Debug.DrawLine(fromPosition + CastOffset, fromPosition - CastOffset - new Vector3(0, dyActual, 0), Color.red);
        return -dyActual;
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

        var distanceFromTarget = destination - optimalDestination;

        if (Mathf.Abs(distanceFromTarget.x) < StoppingDistanceFromTarget)
        {
            optimalDestination.x = destination.x + (StoppingDistanceFromTarget * MathF.Sign(distanceFromTarget.x));
        }

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
