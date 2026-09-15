using UnityEngine;

namespace Flair
{
    /// <summary>
    /// The slot the finished 2D vision drops into.
    ///
    /// VisionDirector owns the transition -- camera move, sniff timing, crossfade,
    /// control lock -- and knows nothing about how a vision is actually shown.
    /// VideoVisionPlayer plays rendered video; PlaceholderVisionPlayer shows a
    /// labelled panel. Either drops into the director's Vision Player field and
    /// the rest of the flow is untouched.
    /// </summary>
    public abstract class VisionPlayer : MonoBehaviour
    {
        /// <summary>
        /// Load whatever the vision needs. Called as the sniff starts, so a video
        /// has time to be ready before it is seen. Optional.
        /// </summary>
        public virtual void Prepare(ScentMarker marker)
        {
        }

        /// <summary>True once Begin will show the first frame straight away.</summary>
        public virtual bool IsReady => true;

        /// <summary>Start showing the vision. It starts invisible -- see SetOpacity.</summary>
        public abstract void Begin(ScentMarker marker);

        /// <summary>
        /// 0 is invisible, 1 is fully shown. The director dissolves the vision in
        /// over the 3D scene and back out with this, rather than cutting through black.
        /// </summary>
        public virtual void SetOpacity(float opacity)
        {
        }

        /// <summary>
        /// Seconds left before the vision ends on its own, so the fade-out can start
        /// while it is still moving instead of on a frozen last frame.
        /// </summary>
        public virtual float SecondsRemaining => 0f;

        /// <summary>Polled every frame by the director until it returns true.</summary>
        public abstract bool IsFinished { get; }

        /// <summary>Tear down. Called once the vision has faded fully out.</summary>
        public abstract void End();
    }
}
