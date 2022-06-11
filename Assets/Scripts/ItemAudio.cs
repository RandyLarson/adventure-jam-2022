using UnityEngine;

public class ItemAudio : MonoBehaviour
{
    public Sounds PlayOnStart = Sounds.Nothing;
    public Sounds PlayOnDestroy = Sounds.Nothing;

    void Start()
    {
        AudioController.Current.PlayRandomSound(PlayOnStart);
    }


    private void OnDestroy()
    {
        AudioController.Current.PlayRandomSound(PlayOnDestroy);
    }
}
