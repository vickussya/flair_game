using UnityEngine;
using UnityEngine.EventSystems;

namespace Flair
{
    /// <summary>
    /// The part of a phone screen that turns the camera: drag anywhere that is not
    /// a button or the joystick. Built by TouchControls, which places it behind
    /// every other control so buttons always win the touch.
    ///
    /// Each finger is tracked by the EventSystem separately, so moving with one
    /// thumb and looking with the other works at the same time.
    /// </summary>
    public class TouchLookZone : MonoBehaviour, IDragHandler
    {
        [Tooltip("Degrees of turn per pixel of drag, measured on a 1920-wide screen.")]
        public float degreesPerPixel = 0.15f;

        public PlayerCameraRig rig;

        public void OnDrag(PointerEventData eventData)
        {
            if (rig == null)
            {
                return;
            }

            // Screens differ wildly in pixel count. Scale so the same physical
            // swipe across the screen turns the camera the same amount on any phone.
            float scale = 1920f / Mathf.Max(1, Screen.width);
            rig.AddTouchLook(eventData.delta * degreesPerPixel * scale);
        }
    }
}
