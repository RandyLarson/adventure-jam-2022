using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ActivationActions
{

    [Tooltip("The name of an overall game objective achieved. This could be a mandatory item for progressing, " +
        "or an item that a bonus objective.")]
    public string GameObjectiveAchieved;

    [Tooltip("Assigned an item to produce when this item is used. This could be a usage effect or something " +
        "that could then be used in the scene (e.g., create a key from a box)")]
    public GameObject ItemProducedWhenUsed;

    [Tooltip("Where to spawn the item produced.")]
    public GameObject ItemProducedLocation;

    [Tooltip("If using this item should cause the original item to be replaced with something else, that goes here." +
        "For example: A unopened can of soup is replaced with the open can of soup object.")]
    public GameObject TargetItemReplacement;

    [Tooltip("Does using this item mean the destruction of the item (i.e., a one-time use item")]
    public bool DestroySelfOnUse = false;

    [Tooltip("Each item here will be enabled.")]
    public GameObject[] ItemsToEnable;

    [Tooltip("Each item here will be disabled.")]
    public GameObject[] ItemsToDisable;

    [Tooltip("Generic actions to take upon usage.")]
    public UnityEvent OnUsage;

    [Tooltip("Call the general activation method on incoming item.")]
    public bool CallActivateOnRoomItem;

    [Tooltip("Changes the parent of the incoming item to the the item accepting.")]
    public GameObject MakeIncomingItemAChildOfThis;
    [Tooltip("Changes the sort order of a reparented item to this.")]
    public int IncomingItemSortOrder;
}
