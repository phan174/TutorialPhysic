using UnityEngine;
using UnityEngine.Audio;

public enum AudioChannel
{
    Master,
    Music,
    Sfx,
}

public sealed class AudioManager : MonoBehaviour
{
    private const float MinVolume = 0.0001f;   // log10(0) is undefined; this maps to -80 dB (silent)
    private const float DefaultVolume = 1f;

    // Must match the exposed parameter names in the Audio Mixer.
    private static readonly string[] MixerParameters = { "MasterVolume", "MusicVolume", "SfxVolume" };

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Volume")]
    [SerializeField] private AudioMixer mixer;

    private float[] volumes;

    // Loaded on first use, so UI that runs before this Awake still reads the saved values.
    private float[] Volumes
    {
        get
        {
            if (volumes == null)
                LoadVolumes();

            return volumes;
        }
    }

    // AudioMixer.SetFloat is ignored in Awake, so saved volumes are applied here.
    private void Start()
    {
        for (int i = 0; i < Volumes.Length; i++)
            ApplyToMixer((AudioChannel)i);

        if (musicSource == null || backgroundMusic == null)
            return;

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    // Short one-shot sounds only (clicks, impacts, landing). They overlap and cannot be stopped.
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource == null || clip == null)
            return;

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public float GetVolume(AudioChannel channel) => Volumes[(int)channel];

    public void SetVolume(AudioChannel channel, float volume)
    {
        Volumes[(int)channel] = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MixerParameters[(int)channel], Volumes[(int)channel]);
        ApplyToMixer(channel);
    }

    private void LoadVolumes()
    {
        volumes = new float[MixerParameters.Length];

        for (int i = 0; i < volumes.Length; i++)
            volumes[i] = PlayerPrefs.GetFloat(MixerParameters[i], DefaultVolume);
    }

    private void ApplyToMixer(AudioChannel channel)
    {
        if (mixer == null)
            return;

        // Hearing is logarithmic: a linear slider must be converted to decibels.
        float decibels = Mathf.Log10(Mathf.Max(Volumes[(int)channel], MinVolume)) * 20f;

        if (!mixer.SetFloat(MixerParameters[(int)channel], decibels))
            Debug.LogWarning($"{name}: exposed parameter '{MixerParameters[(int)channel]}' not found in the Audio Mixer.", this);
    }
}
