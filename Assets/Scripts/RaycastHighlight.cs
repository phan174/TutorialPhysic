using UnityEngine;

/// <summary>
/// Example raycast target: changes color while looked at, gets pushed when clicked.
/// It only knows about itself; it never checks what other components the object has.
/// </summary>
[RequireComponent(typeof(Renderer))]
public sealed class RaycastHighlight : MonoBehaviour, IRaycastTarget
{
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField, Min(0f)] private float pushForce = 5f;

    private Renderer rend;
    private Color originalColor;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    public void OnRaycastEnter() => rend.material.color = highlightColor;

    public void OnRaycastExit() => rend.material.color = originalColor;

    public void OnRaycastInteract(RaycastHit hit)
    {
        // Push away from the viewer if this object has a Rigidbody
        if (TryGetComponent(out Rigidbody rb))
            rb.AddForceAtPosition(-hit.normal * pushForce, hit.point, ForceMode.Impulse);
    }
}
