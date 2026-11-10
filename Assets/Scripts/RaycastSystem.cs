using UnityEngine;
using UnityEngine.InputSystem;

// Implement this on any object you want to react to the raycast.
public interface IRaycastTarget
{
    void OnRaycastEnter();               // crosshair started pointing at it
    void OnRaycastExit();                // crosshair stopped pointing at it
    void OnRaycastInteract(RaycastHit hit); // player pressed the interact button on it
}

// Attach to the player (or the camera). Casts a ray from the center of the camera every frame.
public class RaycastSystem : MonoBehaviour
{
    [Header("Ray Settings")]
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private LayerMask hitLayers = ~0; // Everything by default
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Tooltip("Max number of hits the ray can store per frame. Extra hits are dropped.")]
    [SerializeField] private int maxHits = 10;

    [Header("Input")]
    [Tooltip("Name of the action in the project-wide Input Actions asset")]
    [SerializeField] private string interactActionName = "Attack";

    [Header("Debug")]
    [SerializeField] private bool drawDebugRay = true;

    public bool HasHit { get; private set; }
    public RaycastHit CurrentHit { get; private set; }

    private Camera rayCamera;
    private InputAction interactInput;
    private IRaycastTarget currentTarget;

    // Buffer created once and reused every frame
    private RaycastHit[] hitBuffer;
    private int hitCount;

    // Every hit from this frame (only the first HitCount entries are valid)
    public RaycastHit[] AllHits => hitBuffer;
    public int HitCount => hitCount;

    private void Awake()
    {
        hitBuffer = new RaycastHit[maxHits];
    }

    private void Start()
    {
        rayCamera = GetComponentInChildren<Camera>();
        if (rayCamera == null) rayCamera = Camera.main;

        interactInput = InputSystem.actions.FindAction(interactActionName);
        if (interactInput == null)
            Debug.LogWarning($"RaycastSystem: Input action '{interactActionName}' not found.");
    }

    private void Update()
    {
        CastRay();

        if (interactInput != null && interactInput.WasPressedThisFrame() && HasHit)
        {
            Debug.Log($"Raycast hit: {CurrentHit.collider.name} at {CurrentHit.point}");
            currentTarget?.OnRaycastInteract(CurrentHit);
        }
    }

    private void CastRay()
    {
        Ray ray = rayCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // screen center

        hitCount = Physics.RaycastNonAlloc(ray, hitBuffer, maxDistance, hitLayers, triggerInteraction);

        // The results are NOT sorted, so find the closest one ourselves
        HasHit = false;
        RaycastHit hit = default;
        float closest = float.MaxValue;
        for (int i = 0; i < hitCount; i++)
        {
            // Skip our own colliders (e.g. the player's CharacterController)
            if (hitBuffer[i].collider.transform.IsChildOf(transform)) continue;

            if (hitBuffer[i].distance < closest)
            {
                closest = hitBuffer[i].distance;
                hit = hitBuffer[i];
                HasHit = true;
            }
        }
        CurrentHit = hit;

        IRaycastTarget newTarget = HasHit ? hit.collider.GetComponentInParent<IRaycastTarget>() : null;

        // Handle enter / exit when the target changes
        if (newTarget != currentTarget)
        {
            currentTarget?.OnRaycastExit();
            currentTarget = newTarget;
            currentTarget?.OnRaycastEnter();
        }

        if (drawDebugRay)
        {
            Vector3 end = HasHit ? hit.point : ray.origin + ray.direction * maxDistance;
            Debug.DrawLine(ray.origin, end, HasHit ? Color.green : Color.red);
        }
    }
}
