using UnityEngine;

public class ItemAudio : MonoBehaviour
{
    public Sounds PlayOnStart = Sounds.Nothing;
    public Sounds PlayOnEnable = Sounds.Nothing;
    public Sounds PlayOnDisable = Sounds.Nothing;
    public Sounds PlayOnDestroy = Sounds.Nothing;

    void Start()
    {
        if (AudioController.Current != null)
            AudioController.Current.PlayRandomSound(PlayOnStart);
    }


    private void OnDestroy()
    {
        if (AudioController.Current != null)
            AudioController.Current.PlayRandomSound(PlayOnDestroy);
    }

    private void OnEnable()
    {
        if (AudioController.Current != null)
            AudioController.Current.PlayRandomSound(PlayOnEnable);
    }

    private void OnDisable()
    {
        if (AudioController.Current != null)
            AudioController.Current.PlayRandomSound(PlayOnDisable);

    }
}
