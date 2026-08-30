using System.Collections.Generic;
using UnityEngine;

namespace Flair
{
    /// <summary>
    /// A spot in the world that can be smelled. Points at a ClueData asset
    /// (concept.md 8.4) rather than holding the clue's fields itself, so a
    /// designer can add or edit a clue from the Inspector with no C# changes.
    /// </summary>
    public class ScentMarker : MonoBehaviour
    {
        [Header("Clue")]
        [SerializeField] private ClueData clue;

        [Header("Detection")]
        [Tooltip("How close the player must be, in metres.")]
        [SerializeField] private float smellRadius = 2.5f;

        [Header("Presentation")]
        [Tooltip("Switched off once the scent has been examined. Leave empty to " +
                 "use every renderer on this object and its children.")]
        [SerializeField] private Renderer[] visuals;

        [Tooltip("Switched off with the renderers. Without this a collected scent " +
                 "would leave its glow behind, lighting a sphere that is no longer " +
                 "there. Leave empty to use every light on this object and its " +
                 "children.")]
        [SerializeField] private Light[] glows;

        public ClueData Clue => clue;
        public string ClueId => clue != null ? clue.ClueId : string.Empty;
        public string DisplayName => clue != null ? clue.DisplayName : string.Empty;
        public bool IsTrueScent => clue == null || clue.IsTrueScent;
        public float SmellRadius => smellRadius;
        public float VisionDuration => clue != null ? clue.VisionDuration : 0f;

        private bool alreadyExamined;

        /// <summary>
        /// Set once its vision has played. Stops the prompt reappearing, and
        /// takes the sphere out of the world -- a scent Bunk has already read is
        /// not still hanging there waiting to be read again.
        ///
        /// The director sets this under the black fade, so the sphere is gone by
        /// the time the world comes back rather than blinking out in front of you.
        /// </summary>
        public bool AlreadyExamined
        {
            get => alreadyExamined;
            set
            {
                alreadyExamined = value;
                ApplyExaminedVisuals();
            }
        }

        /// <summary>
        /// Markers announce themselves rather than the player sweeping physics.
        /// Keeps colliders and layer masks out of the greybox setup.
        /// </summary>
        private static readonly List<ScentMarker> active = new List<ScentMarker>();

        public static IReadOnlyList<ScentMarker> Active => active;

        private void Awake()
        {
            if (visuals == null || visuals.Length == 0)
            {
                visuals = GetComponentsInChildren<Renderer>(true);
            }

            if (glows == null || glows.Length == 0)
            {
                glows = GetComponentsInChildren<Light>(true);
            }
        }

        private void OnEnable()
        {
            active.Add(this);

            // A marker could be authored as already examined, or re-enabled after
            // one was. Either way the sphere should match the flag, not the
            // Inspector's idea of it.
            ApplyExaminedVisuals();
        }

        private void OnDisable() => active.Remove(this);

        /// <summary>
        /// Hides the renderers rather than the GameObject. Deactivating the object
        /// would pull it out of Active and lose the record that this spot was ever
        /// a scent -- which the inventory and any later replay still want.
        /// </summary>
        private void ApplyExaminedVisuals()
        {
            bool visible = !alreadyExamined;

            if (visuals != null)
            {
                for (int i = 0; i < visuals.Length; i++)
                {
                    if (visuals[i] != null)
                    {
                        visuals[i].enabled = visible;
                    }
                }
            }

            if (glows != null)
            {
                for (int i = 0; i < glows.Length; i++)
                {
                    if (glows[i] != null)
                    {
                        glows[i].enabled = visible;
                    }
                }
            }
        }

        // Still drawn for an examined marker, so the greybox keeps showing where
        // the scents are while you are building the level.
        private void OnDrawGizmos()
        {
            Gizmos.color = IsTrueScent
                ? new Color(0.2f, 0.9f, 0.4f, 0.9f)
                : new Color(0.9f, 0.3f, 0.2f, 0.9f);

            Gizmos.DrawWireSphere(transform.position, smellRadius);
        }
    }
}
