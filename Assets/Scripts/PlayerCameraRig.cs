using System.Collections;
using UnityEngine;

namespace Flair
{
    /// <summary>
    /// Third-person follow camera. Orbits a pivot on the player, keeps itself out
    /// of walls, and hands the movement code a flattened facing to steer by.
    ///
    /// The camera is deliberately NOT a child of the player. That was originally
    /// so a vision could pull it off Bunk's head; it is now also what makes an
    /// orbit possible without fighting the player's own rotation.
    ///
    /// Lives on the camera itself.
    /// </summary>
    public class PlayerCameraRig : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The point the camera orbits. The player's eye anchor is the right " +
                 "height for this -- head height reads better than hip height.")]
        [SerializeField] private Transform eyeAnchor;

        [Tooltip("Leave empty to find the reader on the anchor's root.")]
        [SerializeField] private PlayerInputReader input;

        [Header("Orbit")]
        [Tooltip("How far behind Bunk the camera sits. Classic follow, so far enough " +
                 "back to see the whole body and read the animation.")]
        [SerializeField] private float distance = 3.5f;

        [Tooltip("Raise the orbit above the pivot so the camera looks slightly down.")]
        [SerializeField] private float heightOffset = 0.35f;

        [SerializeField] private float lookSensitivity = 0.15f;
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float startingPitch = 12f;

        [Header("Wall collision")]
        [Tooltip("Bismarkstrasse has a 5m alley with a right-angle blind corner. " +
                 "Without this the camera ends up inside the masonry.")]
        [SerializeField] private float collisionRadius = 0.25f;

        [Tooltip("Snap in fast when blocked, ease back out slowly -- the reverse " +
                 "feels like the camera is lagging behind the player.")]
        [SerializeField] private float pullInSpeed = 40f;

        [SerializeField] private float pushOutSpeed = 6f;

        private float yaw;
        private float pitch;
        private float currentDistance;
        private bool following = true;
        private Transform playerRoot;
        private PlayerController player;

        /// <summary>False while a vision has taken the camera away.</summary>
        public bool IsFollowing => following;

        /// <summary>Set by TouchControls on phones, where pointer look cannot tell fingers apart.</summary>
        public bool IgnorePointerLook { get; set; }

        private Vector2 pendingTouchLook;

        /// <summary>Degrees of yaw (x) and pitch (y) from the touch look zone, applied next frame.</summary>
        public void AddTouchLook(Vector2 degrees)
        {
            pendingTouchLook += degrees;
        }

        /// <summary>Camera facing, flattened to the ground. Movement steers by this.</summary>
        public Vector3 PlanarForward
        {
            get
            {
                Vector3 f = transform.forward;
                f.y = 0f;
                return f.sqrMagnitude > 0.0001f ? f.normalized : Vector3.forward;
            }
        }

        /// <summary>Camera right, flattened to the ground.</summary>
        public Vector3 PlanarRight
        {
            get
            {
                Vector3 r = transform.right;
                r.y = 0f;
                return r.sqrMagnitude > 0.0001f ? r.normalized : Vector3.right;
            }
        }

        private void Awake()
        {
            if (eyeAnchor == null)
            {
                Debug.LogError("PlayerCameraRig: Eye Anchor is not assigned.", this);
                enabled = false;
                return;
            }

            playerRoot = eyeAnchor.root;

            if (input == null)
            {
                input = playerRoot.GetComponentInChildren<PlayerInputReader>();
            }

            player = playerRoot.GetComponentInChildren<PlayerController>();

            yaw = playerRoot.eulerAngles.y;
            pitch = startingPitch;
            currentDistance = distance;
        }

        // LateUpdate so the player has already moved this frame -- otherwise the
        // camera lags a frame behind and the whole game feels loose.
        private void LateUpdate()
        {
            if (!following)
            {
                return;
            }

            // Do not orbit while a vision or the scent library owns the screen.
            bool canLook = input != null && (player == null || player.ControlEnabled);

            if (canLook)
            {
                // Look is bound to <Pointer>/delta, and on a phone every finger is a
                // pointer -- a thumb on the joystick would spin the camera too. With
                // touch controls on, only the dedicated look zone turns the camera.
                Vector2 look = IgnorePointerLook ? Vector2.zero : input.Look * lookSensitivity;
                look += pendingTouchLook;

                yaw += look.x;
                pitch = Mathf.Clamp(pitch - look.y, minPitch, maxPitch);
            }

            pendingTouchLook = Vector2.zero;

            Vector3 pivot = eyeAnchor.position + Vector3.up * heightOffset;
            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 back = orbit * Vector3.back;

            float wanted = DistanceToWall(pivot, back);
            float speed = wanted < currentDistance ? pullInSpeed : pushOutSpeed;
            currentDistance = Mathf.MoveTowards(currentDistance, wanted, speed * Time.deltaTime);

            transform.SetPositionAndRotation(pivot + back * currentDistance, orbit);
        }

        /// <summary>
        /// How far back the camera can sit before it hits something. Casts a sphere
        /// out from the pivot and ignores anything belonging to the player, which
        /// would otherwise block it immediately.
        /// </summary>
        private float DistanceToWall(Vector3 pivot, Vector3 direction)
        {
            RaycastHit[] hits = Physics.SphereCastAll(pivot, collisionRadius, direction,
                                                      distance, ~0, QueryTriggerInteraction.Ignore);

            float nearest = distance;

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform.IsChildOf(playerRoot))
                {
                    continue;
                }

                if (hits[i].distance < nearest)
                {
                    nearest = hits[i].distance;
                }
            }

            return nearest;
        }

        /// <summary>Blend off the follow rig to an arbitrary world pose.</summary>
        public IEnumerator BlendTo(Vector3 targetPosition, Quaternion targetRotation, float duration)
        {
            following = false;

            Vector3 startPosition = transform.position;
            Quaternion startRotation = transform.rotation;

            for (float t = 0f; t < 1f;)
            {
                t += Time.deltaTime / Mathf.Max(duration, 0.0001f);
                float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

                transform.SetPositionAndRotation(
                    Vector3.Lerp(startPosition, targetPosition, eased),
                    Quaternion.Slerp(startRotation, targetRotation, eased));

                yield return null;
            }
        }

        /// <summary>
        /// Go back to following. The director calls this while the screen is black,
        /// so there is nothing to smooth -- and the orbit is re-aimed at wherever
        /// Bunk is now facing, rather than snapping to a stale yaw.
        /// </summary>
        public void ResumeFollow()
        {
            following = true;
            yaw = playerRoot.eulerAngles.y;
            currentDistance = distance;
            LateUpdate();
        }
    }
}