using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnythingWorld.Utilities.Data
{
    [Serializable]
    public class RequestParamObject
    {
        public bool clampDbScale = false;
        public bool addCollider = false;
        public bool addRigidbody = false;
        public bool serializeAsset = false;
        public bool placeOnGrid = false;
        public bool placeOnGround = false;
        public bool animateModel = true;
        public Vector3Param position = new Vector3Param();
        public Vector3Param scale = new Vector3Param();
        public Vector3Param clampDbScaleUpperBounds = new Vector3Param();
        public Vector3Param clampDbScaleLowerBounds = new Vector3Param();
        public Quaternion rotation = Quaternion.identity;
        public float scaleMultiplier = 1;
        public ScaleType scaleType;
        public TransformSpace transformSpace;
        public Transform parentTransform;
        public Action onSuccessAction;
        public Action onFailAction;
        public Action<CallbackInfo> onSuccessActionCallback;
        public Action<CallbackInfo> onFailActionCallback;
        public Type[] behaviours;
        public Dictionary<DefaultBehaviourType, Type> qualifiedBehaviours;
        public bool setDefaultBehaviourPreset = true;
        public DefaultBehaviourPreset defaultBehaviourPreset = null;

    }
}
