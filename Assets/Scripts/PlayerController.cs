using UnityEngine;

namespace Flair
{
    /// <summary>
    /// Third-person player: walk, jump, and turn to face where you are going.
    ///
    /// The camera is not here -- PlayerCameraRig owns looking, and this only asks
    /// it which way is "forward" so movement is relative to the view. Bunk then
    /// rotates toward whatever direction is pushed, which is what gives the turn
    /// and pivot animations something to do.
    ///
    /// Third person is per the teacher's note of 3 Sep 2026: in first person
    /// nobody ever sees the animation, and animation is the point of this project.
    /// concept.md 6.2 had already listed it as coming.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Leave empty to use the reader on this same object.")]
        [SerializeField] private PlayerInputReader input;

        [Tooltip("Head-height child. The camera orbits it; VisionDirector frames it. " +
                 "No longer pitched here -- looking lives on the camera rig.")]
        [SerializeField] private Transform cameraPivot;

        [Tooltip("Leave empty to use the main camera. Movement is relative to it.")]
        [SerializeField] private Transform cameraTransform;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 5f;

        [Tooltip("Degrees per second Bunk turns toward the direction you push.")]
        [SerializeField] private float turnSpeed = 720f;

        [Tooltip("Peak height of a jump, in metres.")]
        [SerializeField] private float jumpHeight = 1.1f;

        [Tooltip("Exaggerated on purpose -- real -9.81 feels floaty.")]
        [SerializeField] private float gravity = -19.62f;

        [Header("Cursor")]
        [SerializeField] private bool lockCursor = true;

        private CharacterController controller;
        private float verticalVelocity;

        /// <summary>The head-height point a vision camera should frame.</summary>
        public Transform CameraPivot => cameraPivot;

        /// <summary>False while a vision or the scent library has taken control.</summary>
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
        /// Called by VisionDirector and the scent library. While disabled Bunk is
        /// frozen in place -- gravity included, so do not disable him mid-air.
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

            HandleMove();
        }

        private void HandleMove()
        {
            Vector2 move = input.Move;
            Vector3 direction = CameraRelative(move);

            // Turn toward the way you are going. RotateTowards rather than a snap
            // so there is an actual turn to animate.
            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, target, turnSpeed * Time.deltaTime);
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

        /// <summary>
        /// Turns stick/WASD input into a world direction relative to the camera,
        /// flattened so looking up or down never slows Bunk down.
        /// </summary>
        private Vector3 CameraRelative(Vector2 move)
        {
            if (move.sqrMagnitude < 0.0001f)
            {
                return Vector3.zero;
            }

            Transform cam = ResolveCamera();

            if (cam == null)
            {
                // No camera yet: fall back to Bunk's own facing rather than freezing.
                return transform.forward * move.y + transform.right * move.x;
            }

            Vector3 forward = cam.forward;
            Vector3 right = cam.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 direction = right * move.x + forward * move.y;
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        private Transform ResolveCamera()
        {
            if (cameraTransform != null)
            {
                return cameraTransform;
            }

            Camera main = Camera.main;
            if (main != null)
            {
                cameraTransform = main.transform;
            }

            return cameraTransform;
        }
    }
}