using UnityEngine;

// Example target: changes color when looked at, pushes away when clicked.
[RequireComponent(typeof(Renderer))]
public class RaycastHighlight : MonoBehaviour, IRaycastTarget
{
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float pushForce = 5f;

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
        // If it has a Rigidbody, push it in the direction the ray was travelling
        if (TryGetComponent(out Rigidbody rb))
            rb.AddForceAtPosition(-hit.normal * pushForce, hit.point, ForceMode.Impulse);

        // If it's a pendulum, give it a swing
        if (TryGetComponent(out PendulumScript pendulum))
            pendulum.Push(pendulum.initialAngle);
    }
}
