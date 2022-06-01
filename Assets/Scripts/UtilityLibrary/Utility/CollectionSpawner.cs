using UnityEngine;

public class CollectionSpawner : MonoBehaviour, IActivatable
{
    public GameObject[] ItemsSpawn;


    [Tooltip("Is spawning allowed.")]
    public bool DoSpawning = false;

    [Tooltip("Time after spawning to self-destruct. Set to 0 for no destruction.")]
    public float LifetimeOfSpawned = 1;

    public bool IsActive => DoSpawning;

    public void Activate(bool value, bool reset)
    {
        DoSpawning = value;
        SpawnItems();
    }

    public void Activate()
    {
        Activate(true, false);
    }
    public void Deactivate(bool reset) => Activate(false, reset);

    public void SpawnItems()
    {
        if (DoSpawning)
        {
            for (int i = 0; i < ItemsSpawn.Length; i++)
            {
                GlobalSpawnQueue.AddToQueue(ItemsSpawn[i], transform.position, null, LifetimeOfSpawned);
            }
        }
    }
}
