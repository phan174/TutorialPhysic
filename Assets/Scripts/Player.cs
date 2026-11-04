using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float WalkSpeed = 5.0f;
    [SerializeField] private float runningSpeed = 10.0f;

    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 20.0f;

    [SerializeField] private float LookSensitivity = 0.2f;
    [SerializeField] private float LookAngleLimit = 90f;

    private Camera mainCamera;
    private CharacterController characterController;

    private InputAction moveInput;
    private InputAction runInput;
    private InputAction jumpInput;
    private bool jumped = false;

    private float currentMoveSpeed = 0.0f;
    private Vector3 moveDirection = Vector3.zero;
    private float lookAngle = 0.0f;


    private void Start()
    {
        mainCamera =  GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();

        moveInput = InputSystem.actions.FindAction("Move");
        runInput  = InputSystem.actions.FindAction("Sprint");
        jumpInput = InputSystem.actions.FindAction("Jump");
        jumpInput.started += Jumped;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        currentMoveSpeed = WalkSpeed;

    }

    private void Update()
    {
        Vector2 moveVector = moveInput.ReadValue<Vector2>();
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();


        if(!characterController.isGrounded)
            jumped = false;

        currentMoveSpeed = runInput.IsPressed() ? runningSpeed : WalkSpeed;
        HandleMovement(moveVector);
        Handlelooking(mouseDelta);

    }

    private void HandleMovement(Vector2 moveVector)
    {
        Vector3 forwardMovement = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        
        float currentY = moveDirection.y;

        Vector2 newSpeed = new Vector2(moveVector.y * currentMoveSpeed, moveVector.x * currentMoveSpeed);

        moveDirection = (forwardMovement * newSpeed.x) + (right * newSpeed.y);

        moveDirection.y = (jumped && characterController.isGrounded) ? jumpForce : currentY;

        if(!characterController.isGrounded) 
            moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }
    private void Jumped(InputAction.CallbackContext _)
    {
        jumped = true;
    }

    private void Handlelooking(Vector2 mouseDelta)
    {
        transform.Rotate(0, mouseDelta.x * LookSensitivity, 0);

        lookAngle += -mouseDelta.y * LookSensitivity;
        lookAngle = Mathf.Clamp(lookAngle, -LookAngleLimit, LookAngleLimit);
        mainCamera.transform.localRotation = Quaternion.Euler(lookAngle, 0, 0);
    }
}



    // private Rigidbody rb;
    // private Vector2 moveInput;
    // private void Awake()
    // {
    //     rb = GetComponent<Rigidbody>();
    // }

    // public void OnJump(InputAction.CallbackContext context)
    // {
    //     if (context.performed)
    //     {
    //         Debug.Log(context);
    //         rb.AddForce(new Vector3(0, 100, 0));
    //     }
    // }

    // public void OnMovement(InputAction.CallbackContext context)
    // {
    //     moveInput = context.ReadValue<Vector2>();
    // }

    // private void Update()
    // {
    //     rb.linearVelocity = new Vector3(moveInput.x, rb.linearVelocity.y, moveInput.y);
    //     if(Mouse.current.leftButton.isPressed)
    //     {
    //         Debug.Log("Left mouse button is pressed");
    //     }
    // }
