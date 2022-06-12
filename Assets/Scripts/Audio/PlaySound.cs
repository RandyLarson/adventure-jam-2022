using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    public Sounds sound;
    public bool playOnCreate;
    public bool playOnDestroy;
    public bool playOnEnable;

    // Start is called before the first frame update
    void Start()
    {
        if(playOnCreate && AudioController.Current != null)
            AudioController.Current.PlayRandomSound(sound);
    }


    void OnDestroy()
    {
        if(playOnDestroy && AudioController.Current != null)
            AudioController.Current.PlayRandomSound(sound);

    }

    void OnEnable()
    {
        if(playOnEnable && AudioController.Current != null)
            AudioController.Current.PlayRandomSound(sound);
    }
}
