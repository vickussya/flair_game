using UnityEngine;

namespace Flair
{
    /// <summary>
    /// Watches for the nearest scent in range, offers the prompt, and asks the
    /// director for a vision when the player commits. Lives on the Player.
    /// </summary>
    public class SmellInteractor : MonoBehaviour
    {
        [Tooltip("Leave empty to use the reader on this same object.")]
        [SerializeField] private PlayerInputReader input;

        [SerializeField] private VisionDirector visionDirector;
        [SerializeField] private VisionHud hud;

        [Tooltip("The scent library. While it is open the player is reading, not smelling.")]
        [SerializeField] private ScentInventory inventory;

        [Tooltip("Shown while a scent is in range. 'Hold' because the Interact " +
                 "action ships with a Hold interaction.")]
        [SerializeField] private string promptFormat = "Hold F — Smell: {0}";

        private ScentMarker currentTarget;

        private void Awake()
        {
            if (input == null)
            {
                input = GetComponent<PlayerInputReader>();
            }

            if (input == null || visionDirector == null || hud == null)
            {
                Debug.LogError("SmellInteractor: one or more references are unassigned.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            // The director owns the screen during a vision, and the inventory
            // owns it while the player is reading. Neither wants a smell prompt
            // drawn underneath it.
            if (visionDirector.IsPlaying || (inventory != null && inventory.IsOpen))
            {
                hud.HidePrompt();
                return;
            }

            currentTarget = FindNearestInRange();

            if (currentTarget == null)
            {
                hud.HidePrompt();
                return;
            }

            hud.ShowPrompt(string.Format(promptFormat, currentTarget.DisplayName));

            if (input.InteractPerformedThisFrame)
            {
                visionDirector.BeginVision(currentTarget);
            }
        }

        private ScentMarker FindNearestInRange()
        {
            ScentMarker nearest = null;
            float nearestDistance = float.MaxValue;

            // Markers register themselves, so this is a short list, not a scene sweep.
            var markers = ScentMarker.Active;
            for (int i = 0; i < markers.Count; i++)
            {
                ScentMarker marker = markers[i];
                if (marker == null || marker.AlreadyExamined)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, marker.transform.position);
                if (distance <= marker.SmellRadius && distance < nearestDistance)
                {
                    nearest = marker;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }
    }
}
