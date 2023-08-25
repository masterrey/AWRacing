using AnythingWorld.Utilities.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnythingWorld.Utilities;
using AnythingWorld.Animation.Vehicles;

namespace AnythingWorld.Animation
{
    public class AnimationFactory
    {
        /// <summary>
        /// Start process for loading animation components and controllers depending on the animation pipeline. 
        /// </summary>
        /// <param name="data"></param>
        public static void Load(ModelData data)
        {
            switch (data.animationPipeline)
            {
                case AnimationPipeline.Static:
                    StartStaticAnimationPipeline(data);
                    break;
                case AnimationPipeline.Rigged:
                    StartRiggedAnimationPipeline(data);
                    break;
                case AnimationPipeline.WheeledVehicle:
                    StartWheeledVehicleAnimation(data);
                    break;
                case AnimationPipeline.PropellorVehicle:
                    StartPropellorVehicleAnimation(data);
                    break;
                case AnimationPipeline.Shader:
                    StartShaderAnimationPipeline(data);
                    break;
            }
        }
        private static void StartStaticAnimationPipeline(ModelData data)
        {
            data.actions.addBehavioursDelegate?.Invoke(data);
        }
        private static void StartRiggedAnimationPipeline(ModelData data)
        {
            data.Debug($"StartRiggedAnimationPipeline called for {data.guid}");
            AnimationClipLoader.Load(data);
            data.actions.addBehavioursDelegate?.Invoke(data);
        }
        private static void StartShaderAnimationPipeline(ModelData data)
        {
            data.Debug($"StartShaderAnimationPipeline called.");
            AnimationShaderLoader.Load(data);
            data.actions.addBehavioursDelegate?.Invoke(data);
        }
        private static void StartWheeledVehicleAnimation(ModelData data)
        {
            data.Debug($"StartVehicleAnimationPipeline called for {data.guid}");
            WheeledVehicleAnimationLoader.Load(data);
            data.actions.addBehavioursDelegate?.Invoke(data);
        }
        private static void StartPropellorVehicleAnimation(ModelData data)
        {
            data.Debug($"StartVehicleAnimationPipeline called for {data.guid}");
            VehiclePropellorAnimationLoader.Load(data);
            data.actions.addBehavioursDelegate?.Invoke(data);
        }
    }
}
