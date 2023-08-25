using AnythingWorld.Utilities;
using UnityEditor;
using UnityEngine;

namespace AnythingWorld.Editor
{
    public class TransformSettingsEditor : AnythingEditor
    {
        protected bool showGridOptionsDrawer = false;
        protected bool showDefaultBehavioursDrawer = false;
        protected bool showTransformDrawer = false;
        protected bool showGeneralDrawer = false;
        protected bool showPhysicsDrawer = false;
        protected bool gridPlacementEnabled = false;
        protected bool transformSettingsActive = false;

        private bool parentEnabled = false;
        private bool positionEnabled = false;
        private bool rotationEnabled = false;
        private bool scaleEnabled = false;
        private bool riggedBehaviourEnabled = true;
        private bool vehicleBehaviourEnabled = true;
        private bool flyingVehicleBehaviourEnabled = true;
        private bool flyingAnimalBehaviourEnabled = true;
        private bool swimmingAnimalBehaviourEnabled = true;
        private bool shaderBehaviourEnabled = false;
        private bool staticBehaviourEnabled = false;

        private bool animateModel = true;
        private bool placeOnGround = true;
        private bool placeOnGrid = true;
        private bool serializeAsset = false;
        private bool addCollider = false;
        private bool addRigidbody = false;
        private bool showGridHandles = false;
        private bool debugOn = false;

        private static Transform objectParent;
        private Vector3 objectPosition;
        private Vector3 objectRotation;
        private float objectScale;
        private MonoScript defaultRiggedScript;
        private MonoScript defaultVehicleScript;
        private MonoScript defaultFlyingVehicleScript;

        private MonoScript defaultStaticScript;
        private MonoScript defaultShaderScript;
        private MonoScript defaultFlyingAnimalScript;
        private MonoScript defaultSwimmingAnimalScript;

        private readonly float MinSize = 240f;
        private float addedDrawerHeight;

        [MenuItem("Anything World/Transform Settings", false, 52)]
        public static void Initialize()
        {
            Resources.LoadAll<AnythingSettings>("Settings");
            Resources.LoadAll<TransformSettings>("Settings");

            AnythingCreatorEditor tabWindow;
            Vector2 windowSize;

            if (AnythingSettings.HasAPIKey)
            {
                windowSize = new Vector2(375, 240f);

                var browser = GetWindow(typeof(TransformSettingsEditor), false, "Transform Settings") as TransformSettingsEditor;
                browser.position = new Rect(EditorGUIUtility.GetMainWindowPosition().center - windowSize / 2, windowSize);
                browser.maxSize = new Vector2(browser.maxSize.x, EditorGUIUtility.GetMainWindowPosition().height * 0.6f);
                browser.Show();
                browser.Focus();

                EditorUtility.SetDirty(TransformSettings.Instance);
                browser.SetupVariables();
                EditorUtility.SetDirty(browser);
            }
            else
            {
                windowSize = new Vector2(450, 800);

                tabWindow = GetWindow<LogInEditor>("Log In | Sign Up", false, typeof(ModelBrowserEditor));
                tabWindow.position = new Rect(EditorGUIUtility.GetMainWindowPosition().center - windowSize / 2, windowSize);
            }
        }

        internal void SetupVariables()
        {
            parentEnabled = TransformSettings.ParentFieldEnabled;
            positionEnabled = TransformSettings.PositionFieldEnabled;
            rotationEnabled = TransformSettings.RotationFieldEnabled;
            scaleEnabled = TransformSettings.ScaleFieldEnabled;
            riggedBehaviourEnabled = TransformSettings.RiggedBehaviourFieldEnabled;
            vehicleBehaviourEnabled = TransformSettings.VehicleBehaviourFieldEnabled;
            flyingVehicleBehaviourEnabled = TransformSettings.FlyingVehicleBehaviourFieldEnabled;
            flyingAnimalBehaviourEnabled = TransformSettings.FlyingAnimalBehaviourFieldEnabled;
            shaderBehaviourEnabled = TransformSettings.ShaderBehaviourFieldEnabled;
            staticBehaviourEnabled = TransformSettings.StaticBehaviourFieldEnabled;
            swimmingAnimalBehaviourEnabled = TransformSettings.SwimmingAnimalBehaviourFieldEnabled;


            animateModel = TransformSettings.AnimateModel;
            placeOnGrid = TransformSettings.PlaceOnGrid;
            placeOnGround = TransformSettings.PlaceOnGround;

            serializeAsset = TransformSettings.SerializeAsset;
            addCollider = TransformSettings.AddCollider;
            addRigidbody = TransformSettings.AddRigidbody;
            showGridHandles = TransformSettings.ShowGridHandles;
            debugOn = AnythingSettings.DebugEnabled;

            objectParent = AnythingCreatorEditor.objectParent;
            objectPosition = TransformSettings.PositionField;
            objectRotation = TransformSettings.RotationField;
            objectScale = TransformSettings.ScaleField;
            defaultRiggedScript = TransformSettings.RiggedAnimalScript;
            defaultVehicleScript = TransformSettings.GroundVehicleScript;
            defaultFlyingVehicleScript = TransformSettings.FlyingVehicleScript;
            defaultStaticScript = TransformSettings.StaticBehaviourScript;
            defaultShaderScript = TransformSettings.ShaderBehaviourScript;
            defaultFlyingAnimalScript = TransformSettings.FlyingAnimalScript;
            defaultSwimmingAnimalScript = TransformSettings.SwimmingAnimalScript;
        }

        internal void ApplySettings()
        {
            if (AnythingSettings.Instance == null || TransformSettings.Instance == null)
            {
                Debug.LogError("No AnythingSettings instance located.");
            }
            else
            {
                var transformSettingsSerializedObject = new SerializedObject(TransformSettings.Instance);
                transformSettingsSerializedObject.FindProperty("animateModel").boolValue = animateModel;
                transformSettingsSerializedObject.FindProperty("placeOnGround").boolValue = placeOnGround;
                transformSettingsSerializedObject.FindProperty("placeOnGrid").boolValue = placeOnGrid;
                transformSettingsSerializedObject.FindProperty("serializeAsset").boolValue = serializeAsset;
                transformSettingsSerializedObject.FindProperty("addCollider").boolValue = addCollider;
                transformSettingsSerializedObject.FindProperty("addRigidbody").boolValue = addRigidbody;
                transformSettingsSerializedObject.FindProperty("showGridHandles").boolValue = showGridHandles;

                transformSettingsSerializedObject.FindProperty("defaultRiggedBehaviourEnabled").boolValue = riggedBehaviourEnabled;
                transformSettingsSerializedObject.FindProperty("defaultVehicleBehaviourEnabled").boolValue = vehicleBehaviourEnabled;
                transformSettingsSerializedObject.FindProperty("defaultFlyingVehicleBehaviourEnabled").boolValue = flyingVehicleBehaviourEnabled;
                transformSettingsSerializedObject.FindProperty("defaultFlyingAnimalBehaviourEnabled").boolValue = flyingVehicleBehaviourEnabled;
                transformSettingsSerializedObject.FindProperty("defaultShaderBehaviourEnabled").boolValue = shaderBehaviourEnabled;
                transformSettingsSerializedObject.FindProperty("defaultStaticBehaviourEnabled").boolValue = staticBehaviourEnabled;
                transformSettingsSerializedObject.FindProperty("defaultSwimmingAnimalBehaviourEnabled").boolValue = swimmingAnimalBehaviourEnabled;

                transformSettingsSerializedObject.FindProperty("customParentTransformEnabled").boolValue = parentEnabled;
                transformSettingsSerializedObject.FindProperty("customPositionFieldEnabled").boolValue = positionEnabled;
                transformSettingsSerializedObject.FindProperty("customRotationFieldEnabled").boolValue = rotationEnabled;
                transformSettingsSerializedObject.FindProperty("customScaleMultiplierEnabled").boolValue = scaleEnabled;

                transformSettingsSerializedObject.FindProperty("defaultRiggedScript").objectReferenceValue = defaultRiggedScript;
                transformSettingsSerializedObject.FindProperty("defaultVehicleScript").objectReferenceValue = defaultVehicleScript;
                transformSettingsSerializedObject.FindProperty("defaultFlyingVehicleScript").objectReferenceValue = defaultFlyingVehicleScript;
                transformSettingsSerializedObject.FindProperty("defaultShaderScript").objectReferenceValue = defaultShaderScript;
                transformSettingsSerializedObject.FindProperty("defaultStaticScript").objectReferenceValue = defaultStaticScript;
                transformSettingsSerializedObject.FindProperty("defaultFlyingAnimalScript").objectReferenceValue = defaultFlyingAnimalScript;
                transformSettingsSerializedObject.FindProperty("defaultSwimmingAnimalScript").objectReferenceValue = defaultSwimmingAnimalScript;
                AnythingCreatorEditor.objectParent = objectParent;
                transformSettingsSerializedObject.FindProperty("objectPosition").vector3Value = objectPosition;
                transformSettingsSerializedObject.FindProperty("objectRotation").vector3Value = objectRotation;
                transformSettingsSerializedObject.FindProperty("objectScaleMultiplier").floatValue = objectScale;
                transformSettingsSerializedObject.ApplyModifiedProperties();

                var settingsSerializedObject = new SerializedObject(AnythingSettings.Instance);
                settingsSerializedObject.FindProperty("showDebugMessages").boolValue = debugOn;
                settingsSerializedObject.ApplyModifiedProperties();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Undo.RecordObject(TransformSettings.Instance, "Changed Transform Settings");
                EditorUtility.SetDirty(AnythingSettings.Instance);
                EditorUtility.SetDirty(TransformSettings.Instance);
            }
        }

        protected new void OnGUI()
        {
            base.OnGUI();

            InitializeResources();
            DrawTransformSettings();
            if (!docked)
            {
                addedDrawerHeight = (showGeneralDrawer ? 115f : 0f) +
                                    (showPhysicsDrawer ? 47.5f : 0f) +
                                    (showTransformDrawer ? 217.5f : 0f) +
                                    (showDefaultBehavioursDrawer ? (40f*7f): 0f) +
                                    (showGridOptionsDrawer ? 177.5f : 0f);
                minSize = new Vector2(375f, Mathf.Min(maxSize.y, addedDrawerHeight + MinSize));
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply", ButtonInactiveStyle)) ApplySettings();
            if (GUILayout.Button("Reset", ButtonInactiveStyle)) SetupVariables();
        }

        private void OnDestroy()
        {
            if (CheckForChanges())
            {
                if (EditorUtility.DisplayDialog("UNSAVED CHANGES", "You have not saved your changes, would you like to save your changes?", "Yes, save my changes", "No"))
                {
                    ApplySettings();
                }
            }
        }

        protected bool CheckForChanges()
        {
            bool changesDetected = false;

            if (AnythingSettings.Instance == null || TransformSettings.Instance == null)
            {
                Debug.LogError("No AnythingSettings instance located.");
            }
            else
            {
                var transformSettingsSerializedObject = new SerializedObject(TransformSettings.Instance);
                var settingsSerializedObject = new SerializedObject(AnythingSettings.Instance);
                changesDetected = transformSettingsSerializedObject.FindProperty("animateModel").boolValue != animateModel ||
                                  transformSettingsSerializedObject.FindProperty("placeOnGround").boolValue != placeOnGround ||
                                  transformSettingsSerializedObject.FindProperty("placeOnGrid").boolValue != placeOnGrid ||
                                  transformSettingsSerializedObject.FindProperty("serializeAsset").boolValue != serializeAsset ||
                                  transformSettingsSerializedObject.FindProperty("addCollider").boolValue != addCollider ||
                                  transformSettingsSerializedObject.FindProperty("addRigidbody").boolValue != addRigidbody ||
                                  transformSettingsSerializedObject.FindProperty("showGridHandles").boolValue != showGridHandles ||
                                  transformSettingsSerializedObject.FindProperty("defaultRiggedBehaviourEnabled").boolValue != riggedBehaviourEnabled ||
                                  transformSettingsSerializedObject.FindProperty("defaultVehicleBehaviourEnabled").boolValue != vehicleBehaviourEnabled ||
                                  transformSettingsSerializedObject.FindProperty("defaultFlyingVehicleBehaviourEnabled").boolValue != flyingVehicleBehaviourEnabled ||
                                  transformSettingsSerializedObject.FindProperty("defaultShaderBehaviourEnabled").boolValue != shaderBehaviourEnabled ||
                                  transformSettingsSerializedObject.FindProperty("defaultStaticBehaviourEnabled").boolValue != staticBehaviourEnabled ||
                                  transformSettingsSerializedObject.FindProperty("defaultSwimmingAnimalBehaviourEnabled").boolValue != swimmingAnimalBehaviourEnabled ||
                                  transformSettingsSerializedObject.FindProperty("customParentTransformEnabled").boolValue != parentEnabled ||
                                  transformSettingsSerializedObject.FindProperty("customPositionFieldEnabled").boolValue != positionEnabled ||
                                  transformSettingsSerializedObject.FindProperty("customRotationFieldEnabled").boolValue != rotationEnabled ||
                                  transformSettingsSerializedObject.FindProperty("customScaleMultiplierEnabled").boolValue != scaleEnabled ||
                                  transformSettingsSerializedObject.FindProperty("defaultRiggedScript").objectReferenceValue != defaultRiggedScript ||
                                  transformSettingsSerializedObject.FindProperty("defaultVehicleScript").objectReferenceValue != defaultVehicleScript ||
                                  transformSettingsSerializedObject.FindProperty("defaultFlyingVehicleScript").objectReferenceValue != defaultFlyingVehicleScript ||
                                  transformSettingsSerializedObject.FindProperty("defaultSwimmingAnimalScript").objectReferenceValue != defaultSwimmingAnimalScript ||
                                  transformSettingsSerializedObject.FindProperty("defaultFlyingAnimalScript").objectReferenceValue != defaultFlyingAnimalScript ||
                                  transformSettingsSerializedObject.FindProperty("defaultShaderScript").objectReferenceValue != defaultShaderScript ||
                                  transformSettingsSerializedObject.FindProperty("defaultStaticScript").objectReferenceValue != defaultStaticScript ||
                                  transformSettingsSerializedObject.FindProperty("defaultFlyingAnimalScript").objectReferenceValue != defaultFlyingAnimalScript ||
                                  transformSettingsSerializedObject.FindProperty("objectParent").objectReferenceValue != objectParent ||
                                  transformSettingsSerializedObject.FindProperty("objectPosition").vector3Value != objectPosition ||
                                  transformSettingsSerializedObject.FindProperty("objectRotation").vector3Value != objectRotation ||
                                  transformSettingsSerializedObject.FindProperty("objectScaleMultiplier").floatValue != objectScale ||
                                  settingsSerializedObject.FindProperty("showDebugMessages").boolValue != debugOn;
            }

            return changesDetected;
        }

        Vector2 scrollPosition;

        protected void DrawTransformSettings()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            #region General Settings Drawer
            DrawerButton(ref showGeneralDrawer, "General");
            if (showGeneralDrawer)
            {
                var content = new GUIContent("Animate Model", "If animations exist for this model, disable this option to override it and make a static object.");
                EditorGUILayout.Space(5);
                GUILayout.BeginHorizontal();
                animateModel = EditorGUILayout.Toggle(animateModel, GUILayout.Width(20));
                EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                content = new GUIContent("Enable Debug Messages", "Print debug messages from the make process into console.");
                GUILayout.BeginHorizontal();
                debugOn = EditorGUILayout.Toggle(debugOn, GUILayout.Width(20));
                EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                content = new GUIContent("Place On Top Of Ground", "Use bottom of model as the origin instead of the center.");
                GUILayout.BeginHorizontal();
                placeOnGround = EditorGUILayout.Toggle(placeOnGround, GUILayout.Width(20));
                EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                content = new GUIContent("Enable Grid Placement", "Place model following the grid structure.");
                GUILayout.BeginHorizontal();
                placeOnGrid = EditorGUILayout.Toggle(placeOnGrid, GUILayout.Width(20));
                EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                content = new GUIContent("Serialize Asset", "Attempt to serialize model assets to asset database.");
                GUILayout.BeginHorizontal();
                serializeAsset = EditorGUILayout.Toggle(serializeAsset, GUILayout.Width(20));
                EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            #endregion General Settings Drawer
            #region Physics Drawer
            DrawerButton(ref showPhysicsDrawer, "Physics");
            if (showPhysicsDrawer)
            {
                var content = new GUIContent("Add Collider", "A collider encapsulating the mesh(es) to the top level GameObject.");
                GUILayout.BeginHorizontal();
                addCollider = EditorGUILayout.Toggle(addCollider, GUILayout.Width(20));
                EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                content = new GUIContent("Add Rigidbody", "Add a default rigidbody to top level model GameObject.");
                GUILayout.BeginHorizontal();
                addRigidbody = EditorGUILayout.Toggle(addRigidbody, GUILayout.Width(20));
                EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            #endregion Physics Drawer
            #region Transform Drawer
            DrawerButton(ref showTransformDrawer, "Transform");
            if (showTransformDrawer)
            {
                EditorGUILayout.HelpBox("Change positioning, rotation and scale of model within the scene.", MessageType.Info);

                EditorGUILayout.Space(5);
                CustomTransformField(ref objectParent, "Parent Transform", ref parentEnabled);
                EditorGUILayout.Space(5);
                CustomVectorField(ref objectPosition, "Position", ref positionEnabled);
                EditorGUILayout.Space(5);
                CustomVectorField(ref objectRotation, "Rotation", ref rotationEnabled);
                EditorGUILayout.Space(5);
                CustomFloatField(ref objectScale, "Scale Multiplier", ref scaleEnabled);
                GUI.enabled = true;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All"))
                {
                    parentEnabled = true;
                    positionEnabled = true;
                    rotationEnabled = true;
                    scaleEnabled = true;
                }
                if (GUILayout.Button("Deselect All"))
                {
                    parentEnabled = false;
                    positionEnabled = false;
                    rotationEnabled = false;
                    scaleEnabled = false;
                }
                GUILayout.EndHorizontal();
            }
            #endregion Transform Drawer
            #region Behaviours Drawer
            DrawerButton(ref showDefaultBehavioursDrawer, "Default Behaviours");
            if (showDefaultBehavioursDrawer)
            {
                //DrawerTitle("DEFAULT");
                EditorGUILayout.HelpBox("Specify the default behaviours added to models for each type of model.", MessageType.Info);

                EditorGUILayout.Space(5);
                DrawBehaviourField(ref defaultRiggedScript, "Animated Behaviour", ref riggedBehaviourEnabled);
                EditorGUILayout.Space(5);
                DrawBehaviourField(ref defaultVehicleScript, "Vehicle", ref vehicleBehaviourEnabled);
                EditorGUILayout.Space(5);
                DrawBehaviourField(ref defaultFlyingVehicleScript, "Flying Vehicle", ref flyingVehicleBehaviourEnabled);
                EditorGUILayout.Space(5);
                DrawBehaviourField(ref defaultFlyingAnimalScript, "Flying Animal", ref flyingAnimalBehaviourEnabled);
                EditorGUILayout.Space(5);
                DrawBehaviourField(ref defaultSwimmingAnimalScript, "Swimming Animal", ref swimmingAnimalBehaviourEnabled);
                EditorGUILayout.Space(5);
                DrawBehaviourField(ref defaultStaticScript, "Static", ref staticBehaviourEnabled);
                EditorGUILayout.Space(5);
                DrawBehaviourField(ref defaultShaderScript, "Shader", ref shaderBehaviourEnabled);
                GUI.enabled = true;

                if(GUILayout.Button("Save Behaviour Preset"))
                {
                    DefaultBehavioursUtility.CreateSerializedInstance(AnythingCreatorEditor.DefaultBehaviourDictionary);
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All"))
                {
                    riggedBehaviourEnabled = true;
                    vehicleBehaviourEnabled = true;
                    flyingVehicleBehaviourEnabled = true;
                    flyingAnimalBehaviourEnabled = true;
                    staticBehaviourEnabled = true;
                    shaderBehaviourEnabled = true;

                }
                if (GUILayout.Button("Deselect All"))
                {
                    riggedBehaviourEnabled = false;
                    vehicleBehaviourEnabled = false;
                    flyingAnimalBehaviourEnabled = false;
                    flyingVehicleBehaviourEnabled = false;
                    staticBehaviourEnabled = false;
                    shaderBehaviourEnabled = false;
                }
                GUILayout.EndHorizontal();
            }
            #endregion Behaviours Drawer
            #region Grid Settings
            DrawerButton(ref showGridOptionsDrawer, "Grid");
            if (showGridOptionsDrawer)
            {
                EditorGUILayout.HelpBox("Change way grid placement works.", MessageType.Info);
                EditorGUILayout.Space(5);
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(showGridHandles ? "Grid Gizmos: On" : "Grid Gizmos: Off"))
                {
                    showGridHandles = !showGridHandles;
                    SceneView.RepaintAll();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
                CustomVectorField(ref SimpleGrid.origin, "Grid Origin");
                EditorGUILayout.Space(5);
                CustomFloatField(ref SimpleGrid.cellWidth, "Cell Width");
                EditorGUILayout.Space(5);
                CustomIntField(ref SimpleGrid.cellCount, "Grid Width");
            }
            #endregion
            EditorGUILayout.EndScrollView();
        }
    }
}
