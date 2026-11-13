using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class Grabbable : MonoBehaviour, IRaycastTarget
{
    [Header("Carry")]
    [Tooltip("How quickly the object catches up with the hand. 1 = instantly.")]
    [SerializeField, Range(0.05f, 1f)] private float followStrength = 0.5f;
    [SerializeField, Min(0f)] private float maxSpeed = 15f;

    [Header("Highlight")]
    [SerializeField] private Color highlightColor = Color.yellow;

    private Rigidbody rb;
    private Collider[] colliders;
    private ObjectHighlighter highlighter;

    private Transform holdPoint;
    private Quaternion rotationOffset;
    private bool usedGravity;
    private RigidbodyInterpolation previousInterpolation;

    public IReadOnlyList<Collider> Colliders => colliders;
    public bool IsHeld => holdPoint != null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        highlighter = new ObjectHighlighter(gameObject, highlightColor);
    }

    private void FixedUpdate()
    {
        if (!IsHeld)
            return;

        // Move by velocity, not position, so the object still collides while carried.
        Vector3 toHand = holdPoint.position - rb.position;
        rb.linearVelocity = Vector3.ClampMagnitude(toHand * (followStrength / Time.fixedDeltaTime), maxSpeed);

        rb.angularVelocity = Vector3.zero;
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, holdPoint.rotation * rotationOffset, followStrength));
    }

    public void OnRaycastEnter()
    {
        if (!IsHeld)
            highlighter.Show();
    }

    public void OnRaycastExit() => highlighter.Hide();

    public void OnRaycastInteract(RaycastHit hit, PlayerHand hand) => hand.Grab(this);

    public void OnGrabbed(Transform hand)
    {
        highlighter.Hide();

        holdPoint = hand;
        rotationOffset = Quaternion.Inverse(hand.rotation) * rb.rotation;

        usedGravity = rb.useGravity;
        previousInterpolation = rb.interpolation;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void OnReleased()
    {
        holdPoint = null;
        rb.useGravity = usedGravity;
        rb.interpolation = previousInterpolation;
    }
}
