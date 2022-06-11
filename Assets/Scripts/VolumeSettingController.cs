using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSettingController : MonoBehaviour
{
    public Slider musicVolume;
    public Slider effectVolume;
    public Toggle muteBox;

    private bool IsUpdating { get; set; }
    private void OnEnable()
    {
        UpdateControls();
    }

    public void UpdateAudioController()
    {
        if (IsUpdating)
            return;

        if ( musicVolume != null )
            AudioController.MusicVolume = musicVolume.value;
        if ( effectVolume != null )
            AudioController.EffectVolume = effectVolume.value;
        if ( muteBox != null )
            AudioController.MuteAllVolume = muteBox.isOn;
    }

    private void UpdateControls()
    {
        try
        {
            IsUpdating = true;
            if (musicVolume != null)
                musicVolume.value = AudioController.MusicVolume;
            if (effectVolume != null)
                effectVolume.value = AudioController.EffectVolume;
            if (muteBox != null)
                muteBox.isOn = AudioController.MuteAllVolume;
        }
        finally
        {
            IsUpdating = false;
        }
    }

    void Start()
    {
        UpdateControls();    
    }
}
