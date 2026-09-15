using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Flair
{
    /// <summary>
    /// Plays a real 2D vision as a rendered video, per the 6.5 decision of
    /// 3 Sep 2026: PNG sequences are the masters, one rendered video per vision
    /// is what the game plays.
    ///
    /// Loads during Bunk's sniff (Prepare), starts playing when the director begins
    /// the dissolve (Begin), and is faded over the 3D scene with SetOpacity -- so
    /// the world blends into the vision rather than cutting through black.
    ///
    /// A clue with no video yet falls back to the placeholder panel rather than
    /// breaking, because the visions arrive one at a time over months and the
    /// game has to stay playable in between.
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoVisionPlayer : VisionPlayer
    {
        [Serializable]
        private class VisionEntry
        {
            [Tooltip("Must match ClueData.visionId exactly.")]
            public string visionId;

            [Tooltip("Leave empty and this clue falls back to the placeholder panel.")]
            public VideoClip clip;
        }

        [Header("Visions")]
        [Tooltip("Maps a clue's visionId to its rendered video. Clues missing from " +
                 "this list still play -- they just show the placeholder panel.")]
        [SerializeField] private List<VisionEntry> visions = new List<VisionEntry>();

        [Header("References (leave empty to use this same object)")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private VisionHud hud;

        [Tooltip("Full-screen RawImage the video is drawn onto. Leave empty and one is " +
                 "created under the Canvas, just beneath the fade overlay.")]
        [SerializeField] private RawImage surface;

        [Header("Safety")]
        [Tooltip("If a video will not prepare in this long, fall back to the panel. " +
                 "Without it a missing codec would stall the vision indefinitely.")]
        [SerializeField] private float prepareTimeout = 5f;

        [Tooltip("Extra seconds allowed past the clip's own length before we call it stuck.")]
        [SerializeField] private float playbackGrace = 2f;

        private enum Mode { None, Video, Panel }
        private enum State { Idle, Preparing, Ready, Playing, Done }

        private Mode mode = Mode.None;
        private State state = State.Idle;
        private float stateEnteredAt;
        private float deadline;
        private string currentVisionId;
        private ScentMarker currentMarker;
        private RenderTexture target;

        public override bool IsReady
        {
            get
            {
                if (mode == Mode.Panel)
                {
                    return true;
                }

                if (state == State.Preparing && Time.time - stateEnteredAt > prepareTimeout)
                {
                    Debug.LogError($"VideoVisionPlayer: '{currentVisionId}' never prepared, using the " +
                                   "panel instead. Check the clip imports and the codec.", this);
                    SwitchToPanel();
                    return true;
                }

                return state == State.Ready;
            }
        }

        public override bool IsFinished
        {
            get
            {
                if (state == State.Done)
                {
                    return true;
                }

                if (state != State.Playing)
                {
                    return false;
                }

                if (Time.time > deadline)
                {
                    if (mode == Mode.Video)
                    {
                        Debug.LogWarning($"VideoVisionPlayer: '{currentVisionId}' overran its own " +
                                         "length. Ending it rather than hanging.", this);
                    }

                    SetState(State.Done);
                    return true;
                }

                return false;
            }
        }

        public override float SecondsRemaining
        {
            get
            {
                if (state != State.Playing)
                {
                    return 0f;
                }

                if (mode == Mode.Video && videoPlayer.isPlaying)
                {
                    return Mathf.Max(0f, (float)(videoPlayer.length - videoPlayer.time));
                }

                return Mathf.Max(0f, deadline - Time.time);
            }
        }

        private void Awake()
        {
            if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();
            if (hud == null) hud = GetComponent<VisionHud>();

            if (hud == null)
            {
                Debug.LogError("VideoVisionPlayer: no VisionHud found for the fallback panel.", this);
                enabled = false;
                return;
            }

            // Render into a texture shown on the Canvas. This replaced the camera
            // near plane mode, which under URP can simply never draw -- the vision
            // "played" to an empty screen.
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

            videoPlayer.prepareCompleted += OnPrepared;
            videoPlayer.loopPointReached += OnReachedEnd;
            videoPlayer.errorReceived += OnError;
        }

        private void OnDestroy()
        {
            if (videoPlayer != null)
            {
                videoPlayer.prepareCompleted -= OnPrepared;
                videoPlayer.loopPointReached -= OnReachedEnd;
                videoPlayer.errorReceived -= OnError;
            }

            if (target != null)
            {
                target.Release();
                Destroy(target);
            }
        }

        public override void Prepare(ScentMarker marker)
        {
            currentMarker = marker;
            ClueData clue = marker != null ? marker.Clue : null;
            currentVisionId = clue != null ? clue.VisionId : string.Empty;

            VideoClip clip = FindClip(currentVisionId);

            if (clip == null || !EnsureSurface())
            {
                SwitchToPanel();
                return;
            }

            mode = Mode.Video;

            // Present but invisible while it loads; the director fades it up.
            surface.texture = null;
            surface.color = new Color(1f, 1f, 1f, 0f);
            surface.gameObject.SetActive(true);

            Debug.Log($"VideoVisionPlayer: preparing '{currentVisionId}' " +
                      $"({clip.width}x{clip.height}, {clip.length:0.0}s)", this);

            videoPlayer.clip = clip;
            SetState(State.Preparing);
            videoPlayer.Prepare();
        }

        public override void Begin(ScentMarker marker)
        {
            if (mode == Mode.None)
            {
                // Called without Prepare -- do it now rather than show nothing.
                Prepare(marker);
            }

            if (mode == Mode.Panel)
            {
                ClueData clue = currentMarker != null ? currentMarker.Clue : null;
                string name = clue != null ? clue.DisplayName : "An unfamiliar scent";
                float length = currentMarker != null ? currentMarker.VisionDuration : 3f;

                hud.SetVisionOpacity(0f);
                hud.ShowVision($"2D VISION — NOT DRAWN YET\n\n{name}\n({currentVisionId})");

                deadline = Time.time + length;
                SetState(State.Playing);
                return;
            }

            // The clip's own length plus a little, so a stalled decode ends the
            // vision instead of leaving the player frozen with no way out.
            deadline = Time.time + (float)videoPlayer.clip.length + playbackGrace;
            SetState(State.Playing);
            videoPlayer.Play();

            Debug.Log($"VideoVisionPlayer: playing '{currentVisionId}'", this);
        }

        public override void SetOpacity(float opacity)
        {
            if (mode == Mode.Video && surface != null)
            {
                // Until the first frame exists the RawImage has no texture, and an
                // untextured RawImage draws solid white. Tint it black instead.
                surface.color = surface.texture != null
                    ? new Color(1f, 1f, 1f, opacity)
                    : new Color(0f, 0f, 0f, opacity);
            }
            else if (mode == Mode.Panel)
            {
                hud.SetVisionOpacity(opacity);
            }
        }

        public override void End()
        {
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }

            if (surface != null)
            {
                surface.texture = null;
                surface.gameObject.SetActive(false);
            }

            hud.HideVision();

            mode = Mode.None;
            currentMarker = null;
            SetState(State.Idle);
        }

        private void SwitchToPanel()
        {
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }

            if (surface != null)
            {
                surface.gameObject.SetActive(false);
            }

            mode = Mode.Panel;
            SetState(State.Ready);
        }

        private VideoClip FindClip(string visionId)
        {
            if (string.IsNullOrWhiteSpace(visionId))
            {
                return null;
            }

            for (int i = 0; i < visions.Count; i++)
            {
                if (visions[i] != null && visions[i].visionId == visionId)
                {
                    return visions[i].clip;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds or builds the full-screen RawImage. It is placed directly beneath
        /// FadeOverlay in the Canvas, so the level's black fades still draw over it.
        /// </summary>
        private bool EnsureSurface()
        {
            if (surface != null)
            {
                return true;
            }

            GameObject fade = GameObject.Find("FadeOverlay");
            if (fade == null || fade.transform.parent == null)
            {
                Debug.LogWarning("VideoVisionPlayer: no FadeOverlay under a Canvas to place " +
                                 "the video beneath. Showing the placeholder panel instead.", this);
                return false;
            }

            GameObject go = new GameObject("VisionVideoSurface", typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(fade.transform.parent, false);
            go.transform.SetSiblingIndex(fade.transform.GetSiblingIndex());

            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            surface = go.GetComponent<RawImage>();
            surface.raycastTarget = false;
            go.SetActive(false);
            return true;
        }

        private void OnPrepared(VideoPlayer source)
        {
            // Normally this lands while Preparing. If the dissolve outwaited a slow
            // load, Begin has already called Play and we are Playing -- still wire
            // the texture, or the video would play to nothing.
            if (state != State.Preparing && state != State.Playing)
            {
                return;
            }

            int w = (int)source.width;
            int h = (int)source.height;

            if (target == null || target.width != w || target.height != h)
            {
                if (target != null)
                {
                    target.Release();
                    Destroy(target);
                }

                target = new RenderTexture(w, h, 0) { name = "VisionVideo" };
                target.Create();
            }

            // A fresh RenderTexture holds whatever was in that memory. Clear it so
            // the first moment of the dissolve is black rather than noise.
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = previous;

            source.targetTexture = target;
            surface.texture = target;

            if (state == State.Preparing)
            {
                SetState(State.Ready);
            }
        }

        private void OnReachedEnd(VideoPlayer source)
        {
            if (state == State.Playing)
            {
                SetState(State.Done);
            }
        }

        private void OnError(VideoPlayer source, string message)
        {
            Debug.LogError($"VideoVisionPlayer: '{currentVisionId}' failed -- {message}", this);

            // Before it has been seen, the panel can still stand in. After, just end.
            if (state == State.Preparing)
            {
                SwitchToPanel();
            }
            else
            {
                SetState(State.Done);
            }
        }

        private void SetState(State next)
        {
            state = next;
            stateEnteredAt = Time.time;
        }
    }
}
