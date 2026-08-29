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

        public ClueData Clue => clue;
        public string ClueId => clue != null ? clue.ClueId : string.Empty;
        public string DisplayName => clue != null ? clue.DisplayName : string.Empty;
        public bool IsTrueScent => clue == null || clue.IsTrueScent;
        public float SmellRadius => smellRadius;
        public float VisionDuration => clue != null ? clue.VisionDuration : 0f;

        /// <summary>Set once its vision has played. Stops the prompt reappearing.</summary>
        public bool AlreadyExamined { get; set; }

        /// <summary>
        /// Markers announce themselves rather than the player sweeping physics.
        /// Keeps colliders and layer masks out of the greybox setup.
        /// </summary>
        private static readonly List<ScentMarker> active = new List<ScentMarker>();

        public static IReadOnlyList<ScentMarker> Active => active;

        private void OnEnable() => active.Add(this);

        private void OnDisable() => active.Remove(this);

        private void OnDrawGizmos()
        {
            Gizmos.color = IsTrueScent
                ? new Color(0.2f, 0.9f, 0.4f, 0.9f)
                : new Color(0.9f, 0.3f, 0.2f, 0.9f);

            Gizmos.DrawWireSphere(transform.position, smellRadius);
        }
    }
}
