using System;
using UnityEngine;

public abstract class Pendulum : MonoBehaviour, IRaycastTarget
{

    [Tooltip("String length L in meters.")]
    [SerializeField, Min(0.1f)] private float stringLength = 1f;

    [Tooltip("Angle the bob is pulled to before release, in degrees. Keep it small (<= 15).")]
    [SerializeField, Range(1f, 60f)] private float releaseAngle = 10f;

    [Tooltip("Damping b in 1/s. 0 = swings forever.")]
    [SerializeField, Min(0f)] private float damping = 0.05f;

    [SerializeField] private bool logPeriod = true;

  
    protected double Angle { get; set; }
    protected double AngularVelocity { get; set; }

    private int stepCount;              // how many fixed steps since release
    private double lastCrossingTime;    // when the bob last passed the bottom going "up"
    private bool wasNegative;           // was the angle negative in the previous step?

    public bool IsSwinging { get; private set; }
    public float StringLength => stringLength;
    public float Damping => damping;
    public double Gravity => Physics.gravity.magnitude;
    public double SimulatedTime => stepCount * (double)Time.fixedDeltaTime;
    public double TheoreticalPeriod => 2.0 * Math.PI * Math.Sqrt(stringLength / Gravity);
    public double MeasuredPeriod { get; private set; }


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

    public void Release()
    {
        IsSwinging = true;
    }

    public void OnRaycastEnter() { }

    public void OnRaycastExit() { }

    public void OnRaycastInteract(RaycastHit hit, PlayerHand hand)
    {
        if (IsSwinging)
            Reset();
        else
            Release();
    }

    protected abstract void Step(double dt);

    private void MeasurePeriod()
    {
        bool isNegative = Angle < 0.0;

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
