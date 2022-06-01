using UnityEngine;


public class SpawnOnCreateOrDestroy : MonoBehaviour
{
    public GameObject[] SpawnCollection;
    public bool DoSpawnItem = true;
    public bool SpawnOnCreation = false;

    [Tooltip("Time after spawning to self-destruct. Set to 0 for no destruction.")]
    public float LifetimeOfSpawned = 1;

    private void Awake()
    {
        if (SpawnOnCreation)
        {
            SpawnItems();
            DoSpawnItem = false;
        }
    }

    public void SpawnItems()
    {
        if (DoSpawnItem)
        {
            for (int i = 0; i < SpawnCollection.Length; i++)
            {
                GlobalSpawnQueue.AddToQueue(SpawnCollection[i], transform.position, null, LifetimeOfSpawned);
            }
        }
    }

    private void OnDestroy()
    {
        if (!SpawnOnCreation)
            SpawnItems();
    }
}
