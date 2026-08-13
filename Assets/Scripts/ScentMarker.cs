using System.Collections.Generic;
using UnityEngine;

namespace Flair
{
    /// <summary>
    /// A spot in the world that can be smelled. Stage 2 stores the clue as plain
    /// fields on the component: concept.md 8.4 (ScriptableObject vs hard-coded vs
    /// external file) is still undecided, so nothing here commits to an answer.
    /// Whatever wins, it replaces these fields without touching the vision flow.
    /// </summary>
    public class ScentMarker : MonoBehaviour
    {
        [Header("Clue")]
        [Tooltip("Stable id used by the clue log. Must be unique.")]
        [SerializeField] private string clueId = "clue_placeholder";

        [SerializeField] private string displayName = "An unfamiliar scent";

        [Tooltip("Red herrings (6.3) are meant to burn time without paying out. " +
                 "The gate is NOT built yet -- this flag only records the intent " +
                 "so the data is ready when that stage arrives.")]
        [SerializeField] private bool isTrueScent = true;

        [Header("Detection")]
        [Tooltip("How close the player must be, in metres.")]
        [SerializeField] private float smellRadius = 2.5f;

        [Header("Vision")]
        [Tooltip("Placeholder length. 6.5 asks for 10-15s; kept short here so " +
                 "testing the loop is not tedious.")]
        [SerializeField] private float visionDuration = 3f;

        public string ClueId => clueId;
        public string DisplayName => displayName;
        public bool IsTrueScent => isTrueScent;
        public float SmellRadius => smellRadius;
        public float VisionDuration => visionDuration;

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
            Gizmos.color = isTrueScent
                ? new Color(0.2f, 0.9f, 0.4f, 0.9f)
                : new Color(0.9f, 0.3f, 0.2f, 0.9f);

            Gizmos.DrawWireSphere(transform.position, smellRadius);
        }
    }
}
