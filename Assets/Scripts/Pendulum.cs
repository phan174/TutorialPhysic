using System;
using UnityEngine;

/// <summary>
/// Base class for a simple pendulum.
/// Attach a subclass to the PIVOT (the point the string hangs from).
/// The bob must be a child of the pivot so it rotates with it.
///
/// The base class owns: settings, state, click interaction, period measurement, visuals.
/// A subclass only answers one question: "how does the angle change during one step?" (see Step).
/// </summary>
public abstract class Pendulum : MonoBehaviour, IRaycastTarget
{
    // ---------- Settings (Inspector only, nobody else can change them) ----------

    [Tooltip("String length L in meters.")]
    [SerializeField, Min(0.1f)] private float stringLength = 1f;

    [Tooltip("Angle the bob is pulled to before release, in degrees. Keep it small (<= 15).")]
    [SerializeField, Range(1f, 60f)] private float releaseAngle = 10f;

    [Tooltip("Damping b in 1/s. 0 = swings forever.")]
    [SerializeField, Min(0f)] private float damping = 0.05f;

    [SerializeField] private bool logPeriod = true;

    // ---------- State (only this class and its children can change it) ----------

    /// <summary>Angle from the vertical, in radians.</summary>
    protected double Angle { get; set; }

    /// <summary>How fast the angle changes, in radians per second.</summary>
    protected double AngularVelocity { get; set; }

    private int stepCount;              // how many fixed steps since release
    private double lastCrossingTime;    // when the bob last passed the bottom going "up"
    private bool wasNegative;           // was the angle negative in the previous step?

    // ---------- Read-only info for other scripts ----------

    public bool IsSwinging { get; private set; }
    public float StringLength => stringLength;
    public float Damping => damping;
    public double Gravity => Physics.gravity.magnitude;
    public double SimulatedTime => stepCount * (double)Time.fixedDeltaTime;
    public double TheoreticalPeriod => 2.0 * Math.PI * Math.Sqrt(stringLength / Gravity);
    public double MeasuredPeriod { get; private set; }

    // ---------- Unity ----------

    protected virtual void Awake()
    {
        Reset();
    }

    // Private on purpose: subclasses must NOT write their own FixedUpdate.
    private void FixedUpdate()
    {
        if (!IsSwinging)
            return;

        Step(Time.fixedDeltaTime);   // the subclass moves the pendulum
        stepCount++;

        MeasurePeriod();
        transform.localRotation = Quaternion.Euler((float)(Angle * Mathf.Rad2Deg), 0f, 0f);
    }

    // ---------- Actions ----------

    /// <summary>Hold the bob still at the release angle.</summary>
    public void Reset()
    {
        IsSwinging = false;
        stepCount = 0;
        lastCrossingTime = -1.0;
        MeasuredPeriod = 0.0;

        Angle = releaseAngle * Mathf.Deg2Rad;
        AngularVelocity = 0.0;
        wasNegative = false;

        transform.localRotation = Quaternion.Euler(releaseAngle, 0f, 0f);
    }

    /// <summary>Let the bob go.</summary>
    public void Release()
    {
        IsSwinging = true;
    }

    // ---------- IRaycastTarget: click once to release, click again to reset ----------

    public void OnRaycastEnter() { }

    public void OnRaycastExit() { }

    public void OnRaycastInteract(RaycastHit hit)
    {
        if (IsSwinging)
            Reset();
        else
            Release();
    }

    // ---------- For subclasses ----------

    /// <summary>Advance Angle and AngularVelocity by dt seconds. Same input must always give the same output.</summary>
    protected abstract void Step(double dt);

    // ---------- Helpers ----------

    private void MeasurePeriod()
    {
        bool isNegative = Angle < 0.0;

        // The bob just passed the lowest point going from negative to positive angle.
        // The time between two such moments is exactly one period.
        if (wasNegative && !isNegative)
        {
            if (lastCrossingTime >= 0.0)
            {
                MeasuredPeriod = SimulatedTime - lastCrossingTime;

                if (logPeriod)
                    Debug.Log($"{name} | t = {SimulatedTime:F2} s | measured T = {MeasuredPeriod:F3} s | theory T = {TheoreticalPeriod:F3} s");
            }

            lastCrossingTime = SimulatedTime;
        }

        wasNegative = isNegative;
    }
}
