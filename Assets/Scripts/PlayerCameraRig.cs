using System.Collections;
using UnityEngine;

namespace Flair
{
    /// <summary>
    /// Drives the main camera. The camera is deliberately NOT a child of the
    /// player: a vision needs to pull it off the head and frame Bunk from the
    /// outside, and reparenting mid-play is where that goes wrong. Following an
    /// anchor instead makes leaving and returning a blend rather than a surgery.
    ///
    /// Lives on the camera itself.
    /// </summary>
    public class PlayerCameraRig : MonoBehaviour
    {
        [Tooltip("The player's eye-height child transform. The camera copies it in first person.")]
        [SerializeField] private Transform eyeAnchor;

        private bool followEye = true;

        /// <summary>True while the camera is locked to the player's eyes.</summary>
        public bool IsFirstPerson => followEye;

        private void Awake()
        {
            if (eyeAnchor == null)
            {
                Debug.LogError("PlayerCameraRig: Eye Anchor is not assigned.", this);
                enabled = false;
            }
        }

        // LateUpdate so the player has already turned this frame -- otherwise the
        // camera lags the mouse by a frame and the whole game feels loose.
        private void LateUpdate()
        {
            if (followEye)
            {
                transform.SetPositionAndRotation(eyeAnchor.position, eyeAnchor.rotation);
            }
        }

        /// <summary>Blend off the head to an arbitrary world pose.</summary>
        public IEnumerator BlendTo(Vector3 targetPosition, Quaternion targetRotation, float duration)
        {
            followEye = false;

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
        /// Return to the eyes immediately. The director calls this while the
        /// screen is black, so there is nothing to smooth.
        /// </summary>
        public void SnapToEye()
        {
            followEye = true;
            transform.SetPositionAndRotation(eyeAnchor.position, eyeAnchor.rotation);
        }
    }
}
