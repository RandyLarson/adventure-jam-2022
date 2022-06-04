using System;
using UnityEngine;

[Serializable]
public class AcceptedItem
{
    [Tooltip("The prototype of the item accepted. RoomItem names are compared to see if there is a match..")]
    public RoomItem ItemPrototype;

    [Tooltip("Assigned an item to produce when this item is used. Assign here new items that are added to the level.")]
    public GameObject ItemProducedWhenUsed;
    [Tooltip("Wher to spawn the item produced.")]
    public GameObject ItemProducedLocation;

    [Tooltip("If using this item should cause the original item to be replaced with something else, that goes here." +
        "For example: A unopened can of soup is replaced with the open can of soup object.")]
    public GameObject TargetItemReplacement;

    [Tooltip("Does using this item mean the destruction of the item (i.e., a one-time use item")]
    public bool IsAcceptedItemDesroyedOnUse = false;

    [Tooltip("The name of an overall game objective achieved. This could be a mandatory item for progressing, " +
        "or an item that a bonus objective.")]
    public string GameObjectiveAchieved;

}