using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public sealed class PlayerFootsteps : MonoBehaviour
{
    [SerializeField] private AudioClip footstepClip;

    [Tooltip("Horizontal speed (m/s) above which the player counts as walking.")]
    [SerializeField, Min(0f)] private float minMoveSpeed = 0.1f;

    private CharacterController characterController;
    private AudioSource audioSource;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        audioSource.clip = footstepClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;   // 2D: these are the player's own steps
    }

    private void Update()
    {
        bool shouldPlay = IsWalking();

        if (shouldPlay && !audioSource.isPlaying)
            audioSource.Play();
        else if (!shouldPlay && audioSource.isPlaying)
            audioSource.Stop();
    }

    private bool IsWalking()
    {
        if (!characterController.isGrounded)
            return false;

        Vector3 velocity = characterController.velocity;
        velocity.y = 0f;   // ignore falling / jumping speed
        return velocity.magnitude > minMoveSpeed;
    }
}
