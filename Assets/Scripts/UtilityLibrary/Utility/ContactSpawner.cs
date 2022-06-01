using System;
using UnityEngine;

public class ContactSpawnerContactEVent
{

}

public class ContactSpawner : MonoBehaviour
{
    public GameObject[] ItemsToSpawnOnContact;
    public string[] TagFilter;
    public float LifetimeOfSpawned = 1;
    public int MaxTimesToSpawn = -1;
    public float CoolDownTime = -1;
    public bool SpawningIsEnabled = true;

    private int NumberOfSpawns = 0;
    private float LastTimeSpawned = -100;

    public event EventHandler<ContactSpawnerContactEVent> Contacted;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckToSpawn(collision.gameObject);
    }

    private void CheckToSpawn(GameObject contactWith)
    {
        if (SpawningIsEnabled && ItemsToSpawnOnContact.Length > 0)
        {
            if (TagFilter.Length > 0)
            {
                if (null == Array.Find<string>(TagFilter, m => contactWith.gameObject.CompareTag(m)))
                    return;
            }

            if ((CoolDownTime < 0 || (Time.time - LastTimeSpawned > CoolDownTime)) && (MaxTimesToSpawn < 0 || MaxTimesToSpawn > NumberOfSpawns))
            {
                LastTimeSpawned = Time.time;
                NumberOfSpawns++;
                for (int i = 0; i < ItemsToSpawnOnContact.Length; i++)
                {
                    GlobalSpawnQueue.AddToQueue(ItemsToSpawnOnContact[i], transform.position, null, LifetimeOfSpawned);
                }

                Contacted?.Invoke(this, new ContactSpawnerContactEVent());
            }
        }
    }
}
