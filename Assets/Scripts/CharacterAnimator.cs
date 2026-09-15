using UnityEngine;

namespace Flair
{
    /// <summary>
    /// Feeds the character's Animator from gameplay. Idle and walk follow how fast
    /// Bunk is actually moving; the sniff plays the moment a vision begins, so the
    /// gesture lands during the camera push-in rather than as a separate button.
    ///
    /// Knows nothing about which model is underneath. It finds whatever Animator is
    /// in the Player's children, so the Mixamo placeholder and the Bunk we build
    /// ourselves both work -- and with the greybox capsule and no Animator at all,
    /// it simply does nothing.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterAnimator : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int SniffHash = Animator.StringToHash("Sniff");

        [Tooltip("Leave empty to find the Animator on the model under this object.")]
        [SerializeField] private Animator animator;

        [Tooltip("Leave empty to find it in the scene.")]
        [SerializeField] private VisionDirector visionDirector;

        [Tooltip("Smooths the speed value so idle-to-walk does not flicker at the threshold.")]
        [SerializeField] private float speedDampTime = 0.1f;

        private CharacterController controller;
        private PlayerController player;
        private bool wasInVision;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            player = GetComponent<PlayerController>();

            if (visionDirector == null)
            {
                visionDirector = FindFirstObjectByType<VisionDirector>();
            }
        }

        private void Update()
        {
            if (animator == null)
            {
                // The model can be swapped in and out by the editor tools, so look
                // again rather than giving up after Awake.
                animator = GetComponentInChildren<Animator>();
                if (animator == null)
                {
                    return;
                }
            }

            animator.SetFloat(SpeedHash, CurrentSpeed(), speedDampTime, Time.deltaTime);

            bool inVision = visionDirector != null && visionDirector.IsPlaying;
            if (inVision && !wasInVision)
            {
                animator.SetTrigger(SniffHash);
            }

            wasInVision = inVision;
        }

        private float CurrentSpeed()
        {
            // While a vision or the scent library has control, Move is not called,
            // but CharacterController.velocity still reports the last move. Without
            // this Bunk would keep walking on the spot through every vision.
            if (player != null && !player.ControlEnabled)
            {
                return 0f;
            }

            Vector3 velocity = controller.velocity;
            velocity.y = 0f;
            return velocity.magnitude;
        }
    }
}
