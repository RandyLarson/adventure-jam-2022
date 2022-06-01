using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ItemSpawner))]
public class ItemSpawerCountRandomizer : MonoBehaviour
{
    public ItemSpawner Spawner;
    public Range CountRange;

    void Start()
    {
        if (Spawner == null)
            Spawner = GetComponent<ItemSpawner>();

        if (Spawner != null )
        {
            foreach ( var entry in Spawner.SpawnQueue)
            {
                entry.Count = (int)Random.Range(CountRange.min, CountRange.max);
            }
        }
    }

}
