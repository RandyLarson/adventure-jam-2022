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

        CurrentWalkingDestination = new Vector3(wposition.x, transform.position.y, 0);
        RoomItem itemAtLocation = HandTool.LookForRoomItemAtLocation(wposition);

        if (itemAtLocation != null)
        {
            ItemToInteractWithAtDestination = itemAtLocation.gameObject;
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
                CurrentVector = Vector3.zero;
                CurrentWalkingDestination = null;

                if (ItemToInteractWithAtDestination != null)
                {
                    if (HandTool.CurrentlyHolding != null)
                    {
                        HandTool.TryToUseHeldItemOn(ItemToInteractWithAtDestination);
                    }
                    else if (HandTool.IsValidToPickup(ItemToInteractWithAtDestination))
                    {
                        HandTool.AttemptPickup(ItemToInteractWithAtDestination);
                    }

                    ItemToInteractWithAtDestination = null;
                }
            }
            else
            {
                CurrentVector = (CurrentWalkingDestination.Value - transform.position).normalized;
            }
        }

        if (CurrentVector != Vector3.zero)
        {
            Vector3 mvTo = transform.position + new Vector3(WalkingSpeed * CurrentVector.x, 0);
            MoveToward(mvTo);
        }
        else
        {
            CurrentSpeed = 0;
        }

        OurAnimator.SetBool(GameConstants.IsWalking, CurrentSpeed != 0);
        OurAnimator.SetBool(GameConstants.IsJumping, IsJumping);

        //if (CurrentWalkingDestination != Vector3.zero)
        //{
        //    MoveToward(CurrentWalkingDestination);
        //}
    }

    public virtual void MoveToward(Vector3 destination)
    {
        if (destination == Vector3.zero)
        {
            return;
        }

        // Flip the villager depending upon the orientation of it to the target object
        Face(destination.x - transform.position.x);

        // Max move delta, contrained by obstructions
        CurrentSpeed = WalkingSpeed;

        float maxDistance = Time.deltaTime * CurrentSpeed;
        float actualDistance = CalculateMovedDistance(destination, maxDistance);

        if (actualDistance > 0)
        {
            var newPos = Vector2.MoveTowards(transform.position, destination, actualDistance);
            var posDelta = newPos - (Vector2)transform.position;

            transform.position = new Vector2(newPos.x, transform.position.y);
        }
        else
        {
            CurrentSpeed = 0;
        }
    }

    private float CalculateMovedDistance(Vector3 destination, float wishedDistance)
    {
        //// No movement if the animator is mid jump or wave or some action that prevents horizontal movement.
        //if (OurAnimator.GetBool(GameConstants.AnimatorHashMidNonMovingAction))
        //    return 0;

        //if (IsFalling)
        //    return 0;

        if (wishedDistance == 0)
            return 0;

        var distanceFromTarget = destination - transform.position;
        if (Mathf.Abs(distanceFromTarget.x) < StoppingDistanceFromTarget)
            return 0;

        return wishedDistance;
    }


}
