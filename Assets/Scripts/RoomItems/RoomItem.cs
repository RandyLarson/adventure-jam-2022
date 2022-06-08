using Assets.Scripts.Extensions;
using System;
using UnityEngine;


public class RoomItem : MonoBehaviour
{
    public string Kind;

    [Tooltip("Set to true if interactions should be done when an item collides with something else it can " +
        "interact with. Otherwise, it needs to be set to (e.g., after the pointer is clicked).")]
    public bool AutoActivateOnCollision = false;
    public bool CanClimbOnto = false;
    public bool CanBeHandledDirectly = false;
    public bool ReplacesPlacementUi = false;
    public SpriteRenderer ImageIdle;
    public SpriteRenderer ImageActive;

    public AcceptedItem[] AcceptedItems;

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
            if (acceptedItem.AcceptedItemPrototype.Kind == asRoomItem.Kind)
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

    private void ExecuteAcceptance(AcceptedItem acceptedItemProfile, RoomItem incomingItem)
    {
        if ( acceptedItemProfile.ItemProducedWhenUsed != null )
        {
            Vector3 spawnPt = acceptedItemProfile.ItemProducedLocation != null ? acceptedItemProfile.ItemProducedLocation.transform.position : transform.position;
            GlobalSpawnQueue.AddToQueue(acceptedItemProfile.ItemProducedWhenUsed, spawnPt);
        }

        if ( acceptedItemProfile.TargetItemReplacement != null )
        {
            var replacement = GlobalSpawnQueue.Instantiate(acceptedItemProfile.TargetItemReplacement, transform.position, transform.rotation, transform.parent);
            if ( replacement.TryGetComponent<Rigidbody2D>(out var rb))
            {
                var ourRb = gameObject.GetComponent<Rigidbody2D>();
                rb.velocity = ourRb.velocity;
            }
            //GlobalSpawnQueue.AddToQueue(acceptedItemProfile.TargetItemReplacement, transform.position);
            Destroy(gameObject);
        }

        if ( acceptedItemProfile.IsAcceptedItemDesroyedOnUse )
        {
            Destroy(incomingItem.gameObject);
        }

        foreach (var item in acceptedItemProfile.ItemsToEnable)
        {
            item.SafeSetActive(true);
        }

        foreach (var item in acceptedItemProfile.ItemsToDisable)
        {
            item.SafeSetActive(false);
        }

        acceptedItemProfile.OnUsage?.Invoke();

        GameController.TheGameController.LogGameAchievement(acceptedItemProfile.GameObjectiveAchieved);
    }
}
