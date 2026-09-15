using UnityEngine;

namespace Flair
{
    /// <summary>
    /// Stands in for the 2D vision until the animation exists: a flat panel with
    /// the clue name on it, held for the marker's duration. Its only job is to
    /// prove the surrounding transition works and can be timed.
    /// </summary>
    public class PlaceholderVisionPlayer : VisionPlayer
    {
        [Tooltip("Leave empty to use the VisionHud on this same object.")]
        [SerializeField] private VisionHud hud;

        private float finishTime;
        private bool playing;

        public override bool IsFinished => !playing || Time.time >= finishTime;

        public override float SecondsRemaining => playing ? Mathf.Max(0f, finishTime - Time.time) : 0f;

        private void Awake()
        {
            if (hud == null)
            {
                hud = GetComponent<VisionHud>();
            }

            if (hud == null)
            {
                Debug.LogError("PlaceholderVisionPlayer: no VisionHud found.", this);
                enabled = false;
            }
        }

        public override void Begin(ScentMarker marker)
        {
            playing = true;
            finishTime = Time.time + marker.VisionDuration;

            hud.SetVisionOpacity(0f);
            hud.ShowVision($"2D VISION — PLACEHOLDER\n\n{marker.DisplayName}\n({marker.ClueId})");
        }

        public override void SetOpacity(float opacity) => hud.SetVisionOpacity(opacity);

        public override void End()
        {
            playing = false;
            hud.HideVision();
        }
    }
}
