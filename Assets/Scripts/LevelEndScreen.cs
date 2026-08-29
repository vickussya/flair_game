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

            player.SetControlEnabled(false);
            yield return hud.FadeTo(1f, fadeDuration);
            hud.ShowEndCard(endCardText);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
