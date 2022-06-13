using Assets.Scripts.Extensions;
using System;
using UnityEngine;

public class RoomItem : MonoBehaviour
{
    public SerializableGuid ItemId;

    [Tooltip("Set to true if interactions should be done when an item collides with something else it can " +
        "interact with. Otherwise, it needs to be set to (e.g., after the pointer is clicked).")]
    public bool AutoActivateOnCollision = false;
    [Tooltip("Can be climbed onto (e.g., stools)")]
    public bool CanClimbOnto = false;
    [Tooltip("Can be picked up and held")]
    public bool CanBeHandledDirectly = false;
    [Tooltip("The item can't be picked up and held, but the pick-up action peforms the activation. " +
        "This is for opening drawers or cupboards.")]
    public bool PickingUpActivatesAction = false;

    [Tooltip("Will re-parent and picup this item if non-null. This is useful when there is a parent gameobject that is acting as a controller " +
        "and parent for a recipe progression.")]
    public GameObject ParentObjectToPickUpInLieuOfSelf;
    public bool MaintainUpwardOrientation = true;

    [Tooltip("When true, actions performed will toggle between acivation & deactivation.")]
    public bool IsToggle = false;
    [ReadOnly]
    public bool CurrentToggleState = false;

    [Tooltip("Set to false for stools and things that we want the player to move to " +
        "in order to get on. Things like cupboards and cans are set to false so the player " +
        "will interact with them when standing close enough.")]
    public bool AllowActivationWithoutMovingToClickedPoint = true;
    public SpriteRenderer ImageIdle;
    public SpriteRenderer ImageActive;

    public AcceptedItem[] AcceptedItems;

    [Tooltip("Actions to be performed when this item is `activated` (PerformActivationActions is called)")]
    public ActivationActions ActivationActions;

    [Tooltip("Actions to be performed when this item is `de-activated`")]
    public ActivationActions DeActivationActions;


    private void Update()
    {
        if (MaintainUpwardOrientation)
        {
            transform.rotation = Quaternion.identity;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (AutoActivateOnCollision)
            TestForInteractionWith(collision.gameObject, true);
    }

    /// <summary>
    /// Tests to see if the given item is one of the accepted items that triggers an interaction.
    /// Will perform that interaction if <paramref name="performIfPossible"/> is true.
    /// </summary>
    /// <param name="gameObject">The item to test.</param>
    /// <param name="performIfPossible">Will perform the interaction if possible.</param>
    /// <returns>If the interaction is possible or not (true if possible and the interaction was done)</returns>
    public bool TestForInteractionWith(GameObject gameObject, bool performIfPossible)
    {
        // Try ourselves and down for active room items, fall back to upward.
        RoomItem asRoomItem = gameObject.GetComponentInChildren<RoomItem>(false);
        if (asRoomItem == null)
        {
            asRoomItem = gameObject.GetComponentInParent<RoomItem>();
        }

        if (null == asRoomItem)
            return false;

        foreach (var acceptedItem in AcceptedItems)
        {
            if (acceptedItem.CanBeAccepted && acceptedItem.AcceptedItemPrototype.ItemId == asRoomItem.ItemId)
            {
                if (performIfPossible)
                {
                    ExecuteAcceptance(acceptedItem, asRoomItem);
                }
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Present so that they can be invoked via the editor.
    /// </summary>
    public void DeActivateSelf() => PerformSelfDeActivationActions();
    /// <summary>
    /// Present so that they can be invoked via the editor.
    /// </summary>
    public void ActivateSelf() => PerformSelfActivationActions();

    /// <summary>
    /// Detaches each child object of the given one, setting their parent to nothing.
    /// The intention is to pass an placeholding 'contents' game object.
    /// </summary>
    /// <param name="parent"></param>
    public void DropAllItems(GameObject parent)
    {
        if (parent == null)
            return;

        parent.transform.DetachChildren();
    }

    public GameObject PerformSelfDeActivationActions() => PerformSelfActions(DeActivationActions);

    public GameObject PerformSelfActivationActions()
    {
        if (!IsToggle || (IsToggle && CurrentToggleState == false))
        {
            CurrentToggleState = true;
            return PerformSelfActions(ActivationActions);
        }
        else
        {
            CurrentToggleState = false;
            return PerformSelfActions(DeActivationActions);
        }
    }

    public GameObject PerformSelfActions(ActivationActions actions)
    {
        GameObject replacementItem = PerformActivationActions(actions);
        if (actions.DestroySelfOnUse)
            Destroy(gameObject);

        if (actions.TargetItemReplacement != null)
            Destroy(gameObject);

        return replacementItem;
    }

    public GameObject PerformActivationActions(ActivationActions activationActions)
    {
        GameObject replacementItem = null;
        if (null == activationActions)
            return replacementItem;


        if (activationActions.ItemProducedWhenUsed != null)
        {
            Vector3 spawnPt = activationActions.ItemProducedLocation != null ? activationActions.ItemProducedLocation.transform.position : transform.position;
            GlobalSpawnQueue.AddToQueue(activationActions.ItemProducedWhenUsed, spawnPt);
        }

        if (activationActions.TargetItemReplacement != null)
        {
            replacementItem = GlobalSpawnQueue.Instantiate(activationActions.TargetItemReplacement, transform.position, transform.rotation, transform.parent);
            if (replacementItem.TryGetComponent<Rigidbody2D>(out var rb))
            {
                var ourRb = gameObject.GetComponent<Rigidbody2D>();
                rb.velocity = ourRb.velocity;
            }
        }

        foreach (var item in activationActions.ItemsToEnable)
        {
            item.SafeSetActive(true);
        }

        foreach (var item in activationActions.ItemsToDisable)
        {
            item.SafeSetActive(false);
        }

        activationActions.OnUsage?.Invoke();

        GameController.TheGameController.LogGameAchievement(activationActions.GameObjectiveAchieved);

        return replacementItem;
    }

    private void ExecuteAcceptance(AcceptedItem acceptedItemProfile, RoomItem incomingItem)
    {
        GameObject replacementItem = PerformActivationActions(acceptedItemProfile);

        if (acceptedItemProfile.TargetItemReplacement != null)
        {
            Destroy(gameObject);
        }

        if (acceptedItemProfile.IsAcceptedItemDestroyedOnUse)
        {
            Destroy(incomingItem.gameObject);
        }

        if (acceptedItemProfile.OnlyAcceptedOnce)
            acceptedItemProfile.CanBeAccepted = false;
    }
}
