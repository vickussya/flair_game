using System;
using System.Collections.Generic;
using UnityEngine;

namespace Flair
{
    /// <summary>
    /// What the player has established, and the gate that decides when the finale
    /// is reachable. Holds ClueData assets rather than strings, so everything
    /// downstream -- the case file UI (6.9) and the mind board (6.10) -- reads the
    /// same authored data the designer edited in the Inspector.
    ///
    /// Red herrings never advance the gate. concept.md 5.4 has two of them, and a
    /// dead end that counted toward the finale would not be a dead end.
    /// </summary>
    public class ClueLog : MonoBehaviour
    {
        [Tooltip("True clues needed before the finale unlocks. concept.md 5.3 says 3. " +
                 "Kept here rather than in code so the case can be retuned without a recompile.")]
        [SerializeField] private int cluesToUnlockFinale = 3;

        private readonly List<ClueData> collected = new List<ClueData>();

        public IReadOnlyList<ClueData> Collected => collected;

        /// <summary>Everything found, red herrings included.</summary>
        public int Count => collected.Count;

        /// <summary>Only the clues that count toward the finale.</summary>
        public int TrueClueCount { get; private set; }

        public int CluesToUnlockFinale => cluesToUnlockFinale;

        public bool IsFinaleUnlocked { get; private set; }

        /// <summary>Raised with the clue whenever a new one is recorded.</summary>
        public event Action<ClueData> ClueLogged;

        /// <summary>Raised once, the moment enough true clues are held.</summary>
        public event Action FinaleUnlocked;

        public bool Contains(ClueData clue) => clue != null && collected.Contains(clue);

        public bool Contains(string clueId)
        {
            for (int i = 0; i < collected.Count; i++)
            {
                if (collected[i] != null && collected[i].ClueId == clueId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Records a clue. Returns false if it was null or already known.</summary>
        public bool TryLog(ClueData clue)
        {
            if (clue == null || collected.Contains(clue))
            {
                return false;
            }

            collected.Add(clue);

            if (clue.IsTrueScent)
            {
                TrueClueCount++;
            }

            Debug.Log($"[ClueLog] {TrueClueCount}/{cluesToUnlockFinale}  {clue.DisplayName}  " +
                      $"({clue.ClueId}){(clue.IsTrueScent ? string.Empty : "  [red herring]")}", this);

            ClueLogged?.Invoke(clue);

            if (!IsFinaleUnlocked && TrueClueCount >= cluesToUnlockFinale)
            {
                IsFinaleUnlocked = true;
                Debug.Log("[ClueLog] Finale unlocked.", this);
                FinaleUnlocked?.Invoke();
            }

            return true;
        }

        private void OnValidate()
        {
            // A threshold of zero would unlock the finale on the first clue found.
            cluesToUnlockFinale = Mathf.Max(1, cluesToUnlockFinale);
        }
    }
}
