using UnityEngine;

public class CollectionActivator : Activatable, IActivatable
{
    public Activatable[] ItemsToActivate;

    [Tooltip("Is activation allowed.")]
    public bool DoActivation = false;
    public bool DoReset = true;
    public bool ApplyActivation = false;

    /// <summary>
    /// True if any item in the collection reports as active.
    /// </summary>
    public bool IsActive
    { 
        get
        {
            for (int i = 0; i < ItemsToActivate.Length; i++)
            {
                IActivatable asIActivatable = ItemsToActivate[i].GetComponent<IActivatable>();
                if (asIActivatable.IsActive)
                    return true;
            }
            return false;
        }
    }

    //private void Start()
    //{
    //    // If activation is enabled from the start.
    //    CheckToActivateItems();
    //}

    private void Update()
    {
        CheckToActivateItems();
    }

    public void Activate(bool value, bool reset)
    {
        if ( value != DoActivation )
        {
            DoActivation = value;
            DoReset = reset;
            ApplyActivation = true;
        }
    }
    public void Activate() => Activate(true, false);
    public void Deactivate(bool reset) => Activate(false, reset);

    public void CheckToActivateItems()
    {
        if (!ApplyActivation)
            return;

        ApplyActivation = false;
        for (int i = 0; i < ItemsToActivate.Length; i++)
        {
            IActivatable asIActivatable = ItemsToActivate[i].GetComponent<IActivatable>();
            if ( asIActivatable != null )
                asIActivatable.Activate(DoActivation, DoReset);
        }
    }
}