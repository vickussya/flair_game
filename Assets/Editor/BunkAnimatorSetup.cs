using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Flair.EditorTools
{
    /// <summary>
    /// Builds Bunk's Animator Controller from the three Mixamo clips and wires it to
    /// the model on the Player (task 46): idle, walk, and the sniff as the action.
    ///
    /// The controller only references the animation files, never the placeholder
    /// body, so it can be committed on its own and will drive the Bunk we build
    /// ourselves later without changes.
    ///
    /// Safe to run again. The controller is rebuilt in place rather than deleted and
    /// recreated, so its GUID -- and every reference to it -- survives.
    /// </summary>
    public static class BunkAnimatorSetup
    {
        private const string AnimationsFolder = "Assets/Characters/Bunk/Animations";
        private const string ControllerPath = "Assets/Characters/Bunk/Bunk.controller";
        private const string ModelPath = "Assets/Characters/Bunk/Bunk.fbx";

        [MenuItem("FLAIR/Character/Build Bunk Animator")]
        public static void Build()
        {
            AnimationClip idle = FindClip("idle");
            AnimationClip walk = FindClip("walk");
            AnimationClip sniff = FindClip("sniff");

            if (idle == null || walk == null || sniff == null)
            {
                Debug.LogError("[BunkAnimatorSetup] Missing a clip in " + AnimationsFolder +
                               $". Found idle={idle != null}, walk={walk != null}, sniff={sniff != null}.");
                return;
            }

            if (!idle.isHumanMotion || !walk.isHumanMotion || !sniff.isHumanMotion)
            {
                Debug.LogWarning("[BunkAnimatorSetup] The clips did not import as Humanoid. " +
                                 "Right-click Assets/Characters > Reimport, then run this again.");
            }

            AnimatorController controller = BuildController(idle, walk, sniff);
            WireScene(controller, sniff);

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log($"[BunkAnimatorSetup] Done. Idle, Walk, Sniff ({sniff.length:0.00}s). " +
                      "Save the scene with Ctrl+S.");
        }

        private static AnimationClip FindClip(string keyword)
        {
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { AnimationsFolder });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.ToLowerInvariant().Contains(keyword))
                {
                    continue;
                }

                AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<AnimationClip>()
                    .FirstOrDefault(c => !c.name.StartsWith("__preview__"));

                if (clip != null)
                {
                    return clip;
                }
            }

            return null;
        }

        private static AnimatorController BuildController(AnimationClip idle, AnimationClip walk,
                                                          AnimationClip sniff)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            }

            // Clear parameters and states so a second run rebuilds cleanly instead
            // of stacking duplicates.
            for (int i = controller.parameters.Length - 1; i >= 0; i--)
            {
                controller.RemoveParameter(i);
            }

            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Sniff", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;

            foreach (ChildAnimatorState child in sm.states.ToArray())
            {
                sm.RemoveState(child.state);
            }

            foreach (AnimatorStateTransition t in sm.anyStateTransitions.ToArray())
            {
                sm.RemoveAnyStateTransition(t);
            }

            AnimatorState idleState = sm.AddState("Idle");
            idleState.motion = idle;

            AnimatorState walkState = sm.AddState("Walk");
            walkState.motion = walk;

            AnimatorState sniffState = sm.AddState("Sniff");
            sniffState.motion = sniff;

            sm.defaultState = idleState;

            // Foot IK pins the feet to where the clip put them after retargeting.
            // Without it a Mixamo clip on a different body can bend a knee the
            // wrong way or let a foot drift -- often on one leg only. It lives on
            // each state, which is why it is so hard to find in the Inspector.
            idleState.iKOnFeet = true;
            walkState.iKOnFeet = true;
            sniffState.iKOnFeet = true;

            AnimatorStateTransition toWalk = idleState.AddTransition(walkState);
            toWalk.hasExitTime = false;
            toWalk.duration = 0.15f;
            toWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

            AnimatorStateTransition toIdle = walkState.AddTransition(idleState);
            toIdle.hasExitTime = false;
            toIdle.duration = 0.2f;
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

            // From anywhere, so a vision that starts mid-stride still gets its sniff.
            AnimatorStateTransition toSniff = sm.AddAnyStateTransition(sniffState);
            toSniff.hasExitTime = false;
            toSniff.duration = 0.15f;
            toSniff.canTransitionToSelf = false;
            toSniff.AddCondition(AnimatorConditionMode.If, 0f, "Sniff");

            AnimatorStateTransition sniffDone = sniffState.AddTransition(idleState);
            sniffDone.hasExitTime = true;
            sniffDone.exitTime = 0.95f;
            sniffDone.duration = 0.2f;

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void WireScene(AnimatorController controller, AnimationClip sniff)
        {
            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogWarning("[BunkAnimatorSetup] No Player in the scene. Controller built, nothing wired.");
                return;
            }

            if (player.GetComponent<CharacterAnimator>() == null)
            {
                Undo.AddComponent<CharacterAnimator>(player);
            }

            Transform model = player.transform.Find("BunkModel");
            if (model == null)
            {
                Debug.LogWarning("[BunkAnimatorSetup] No BunkModel on the Player. Run FLAIR > Character > " +
                                 "Set Bunk As Player Model first, then run this again.");
                return;
            }

            Animator animator = model.GetComponent<Animator>();
            if (animator == null)
            {
                animator = Undo.AddComponent<Animator>(model.gameObject);
            }

            Undo.RecordObject(animator, "Wire Bunk animator");
            animator.runtimeAnimatorController = controller;
            // PlayerController moves him. Root motion on top would move him twice.
            animator.applyRootMotion = false;

            Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<Avatar>().FirstOrDefault();
            if (avatar != null)
            {
                animator.avatar = avatar;
                if (!avatar.isHuman)
                {
                    Debug.LogWarning("[BunkAnimatorSetup] Bunk.fbx imported as Generic, so the Humanoid clips " +
                                     "will not fit. Right-click Assets/Characters > Reimport, then run this again.");
                }
            }

            // The sniff is the breath the vision transition waits on, so time the
            // hold to the real clip. The camera move already covers the first part.
            VisionDirector director = Object.FindFirstObjectByType<VisionDirector>();
            if (director != null)
            {
                SerializedObject so = new SerializedObject(director);
                float move = so.FindProperty("moveToObserveDuration").floatValue;
                so.FindProperty("breathHoldDuration").floatValue = Mathf.Max(0.5f, sniff.length - move);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
