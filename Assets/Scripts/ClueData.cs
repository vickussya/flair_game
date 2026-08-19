using UnityEngine;

namespace Flair
{
    /// <summary>
    /// A clue as a designer-editable asset: create one from the Create menu,
    /// fill in the Inspector, no C# required. Settles concept.md 8.4 and
    /// replaces the inline fields ScentMarker used to hold directly.
    /// </summary>
    [CreateAssetMenu(fileName = "NewClue", menuName = "Flair/Clue")]
    public class ClueData : ScriptableObject
    {
        [Tooltip("Stable id used by the clue log. Must be unique.")]
        [SerializeField] private string clueId = "clue_placeholder";

        [SerializeField] private string displayName = "An unfamiliar scent";

        [TextArea]
        [Tooltip("What the clue actually is, for whoever is authoring the case.")]
        [SerializeField] private string description;

        [Tooltip("Red herrings (6.3) are meant to burn time without paying out. " +
                 "The gate is NOT built yet -- this flag only records the intent " +
                 "so the data is ready when that stage arrives.")]
        [SerializeField] private bool isTrueScent = true;

        [Tooltip("The next clue in the chain (concept.md 5.3). Leave empty if " +
                 "this one is a dead end or the finale clue.")]
        [SerializeField] private ClueData leadsTo;

        [Tooltip("Placeholder length. 6.5 asks for 10-15s; kept short here so " +
                 "testing the loop is not tedious.")]
        [SerializeField] private float visionDuration = 3f;

        [Tooltip("Which vision plays for this clue. Free text until 6.5 " +
                 "(pre-rendered video vs in-engine animation) is decided.")]
        [SerializeField] private string visionId;

        public string ClueId => clueId;
        public string DisplayName => displayName;
        public string Description => description;
        public bool IsTrueScent => isTrueScent;
        public ClueData LeadsTo => leadsTo;
        public float VisionDuration => visionDuration;
        public string VisionId => visionId;
    }
}
