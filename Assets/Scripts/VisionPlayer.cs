using UnityEngine;

namespace Flair
{
    /// <summary>
    /// The slot the finished 2D vision drops into.
    ///
    /// VisionDirector owns the transition -- camera move, breath beat, fade,
    /// control lock -- and knows nothing about how a vision is actually shown.
    /// When the animation exists, write a subclass that drives a VideoPlayer or a
    /// Timeline, drag it into the director's Vision Player field in place of
    /// PlaceholderVisionPlayer, and the rest of the flow is untouched.
    ///
    /// That is why concept.md 6.5 (pre-rendered video vs in-engine animation)
    /// does not have to be decided yet.
    /// </summary>
    public abstract class VisionPlayer : MonoBehaviour
    {
        /// <summary>Start showing the vision. Called while the screen is black.</summary>
        public abstract void Begin(ScentMarker marker);

        /// <summary>Polled every frame by the director until it returns true.</summary>
        public abstract bool IsFinished { get; }

        /// <summary>Tear down. Also called while the screen is black.</summary>
        public abstract void End();
    }
}
