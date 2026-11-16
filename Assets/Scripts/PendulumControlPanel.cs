using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PendulumControlPanel : MonoBehaviour
{
    [SerializeField] private Pendulum pendulum;

    [Header("UI")]
    [SerializeField] private Slider angleSlider;
    [SerializeField] private TMP_Text angleLabel;
    [SerializeField] private Button releaseButton;
    [SerializeField] private Button resetButton;

    private void OnEnable()
    {
        if (pendulum == null || angleSlider == null || angleLabel == null || releaseButton == null || resetButton == null)
        {
            Debug.LogWarning($"{name}: assign every field in PendulumControlPanel.", this);
            return;
        }

        angleSlider.minValue = Pendulum.MinReleaseAngle;
        angleSlider.maxValue = Pendulum.MaxReleaseAngle;
        angleSlider.wholeNumbers = true;

        angleSlider.onValueChanged.AddListener(OnAngleChanged);
        releaseButton.onClick.AddListener(pendulum.Release);
        resetButton.onClick.AddListener(pendulum.ResetToStart);

        // The pendulum may not be initialised yet; ResetDone fires once it is.
        pendulum.ResetDone += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (pendulum == null || angleSlider == null || releaseButton == null || resetButton == null)
            return;

        angleSlider.onValueChanged.RemoveListener(OnAngleChanged);
        releaseButton.onClick.RemoveListener(pendulum.Release);
        resetButton.onClick.RemoveListener(pendulum.ResetToStart);
        pendulum.ResetDone -= Refresh;
    }

    private void OnAngleChanged(float degrees) => pendulum.SetReleaseAngle(degrees);

    private void Refresh()
    {
        angleSlider.SetValueWithoutNotify(pendulum.ReleaseAngle);
        angleLabel.text = $"Release angle: {pendulum.ReleaseAngle:F0}°";
    }
}
