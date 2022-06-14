using UnityEngine;

public class ItemAudio : MonoBehaviour
{
    public Sounds PlayOnStart = Sounds.Nothing;
    public Sounds PlayOnEnable = Sounds.Nothing;
    public Sounds PlayOnDisable = Sounds.Nothing;
    public Sounds PlayOnDestroy = Sounds.Nothing;

    public bool LoopAudio = false;
    public float LoopLength = 5;
    public bool stopOnDisable = true;
    private GameObject loopSound;


    void TryPlay(Sounds which)
    {

        if (AudioController.Current != null && which != Sounds.Nothing)
        {
            if (!LoopAudio)
                AudioController.Current.PlayRandomSound(which);
            else
                loopSound = AudioController.Current.LoopRandomSound(which, LoopLength);
        }
    }

    void Start()
    {
        TryPlay(PlayOnStart);
    }

    private void OnDestroy()
    {
        TryPlay(PlayOnDestroy);
    }

    private void OnEnable()
    {
        TryPlay(PlayOnEnable);
    }

    private void OnDisable()
    {
        TryPlay(PlayOnDisable);
        if(stopOnDisable && loopSound) {
            //Clean up any old looping sounds
            if(loopSound) {
                Destroy(loopSound);
                loopSound = null;
            }
        }
    }
}
