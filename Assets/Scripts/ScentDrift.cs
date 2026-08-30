using UnityEngine;

namespace Flair
{
    /// <summary>
    /// Drifts a scent marker slowly around the spot it was authored at, inside a
    /// box it can never leave. A scent that hangs perfectly still reads as a prop;
    /// one that moves a little reads as something in the air.
    ///
    /// Perlin noise rather than a sine or a random walk: sine loops visibly and a
    /// random walk wanders off. Noise gives smooth, unrepeating motion that stays
    /// bounded by construction, so the sphere cannot drift into a wall or out of
    /// its own smell radius.
    ///
    /// The seed comes from the marker's authored position, so the motion is the
    /// same every run -- the same reason the rubble placement is seeded rather
    /// than random.
    /// </summary>
    public class ScentDrift : MonoBehaviour
    {
        [Tooltip("Half the size of the box it may wander in, in metres. The sphere " +
                 "never leaves this, so keep it well inside the smell radius.")]
        [SerializeField] private Vector3 halfExtents = new Vector3(0.7f, 0.45f, 0.7f);

        [Tooltip("Cycles per second through the noise. Very low -- this should be " +
                 "something you notice only if you stand and watch it.")]
        [SerializeField] private float speed = 0.3f;

        private Vector3 home;
        private float seedX;
        private float seedY;
        private float seedZ;

        private void Awake()
        {
            home = transform.position;

            // Derived from where the marker sits, so two markers never drift in
            // step and the same marker moves identically in every play session.
            float baseSeed = Mathf.Abs(home.x * 73.13f + home.y * 31.7f + home.z * 19.77f);
            seedX = baseSeed % 100f;
            seedY = (baseSeed + 37.4f) % 100f;
            seedZ = (baseSeed + 71.9f) % 100f;
        }

        private void Update()
        {
            float t = Time.time * speed;

            // PerlinNoise returns 0..1, so this maps to -1..1 and then to the box.
            var offset = new Vector3(
                (Mathf.PerlinNoise(seedX, t) - 0.5f) * 2f * halfExtents.x,
                (Mathf.PerlinNoise(seedY, t) - 0.5f) * 2f * halfExtents.y,
                (Mathf.PerlinNoise(seedZ, t) - 0.5f) * 2f * halfExtents.z);

            transform.position = home + offset;
        }

        /// <summary>
        /// The box, drawn only when the marker is selected -- useful when placing
        /// one near a wall or a shelf, where the drift is what would clip through.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // Before Awake runs, home is not set yet, so draw around where it sits.
            Vector3 centre = Application.isPlaying ? home : transform.position;

            Gizmos.color = new Color(0.9f, 0.3f, 0.2f, 0.5f);
            Gizmos.DrawWireCube(centre, halfExtents * 2f);
        }
    }
}
