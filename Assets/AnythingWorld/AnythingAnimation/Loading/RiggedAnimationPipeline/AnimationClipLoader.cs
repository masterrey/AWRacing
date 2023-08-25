
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using AnythingWorld.Utilities.Data;
using System.Collections.Generic;

namespace AnythingWorld.Animation
{
    using Animation = UnityEngine.Animation;
    public class AnimationClipLoader
    {
        /// <summary>
        /// Load animations onto model.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sourceScript"></param>
        public static void Load(ModelData data)
        {
            LoadAnimClipLegacy(data.loadedData.gltf.animationClips, data.rig, data);
        }

        /// <summary>
        /// Load animation clips into a legacy Animation component. 
        /// </summary>
        /// <param name="animationClips"></param>
        /// <param name="target"></param>
        public static void LoadAnimClipLegacy(Dictionary<string,AnimationClip> animationClips, GameObject target, ModelData data)
        {
            //Load legacy animations
            var anim = target.AddComponent<Animation>();

            LegacyAnimationController legacyAnimationController = null;
            switch (data.defaultBehaviourType)
            {
                case Utilities.DefaultBehaviourType.Static:
                    break;
                case Utilities.DefaultBehaviourType.Shader:
                    break;
                case Utilities.DefaultBehaviourType.WalkingAnimal:
                    legacyAnimationController = target.AddComponent<RunWalkIdleController>();
                    break;
                case Utilities.DefaultBehaviourType.WheeledVehicle:
                    break;
                case Utilities.DefaultBehaviourType.FlyingVehicle:
                    break;
                case Utilities.DefaultBehaviourType.FlyingAnimal:
                    legacyAnimationController = target.AddComponent<FlyingAnimationController>();
                    break;
                case Utilities.DefaultBehaviourType.SwimmingAnimal:
                    break;
            }
            if (legacyAnimationController != null)
            {
                foreach (var kvp in animationClips)
                {

                    var clipName = kvp.Key;
                    var clip = kvp.Value;
                    clip.legacy = true;
                    clip.wrapMode = WrapMode.Loop;
                    anim.AddClip(clip, clipName);
                    legacyAnimationController.loadedAnimations.Add(clipName);
                }

            }
           


        }

        #region Modern Animation System
#if UNITY_EDITOR
        /// <summary>
        /// Serialize animation clip to asset database, 
        /// </summary>
        /// <param name="animations"></param>
        /// <param name="target"></param>
        public static void LoadAnimClip(AnimationClip[] animations, GameObject target)
        {
           
            //todo, loop through clips
            var animationClip = animations[0];

            SerializeAnimationClip(animationClip);

            SetLoopProperty(animationClip, true);

            if (target == null) return;
            if (animations.Length == 0) return;

            //Add animator to model
            if (!target.TryGetComponent<Animator>(out var animator))
            {
                animator = target.AddComponent<Animator>();
            }

            //Create override animator controller and add to animator component
            var animatorOverrideController = CreateAnimatorOverrideController(animator);

            SetAnimationClipOverride(animatorOverrideController, animationClip);
        }

        private static void SerializeAnimationClip(AnimationClip animationClip)
        {
            //If asset database contains animation clip, replace.
            if (UnityEditor.AssetDatabase.Contains(animationClip))
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(animationClip);
                UnityEditor.AssetDatabase.DeleteAsset(path);
            }
            //Sub this for the clip name when finished.
            UnityEditor.AssetDatabase.CreateAsset(animationClip,
                $"Assets/walk.anim");
            UnityEditor.AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Get clips overrides from the controller and apply to the new animation clip. 
        /// </summary>
        /// <param name="animatorOverrideController"></param>
        /// <param name="animationClip"></param>
        private static void SetAnimationClipOverride(AnimatorOverrideController animatorOverrideController, AnimationClip animationClip)
        {
            var clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
            animatorOverrideController.GetOverrides(clipOverrides);

            clipOverrides["DefaultBase"] = animationClip;
            animatorOverrideController.ApplyOverrides(clipOverrides);
        }
        /// <summary>
        /// Create override animation controller from template controller.
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private static AnimatorOverrideController CreateAnimatorOverrideController(Animator animator)
        {
            var animatorOverrideController =
                new AnimatorOverrideController(Resources.Load("BaseController") as RuntimeAnimatorController)
                {
                    name = "Override Controller"
                };
            animator.runtimeAnimatorController = animatorOverrideController;
            return animatorOverrideController;
        }
        /// <summary>
        /// Set loop properties from animation clip properties.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="loopTime"></param>
        private static void SetLoopProperty(AnimationClip clip, bool loopTime)
        {
            if (clip == null) return;
            var serializedClip = new SerializedObject(clip);
            var clipSettings = new AnimationClipSettings(serializedClip.FindProperty("m_AnimationClipSettings"))
            {
                loopTime = loopTime
            };

            serializedClip.ApplyModifiedProperties();
        }
#endif
        #endregion
    }
}
