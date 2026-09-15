using UnityEditor;
using UnityEngine;

namespace Flair.EditorTools
{
    /// <summary>
    /// Shows the phone controls in the editor, with the mouse standing in for a
    /// finger, so the layout can be checked without building for the web. It is a
    /// per-machine editor preference, never saved into the project or a build.
    /// </summary>
    public static class TouchPreviewMenu
    {
        private const string MenuPath = "FLAIR/Build/Preview Touch Controls In Editor";

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            bool on = !EditorPrefs.GetBool(TouchControls.PreviewPrefKey, false);
            EditorPrefs.SetBool(TouchControls.PreviewPrefKey, on);
            Menu.SetChecked(MenuPath, on);

            Debug.Log(on
                ? "[TouchPreview] On. Press Play to see the phone controls. Only one finger at a time with a mouse."
                : "[TouchPreview] Off. Keyboard and mouse controls again.");
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, EditorPrefs.GetBool(TouchControls.PreviewPrefKey, false));
            return true;
        }
    }
}
