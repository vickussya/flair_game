using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Flair
{
    /// <summary>
    /// On-screen controls for phones: a joystick to walk, SNIFF, JUMP and SCENTS
    /// buttons, and a drag area to look around. Appears only on mobile devices, so
    /// desktop and the Windows build are untouched.
    ///
    /// The joystick and buttons pretend to be a gamepad, and every Player action
    /// already has a gamepad binding -- so walking, jumping, sniffing and the scent
    /// library run through the exact same code as the keyboard, with nothing
    /// duplicated. Look is the exception: see PlayerCameraRig.IgnorePointerLook.
    ///
    /// Builds itself when the scene loads, so no scene wiring is needed.
    /// </summary>
    public class TouchControls : MonoBehaviour
    {
        public const string PreviewPrefKey = "Flair.PreviewTouchControls";

        // Palette from docs/vision-style-guide.md.
        private static readonly Color Black = new Color(18 / 255f, 4 / 255f, 23 / 255f, 0.55f);
        private static readonly Color Red = new Color(182 / 255f, 12 / 255f, 40 / 255f, 0.85f);
        private static readonly Color Knob = new Color(1f, 1f, 1f, 0.35f);

        /// <summary>True while on-screen controls are showing. Prompts check it to drop keyboard hints.</summary>
        public static bool IsActive { get; private set; }

        private PlayerCameraRig rig;
        private VisionDirector director;
        private ClueLog clueLog;
        private CanvasGroup group;
        private Sprite circle;
        private Font font;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!ShouldUseTouch() || FindFirstObjectByType<TouchControls>() != null)
            {
                return;
            }

            new GameObject("TouchControls").AddComponent<TouchControls>();
        }

        private static bool ShouldUseTouch()
        {
#if UNITY_EDITOR
            // FLAIR > Build > Preview Touch Controls In Editor, to test the layout
            // with the mouse standing in for a finger.
            if (UnityEditor.EditorPrefs.GetBool(PreviewPrefKey, false))
            {
                return true;
            }
#endif
            return Application.isMobilePlatform;
        }

        private void Start()
        {
            rig = FindFirstObjectByType<PlayerCameraRig>();
            director = FindFirstObjectByType<VisionDirector>();
            clueLog = FindFirstObjectByType<ClueLog>();

            if (rig == null)
            {
                Debug.LogWarning("TouchControls: no PlayerCameraRig in the scene, so there is nothing to control.", this);
                Destroy(gameObject);
                return;
            }

            rig.IgnorePointerLook = true;

            // A locked cursor cannot press on-screen buttons. Phones have no cursor
            // anyway; this matters for previewing in the editor.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            EnsureEventSystem();

            circle = MakeCircleSprite();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            BuildOverlay();
            IsActive = true;
        }

        private void OnDestroy()
        {
            IsActive = false;

            if (rig != null)
            {
                rig.IgnorePointerLook = false;
            }
        }

        private void Update()
        {
            if (group == null)
            {
                return;
            }

            // Out of the way during a vision and once the level has ended. A
            // CanvasGroup rather than deactivating, so a thumb still down on a
            // button gets its release and the button does not stick pressed.
            bool hide = (director != null && director.IsPlaying) ||
                        (clueLog != null && clueLog.IsFinaleUnlocked);

            group.alpha = hide ? 0f : 1f;
            group.blocksRaycasts = !hide;
            group.interactable = !hide;
        }

        private void BuildOverlay()
        {
            GameObject canvasGo = new GameObject("TouchControlsCanvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            canvasGo.transform.SetParent(transform, false);

            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            // Lay out against 1920x1080 and scale, so the buttons are the same
            // size relative to the screen on every phone.
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            group = canvasGo.GetComponent<CanvasGroup>();

            // Behind everything else, so any touch that misses a control looks around.
            GameObject look = NewUi("LookZone", canvasGo.transform);
            Stretch((RectTransform)look.transform);
            Image lookImage = look.AddComponent<Image>();
            lookImage.color = new Color(0f, 0f, 0f, 0f);
            TouchLookZone zone = look.AddComponent<TouchLookZone>();
            zone.rig = rig;

            BuildJoystick(canvasGo.transform);

            BuildButton(canvasGo.transform, "SNIFF", "<Gamepad>/buttonNorth", Red,
                        new Vector2(1f, 0f), new Vector2(-250f, 270f), 240f, 44);
            BuildButton(canvasGo.transform, "JUMP", "<Gamepad>/buttonSouth", Black,
                        new Vector2(1f, 0f), new Vector2(-520f, 150f), 160f, 32);
            BuildButton(canvasGo.transform, "SCENTS", "<Gamepad>/select", Black,
                        new Vector2(1f, 1f), new Vector2(-150f, -150f), 150f, 28);
        }

        private void BuildJoystick(Transform parent)
        {
            GameObject pad = NewUi("JoystickBase", parent);
            RectTransform padRect = (RectTransform)pad.transform;
            Anchor(padRect, new Vector2(0f, 0f), new Vector2(280f, 280f), 320f);
            Image padImage = pad.AddComponent<Image>();
            padImage.sprite = circle;
            padImage.color = Black;
            padImage.raycastTarget = false;

            // The knob is what the thumb drags. The control path has to be set
            // before the component enables, so it is added to an inactive object.
            GameObject knob = NewUi("JoystickKnob", pad.transform);
            knob.SetActive(false);
            RectTransform knobRect = (RectTransform)knob.transform;
            knobRect.anchorMin = knobRect.anchorMax = new Vector2(0.5f, 0.5f);
            knobRect.sizeDelta = new Vector2(160f, 160f);
            knobRect.anchoredPosition = Vector2.zero;
            Image knobImage = knob.AddComponent<Image>();
            knobImage.sprite = circle;
            knobImage.color = Knob;

            OnScreenStick stick = knob.AddComponent<OnScreenStick>();
            stick.controlPath = "<Gamepad>/leftStick";
            stick.movementRange = 120f;
            knob.SetActive(true);
        }

        private void BuildButton(Transform parent, string label, string controlPath, Color colour,
                                 Vector2 anchor, Vector2 position, float size, int fontSize)
        {
            GameObject button = NewUi(label + "Button", parent);
            button.SetActive(false);

            RectTransform rect = (RectTransform)button.transform;
            Anchor(rect, anchor, position, size);

            Image image = button.AddComponent<Image>();
            image.sprite = circle;
            image.color = colour;

            OnScreenButton onScreen = button.AddComponent<OnScreenButton>();
            onScreen.controlPath = controlPath;

            GameObject text = NewUi("Label", button.transform);
            Stretch((RectTransform)text.transform);
            Text t = text.AddComponent<Text>();
            t.text = label;
            t.font = font;
            t.fontSize = fontSize;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.raycastTarget = false;

            button.SetActive(true);
        }

        private static GameObject NewUi(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Anchor(RectTransform rect, Vector2 anchor, Vector2 position, float size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(size, size);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            DontDestroyOnLoad(es);
        }

        /// <summary>
        /// A soft-edged white circle, generated rather than loaded, because Unity's
        /// built-in UI circle is an editor resource and is missing from builds.
        /// </summary>
        private static Sprite MakeCircleSprite()
        {
            const int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "TouchCircle",
            };

            float radius = size * 0.5f;
            Color32[] pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius));
                    byte a = (byte)(Mathf.Clamp01(radius - d) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
