using UnityEngine;

public class AudioManager : MonoBehaviour
{   
    [Header("Audio Sources")]
    [SerializeField]  AudioSource musicSource;
    [SerializeField]  AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip walkingSound;

    private void Start()
    {
        musicSource.clip = backgroundMusic;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
