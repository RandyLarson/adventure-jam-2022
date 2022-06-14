using UnityEngine;

public class AudioClipController : MonoBehaviour
{
    public AudioSource audioSource;
    public bool loop;
    public float loopDuration;

    public bool AutoDestroy = true;

    public float StartTime;


    public void Play(AudioClip clip, bool loopIt, float loopForDuration)
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.loop = loopIt;

        if (loopIt)
            loopDuration = loopForDuration;
        else
            loopDuration = clip.length;

        audioSource.Play();
        StartTime = Time.time;
    }

    private void Update()
    {
        if ( loopDuration >= 0 && Time.time > StartTime + loopDuration )
        {
            audioSource.Stop();

            if (AutoDestroy)
                Destroy(gameObject);
        }
    }
}
