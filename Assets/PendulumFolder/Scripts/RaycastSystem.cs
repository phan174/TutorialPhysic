using UnityEngine;
using UnityEngine.InputSystem;


public interface IRaycastTarget
{
    void OnRaycastEnter();              
    void OnRaycastExit();                
    void OnRaycastInteract(RaycastHit hit, PlayerHand hand);
}

[RequireComponent(typeof(PlayerHand))]
public class RaycastSystem : MonoBehaviour
{
    [Header("Ray Settings")]
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private LayerMask hitLayers = ~0; 
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Tooltip("Max number of hits the ray can store per frame. Extra hits are dropped.")]
    [SerializeField] private int maxHits = 10;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRay = true;

    public bool HasHit { get; private set; }
    public RaycastHit CurrentHit { get; private set; }

    private Camera rayCamera;
    private PlayerHand hand;
    private IRaycastTarget currentTarget;
    private bool interactRequested;

    // Buffer created once and reused every frame
    private RaycastHit[] hitBuffer;
    private int hitCount;

    // Every hit from this frame (only the first HitCount entries are valid)
    public RaycastHit[] AllHits => hitBuffer;
    public int HitCount => hitCount;

    private void Awake()
    {
        hitBuffer = new RaycastHit[maxHits];
        hand = GetComponent<PlayerHand>();
    }

    private void Start()
    {
        rayCamera = GetComponentInChildren<Camera>();
        if (rayCamera == null) rayCamera = Camera.main;
    }

    private void OnAttack(InputValue value) => interactRequested = true;

    private void Update()
    {
        CastRay();

        if (interactRequested)
        {
            if (hand.IsHolding)
            {
                // While holding something, a click always means "put it down".
                Debug.Log($"Click: dropped {hand.Held.name}");
                hand.Drop();
            }
            else if (HasHit)
            {
                Debug.Log($"Click: hit {CurrentHit.collider.name} at {CurrentHit.point}");
                currentTarget?.OnRaycastInteract(CurrentHit, hand);
            }
            else
            {
                Debug.Log($"Click: nothing within {maxDistance} m");
            }
        }

        interactRequested = false;
    }

    private void CastRay()
    {
        Ray ray = rayCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // screen center

        hitCount = Physics.RaycastNonAlloc(ray, hitBuffer, maxDistance, hitLayers, triggerInteraction);

        HasHit = false;
        RaycastHit hit = default;
        float closest = float.MaxValue;
        for (int i = 0; i < hitCount; i++)
        {
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
