using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerLandingSound : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioClip landingClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.6f;

    [Tooltip("Seconds in the air before a landing makes a sound. Stops tiny bumps from triggering it.")]
    [SerializeField, Min(0f)] private float minAirTime = 0.1f;

    private CharacterController characterController;
    private bool wasGrounded = true; 
    private float airTime;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (audioManager == null)
            Debug.LogWarning($"{name}: drag the AudioManager object into PlayerLandingSound.", this);
    }

    private void Update()
    {
        bool isGrounded = characterController.isGrounded;

        if (!isGrounded)
            airTime += Time.deltaTime;
        else if (!wasGrounded && airTime >= minAirTime && audioManager != null)
            audioManager.PlaySFX(landingClip, volume);

        if (isGrounded)
            airTime = 0f;

        wasGrounded = isGrounded;
    }
}
