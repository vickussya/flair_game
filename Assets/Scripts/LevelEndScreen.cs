using System.Collections;
using UnityEngine;

namespace Flair
{
    /// <summary>
    /// The minimal end of the level: when the clue gate opens, wait for the vision
    /// in progress to finish, take control, fade down and show a card.
    ///
    /// This stands in for the finale (7) and the results screen (10) until those
    /// exist. It deliberately knows nothing about how the gate was reached, so
    /// replacing it later means deleting it, not unpicking it.
    /// </summary>
    public class LevelEndScreen : MonoBehaviour
    {
        [Header("References (leave empty to use this same object)")]
        [SerializeField] private ClueLog clueLog;
        [SerializeField] private VisionHud hud;

        [Tooltip("So the end card does not interrupt a vision that is still playing.")]
        [SerializeField] private VisionDirector visionDirector;

        [Header("Player")]
        [SerializeField] private PlayerController player;

        [Header("Card")]
        [TextArea]
        [SerializeField] private string endCardText =
            "THE TRAIL IS WARM\n\nThree clues. Enough to name a thief.\n\nEND OF SLICE";

        [SerializeField] private float pauseBeforeCard = 0.75f;
        [SerializeField] private float fadeDuration = 1f;

        [Header("Think time")]
        [Tooltip("Seconds between the last clue landing and the card taking the " +
                 "screen. The player keeps control throughout -- the point is to " +
                 "let them walk, re-read the scents and reach the answer " +
                 "themselves, rather than being told the trail is warm the instant " +
                 "the third clue lands.")]
        [SerializeField] private float thinkingTime = 20f;

        private bool triggered;

        private void Awake()
        {
            if (clueLog == null) clueLog = GetComponent<ClueLog>();
            if (hud == null) hud = GetComponent<VisionHud>();
            if (visionDirector == null) visionDirector = GetComponent<VisionDirector>();

            if (clueLog == null || hud == null || player == null)
            {
                Debug.LogError("LevelEndScreen: Clue Log, Hud or Player is unassigned.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (clueLog != null)
            {
                clueLog.FinaleUnlocked += HandleFinaleUnlocked;
            }
        }

        private void OnDisable()
        {
            if (clueLog != null)
            {
                clueLog.FinaleUnlocked -= HandleFinaleUnlocked;
            }
        }

        private void HandleFinaleUnlocked()
        {
            if (triggered)
            {
                return;
            }

            triggered = true;
            StartCoroutine(EndRoutine());
        }

        private IEnumerator EndRoutine()
        {
            // The gate opens while the clue's vision is still playing out, so let
            // the player see it land before the level takes the screen back.
            while (visionDirector != null && visionDirector.IsPlaying)
            {
                yield return null;
            }

            yield return new WaitForSeconds(pauseBeforeCard);

            yield return ThinkTime();

            player.SetControlEnabled(false);
            yield return hud.FadeTo(1f, fadeDuration);
            hud.ShowEndCard(endCardText);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// Counts the think time down in the corner, with the player still in
        /// control. Deliberately not skippable: the whole point is that the last
        /// clue is not the same moment as the answer.
        /// </summary>
        private IEnumerator ThinkTime()
        {
            float remaining = thinkingTime;

            while (remaining > 0f)
            {
                // A red herring smelled during the think time would otherwise run
                // the clock down behind the vision, and the card would land on top
                // of it. The clock waits; the player is still thinking either way.
                if (visionDirector != null && visionDirector.IsPlaying)
                {
                    hud.HideCountdown();
                    yield return null;
                    continue;
                }

                hud.ShowCountdown(Format(remaining));
                remaining -= Time.deltaTime;
                yield return null;
            }

            hud.HideCountdown();
        }

        /// <summary>Ceiling, so the timer shows "0:01" for a whole second and never "0:00".</summary>
        private static string Format(float remaining)
        {
            int seconds = Mathf.Max(0, Mathf.CeilToInt(remaining));
            return $"{seconds / 60}:{seconds % 60:00}";
        }
    }
}
