using UnityEngine;

namespace Flair
{
    /// <summary>
    /// First-person player: walk, look, jump. Per concept.md 6.2 ("camera-eyes").
    /// The camera itself is not parented here -- this only aims the eye anchor,
    /// and PlayerCameraRig decides where the camera actually sits. That split is
    /// what lets a vision take the camera away from the head.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Leave empty to use the reader on this same object.")]
        [SerializeField] private PlayerInputReader input;

        [Tooltip("Child transform at eye height. Pitches up/down; the body yaws.")]
        [SerializeField] private Transform cameraPivot;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;

        [Tooltip("Peak height of a jump, in metres.")]
        [SerializeField] private float jumpHeight = 1.1f;

        [Tooltip("Exaggerated on purpose -- real -9.81 feels floaty in first person.")]
        [SerializeField] private float gravity = -19.62f;

        [Header("Look")]
        [Tooltip("Degrees per unit of mouse delta.")]
        [SerializeField] private float lookSensitivity = 0.1f;

        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;
        [SerializeField] private bool lockCursor = true;

        private CharacterController controller;
        private float pitch;
        private float verticalVelocity;

        /// <summary>The eye position a vision camera should frame.</summary>
        public Transform CameraPivot => cameraPivot;

        /// <summary>False while a vision has taken control away from the player.</summary>
        public bool ControlEnabled { get; private set; } = true;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (input == null)
            {
                input = GetComponent<PlayerInputReader>();
            }

            if (input == null)
            {
                Debug.LogError("PlayerController: no PlayerInputReader found.", this);
                enabled = false;
                return;
            }

            if (cameraPivot == null)
            {
                Debug.LogError("PlayerController: Camera Pivot is not assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// Called by VisionDirector. While disabled the player is frozen in place --
        /// gravity included, so do not disable control mid-air.
        /// </summary>
        public void SetControlEnabled(bool value)
        {
            ControlEnabled = value;

            if (!value)
            {
                verticalVelocity = 0f;
            }
        }

        private void Update()
        {
            if (!ControlEnabled)
            {
                return;
            }

            HandleLook();
            HandleMove();
        }

        private void HandleLook()
        {
            // Mouse delta is already per-frame, so no Time.deltaTime here.
            Vector2 look = input.Look * lookSensitivity;

            // Yaw turns the whole body so movement follows the camera.
            transform.Rotate(Vector3.up, look.x);

            // Pitch stays on the eye anchor only, clamped so you cannot backflip.
            pitch = Mathf.Clamp(pitch - look.y, minPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void HandleMove()
        {
            Vector2 move = input.Move;
            Vector3 direction = transform.right * move.x + transform.forward * move.y;
            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            if (controller.isGrounded)
            {
                // A small downward bias keeps isGrounded true on slopes and seams.
                if (verticalVelocity < 0f)
                {
                    verticalVelocity = -2f;
                }

                if (input.JumpPressedThisFrame)
                {
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = direction * walkSpeed + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
