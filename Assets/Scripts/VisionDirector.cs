using System.Collections;
using UnityEngine;

namespace Flair
{
    /// <summary>
    /// Owns the whole trip from the street into a vision and back:
    ///
    ///   lock control -> Bunk starts to sniff, the camera pushes in and the vision
    ///   starts loading -> halfway through the sniff the world dissolves into the
    ///   vision -> near its end the vision dissolves back out -> clue logged ->
    ///   control returned
    ///
    /// No cut through black: the scene blends into the vision and out again, which
    /// is the "viewport slowly blending into the 2D vision" the concept asked for.
    ///
    /// It never touches how a vision is drawn -- that is VisionPlayer's job -- so
    /// the finished 2D animation slots in without this file changing.
    /// </summary>
    public class VisionDirector : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerCameraRig cameraRig;

        [Tooltip("What the camera frames during the sniff. The player's eye anchor.")]
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

        [Header("Sniff timing")]
        [Tooltip("Animator state that holds the sniff. Read at runtime, so the timing " +
                 "stays right when the Mixamo sniff is replaced by our own.")]
        [SerializeField] private string sniffStateName = "Sniff";

        [Tooltip("How far through the sniff the vision starts to dissolve in. 0.5 is halfway.")]
        [Range(0f, 1f)]
        [SerializeField] private float visionStartsAt = 0.5f;

        [Tooltip("How long to wait for the Animator to enter the Sniff state before giving up on it.")]
        [SerializeField] private float sniffEnterTimeout = 1f;

        [Tooltip("Sniff length assumed when there is no animated character -- the greybox capsule.")]
        [SerializeField] private float fallbackSniffSeconds = 2f;

        [Header("Dissolve")]
        [SerializeField] private float fadeInDuration = 1.2f;
        [SerializeField] private float fadeOutDuration = 1f;

        [Tooltip("Longest the sniff will hold for a video that is still loading. Past this " +
                 "the dissolve starts anyway; the video appears as soon as it is ready.")]
        [SerializeField] private float maxReadyWait = 2f;

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
        /// Ignored if a vision is already running. Every scent still plays its
        /// vision, true or false -- what a red herring does NOT do is advance the
        /// finale gate, and ClueLog handles that. The rest of 6.3's real/fake
        /// split (burning stamina, going nowhere) is still a later stage.
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

            // 1. Start loading the vision now, while Bunk sniffs, so it is ready
            //    by the time it is seen. CharacterAnimator fires the sniff itself
            //    the moment IsPlaying goes true.
            visionPlayer.Prepare(marker);

            // 2. Push the camera in on Bunk, alongside the sniff rather than before it.
            GetObservePose(out Vector3 position, out Quaternion rotation);
            Coroutine cameraMove = StartCoroutine(cameraRig.BlendTo(position, rotation, moveToObserveDuration));

            // 3. Wait for the moment in the sniff where the scent lands.
            yield return WaitForSniffPoint();

            for (float waited = 0f; !visionPlayer.IsReady && waited < maxReadyWait; waited += Time.deltaTime)
            {
                yield return null;
            }

            // 4. Dissolve the world into the vision.
            visionPlayer.Begin(marker);
            visionPlayer.SetOpacity(0f);
            yield return Dissolve(0f, 1f, fadeInDuration);

            // 5. Let it play, and start dissolving out while it is still moving
            //    rather than on a frozen last frame.
            while (!visionPlayer.IsFinished && visionPlayer.SecondsRemaining > fadeOutDuration)
            {
                yield return null;
            }

            // 6. The vision fully covers the screen here, so the camera's jump back
            //    to the follow rig is never seen -- the dissolve reveals gameplay.
            if (cameraMove != null)
            {
                StopCoroutine(cameraMove);
            }

            cameraRig.ResumeFollow();
            yield return Dissolve(1f, 0f, fadeOutDuration);
            visionPlayer.End();

            // 7. Bank the clue and hand control back.
            marker.AlreadyExamined = true;
            clueLog.TryLog(marker.Clue);
            player.SetControlEnabled(true);

            IsPlaying = false;
        }

        /// <summary>
        /// Waits until the Sniff state is <see cref="visionStartsAt"/> of the way
        /// through. Reads the real animation, so a longer or shorter sniff moves the
        /// dissolve with it. With no animated character it waits a fixed time.
        /// </summary>
        private IEnumerator WaitForSniffPoint()
        {
            Animator animator = player.GetComponentInChildren<Animator>();
            float start = Time.time;

            if (animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
            {
                int sniffHash = Animator.StringToHash(sniffStateName);

                while (Time.time - start < sniffEnterTimeout)
                {
                    if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == sniffHash)
                    {
                        while (true)
                        {
                            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                            if (state.shortNameHash != sniffHash || state.normalizedTime >= visionStartsAt)
                            {
                                yield break;
                            }

                            yield return null;
                        }
                    }

                    yield return null;
                }
            }

            float remaining = fallbackSniffSeconds * visionStartsAt - (Time.time - start);
            if (remaining > 0f)
            {
                yield return new WaitForSeconds(remaining);
            }
        }

        private IEnumerator Dissolve(float from, float to, float duration)
        {
            for (float t = 0f; t < 1f;)
            {
                t += Time.deltaTime / Mathf.Max(duration, 0.0001f);
                float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                visionPlayer.SetOpacity(Mathf.Lerp(from, to, eased));
                yield return null;
            }

            visionPlayer.SetOpacity(to);
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
