using System;
using UnityEngine;


public class RoomItem : MonoBehaviour
{
    public RoomItemKind Kind;

    public AcceptedItem[] AcceptedItems;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EvaluateCollision(collision);
    }

    private void EvaluateCollision(Collider2D collision)
    {
        EvaluateItemAsAcceptableItem(collision.gameObject);
    }

    private void EvaluateItemAsAcceptableItem(GameObject gameObject)
    {
        var asRoomItem = gameObject.GetComponentInParent<RoomItem>();
        if (asRoomItem == null)
        {
            return;
        }

        foreach (var acceptedItem in AcceptedItems)
        {
            if (acceptedItem.ItemPrototype.Kind == asRoomItem.Kind)
            {
                ExecuteAcceptance(acceptedItem, asRoomItem);
                break;
            }
        }

    }

    private void ExecuteAcceptance(AcceptedItem acceptedItemProfile, RoomItem incomingItem)
    {
        if ( acceptedItemProfile.ItemProducedWhenUsed != null )
        {
            Vector3 spawnPt = acceptedItemProfile.ItemProducedLocation != null ? acceptedItemProfile.ItemProducedLocation.transform.position : transform.position;
            GlobalSpawnQueue.AddToQueue(acceptedItemProfile.ItemProducedLocation, spawnPt);
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

        GameController.TheGameController.LogGameAchievement(acceptedItemProfile.GameObjectiveAchieved);
    }
}
