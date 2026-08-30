using System.Collections.Generic;
using UnityEngine;

namespace Flair
{
    /// <summary>
    /// The scent library: E opens it, E closes it again. Lives on the Player.
    ///
    /// It holds no clue data of its own -- ClueLog is already the record of what
    /// has been found, so this reads that rather than keeping a second list that
    /// could disagree with it. What it adds is the authored order of the whole
    /// case, so a scent the player has not found yet can still occupy its slot.
    ///
    /// Opening the inventory is meant to feed the composure meter later
    /// (systems.md 6.7 -- the photo of his sister, and the cost of standing still
    /// inside your own head). That hook is not built; TimeOpen is where it lands.
    /// </summary>
    public class ScentInventory : MonoBehaviour
    {
        [Header("References (leave empty to use this same object)")]
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private PlayerController player;

        [Header("References (must be assigned)")]
        [SerializeField] private ClueLog clueLog;
        [SerializeField] private InventoryHud hud;

        [Tooltip("The inventory refuses to open mid-vision -- the director owns " +
                 "the screen then.")]
        [SerializeField] private VisionDirector visionDirector;

        [Header("First-time hint")]
        [Tooltip("Off by default so the hint comes back every time you enter play " +
                 "mode -- otherwise you would see it once on this machine, ever, " +
                 "and never be able to test it again. Turn it on for a real build, " +
                 "where a player who has already learned E should not be told twice.")]
        [SerializeField] private bool rememberAcrossSessions;

        [Tooltip("PlayerPrefs key used only when the setting above is on.")]
        [SerializeField] private string learnedPrefsKey = "flair.inventory.learned";

        [Tooltip("Seconds after spawn before the hint appears. Long enough that " +
                 "the player has looked at the street first and the prompt reads " +
                 "as an offer rather than a tutorial.")]
        [SerializeField] private float hintDelay = 5f;

        [Header("Contents")]
        [Tooltip("Every scent in the case, in the order they should appear. " +
                 "Unfound ones are drawn as empty slots, so this is the case's " +
                 "shape as well as its contents. SliceSetup fills this in.")]
        [SerializeField] private List<ClueData> allScents = new List<ClueData>();

        /// <summary>True while the overlay is up and the player is frozen.</summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Seconds spent with the inventory open this session. Nothing reads it
        /// yet -- it is here for the composure meter in 6.7.
        /// </summary>
        public float TimeOpen { get; private set; }

        private CursorLockMode cursorLockOnOpen;
        private bool cursorVisibleOnOpen;
        private float spawnTime;

        /// <summary>True once the player has opened the inventory and learned the key.</summary>
        public bool HasLearned { get; private set; }

        private void Awake()
        {
            if (input == null) input = GetComponent<PlayerInputReader>();
            if (player == null) player = GetComponent<PlayerController>();

            if (input == null || player == null || clueLog == null || hud == null)
            {
                Debug.LogError("ScentInventory: one or more references are unassigned.", this);
                enabled = false;
                return;
            }

            HasLearned = rememberAcrossSessions && PlayerPrefs.GetInt(learnedPrefsKey, 0) == 1;
        }

        private void Start()
        {
            // Start, not Awake: InventoryHud clears its own labels in Awake, and
            // asking it to show the hint before that would just be undone.
            spawnTime = Time.time;
            hud.SetHintVisible(false);
        }

        private void Update()
        {
            // A vision takes the screen. If one starts while the inventory is up
            // -- it should not, but a red herring firing on the same frame would
            // do it -- close rather than draw over the director.
            if (visionDirector != null && visionDirector.IsPlaying)
            {
                if (IsOpen)
                {
                    Close();
                }

                // The hint would otherwise sit in the corner of the vision, which
                // is the one place the player is meant to be reading the picture.
                hud.SetHintVisible(false);
                return;
            }

            if (!HasLearned && Time.time - spawnTime >= hintDelay)
            {
                hud.SetHintVisible(true);
            }

            if (IsOpen)
            {
                TimeOpen += Time.deltaTime;
            }

            if (input.InventoryPressedThisFrame)
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public void Open()
        {
            if (IsOpen)
            {
                return;
            }

            IsOpen = true;

            // Opening it once is the whole lesson. Never offer it again.
            if (!HasLearned)
            {
                HasLearned = true;

                if (rememberAcrossSessions)
                {
                    PlayerPrefs.SetInt(learnedPrefsKey, 1);
                    PlayerPrefs.Save();
                }
            }

            hud.SetHintVisible(false);
            hud.Show(allScents, clueLog);

            // Freeze rather than hide the player: SetControlEnabled already zeroes
            // vertical velocity, so do not open this mid-jump and expect gravity.
            player.SetControlEnabled(false);

            cursorLockOnOpen = Cursor.lockState;
            cursorVisibleOnOpen = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            IsOpen = false;
            hud.Hide();
            player.SetControlEnabled(true);

            // Restore what was there rather than assuming Locked -- the end card
            // and the editor both leave the cursor in states worth keeping.
            Cursor.lockState = cursorLockOnOpen;
            Cursor.visible = cursorVisibleOnOpen;
        }

        /// <summary>
        /// Adds a scent to the displayed case at runtime. Only needed if clues are
        /// ever spawned rather than authored; SliceSetup fills the list up front.
        /// </summary>
        public void Register(ClueData scent)
        {
            if (scent != null && !allScents.Contains(scent))
            {
                allScents.Add(scent);
            }
        }
    }
}
