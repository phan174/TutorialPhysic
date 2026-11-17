using System;
using UnityEngine;

/// <summary>
/// Pendulum that solves the physics equation itself (no Rigidbody):
///
///     angular acceleration = -(g / L) * sin(angle) - damping * angularVelocity
///
/// Each FixedUpdate is split into small sub-steps. In every sub-step we:
///   1. compute the acceleration from the current angle,
///   2. update the velocity with that acceleration,
///   3. update the angle with the NEW velocity  (this order keeps the energy stable).
/// Everything is in double and the step is fixed, so every run gives identical numbers.
/// </summary>
public sealed class MathPendulum : Pendulum
{
    [Tooltip("Sub-steps per FixedUpdate. More = more accurate.")]
    [SerializeField, Range(1, 100)] private int substeps = 20;

    protected override void Step(double dt)
    {
        double h = dt / substeps;

        for (int i = 0; i < substeps; i++)
        {
            double acceleration = -(Gravity / StringLength) * Math.Sin(Angle) - Damping * AngularVelocity;

            AngularVelocity += acceleration * h;
            Angle += AngularVelocity * h;
        }
    }
}
