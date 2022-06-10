using System;
using UnityEngine;

[Serializable]
public class AcceptedItem : ActivationActions
{
    [Tooltip("The prototype of the item accepted. RoomItem names are compared to see if there is a match..")]
    public RoomItem AcceptedItemPrototype;
}
