using AnythingWorld.Utilities;
using UnityEditor;
using UnityEngine;

namespace AnythingWorld.Editor
{
    public class GridSettingsEditor : AnythingEditor
    {
        protected bool gridPlacementEnabled = false;

        private bool showGridHandles = false;

        [MenuItem("Anything World/Grid Settings", false, 53)]
        public static void Initialize()
        {
            Resources.LoadAll<TransformSettings>("Settings");

            AnythingCreatorEditor tabWindow;
            Vector2 windowSize;

            if (AnythingSettings.HasAPIKey)
            {
                windowSize = new Vector2(375, 240f);

                var browser = GetWindow(typeof(GridSettingsEditor), false, "Grid Settings") as GridSettingsEditor;
                browser.position = new Rect(EditorGUIUtility.GetMainWindowPosition().center - windowSize / 2, windowSize);
                browser.minSize = new Vector2(375f, 240f);
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
            showGridHandles = TransformSettings.ShowGridHandles;
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
                transformSettingsSerializedObject.FindProperty("showGridHandles").boolValue = showGridHandles;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Undo.RecordObject(TransformSettings.Instance, "Changed Grid Settings");
                EditorUtility.SetDirty(AnythingSettings.Instance);
                EditorUtility.SetDirty(TransformSettings.Instance);
            }
        }

        protected new void OnGUI()
        {
            base.OnGUI();

            InitializeResources();
            DrawTransformSettings();

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
                changesDetected = transformSettingsSerializedObject.FindProperty("showGridHandles").boolValue != showGridHandles;
            }

            return changesDetected;
        }

        protected void DrawTransformSettings()
        {
            #region Grid Settings
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
            #endregion
        }
    }
}
