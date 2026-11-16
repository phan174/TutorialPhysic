using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class AudioSettingsPanel : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;

    [Header("Master")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private TMP_Text masterLabel;

    [Header("Music")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TMP_Text musicLabel;

    [Header("SFX")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxLabel;

    private UnityAction<float> onMasterChanged;
    private UnityAction<float> onMusicChanged;
    private UnityAction<float> onSfxChanged;

    private void Awake()
    {
        // Stored so the exact same delegates can be removed in OnDisable.
        onMasterChanged = value => OnVolumeChanged(AudioChannel.Master, masterLabel, "Master", value);
        onMusicChanged = value => OnVolumeChanged(AudioChannel.Music, musicLabel, "Music", value);
        onSfxChanged = value => OnVolumeChanged(AudioChannel.Sfx, sfxLabel, "SFX", value);
    }

    private void OnEnable()
    {
        if (audioManager == null || masterSlider == null || musicSlider == null || sfxSlider == null ||
            masterLabel == null || musicLabel == null || sfxLabel == null)
        {
            Debug.LogWarning($"{name}: assign every field in AudioSettingsPanel.", this);
            return;
        }

        Bind(masterSlider, masterLabel, "Master", AudioChannel.Master, onMasterChanged);
        Bind(musicSlider, musicLabel, "Music", AudioChannel.Music, onMusicChanged);
        Bind(sfxSlider, sfxLabel, "SFX", AudioChannel.Sfx, onSfxChanged);
    }

    private void OnDisable()
    {
        if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(onMasterChanged);
        if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(onMusicChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(onSfxChanged);
    }

    private void Bind(Slider slider, TMP_Text label, string title, AudioChannel channel, UnityAction<float> listener)
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        float volume = audioManager.GetVolume(channel);
        slider.SetValueWithoutNotify(volume);
        ShowPercent(label, title, volume);

        slider.onValueChanged.AddListener(listener);
    }

    private void OnVolumeChanged(AudioChannel channel, TMP_Text label, string title, float value)
    {
        audioManager.SetVolume(channel, value);
        ShowPercent(label, title, value);
    }

    private static void ShowPercent(TMP_Text label, string title, float value) =>
        label.text = $"{title} {value * 100f:F0}%";
}
