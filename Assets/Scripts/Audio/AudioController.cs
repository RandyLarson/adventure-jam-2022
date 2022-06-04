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
        musicPlayer = gameObject.AddComponent<AudioSource>();

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

}
