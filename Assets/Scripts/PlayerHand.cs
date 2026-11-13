using UnityEngine;

// The player's hand: holds at most one Grabbable in front of the camera.
[RequireComponent(typeof(CharacterController))]
public sealed class PlayerHand : MonoBehaviour
{
    [Tooltip("Distance in front of the camera where held objects float (m).")]
    [SerializeField, Min(0.3f)] private float holdDistance = 1.5f;

    [Tooltip("Drop automatically if the object gets stuck this far from the hand, e.g. behind a wall (m).")]
    [SerializeField, Min(0.1f)] private float maxStretch = 2f;

    private Transform holdPoint;
    private Collider playerCollider;

    public Grabbable Held { get; private set; }
    public bool IsHolding => Held != null;

    private void Awake()
    {
        playerCollider = GetComponent<CharacterController>();

        Transform cameraTransform = GetComponentInChildren<Camera>().transform;
        holdPoint = new GameObject("HoldPoint").transform;
        holdPoint.SetParent(cameraTransform, false);
        holdPoint.localPosition = Vector3.forward * holdDistance;
    }

    private void Update()
    {
        if (IsHolding && Vector3.Distance(Held.transform.position, holdPoint.position) > maxStretch)
            Drop();
    }

    public void Grab(Grabbable item)
    {
        if (IsHolding || item == null)
            return;

        Held = item;
        IgnorePlayerCollision(true);   // otherwise the held object bumps into the player's own body
        Held.OnGrabbed(holdPoint);
    }

    public void Drop()
    {
        if (!IsHolding)
            return;

        IgnorePlayerCollision(false);
        Held.OnReleased();
        Held = null;
    }

    private void IgnorePlayerCollision(bool ignore)
    {
        foreach (Collider itemCollider in Held.Colliders)
            Physics.IgnoreCollision(itemCollider, playerCollider, ignore);
    }
}
