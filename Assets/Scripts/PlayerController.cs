using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
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

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isSprinting;
    private bool jumpRequested;

    private float verticalVelocity;
    private float pitch;   // camera up/down angle

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        cameraTransform = GetComponentInChildren<Camera>().transform;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look();
        Move();
    }

    private void OnMove(InputValue value) => moveInput = value.Get<Vector2>();

    // Mouse delta can arrive several times per frame, so add them up; Look() clears it after use.
    private void OnLook(InputValue value) => lookInput += value.Get<Vector2>();

    // Needs the "Press And Release" interaction on the Sprint action, otherwise releasing Shift is never reported.
    private void OnSprint(InputValue value) => isSprinting = value.isPressed;

    private void OnJump(InputValue value) => jumpRequested = true;

    private void Move()
    {
        float speed = isSprinting ? runningSpeed : walkSpeed;
        Vector3 horizontal = (transform.forward * moveInput.y + transform.right * moveInput.x) * speed;

        UpdateVerticalVelocity();

        Vector3 velocity = horizontal + Vector3.up * verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void UpdateVerticalVelocity()
    {
        if (characterController.isGrounded)
            verticalVelocity = jumpRequested ? jumpForce : GroundedStickForce;
        else
            verticalVelocity -= gravity * Time.deltaTime;

        jumpRequested = false;  
    }

    private void Look()
    {
        transform.Rotate(0f, lookInput.x * lookSensitivity, 0f);

        pitch = Mathf.Clamp(pitch - lookInput.y * lookSensitivity, -lookAngleLimit, lookAngleLimit);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        lookInput = Vector2.zero;
    }
}
