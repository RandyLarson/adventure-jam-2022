using Assets.Scripts.Extensions;
using System.Collections.Generic;
using UnityEngine;


public static class GlobalSpawnQueue
{

    static List<SpawnQueueItem> TheQueue { get; set; } = new List<SpawnQueueItem>();
    static public GameObject DefaultParentObject;

    public static GameObject Instantiate(GameObject original, Vector3 position, Quaternion? rotation, Transform parent, float? autoDestructIn = null)
    {
        if (original == null)
            return null;

        Transform itemParent = null;
        if (parent != null)
            itemParent = parent;
        else if (DefaultParentObject != null)
            itemParent = DefaultParentObject.transform;


        var instantiated = Object.Instantiate(original, position, rotation != null ? rotation.Value : Quaternion.identity, itemParent);

        if (autoDestructIn != null && autoDestructIn.Value > 0)
            GameObject.Destroy(instantiated, autoDestructIn.Value);

        return instantiated;
    }


    static public void AddToQueue(GameObject protoType, Vector3 atPosition, Vector3 initialVelocity, GameObject parent=null, float lifetime=0)
    {
        if (protoType == null)
            return;

        TheQueue.Add(new SpawnQueueItem
        {
            toSpawn = protoType,
            position = atPosition,
            parent = parent,
            Lifetime = lifetime,
            velocity = initialVelocity
        });

    }

    static public void AddToQueue(GameObject protoType, Vector3 atPosition, GameObject parent=null, float lifetime=0)
    {
        AddToQueue(protoType, atPosition, Vector3.zero, parent, lifetime);
    }

    static public void SpawnQueueItems()
    {
        for (int i = 0; i < TheQueue.Count; i++)
        {
            TheQueue[i].toSpawn.SafeInstantiate(TheQueue[i].position, out GameObject spawned, TheQueue[i].Lifetime);
            Transform parent = null;
            if (TheQueue[i].parent != null)
                parent = TheQueue[i].parent.transform;
            else if (DefaultParentObject != null)
                parent = DefaultParentObject.transform;

            if ( parent != null )
                spawned.transform.SetParent(parent, true);

            var itsRb = spawned.GetComponent<Rigidbody2D>();
            if (itsRb != null)
                itsRb.velocity = TheQueue[i].velocity;
        }

        TheQueue.Clear();
    }
}
