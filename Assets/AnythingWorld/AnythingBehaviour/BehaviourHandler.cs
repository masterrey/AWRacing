using AnythingWorld.Utilities.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnythingWorld.Utilities;
using System.Linq;

namespace AnythingWorld.Behaviour
{
    public static class BehaviourHandler
    {
        public static void AddBehaviours(ModelData data)
        {
            if (data.parameters.qualifiedBehaviours != null)
            {
                TrySetBehaviour(data, data.parameters.qualifiedBehaviours);
            }


            if (data.parameters.setDefaultBehaviourPreset)
            {
                if (data.parameters.defaultBehaviourPreset != null)
                {
                    TrySetBehaviour(data, data.parameters.defaultBehaviourPreset);
                }
                else
                {
                    var firstInstance = Resources.LoadAll<DefaultBehaviourPreset>("").FirstOrDefault() as DefaultBehaviourPreset;
                    if (firstInstance != null)
                    {
                        TrySetBehaviour(data, firstInstance as DefaultBehaviourPreset);
                    }
                    else
                    {
                        Debug.Log("Couldn't find DefaultBehaviourPreset in Resources to apply to model (Do you need to create a preset in resources?)");
                    }
                }
            }

            if (data.parameters.behaviours != null)
            {
                foreach (var behaviour in data.parameters.behaviours)
                {
                    data.model.AddComponent(behaviour);
                }
            }

            data.actions.postProcessingDelegate?.Invoke(data);
        }

        private static void TrySetBehaviour(ModelData data, Dictionary<DefaultBehaviourType, System.Type> dict)
        {
            if (dict.TryGetValue(data.defaultBehaviourType, out var scriptType))
            { 
                data.model.AddComponent(scriptType);
            }
        }
        private static void TrySetBehaviour(ModelData data, DefaultBehaviourPreset preset)
        {
            foreach(var rule in preset.behaviourRules)
            {
                if (rule.behaviourType == data.defaultBehaviourType)
                {
                    data.model.AddComponent(System.Type.GetType(rule.scriptName));
                }
            }
        }
    }





}
