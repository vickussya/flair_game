using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Flair
{
    /// <summary>
    /// Draws the scent inventory: a full-black overlay listing every scent the
    /// case holds, found ones in white and unfound ones as empty slots.
    ///
    /// This is the SCENTS tab of the library overlay in world.md -- the VISIONS,
    /// DIALOGUE and OBJECTS tabs are not built yet, and neither is Viki's mind
    /// board (task 34). There is no tab strip either: one tab does not need one,
    /// and guessing at its layout now would only be in the way when the rest land.
    ///
    /// Greybox UI on purpose, same as VisionHud -- no art, no layout polish.
    /// Rows are built once on the first open and then reused.
    /// </summary>
    public class InventoryHud : MonoBehaviour
    {
        [Tooltip("Full-screen panel. Must sit BELOW the fade overlay in the Canvas " +
                 "so a vision's black still covers it.")]
        [SerializeField] private GameObject panel;

        [Tooltip("Parent for the scent rows. Wants a VerticalLayoutGroup.")]
        [SerializeField] private RectTransform rowParent;

        [Tooltip("Header line, e.g. 'SCENTS  2/3'.")]
        [SerializeField] private Text headerLabel;

        [Header("First-time hint")]
        [Tooltip("Sits OUTSIDE the panel -- the panel is inactive while closed, so " +
                 "a hint parented to it could never be seen.")]
        [SerializeField] private Text hintLabel;

        [SerializeField] private string hintText = "Press E to open Scent Inventory";

        [Tooltip("Brightest point of the blink. Faint on purpose -- it is a nudge, " +
                 "not part of the case.")]
        [Range(0f, 1f)]
        [SerializeField] private float hintOpacity = 0.35f;

        [Tooltip("Dimmest point of the blink. Never fully to zero: a hint that " +
                 "vanishes completely reads as a bug rather than a pulse.")]
        [Range(0f, 1f)]
        [SerializeField] private float hintMinOpacity = 0.08f;

        [Tooltip("Seconds for one full fade down and back up. Slow on purpose.")]
        [SerializeField] private float hintPulsePeriod = 2.4f;

        [Tooltip("Seconds to fade up from nothing before the blink starts. The " +
                 "hint should arrive rather than appear.")]
        [SerializeField] private float hintFadeInDuration = 1.2f;

        [Header("Look")]
        [SerializeField] private Color foundColor = Color.white;

        [Tooltip("Unfound slots. Dim rather than invisible -- an empty slot is " +
                 "information: it tells the player something is still out there.")]
        [SerializeField] private Color unfoundColor = new Color(1f, 1f, 1f, 0.25f);

        [Tooltip("Red is crime and scent per the style guide. Used for the count " +
                 "once every scent in the case has been found.")]
        [SerializeField] private Color completeColor = new Color(0.85f, 0.1f, 0.15f);

        [SerializeField] private int rowFontSize = 22;

        private readonly List<Text> rows = new List<Text>();
        private float hintShownTime;

        public bool IsVisible => panel != null && panel.activeSelf;

        private void Awake()
        {
            if (panel == null || rowParent == null || headerLabel == null)
            {
                Debug.LogError("InventoryHud: one or more UI references are unassigned.", this);
                enabled = false;
                return;
            }

            panel.SetActive(false);

            if (hintLabel != null)
            {
                hintLabel.text = hintText;
                hintLabel.color = new Color(1f, 1f, 1f, hintOpacity);
                hintLabel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// The one-line nudge in the corner, shown until the player opens the
        /// inventory for the first time. ScentInventory decides when that is.
        /// </summary>
        public void SetHintVisible(bool visible)
        {
            if (hintLabel == null || hintLabel.gameObject.activeSelf == visible)
            {
                return;
            }

            if (visible)
            {
                // Start from nothing, or the first frame would flash at full
                // opacity before the fade had a chance to run.
                SetHintAlpha(0f);
                hintShownTime = Time.time;
            }

            hintLabel.gameObject.SetActive(visible);
        }

        /// <summary>
        /// Fades the hint up from nothing, then blinks it. A sine rather than a
        /// hard on/off -- a toggling label reads as an alert, and this is meant to
        /// be noticeable without being urgent.
        /// </summary>
        private void Update()
        {
            if (hintLabel == null || !hintLabel.gameObject.activeSelf)
            {
                return;
            }

            float elapsed = Time.time - hintShownTime;

            if (elapsed < hintFadeInDuration)
            {
                SetHintAlpha(Mathf.Lerp(0f, hintOpacity,
                                        elapsed / Mathf.Max(hintFadeInDuration, 0.0001f)));
                return;
            }

            // Cosine, not sine, so the pulse begins at its peak -- which is exactly
            // where the fade-in left off. A sine would start mid-brightness and
            // put a visible step between the two.
            float phase = (elapsed - hintFadeInDuration)
                          * (2f * Mathf.PI / Mathf.Max(hintPulsePeriod, 0.01f));

            SetHintAlpha(Mathf.Lerp(hintMinOpacity, hintOpacity,
                                    (Mathf.Cos(phase) + 1f) * 0.5f));
        }

        private void SetHintAlpha(float alpha)
        {
            Color c = hintLabel.color;
            c.a = alpha;
            hintLabel.color = c;
        }

        /// <summary>
        /// Redraws and shows the overlay. <paramref name="allScents"/> is the whole
        /// case in authored order, so unfound entries can hold their place;
        /// <paramref name="log"/> decides which of them have been found.
        /// </summary>
        public void Show(IReadOnlyList<ClueData> allScents, ClueLog log)
        {
            Redraw(allScents, log);
            panel.SetActive(true);
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void Redraw(IReadOnlyList<ClueData> allScents, ClueLog log)
        {
            int count = allScents != null ? allScents.Count : 0;
            EnsureRows(count);

            int found = 0;

            for (int i = 0; i < count; i++)
            {
                ClueData scent = allScents[i];
                bool isFound = scent != null && log != null && log.Contains(scent);

                if (isFound)
                {
                    found++;
                }

                Text row = rows[i];
                row.gameObject.SetActive(true);
                row.color = isFound ? foundColor : unfoundColor;

                // Numbered so an empty slot still reads as a specific missing
                // thing rather than a blank line.
                string number = (i + 1).ToString("00");

                if (!isFound)
                {
                    row.text = $"{number}   — — — — —";
                    continue;
                }

                string body = $"{number}   {scent.DisplayName.ToUpperInvariant()}";

                if (!string.IsNullOrWhiteSpace(scent.Description))
                {
                    body += $"\n        {scent.Description}";
                }

                // A red herring is never labelled as one. 6.3 is explicit that
                // telling true from false is the player's job, so the inventory
                // shows what Bunk smelled, not whether it was worth smelling.
                row.text = body;
            }

            // Rows left over from a longer previous case.
            for (int i = count; i < rows.Count; i++)
            {
                rows[i].gameObject.SetActive(false);
            }

            headerLabel.text = $"SCENTS   {found}/{count}";
            headerLabel.color = count > 0 && found == count ? completeColor : foundColor;
        }

        private void EnsureRows(int needed)
        {
            while (rows.Count < needed)
            {
                var go = new GameObject($"ScentRow_{rows.Count + 1:00}",
                                        typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                go.transform.SetParent(rowParent, false);

                Text text = go.GetComponent<Text>();
                text.font = headerLabel.font;
                text.fontSize = rowFontSize;
                text.alignment = TextAnchor.UpperLeft;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Overflow;

                rows.Add(text);
            }
        }
    }
}
