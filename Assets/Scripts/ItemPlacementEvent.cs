using System;
using UnityEngine;


public class ItemPlacementEvent
{
    public GameObject TheItem;
    public ContactPoint ContactPoint;
}

public class ItemUnPlacementEvent
{
    public GameObject TheItem;
}