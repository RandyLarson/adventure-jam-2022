using UnityEngine;
using Assets.Scripts.Extensions;

public class ItemAudio : MonoBehaviour
{
    public Sounds PlayOnStart = Sounds.Nothing;
    public Sounds PlayOnEnable = Sounds.Nothing;
    public Sounds PlayOnDisable = Sounds.Nothing;
    public Sounds PlayOnDestroy = Sounds.Nothing;

    public bool LoopAudio = false;
    public float LoopLength = 5;
    public bool stopOnDisable = true;
    public float delay = 0;
    public bool repeat = false;
    public float randomizeRepeatMinInterval = 0;
    public float randomizeRepeatMaxInterval = 0;
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

        if(repeat) {
            this.Invoke(() => TryPlay(which), Random.Range(randomizeRepeatMinInterval, randomizeRepeatMaxInterval));
        }
    }

    void Start()
    {
        this.Invoke(() => TryPlay(PlayOnStart), delay);
    }

    private void OnDestroy()
    {
        this.Invoke(() => TryPlay(PlayOnDestroy), delay);
    }

    private void OnEnable()
    {
        this.Invoke(() => TryPlay(PlayOnEnable), delay);
    }

    private void OnDisable()
    {
        this.Invoke(() => TryPlay(PlayOnDisable), delay);
        if(stopOnDisable && loopSound) {
            //Clean up any old looping sounds
            if(loopSound) {
                Destroy(loopSound);
                loopSound = null;
            }
        }
    }
}
