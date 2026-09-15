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
    /// Drops into VisionDirector's Vision Player slot in place of
    /// PlaceholderVisionPlayer. The director is untouched -- that was the point
    /// of building VisionPlayer as a slot back in Stage 2.
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
        [Tooltip("If a video will not prepare in this long, give up and move on. " +
                 "Without it a missing codec would lock the player out of the game.")]
        [SerializeField] private float prepareTimeout = 5f;

        [Tooltip("Extra seconds allowed past the clip's own length before we call it stuck.")]
        [SerializeField] private float playbackGrace = 2f;

        private enum State { Idle, Preparing, Playing, Placeholder, Done }

        private State state = State.Idle;
        private float stateEnteredAt;
        private float deadline;
        private string currentVisionId;
        private RenderTexture target;

        public override bool IsFinished
        {
            get
            {
                switch (state)
                {
                    case State.Placeholder:
                        return Time.time >= deadline;

                    case State.Preparing:
                        if (Time.time - stateEnteredAt > prepareTimeout)
                        {
                            Debug.LogError($"VideoVisionPlayer: '{currentVisionId}' never prepared. " +
                                           "Check the clip imports and the platform codec.", this);
                            SetState(State.Done);
                            return true;
                        }
                        return false;

                    case State.Playing:
                        if (Time.time > deadline)
                        {
                            Debug.LogWarning($"VideoVisionPlayer: '{currentVisionId}' overran its " +
                                             "own length. Ending it rather than hanging.", this);
                            SetState(State.Done);
                            return true;
                        }
                        return false;

                    default:
                        return true;
                }
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
            // "played" to an empty screen. A RawImage on the Canvas also sits below
            // the fade overlay by construction, so the crossfade still covers it.
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

        public override void Begin(ScentMarker marker)
        {
            ClueData clue = marker != null ? marker.Clue : null;
            currentVisionId = clue != null ? clue.VisionId : string.Empty;

            VideoClip clip = FindClip(currentVisionId);

            if (clip == null || !EnsureSurface())
            {
                BeginPlaceholder(marker, clue);
                return;
            }

            // Solid black until the first frame exists, so the fade-in from black
            // reveals black rather than a flash of the 3D scene behind the video.
            surface.texture = null;
            surface.color = Color.black;
            surface.gameObject.SetActive(true);

            Debug.Log($"VideoVisionPlayer: preparing '{currentVisionId}' " +
                      $"({clip.width}x{clip.height}, {clip.length:0.0}s)", this);

            videoPlayer.clip = clip;
            SetState(State.Preparing);
            videoPlayer.Prepare();
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
            SetState(State.Idle);
        }

        private void BeginPlaceholder(ScentMarker marker, ClueData clue)
        {
            string name = clue != null ? clue.DisplayName : "An unfamiliar scent";
            float length = marker != null ? marker.VisionDuration : 3f;

            hud.ShowVision($"2D VISION — NOT DRAWN YET\n\n{name}\n({currentVisionId})");

            deadline = Time.time + length;
            SetState(State.Placeholder);
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
        /// FadeOverlay in the Canvas, so the black fade always draws over the video.
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
            if (state != State.Preparing)
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

            source.targetTexture = target;
            surface.texture = target;
            surface.color = Color.white;

            // The clip's own length plus a little, so a stalled decode ends the
            // vision instead of leaving the player frozen with no way out.
            deadline = Time.time + (float)source.length + playbackGrace;
            SetState(State.Playing);
            source.Play();

            Debug.Log($"VideoVisionPlayer: playing '{currentVisionId}'", this);
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
            SetState(State.Done);
        }

        private void SetState(State next)
        {
            state = next;
            stateEnteredAt = Time.time;
        }
    }
}
