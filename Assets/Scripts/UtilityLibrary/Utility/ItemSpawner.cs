using System;
using UnityEngine;
using Assets.Scripts.Extensions;

[Serializable]
public class LaunchQueueItem
{
    public GameObject ToLaunch;
    public int Count = 1;
}

public interface IActivatable
{
    bool IsActive { get; }
    void Activate(bool value, bool reset);
    void Activate();
    void Deactivate(bool reset);
}

public class ItemSpawner : Activatable, IActivatable
{
    [Tooltip("Is the spawner active and processing the spawn queue.")]
    public bool IsActive = false;
    [Tooltip("The items to process and spawn.")]
    public LaunchQueueItem[] SpawnQueue;
    [Tooltip("The vector items are launched toward.")]
    public GameObject LaunchVector;
    [Tooltip("Randomizes the angle items are launched at with respect to the launch vector.")]
    public float SpreadAngle = 0;
    [Tooltip("An initial delay after start-up before processing the launch queue.")]
    public float AutoActivateAfterDelay = 0f;
    [Tooltip("A delay after activation, but before the first item is activated.")]
    public float InitialDelayBeforeFirstSpawn = 0f;
    [Tooltip("The delay time between items in the same queue entry, used when the count is > 1.")]
    public float ItemSpawnInterval = 3f;
    [Tooltip("The delay time between members of the queue.")]
    public float InterArrayMemberInterval = 10f;
    [Tooltip("The delay time between ending the queue and starting it again.")]
    public float InterQueueLoopInterval = 0f;
    [Tooltip("Will add force such that the velocity of the item is this. The force is based upon the item's mass.")]
    public float LaunchAccel = 0f;
    [Tooltip("The force to add to the item at launch time, used if the LaunchAccel is 0.")]
    public float LaunchPower = 200f;
    [Tooltip("Will add force such that an item will rotate.")]
    public bool TumbleItem = false;
    [Tooltip("The number of times to process the launch queue, 0 being infinite.")]
    public int LoopsOfLaunchQueue = 1;
    [Tooltip("Should this item auto-destruct after processing the queue.")]
    public bool AutoDestroyAfterQueueExhausted = false;
    [Tooltip("Location to spawn at if present")]
    public GameObject SpawnLocation;
    [Tooltip("If yes, 1 item will be chosen from the launch queue each spawn (instead of walking sequentially through it).")]
    public bool RandomlySampleFromQueuePerLaunch = false;
    public Range RandomizeAllTimesBy = new Range(0, 0);
    [Tooltip("If non-zero, puts a cap on the total number of items that can be spawned during 1 activation. Useful when picking randomly.")]
    public int MaxSpawnsPerActivation = 0;
    [Tooltip("True => cap the total number of items spawned to match the launch queue loop count.")]
    public bool LinkMaxSpawnsToLaunchLoops = false;

    private float NextSpawnTime = 3f;
    private int QueuePostion = 0;
    private int WaveCount = 0;
    private float StartTime = 0f;
    private int QueueLoopsPerformed = 0;
    private float ActualInitialDelayBeforeFirstSpawn = 0f;
    private int NumSpawnsThisActivation = 0;

    public void SetLoopsOfLaunchQueue(int to)
    {
        LoopsOfLaunchQueue = to;
        if (LinkMaxSpawnsToLaunchLoops)
            MaxSpawnsPerActivation = LoopsOfLaunchQueue;
    }

    public void Activate() => Activate(true, true);
    bool IActivatable.IsActive { get => IsActive; }

    public void Activate(bool value, bool resetToStart)
    {
        IsActive = value;
        if (resetToStart)
        {
            Reset();
        }
    }

    public void Deactivate(bool reset) => Activate(false, reset);

    public void Reset()
    {
        QueuePostion = 0;
        WaveCount = 0;
        StartTime = Time.time;
        NextSpawnTime = Time.time;
        QueueLoopsPerformed = 0;
        ActualInitialDelayBeforeFirstSpawn = RandomizedTimeRange(InitialDelayBeforeFirstSpawn);
        NumSpawnsThisActivation = 0;
    }

    private void Start()
    {
        Reset();
    }

    float RandomizedTimeRange(float initial)
    {
        if (RandomizeAllTimesBy.min != 0 && RandomizeAllTimesBy.max != 0)
        {
            return initial + UnityEngine.Random.Range(RandomizeAllTimesBy.min, RandomizeAllTimesBy.max);
        }
        return initial;
    }

    void Update()
    {
        if (SpawnQueue.Length == 0)
            return;

        if (!IsActive)
        {
            if (AutoActivateAfterDelay > 0 && StartTime + AutoActivateAfterDelay > Time.time)
                Activate(true, true);
            return;
        }

        if (ActualInitialDelayBeforeFirstSpawn > 0 && StartTime + ActualInitialDelayBeforeFirstSpawn > Time.time)
            return;

        if (MaxSpawnsPerActivation != 0 && NumSpawnsThisActivation >= MaxSpawnsPerActivation)
        {
            if (AutoDestroyAfterQueueExhausted)
                Destroy(gameObject);
            else
                Activate(false, true);

            return;
        }


        if (QueuePostion >= SpawnQueue.Length)
        {
            QueueLoopsPerformed++;
            if (LoopsOfLaunchQueue == 0 || QueueLoopsPerformed < LoopsOfLaunchQueue)
            {
                NextSpawnTime = Time.time + RandomizedTimeRange(InterQueueLoopInterval);
                QueuePostion = 0;
            }
            else
            {
                if (AutoDestroyAfterQueueExhausted)
                    Destroy(gameObject);
                else
                    Activate(false, true);

                return;
            }
        }


        if (Time.time >= NextSpawnTime)
        {
            LaunchQueueItem currentQueueEntry = SpawnQueue[QueuePostion];
            if (RandomlySampleFromQueuePerLaunch)
            {
                currentQueueEntry = SpawnQueue[UnityEngine.Random.Range(0, SpawnQueue.Length)];
            }

            if (currentQueueEntry == null || currentQueueEntry.ToLaunch == null)
            {
                Debug.LogError("Spawner lacking valid item" + name + " at: " + transform.position);
                //Debug.Break();
            }

            if (currentQueueEntry.ToLaunch != null)
            {
                Transform spawnLocationToUse = transform;
                if (SpawnLocation != null)
                    spawnLocationToUse = SpawnLocation.gameObject.transform;

                GameObject spawnedObject = GlobalSpawnQueue.Instantiate(currentQueueEntry.ToLaunch, spawnLocationToUse.position, spawnLocationToUse.rotation, null, null);
                NumSpawnsThisActivation++;

                if ((LaunchAccel > 0 || LaunchPower > 0) && spawnedObject.GetComponent(out Rigidbody2D itsRB))
                {
                    if (SpreadAngle != 0)
                    {
                        gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                        transform.Rotate(Vector3.forward, UnityEngine.Random.Range(-SpreadAngle, SpreadAngle));
                    }

                    Vector3 lVector = (LaunchVector != null ? LaunchVector.transform.position - transform.position : transform.right);

                    if (LaunchAccel != 0)
                        lVector = lVector.normalized * itsRB.mass * LaunchAccel;
                    else
                        lVector *= LaunchPower;

                    if (TumbleItem)
                    {
                        itsRB.AddForceAtPosition(lVector, itsRB.transform.position + new Vector3(3, 5, 0), ForceMode2D.Impulse);
                    }
                    else
                    {
                        itsRB.AddForce(lVector, ForceMode2D.Impulse);
                    }
                }
            }

            WaveCount++;
            if (WaveCount >= currentQueueEntry.Count)
            {
                WaveCount = 0;
                QueuePostion++;

                if (QueuePostion < SpawnQueue.Length)
                {
                    NextSpawnTime = Time.time + RandomizedTimeRange(InterArrayMemberInterval);
                }
            }
            else
            {
                NextSpawnTime = Time.time + RandomizedTimeRange(ItemSpawnInterval);
            }
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 10);
        if (LaunchVector != null)
            Gizmos.DrawLine(transform.position, LaunchVector.transform.position);
    }
}
