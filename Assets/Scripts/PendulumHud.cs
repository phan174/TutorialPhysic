using TMPro;
using UnityEngine;

public sealed class PendulumHud : MonoBehaviour
{
    private const string NotMeasured = "--";

    [SerializeField] private Pendulum pendulum;
    [SerializeField] private TMP_Text label;

    private void OnEnable()
    {
        if (pendulum == null || label == null)
        {
            Debug.LogWarning($"{name}: assign Pendulum and Label in PendulumHud.", this);
            return;
        }

        pendulum.PeriodMeasured += ShowMeasured;
        pendulum.ResetDone += ShowNotMeasured;
        ShowNotMeasured();
    }

    private void OnDisable()
    {
        if (pendulum == null)
            return;

        pendulum.PeriodMeasured -= ShowMeasured;
        pendulum.ResetDone -= ShowNotMeasured;
    }

    private void ShowMeasured(double period) => ShowText($"{period:F3} s");

    private void ShowNotMeasured() => ShowText(NotMeasured);

    private void ShowText(string measured)
    {
        label.text = $"L = {pendulum.StringLength:F2} m    θ0 = {pendulum.ReleaseAngle:F0}°    " +
                     $"T (theory) = {pendulum.TheoreticalPeriod:F3} s    T (measured) = {measured}";
    }
}
