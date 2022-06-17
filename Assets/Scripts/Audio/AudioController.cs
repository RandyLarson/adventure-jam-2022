using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SoundEffect
{
    public Sounds sound;
    public AudioClip[] clips;
}

public class AudioController : MonoBehaviour
{
    private AudioSource AudioSource;
    private AudioSource AmbientSource;
    [ArrayElementTitle("sound")]
    public SoundEffect[] sounds;
    public Dictionary<Sounds, SoundEffect> soundTable;
    public AudioClip[] MusicTracks;
    public AudioClip ThemeTrack;
    public AudioClip StreetScene;

    public static AudioController Current;
    private AudioSource RadioPlayer;
    private AudioSource musicPlayer;
    private int currentTrack = -1;

    void Start()
    {
        Current = this;

        // Create hash table of sounds
        soundTable = new Dictionary<Sounds, SoundEffect>();
        foreach (var se in sounds)
        {
            soundTable.Add(se.sound, se);
        }

        AudioSource = GetComponent<AudioSource>();
        AmbientSource = this.gameObject.AddComponent<AudioSource>();
        if (musicPlayer == null) {
            musicPlayer = gameObject.AddComponent<AudioSource>();
        }
        RadioPlayer = gameObject.AddComponent<AudioSource>();
        RadioPlayer.loop = true;

        MusicVolume = GameController.TheGameData.GamePrefs.GameSettings.MusicVolume;
        MuteAllVolume = GameController.TheGameData.GamePrefs.GameSettings.MuteAll;
        PlayMusic();
    }

    void Update()
    {
        PlayMusic();
    }

    public static float MusicVolume
    {
        get
        {
            return Current.musicPlayer.volume;
        }
        set
        {
            Current.musicPlayer.volume = value;
            Current.RadioPlayer.volume = value;
            GameController.TheGameController.GameData.GamePrefs.GameSettings.MusicVolume = value;
        }
    }

    public static float EffectVolume
    {
        get
        {
            return GameController.TheGameData.GamePrefs.GameSettings.EffectVolume;
        }
        set
        {
            GameController.TheGameData.GamePrefs.GameSettings.EffectVolume = value;
        }
    }

    public static bool MuteAllVolume
    {
        get
        {
            return Current.musicPlayer.mute;
        }
        set
        {
            Current.musicPlayer.mute = value;
            GameController.TheGameData.GamePrefs.GameSettings.MuteAll = value;
        }
    }

    public static void StopAmbientTrack()
    {
        Current.AmbientSource.Stop();
    }

    public static void PlaySound(Sounds sound)
    {
        Debug.LogFormat("Playing Sound: {0}", sound.ToString());
        Current.PlayRandomSound(sound);
    }

    public void PlayMusic()
    {
        if (!musicPlayer.isPlaying)
        {
            currentTrack = currentTrack + 1;
            if (currentTrack >= MusicTracks.Length)
            {
                currentTrack = 0;
            }

            if (currentTrack < MusicTracks.Length)
                musicPlayer.PlayOneShot(MusicTracks[currentTrack]);
        }
    }

    AudioClip CurrentRadioClip;
    public void PlayOnRadio(Sounds sound)
    {
        if (soundTable.ContainsKey(sound))
        {
            var audioClips = soundTable[sound].clips;
            var index = UnityEngine.Random.Range(0, audioClips.Length);
            CurrentRadioClip = audioClips[index];
            PlaySoundOnSource(RadioPlayer, CurrentRadioClip, true);
        }
    }

    public void StopPlayingRadio()
    {
        RadioPlayer.Stop();
    }

    public void StopPlaying(Sounds sound)
    {
        if (AudioController.MuteAllVolume)
            return;

        if (soundTable.ContainsKey(sound))
        {
            AudioSource.Stop();
        }
    }

    public void PlaySoundOnSource(AudioSource onAudioSource, AudioClip sound, bool seekRandomly)
    {
        if (AudioController.MuteAllVolume)
            return;

        onAudioSource.Stop();
        onAudioSource.clip = sound;
        if ( seekRandomly )
            RadioPlayer.timeSamples = UnityEngine.Random.Range(0, sound.samples);
        onAudioSource.Play();
    }

    public void PlayRandomSound(Sounds sound)
    {
        if (AudioController.MuteAllVolume)
            return;

        var position = GameObject.FindObjectOfType<Camera>().transform.position;
        if (soundTable.ContainsKey(sound))
        {
            var audioClips = soundTable[sound].clips;
            var index = UnityEngine.Random.Range(0, audioClips.Length);
            var clip = audioClips[index];

            AudioSource.PlayClipAtPoint(clip, position, AudioController.EffectVolume);
        }
    }

    public GameObject LoopRandomSound(Sounds sound, float forHowLong)
    {
        if (AudioController.MuteAllVolume)
            return null;

        var position = GameObject.FindObjectOfType<Camera>().transform.position;
        if (soundTable.ContainsKey(sound))
        {
            var audioClips = soundTable[sound].clips;
            var index = UnityEngine.Random.Range(0, audioClips.Length);
            var clip = audioClips[index];

            GameObject itsController = new GameObject();
            itsController.transform.position = position;
            var acc = itsController.AddComponent<AudioClipController>();
            acc.Play(clip, true, forHowLong);
            return itsController;
        }

        return null;
    }

    public void PlayTheme()
    {
        if(musicPlayer == null) {
            musicPlayer = gameObject.AddComponent<AudioSource>();
        }
        musicPlayer.clip = ThemeTrack;
        musicPlayer.loop = true;
        musicPlayer.Play();
    }

    public void StopTheme()
    {
        if (musicPlayer != null)
        {
            musicPlayer.Stop();
        }
    }

    public void StopAllSounds()
    {
        var audio = FindObjectsOfType<AudioSource>();
        foreach(var audioS in audio) {
            audioS.Stop();
        }
    }

}
