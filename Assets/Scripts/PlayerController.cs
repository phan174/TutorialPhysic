using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerController : MonoBehaviour
{
   
    private const float GroundedStickForce = -2f;

    [Header("Movement")]
    [SerializeField, Min(0f), FormerlySerializedAs("WalkSpeed")] private float walkSpeed = 5f;
    [SerializeField, Min(0f)] private float runningSpeed = 10f;
    [SerializeField, Min(0f)] private float jumpForce = 8f;
    [SerializeField, Min(0f)] private float gravity = 20f;

    [Header("Look")]
    [SerializeField, Min(0f), FormerlySerializedAs("LookSensitivity")] private float lookSensitivity = 0.2f;
    [SerializeField, Range(0f, 90f), FormerlySerializedAs("LookAngleLimit")] private float lookAngleLimit = 90f;

    private CharacterController characterController;
    private Transform cameraTransform;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction jumpAction;

    private float verticalVelocity;
    private float pitch;   // camera up/down angle

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        cameraTransform = GetComponentInChildren<Camera>().transform;

        moveAction = FindAction("Move");
        lookAction = FindAction("Look");
        sprintAction = FindAction("Sprint");
        jumpAction = FindAction("Jump");
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look(lookAction.ReadValue<Vector2>());
        Move(moveAction.ReadValue<Vector2>());
    }

    private void Move(Vector2 input)
    {
        float speed = sprintAction.IsPressed() ? runningSpeed : walkSpeed;
        Vector3 horizontal = (transform.forward * input.y + transform.right * input.x) * speed;

        UpdateVerticalVelocity();

        Vector3 velocity = horizontal + Vector3.up * verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void UpdateVerticalVelocity()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = jumpAction.WasPressedThisFrame() ? jumpForce : GroundedStickForce;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
    }

    // Mouse delta is already "per frame", so it is not multiplied by Time.deltaTime.
    private void Look(Vector2 delta)
    {
        transform.Rotate(0f, delta.x * lookSensitivity, 0f);

        pitch = Mathf.Clamp(pitch - delta.y * lookSensitivity, -lookAngleLimit, lookAngleLimit);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private InputAction FindAction(string actionName)
    {
        InputAction action = InputSystem.actions.FindAction(actionName);

        if (action == null)
        {
            Debug.LogError($"{name}: input action '{actionName}' not found in the project-wide Input Actions.", this);
            enabled = false;   // stop Update from crashing every frame
        }

        return action;
    }
}
