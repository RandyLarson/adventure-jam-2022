using System;
using UnityEngine;

[Serializable]
public class AcceptedItem : ActivationActions
{
    [Tooltip("The prototype of the item accepted. RoomItem names are compared to see if there is a match..")]
    public RoomItem AcceptedItemPrototype;

    [Tooltip("Does using this item mean the destruction of the item (i.e., a one-time use item")]
    public bool IsAcceptedItemDestroyedOnUse = false;

    [Tooltip("For recipes, we don't want the same ingredient to be used multiple times.")]
    public bool OnlyAcceptedOnce = true;
    public bool CanBeAccepted = true;
}
