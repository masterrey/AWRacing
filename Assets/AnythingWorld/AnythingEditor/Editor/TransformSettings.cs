using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AnythingWorld.Editor
{
    public class TransformSettings : ScriptableObject
    {
        public static TransformSettings Instance
        {
            get
            {
#if UNITY_EDITOR
                var instance = AssetDatabase.LoadAssetAtPath<TransformSettings>("Assets/AnythingWorld/AnythingEditor/Editor/Settings/TransformSettings.asset");
                //var instance = Resources.Load<TransformSettings>("Settings/TransformSettings");
                if (instance == null)
                {
                    Debug.Log("Instance is null, making new TransformSettings file");

                    var asset = CreateInstance<TransformSettings>();
                    AssetDatabase.CreateAsset(asset, "Assets/AnythingWorld/AnythingEditor/Editor/Settings/TransformSettings.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    return asset;
#endif
                }
                return instance;
            }
        }

        [SerializeField] private bool animateModel = true;
        [SerializeField] private bool placeOnGround = true;

        [SerializeField] private bool placeOnGrid = true;
        [SerializeField] private bool serializeAsset = false;
        [SerializeField] private bool addCollider = false;
        [SerializeField] private bool addRigidbody = false;
        [SerializeField] private bool showGridHandles = false;

        [SerializeField] private bool customParentTransformEnabled = false;
        [SerializeField] private bool customPositionFieldEnabled = false;
        [SerializeField] private bool customRotationFieldEnabled = false;
        [SerializeField] private bool customScaleMultiplierEnabled = false;

        [SerializeField] private bool defaultRiggedBehaviourEnabled = true;
        [SerializeField] private bool defaultVehicleBehaviourEnabled = true;
        [SerializeField] private bool defaultFlyingVehicleBehaviourEnabled = true;
        [SerializeField] private bool defaultFlyingAnimalBehaviourEnabled = true;
        [SerializeField] private bool defaultShaderBehaviourEnabled = false;
        [SerializeField] private bool defaultStaticBehaviourEnabled = false;
        [SerializeField] private bool defaultSwimmingAnimalBehaviourEnabled = false;

        [SerializeField] private Transform objectParent;
        [SerializeField] private Vector3 objectPosition;
        [SerializeField] private Vector3 objectRotation;
        [SerializeField] private float objectScaleMultiplier = 1f;
        [SerializeField] private MonoScript defaultRiggedScript;
        [SerializeField] private MonoScript defaultVehicleScript;
        [SerializeField] private MonoScript defaultFlyingVehicleScript;
        [SerializeField] private MonoScript defaultFlyingAnimalScript;
        [SerializeField] private MonoScript defaultStaticScript;
        [SerializeField] private MonoScript defaultShaderScript;
        [SerializeField] private MonoScript defaultSwimmingAnimalScript;

        public static bool AnimateModel { get => Instance.animateModel; set => Instance.animateModel = value; }
        public static bool PlaceOnGround { get => Instance.placeOnGround; set => Instance.placeOnGround = value; }
        public static bool PlaceOnGrid { get => Instance.placeOnGrid; set => Instance.placeOnGrid = value; }
        public static bool SerializeAsset { get => Instance.serializeAsset; set => Instance.serializeAsset = value; }
        public static bool AddCollider { get => Instance.addCollider; set => Instance.addCollider = value; }
        public static bool AddRigidbody { get => Instance.addRigidbody; set => Instance.addRigidbody = value; }
        public static bool ShowGridHandles { get => Instance.showGridHandles; set => Instance.showGridHandles = value; }

        public static bool ParentFieldEnabled { get => Instance.customParentTransformEnabled; set => Instance.customParentTransformEnabled = value; }
        public static bool PositionFieldEnabled { get => Instance.customPositionFieldEnabled; set => Instance.customPositionFieldEnabled = value; }
        public static bool RotationFieldEnabled { get => Instance.customRotationFieldEnabled; set => Instance.customRotationFieldEnabled = value; }
        public static bool ScaleFieldEnabled { get => Instance.customScaleMultiplierEnabled; set => Instance.customScaleMultiplierEnabled = value; }
        
        public static bool RiggedBehaviourFieldEnabled { get => Instance.defaultRiggedBehaviourEnabled; set => Instance.defaultRiggedBehaviourEnabled = value; }
        public static bool VehicleBehaviourFieldEnabled { get => Instance.defaultVehicleBehaviourEnabled; set => Instance.defaultVehicleBehaviourEnabled = value; }
        public static bool FlyingVehicleBehaviourFieldEnabled { get => Instance.defaultFlyingVehicleBehaviourEnabled; set => Instance.defaultFlyingVehicleBehaviourEnabled = value; }
        public static bool ShaderBehaviourFieldEnabled { get => Instance.defaultShaderBehaviourEnabled; set => Instance.defaultShaderBehaviourEnabled = value; }
        public static bool StaticBehaviourFieldEnabled { get => Instance.defaultStaticBehaviourEnabled; set => Instance.defaultStaticBehaviourEnabled = value; }
        public static bool FlyingAnimalBehaviourFieldEnabled { get => Instance.defaultFlyingAnimalBehaviourEnabled; set => Instance.defaultFlyingAnimalBehaviourEnabled = value; }
        public static bool SwimmingAnimalBehaviourFieldEnabled { get => Instance.defaultSwimmingAnimalBehaviourEnabled; set => Instance.defaultSwimmingAnimalBehaviourEnabled = value; }

        public static Transform ParentField { get => Instance.objectParent; set => Instance.objectParent = value; }
        public static Vector3 PositionField { get => Instance.objectPosition; set => Instance.objectPosition = value; }
        public static Vector3 RotationField { get => Instance.objectRotation; set => Instance.objectRotation = value; }
        public static float ScaleField { get => Instance.objectScaleMultiplier; set => Instance.objectScaleMultiplier = value; }

        public static MonoScript RiggedAnimalScript { get => Instance.defaultRiggedScript; set => Instance.defaultRiggedScript = value; }
        public static MonoScript GroundVehicleScript { get => Instance.defaultVehicleScript; set => Instance.defaultVehicleScript = value; }
        public static MonoScript FlyingVehicleScript { get => Instance.defaultFlyingVehicleScript; set => Instance.defaultFlyingVehicleScript = value; }
        public static MonoScript ShaderBehaviourScript { get => Instance.defaultShaderScript; set => Instance.defaultShaderScript = value; }
        public static MonoScript StaticBehaviourScript { get => Instance.defaultStaticScript; set => Instance.defaultStaticScript = value; }
        public static MonoScript FlyingAnimalScript { get => Instance.defaultFlyingAnimalScript; set => Instance.defaultFlyingAnimalScript = value; }
        public static MonoScript SwimmingAnimalScript { get => Instance.defaultSwimmingAnimalScript; set => Instance.defaultSwimmingAnimalScript = value; }

        public static void ClearSettings()
        {
            ParentField = null;
            PositionField = Vector3.zero;
            RotationField = Vector3.zero;
            ScaleField = 1f;
            
            ParentFieldEnabled = false;
            PositionFieldEnabled = false;
            RotationFieldEnabled = false;
            ScaleFieldEnabled = false;

            AnimateModel = true;
            PlaceOnGround = true;
            PlaceOnGrid = true;
            SerializeAsset = false;
            AddCollider = false;
            AddRigidbody = false;
            ShowGridHandles = false;

            GroundVehicleScript = null;
            FlyingVehicleScript = null;
            RiggedAnimalScript = null;
            StaticBehaviourScript = null;
            ShaderBehaviourScript = null;


            VehicleBehaviourFieldEnabled = false;
            FlyingVehicleBehaviourFieldEnabled = false;
            RiggedBehaviourFieldEnabled = false;
            StaticBehaviourFieldEnabled = false;
            ShaderBehaviourFieldEnabled = false;
            FlyingAnimalBehaviourFieldEnabled = false;

        }
    }
}
