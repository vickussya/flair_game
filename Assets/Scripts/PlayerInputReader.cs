using UnityEngine;
using UnityEngine.InputSystem;

namespace Flair
{
    /// <summary>
    /// The only place in the player code that talks to the Input System.
    /// Everything else reads plain values from here, so the camera rework for
    /// third-person (concept.md 6.2) will not have to touch input wiring.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("Input")]
        [Tooltip("Drag Assets/InputSystem_Actions here.")]
        [SerializeField] private InputActionAsset inputActions;

        [Tooltip("Action map to enable. The shipped asset calls it 'Player'.")]
        [SerializeField] private string actionMapName = "Player";

        private InputActionMap playerMap;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction jumpAction;
        private InputAction interactAction;

        /// <summary>WASD / left stick, as (x = strafe, y = forward).</summary>
        public Vector2 Move => moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

        /// <summary>
        /// Mouse delta for this frame. Already frame-relative, so do NOT
        /// multiply this by Time.deltaTime.
        /// </summary>
        public Vector2 Look => lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;

        /// <summary>True only on the frame the jump button went down.</summary>
        public bool JumpPressedThisFrame => jumpAction != null && jumpAction.WasPressedThisFrame();

        /// <summary>
        /// True on the frame Interact completes. The shipped asset gives Interact
        /// a Hold interaction (~0.4s), so this is a short hold rather than a tap.
        /// Delete "Hold" on the Interact action to make it instant.
        /// </summary>
        public bool InteractPerformedThisFrame =>
            interactAction != null && interactAction.WasPerformedThisFrame();

        private void Awake()
        {
            if (inputActions == null)
            {
                Debug.LogError("PlayerInputReader: no Input Actions asset assigned. " +
                               "Drag Assets/InputSystem_Actions into the Input Actions field.", this);
                enabled = false;
                return;
            }

            playerMap = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
            if (playerMap == null)
            {
                Debug.LogError($"PlayerInputReader: no action map named '{actionMapName}' " +
                               $"in {inputActions.name}.", this);
                enabled = false;
                return;
            }

            moveAction = playerMap.FindAction("Move", throwIfNotFound: false);
            lookAction = playerMap.FindAction("Look", throwIfNotFound: false);
            jumpAction = playerMap.FindAction("Jump", throwIfNotFound: false);
            interactAction = playerMap.FindAction("Interact", throwIfNotFound: false);

            if (moveAction == null || lookAction == null ||
                jumpAction == null || interactAction == null)
            {
                Debug.LogError($"PlayerInputReader: '{actionMapName}' is missing one of " +
                               "Move, Look, Jump or Interact.", this);
                enabled = false;
            }
        }

        private void OnEnable() => playerMap?.Enable();

        private void OnDisable() => playerMap?.Disable();
    }
}
