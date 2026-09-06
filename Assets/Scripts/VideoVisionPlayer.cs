using System;
using System.Collections.Generic;
using UnityEngine;
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

            // Render straight onto the camera's near plane: no RenderTexture asset
            // to create and wire, and the Canvas -- including the fade overlay --
            // still draws on top, which is what the crossfade depends on.
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

            videoPlayer.prepareCompleted += OnPrepared;
            videoPlayer.loopPointReached += OnReachedEnd;
            videoPlayer.errorReceived += OnError;
        }

        private void OnDestroy()
        {
            if (videoPlayer == null)
            {
                return;
            }

            videoPlayer.prepareCompleted -= OnPrepared;
            videoPlayer.loopPointReached -= OnReachedEnd;
            videoPlayer.errorReceived -= OnError;
        }

        public override void Begin(ScentMarker marker)
        {
            ClueData clue = marker != null ? marker.Clue : null;
            currentVisionId = clue != null ? clue.VisionId : string.Empty;

            VideoClip clip = FindClip(currentVisionId);

            if (clip == null)
            {
                BeginPlaceholder(marker, clue);
                return;
            }

            videoPlayer.targetCamera = ResolveCamera();

            if (videoPlayer.targetCamera == null)
            {
                Debug.LogWarning("VideoVisionPlayer: no camera to render onto, using the panel.", this);
                BeginPlaceholder(marker, clue);
                return;
            }

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

        private Camera ResolveCamera()
        {
            return videoPlayer.targetCamera != null ? videoPlayer.targetCamera : Camera.main;
        }

        private void OnPrepared(VideoPlayer source)
        {
            if (state != State.Preparing)
            {
                return;
            }

            // The clip's own length plus a little, so a stalled decode ends the
            // vision instead of leaving the player frozen with no way out.
            deadline = Time.time + (float)source.length + playbackGrace;
            SetState(State.Playing);
            source.Play();
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