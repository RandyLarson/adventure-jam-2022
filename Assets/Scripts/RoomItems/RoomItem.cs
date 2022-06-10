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
    public SpriteRenderer ImageIdle;
    public SpriteRenderer ImageActive;

    public AcceptedItem[] AcceptedItems;

    [Tooltip("Actions to be performed when this item is `activated` (PerformActivationActions is called)")]
    public ActivationActions ActivationActions;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ( AutoActivateOnCollision )
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
        var asRoomItem = gameObject.GetComponentInParent<RoomItem>();
        if (asRoomItem == null)
        {
            return false;
        }

        foreach (var acceptedItem in AcceptedItems)
        {
            if (acceptedItem.AcceptedItemPrototype.ItemId == asRoomItem.ItemId)
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

    public void PerformSelfActivationActions()
    {
        PerformActivationActions(ActivationActions);
        if (ActivationActions.DestroySelfOnUse)
            Destroy(gameObject);

        if (ActivationActions.TargetItemReplacement != null)
            Destroy(gameObject);
    }

    public void PerformActivationActions(ActivationActions activationActions)
    {
        if (null == activationActions)
            return;

        if (activationActions.ItemProducedWhenUsed != null)
        {
            Vector3 spawnPt = activationActions.ItemProducedLocation != null ? activationActions.ItemProducedLocation.transform.position : transform.position;
            GlobalSpawnQueue.AddToQueue(activationActions.ItemProducedWhenUsed, spawnPt);
        }

        if (activationActions.TargetItemReplacement != null)
        {
            var replacement = GlobalSpawnQueue.Instantiate(activationActions.TargetItemReplacement, transform.position, transform.rotation, transform.parent);
            if (replacement.TryGetComponent<Rigidbody2D>(out var rb))
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
    }

    private void ExecuteAcceptance(AcceptedItem acceptedItemProfile, RoomItem incomingItem)
    {
        PerformActivationActions(acceptedItemProfile);

        if (acceptedItemProfile.TargetItemReplacement != null)
        {
            Destroy(gameObject);
        }    

        if ( acceptedItemProfile.IsAcceptedItemDestroyedOnUse )
        {
            Destroy(incomingItem.gameObject);
        }
    }
}
