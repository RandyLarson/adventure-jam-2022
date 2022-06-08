#if false
using System;
using UnityEngine;
using UnityEngine.Events;

public class PickableItem : MonoBehaviour
{
    //public bool CanBeHandledDirectly = true;
    //public bool IgnoreSortLayerAdjustmentWhenHeld = false;
    //[ReadOnly]
    //public bool IsBeingHeld = false;
    //[Tooltip("A scale to apply to the item when connected to contact points that also have scaling enabled.")]
    //public Vector2 ScaleWhenItemAttached = Vector2.one;

    //public event EventHandler<ItemPlacementEvent> OnItemPlaced;
    //public event EventHandler<ItemUnPlacementEvent> OnItemPickedUp;

    //public bool ReplacesPlacementUi = false;
    //public SpriteRenderer ImageIdle;
    //public SpriteRenderer ImageActive;


    [Tooltip("Invoked when placed")]
    public UnityEvent OnPlaced = new UnityEvent();

    [Tooltip("Invoked when picked-up")]
    public UnityEvent OnPickedUp = new UnityEvent();

    private void Start()
    {
    }

    internal void ItemWasPickedUp()
    {
        IsBeingHeld = true;
        AdjustImagesForState(false);
        OnItemPickedUp?.Invoke(this, new ItemUnPlacementEvent { TheItem = gameObject });
        OnPickedUp?.Invoke();
    }

    internal void ItemWasPlaced(GameObject itemPlaced, ContactPoint contactPoint)
    {
        if (itemPlaced != gameObject)
        {
            Debug.LogWarning("Placement event sent to Pickable item that was not for itself.");
            return;
        }

        IsBeingHeld = false;

        AdjustImagesForState(true);
        // Perhaps we move the other instance of this
        // should get moved here.
        // GameStateData.CurrentGameController.ItemWasPlaced(itemLocation, itemPlaced, contactPoint);

        OnItemPlaced?.Invoke(this, new ItemPlacementEvent
        {
            ContactPoint = contactPoint,
            TheItem = itemPlaced
        });

        OnPlaced?.Invoke();
    }

    void AdjustImagesForState(bool isIdle)
    {
        if (ImageActive == null && ImageIdle == null)
            return;

        ImageActive.gameObject.SetActive(!isIdle);
        ImageIdle.gameObject.SetActive(isIdle);
    }
}

#endif