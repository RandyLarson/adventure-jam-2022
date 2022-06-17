using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroAudio : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlayBackground()
    {
        AudioController.Current.StopAllSounds();
        AudioController.Current.PlayRandomSound(Sounds.IntroBackground);
    }
}
