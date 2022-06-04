using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [Tooltip("Items spawned on this level using the GlobalSpawnQueue will get this item as their parent if they are not specifically given one, allowing hiding of all spawned items when unloading the level.")]
    public GameObject LevelSpawnItemParent;
    private void Start()
    {
        GlobalSpawnQueue.DefaultParentObject = LevelSpawnItemParent != null ? LevelSpawnItemParent : gameObject;

        var controller = GameController.TheGameController.gameObject;
        int i = 0;
    }
}
