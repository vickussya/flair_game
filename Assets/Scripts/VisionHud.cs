using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Flair
{
    /// <summary>
    /// Every pixel the vision flow draws: the smell prompt, the full-screen fade
    /// used to cross between the world and a vision, and the placeholder panel.
    /// Greybox UI on purpose -- no art, no layout work.
    /// </summary>
    public class VisionHud : MonoBehaviour
    {
        [Tooltip("Full-screen black Image with a CanvasGroup. Must be LAST under " +
                 "the Canvas so it draws over everything else.")]
        [SerializeField] private CanvasGroup fadeOverlay;

        [Tooltip("Full-screen panel that stands in for the 2D vision.")]
        [SerializeField] private GameObject visionPanel;

        [SerializeField] private Text visionLabel;

        [SerializeField] private Text promptLabel;

        private void Awake()
        {
            if (fadeOverlay == null || visionPanel == null ||
                visionLabel == null || promptLabel == null)
            {
                Debug.LogError("VisionHud: one or more UI references are unassigned.", this);
                enabled = false;
                return;
            }

            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
            fadeOverlay.interactable = false;

            visionPanel.SetActive(false);
            promptLabel.gameObject.SetActive(false);
        }

        public void ShowPrompt(string text)
        {
            promptLabel.text = text;
            promptLabel.gameObject.SetActive(true);
        }

        public void HidePrompt() => promptLabel.gameObject.SetActive(false);

        public void ShowVision(string text)
        {
            visionLabel.text = text;
            visionPanel.SetActive(true);
        }

        public void HideVision() => visionPanel.SetActive(false);

        /// <summary>Fade the black overlay to <paramref name="target"/> alpha (0 = clear, 1 = black).</summary>
        public IEnumerator FadeTo(float target, float duration)
        {
            float start = fadeOverlay.alpha;

            for (float t = 0f; t < 1f;)
            {
                t += Time.deltaTime / Mathf.Max(duration, 0.0001f);
                fadeOverlay.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(t));
                yield return null;
            }

            fadeOverlay.alpha = target;
        }
    }
}
