using UnityEngine;

public struct SpawnQueueItem
{
    public GameObject toSpawn;
    public Vector3 position;
    public Vector3 velocity;
    public GameObject parent;
    public float Lifetime;
}
