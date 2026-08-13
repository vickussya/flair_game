using System;
using System.Collections.Generic;
using UnityEngine;

namespace Flair
{
    /// <summary>
    /// What the player has established. Stage 2 only needs "a clue is logged" to
    /// be observable, so this writes to the Console and raises an event; the case
    /// file UI from 6.9 subscribes to that event later without changing this.
    /// </summary>
    public class ClueLog : MonoBehaviour
    {
        private readonly List<string> collected = new List<string>();

        public IReadOnlyList<string> Collected => collected;

        public int Count => collected.Count;

        /// <summary>Raised with the clue id whenever a new clue is recorded.</summary>
        public event Action<string> ClueLogged;

        public bool Contains(string clueId) => collected.Contains(clueId);

        /// <summary>Records a clue. Returns false if it was already known.</summary>
        public bool TryLog(string clueId, string displayName)
        {
            if (string.IsNullOrWhiteSpace(clueId) || collected.Contains(clueId))
            {
                return false;
            }

            collected.Add(clueId);
            Debug.Log($"[ClueLog] {collected.Count}. {displayName}  ({clueId})", this);
            ClueLogged?.Invoke(clueId);
            return true;
        }
    }
}
