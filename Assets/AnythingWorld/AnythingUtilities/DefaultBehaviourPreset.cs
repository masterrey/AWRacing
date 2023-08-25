using System.Collections.Generic;
using UnityEngine;

namespace AnythingWorld.Utilities
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "DefaultBehaviours", menuName = "ScriptableObjects/DefaultBehaviour", order = 1)]
    public class DefaultBehaviourPreset : ScriptableObject
    {
        public List<BehaviourRule> behaviourRules = new List<BehaviourRule>();
    }
    [System.Serializable]
    public class BehaviourRule
    {
        [SerializeField]
        public DefaultBehaviourType behaviourType;
        [SerializeField]
        public string scriptName;
        public BehaviourRule(DefaultBehaviourType _behaviourType, string _scriptName)
        {
            behaviourType = _behaviourType;
            scriptName = _scriptName;
        }
    }
}
