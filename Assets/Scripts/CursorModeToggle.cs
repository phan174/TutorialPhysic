using UnityEngine;
using UnityEngine.InputSystem;

// Tab switches between playing (cursor locked) and using the UI (cursor free, player frozen).
[RequireComponent(typeof(PlayerInput))]
public sealed class CursorModeToggle : MonoBehaviour
{
    private const string PlayerMap = "Player";
    private const string UiMap = "UI";

    [Tooltip("Optional. Shown only while the UI mode is on (e.g. the control panel).")]
    [SerializeField] private GameObject uiRoot;

    private PlayerInput playerInput;

    public bool IsUiMode { get; private set; }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        SetUiMode(false);
    }

    private void OnToggleMenu(InputValue value) => SetUiMode(!IsUiMode);

    private void SetUiMode(bool on)
    {
        IsUiMode = on;

        // Disabling the Player map stops movement, looking and raycast clicks in one place.
        playerInput.SwitchCurrentActionMap(on ? UiMap : PlayerMap);

        Cursor.lockState = on ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = on;

        if (uiRoot != null)
            uiRoot.SetActive(on);
    }
}
