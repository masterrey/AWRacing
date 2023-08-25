using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace AnythingWorld.Utilities
{
    public static class DefaultBehavioursUtility
    {
        /// <summary>
        /// Create an unserialized instance of behaviour preset (runtime only)
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static DefaultBehaviourPreset CreateNewTemporaryInstance(Dictionary<DefaultBehaviourType, Type> dictionary)
        {
            var asset = ScriptableObject.CreateInstance<DefaultBehaviourPreset>();
            foreach (var tuple in dictionary)
            {
                asset.behaviourRules.Add(new BehaviourRule(tuple.Key, tuple.Value.AssemblyQualifiedName.ToString()));
            }
            return asset;
        }
#if UNITY_EDITOR
        /// <summary>
        /// Creates a scriptable instance serialized in the asset database.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static DefaultBehaviourPreset CreateSerializedInstance(Dictionary<DefaultBehaviourType, Type> dictionary)
        {
            var asset = CreateNewInstance();
            foreach (var tuple in dictionary)
            {
                asset.behaviourRules.Add(new BehaviourRule(tuple.Key, tuple.Value.AssemblyQualifiedName.ToString()));
            }
            return asset;

        }

        public static DefaultBehaviourPreset CreateNewInstance()
        {
            var asset = ScriptableObject.CreateInstance<DefaultBehaviourPreset>();
            var uniquePath = AssetDatabase.GenerateUniqueAssetPath("Assets/AnythingWorld/Resources/Settings/RuntimeDefaultBehaviours.asset");
            AssetDatabase.CreateAsset(asset, uniquePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Saved preset to {uniquePath}");
            return asset;
        }
#endif
    }


}
