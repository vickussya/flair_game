using System.Collections;
using UnityEngine;

namespace Flair
{
    /// <summary>
    /// Owns the whole trip from first person into a vision and back:
    ///
    ///   lock control -> camera leaves the head and frames Bunk -> a beat for the
    ///   deep breath -> fade out -> vision -> fade out -> camera back on the eyes
    ///   -> clue logged -> control returned
    ///
    /// It never touches how a vision is drawn -- that is VisionPlayer's job -- so
    /// the finished 2D animation slots in without this file changing.
    /// </summary>
    public class VisionDirector : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerCameraRig cameraRig;

        [Tooltip("What the camera frames during the breath. The player's eye anchor.")]
        [SerializeField] private Transform focusPoint;

        [Header("Systems (leave empty to use this same object)")]
        [SerializeField] private VisionPlayer visionPlayer;
        [SerializeField] private VisionHud hud;
        [SerializeField] private ClueLog clueLog;

        [Header("Observation shot")]
        [Tooltip("How far in front of Bunk the camera sits.")]
        [SerializeField] private float observeDistance = 2.2f;

        [Tooltip("Degrees off dead-centre, so the shot is angled rather than a mugshot.")]
        [SerializeField] private float observeYawOffset = 25f;

        [SerializeField] private float moveToObserveDuration = 1f;

        [Tooltip("Dead air where the deep-breath animation will play once Bunk is modelled.")]
        [SerializeField] private float breathHoldDuration = 1f;

        [SerializeField] private float fadeDuration = 0.5f;

        /// <summary>True from the moment a vision is requested until control returns.</summary>
        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            if (visionPlayer == null) visionPlayer = GetComponent<VisionPlayer>();
            if (hud == null) hud = GetComponent<VisionHud>();
            if (clueLog == null) clueLog = GetComponent<ClueLog>();

            if (player == null || cameraRig == null || focusPoint == null ||
                visionPlayer == null || hud == null || clueLog == null)
            {
                Debug.LogError("VisionDirector: one or more references are unassigned.", this);
                enabled = false;
            }
        }

        /// <summary>
        /// Ignored if a vision is already running. Note there is deliberately no
        /// red-herring check here yet: ScentMarker.IsTrueScent carries the intent,
        /// but 6.3's real/fake split is a later stage.
        /// </summary>
        public void BeginVision(ScentMarker marker)
        {
            if (IsPlaying || marker == null || !enabled)
            {
                return;
            }

            StartCoroutine(VisionRoutine(marker));
        }

        private IEnumerator VisionRoutine(ScentMarker marker)
        {
            IsPlaying = true;
            player.SetControlEnabled(false);
            hud.HidePrompt();

            // 1. Pull the camera off the eyes and turn it back on Bunk.
            GetObservePose(out Vector3 position, out Quaternion rotation);
            yield return cameraRig.BlendTo(position, rotation, moveToObserveDuration);

            // 2. The beat where he draws breath. Empty until the model is animated.
            yield return new WaitForSeconds(breathHoldDuration);

            // 3. Cross into the vision under black.
            yield return hud.FadeTo(1f, fadeDuration);
            visionPlayer.Begin(marker);
            yield return hud.FadeTo(0f, fadeDuration);

            while (!visionPlayer.IsFinished)
            {
                yield return null;
            }

            // 4. Back out under black, so the camera snap is never seen.
            yield return hud.FadeTo(1f, fadeDuration);
            visionPlayer.End();
            cameraRig.SnapToEye();

            // 5. Bank the clue and hand control back.
            marker.AlreadyExamined = true;
            clueLog.TryLog(marker.ClueId, marker.DisplayName);

            player.SetControlEnabled(true);
            yield return hud.FadeTo(0f, fadeDuration);

            IsPlaying = false;
        }

        private void GetObservePose(out Vector3 position, out Quaternion rotation)
        {
            Vector3 focus = focusPoint.position;
            Vector3 direction = Quaternion.AngleAxis(observeYawOffset, Vector3.up)
                                * player.transform.forward;

            position = focus + direction * observeDistance;
            rotation = Quaternion.LookRotation(focus - position, Vector3.up);
        }
    }
}
