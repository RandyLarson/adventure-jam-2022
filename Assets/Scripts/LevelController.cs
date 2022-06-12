using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [Tooltip("Items spawned on this level using the GlobalSpawnQueue will get this item as their parent if they are not specifically given one, allowing hiding of all spawned items when unloading the level.")]
    public GameObject LevelSpawnItemParent;

    [Tooltip("Define the playable area on the scene. Used to help the camera understand how far it can pan " +
        "as it follows the player.")]
    public Rect LevelBounds;

    public LevelGoal[] LevelGoals;


    private void Start()
    {
        GlobalSpawnQueue.DefaultParentObject = LevelSpawnItemParent != null ? LevelSpawnItemParent : gameObject;

        InitCameraFollower();
        InitPlayer();
    }

    private void InitPlayer()
    {
        MeepleController player = FindObjectOfType<MeepleController>();
        if ( player != null)
        {
            player.LevelBounds = LevelBounds;
        }
    }

    private void InitCameraFollower()
    {
        if ( TryGetComponent<Camera2DFollow>(out var follower))
        {
            follower.Init(Camera.main, LevelBounds);
        }        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(LevelBounds.center, LevelBounds.size);

        Vector2 box2 = new Vector2(LevelBounds.size.x - 2, LevelBounds.size.y - 2);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(LevelBounds.center, box2);
    }
}
