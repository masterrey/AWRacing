using AnythingWorld.Utilities;

using UnityEngine;
using UnityEditor;

namespace AnythingWorld.Editor
{
    public class AnythingSettingsEditor : AnythingEditor
    {
        protected enum SettingsOptions { None, General, Development }
        protected SettingsOptions landscapeActiveOption;

        private bool portraitShowGeneralDrawer = false;
        private bool portraitShowDevelopmentDrawer = false;
        private bool closeAfterApply = true;
        private Vector2 scrollPosition;
        private float topBarPadding, topBarSize, applyBarHeight;

        private string apiKey;
        private string email;
        private string appName;
        private bool debugOn;
        private bool serialize;

        [MenuItem("Anything World/Settings", false, 51)]
        internal static void Initialize()
        {
            Resources.LoadAll<AnythingSettings>("Settings");
            Resources.LoadAll<TransformSettings>("Settings");

            Vector2 windowSize = EditorGUIUtility.GetMainWindowPosition().size / 4;

            var browser = GetWindow(typeof(AnythingSettingsEditor), false, "Anything Settings") as AnythingSettingsEditor;
            browser.position = new Rect(EditorGUIUtility.GetMainWindowPosition().center - windowSize / 2, windowSize);
            browser.minSize = new Vector2(500, 250);
            browser.maxSize = EditorGUIUtility.GetMainWindowPosition().size / 2;
            browser.Show();
            browser.Focus();

            EditorUtility.SetDirty(AnythingSettings.Instance);
            browser.SetupVariables();
            EditorUtility.SetDirty(browser);
        }

        internal void SetupVariables()
        {
            apiKey = AnythingSettings.APIKey;
            appName = AnythingSettings.AppName;
            email = AnythingSettings.Email;

            debugOn = AnythingSettings.DebugEnabled;
            serialize = TransformSettings.SerializeAsset;
        }

        protected new void OnGUI()
        {
            base.OnGUI();
            if (!editorInitialized)
            {
                if (!InitializeResources())
                {
                    return;
                }
            }
            topBarPadding = Mathf.Min(position.width / 80, 10);
            topBarSize = Mathf.Min(position.width / 12.5f, 64);

            DrawTopBar();
            if (position.width < position.height)
            {
                applyBarHeight = 90f;
                DrawSettingsPortrait();
            }
            else
            {
                applyBarHeight = 60f;
                DrawSettingsLandscape();
            }
        }

        private void OnDestroy()
        {
            if (CheckForChanges())
            {
                if (EditorUtility.DisplayDialog("UNSAVED CHANGES", "You have not saved your changes, would you like to save your changes?", "Yes, save my changes", "No"))
                {
                    ApplySettings(true);
                }
            }
        }

        protected void DrawTopBar()
        {
            var globeRect = new Rect(10, 10,
                                     topBarSize, topBarSize);
            GUI.DrawTexture(globeRect, TintedAnythingGlobeLogo);

            var titleRect = new Rect(globeRect.xMax + topBarPadding, globeRect.y, position.width - globeRect.xMax - 10 - topBarPadding, globeRect.height);
            var settingsText = new GUIContent("ANYTHING WORLD SETTINGS");
            GUI.Label(titleRect, settingsText, new GUIStyle(EditorStyles.label) { font = GetPoppinsFont(PoppinsStyle.Bold), fontSize = (int)(titleRect.height * 0.74f), alignment = TextAnchor.UpperLeft });
            GUILayoutUtility.GetRect(position.width, globeRect.yMax + topBarPadding);
        }

        protected void DrawSettingsPortrait()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height - topBarPadding - topBarSize - applyBarHeight));
            DrawerButton(ref portraitShowGeneralDrawer, "General Settings");
            if (portraitShowGeneralDrawer)
            {
                DrawPortraitGeneralDrawer();
            }
            DrawerButton(ref portraitShowDevelopmentDrawer, "Development Settings");
            if (portraitShowDevelopmentDrawer)
            {
                DrawPortraitDevelopmentDrawer();
            }
            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button($"Apply{(closeAfterApply ? " & Close" : "")}", ButtonInactiveStyle))
            {
                ApplySettings();
            }

            var applyButtonRect = GUILayoutUtility.GetLastRect();
            var toggleHeight = applyButtonRect.height;

            var toggleRect = new Rect(applyButtonRect.xMin, applyButtonRect.yMin - toggleHeight - Mathf.Min(position.width / 80, position.height / 40, 10), applyButtonRect.width, toggleHeight);

            closeAfterApply = EditorGUI.ToggleLeft(toggleRect, "Close After Apply", closeAfterApply, BodyLabelStyle);

            if (GUILayout.Button("Reset", ButtonInactiveStyle))
            {
                SetupVariables();
            }
        }

        private void DrawPortraitDevelopmentDrawer()
        {
            var content = new GUIContent("Enable Debug Messages", "Print debug messages from the make process into console.");
            GUILayout.BeginHorizontal();
            debugOn = EditorGUILayout.Toggle(debugOn, GUILayout.Width(20));
            EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            content = new GUIContent("Serialize Assets", "Attempt to serialize all model assets to asset database when called from Anything World creator windows.");
            GUILayout.BeginHorizontal();
            serialize = EditorGUILayout.Toggle(serialize, GUILayout.Width(20));
            EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawPortraitGeneralDrawer()
        {
            var content = new GUIContent("API Key", "The key necessary to access the Anything World database.");
            EditorGUILayout.Space(5);
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
            apiKey = EditorGUILayout.TextField(apiKey);
            GUILayout.EndVertical();

            content = new GUIContent("Email", "The email address of the account tied to your Anything World account.");
            EditorGUILayout.Space(5);
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
            email = EditorGUILayout.TextField(email);
            GUILayout.EndVertical();

            content = new GUIContent("Application Name", "The name of the application. Please make sure that this value is different for each app.");
            EditorGUILayout.Space(5);
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
            appName = EditorGUILayout.TextField(appName);
            GUILayout.EndVertical();

            content = new GUIContent($"Version: {AnythingSettings.PackageVersion}", "Anything World Version");
            EditorGUILayout.Space(5);
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Check for updates"))
            {
                AnythingWorld.Utilities.Editor.VersionCheckEditor.CheckVersion();
            }
            GUILayout.EndVertical();


        }

        protected void DrawSettingsLandscape()
        {
            float buttonWidth = Mathf.Min(position.width / 3, 300f);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(buttonWidth));
            TabButton(SettingsOptions.General, "General Settings");
            TabButton(SettingsOptions.Development, "Development Settings");
            GUILayout.EndVertical();
            GUILayout.BeginVertical();

            GUIContent content;

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height - topBarPadding - topBarSize - applyBarHeight));
            switch (landscapeActiveOption)
            {
                case SettingsOptions.General:
                    portraitShowGeneralDrawer = true;
                    portraitShowDevelopmentDrawer = false;

                    content = new GUIContent("API Key", "The key necessary to access the Anything World database.");
                    GUILayout.BeginVertical();
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
                    apiKey = EditorGUILayout.TextField(apiKey);
                    GUILayout.EndVertical();

                    content = new GUIContent("Email", "The email address of the account tied to your Anything World account.");
                    GUILayout.BeginVertical();
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
                    email = EditorGUILayout.TextField(email);
                    GUILayout.EndVertical();

                    content = new GUIContent("Application Name", "The name of the application. Please make sure that this value is different for each app.");
                    GUILayout.BeginVertical();
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
                    appName = EditorGUILayout.TextField(appName);
                    GUILayout.EndVertical();

                    content = new GUIContent($"Version: {AnythingSettings.PackageVersion}", "Anything World Version");
                    GUILayout.BeginVertical();
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
                    if(GUILayout.Button("Check for updates"))
                    {
                        AnythingWorld.Utilities.Editor.VersionCheckEditor.CheckVersion();
                    }
                    GUILayout.EndVertical();
                    break;
                case SettingsOptions.Development:
                    portraitShowGeneralDrawer = false;
                    portraitShowDevelopmentDrawer = true;

                    content = new GUIContent("Enable Debug Messages", "Print debug messages from the make process into console.");
                    GUILayout.BeginHorizontal();
                    debugOn = EditorGUILayout.Toggle(debugOn, GUILayout.Width(20));
                    EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    content = new GUIContent("Serialize Assets", "Attempt to serialize all model assets to asset database when called from Anything World creator windows.");
                    GUILayout.BeginHorizontal();
                    serialize = EditorGUILayout.Toggle(serialize, GUILayout.Width(20));
                    EditorGUILayout.LabelField(content, GUILayout.ExpandWidth(false));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    break;
                default:
                    portraitShowGeneralDrawer = false;
                    portraitShowDevelopmentDrawer = false;
                    break;
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal(GUILayout.Height(30));
            GUILayout.FlexibleSpace();
            GUILayout.Space(10);
            if (GUILayout.Button($"Apply{(closeAfterApply ? " & Close" : "")}", ButtonInactiveStyle, GUILayout.MaxWidth(300f)))
            {
                ApplySettings();
            }

            var applyButtonRect = GUILayoutUtility.GetLastRect();
            var toggleHeight = applyButtonRect.height;

            var toggleRect = new Rect(applyButtonRect.xMin, applyButtonRect.yMin - toggleHeight - Mathf.Min(position.width / 80, position.height / 40, 10), applyButtonRect.width, toggleHeight);

            closeAfterApply = EditorGUI.ToggleLeft(toggleRect, "Close After Apply", closeAfterApply, BodyLabelStyle);

            if (GUILayout.Button("Reset", ButtonInactiveStyle, GUILayout.MaxWidth(300f)))
            {
                SetupVariables();
            }
            GUILayout.Space(10);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
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
                var settingsSerializedObject = new SerializedObject(AnythingSettings.Instance);
                var transformSettingsSerializedObject = new SerializedObject(TransformSettings.Instance);
                changesDetected = settingsSerializedObject.FindProperty("apiKey").stringValue != apiKey || 
                                  settingsSerializedObject.FindProperty("appName").stringValue != appName || 
                                  settingsSerializedObject.FindProperty("email").stringValue != email || 
                                  settingsSerializedObject.FindProperty("showDebugMessages").boolValue != debugOn || 
                                  transformSettingsSerializedObject.FindProperty("serializeAsset").boolValue != serialize;
            }

            return changesDetected;
        }

        protected void ApplySettings(bool dontClose = false)
        {
            if (AnythingSettings.Instance == null || TransformSettings.Instance == null)
            {
                Debug.LogError("No AnythingSettings instance located.");
                if (closeAfterApply && !dontClose) CloseWindowIfOpen<AnythingSettingsEditor>();
            }
            else
            {
                var settingsSerializedObject = new SerializedObject(AnythingSettings.Instance);
                settingsSerializedObject.FindProperty("apiKey").stringValue = apiKey;
                settingsSerializedObject.FindProperty("appName").stringValue = appName;
                settingsSerializedObject.FindProperty("email").stringValue = email;
                settingsSerializedObject.FindProperty("showDebugMessages").boolValue = debugOn;
                settingsSerializedObject.ApplyModifiedProperties();

                var transformSettingsSerializedObject = new SerializedObject(TransformSettings.Instance);
                transformSettingsSerializedObject.FindProperty("serializeAsset").boolValue = serialize;
                transformSettingsSerializedObject.ApplyModifiedProperties();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Undo.RecordObject(AnythingSettings.Instance, "Changed AnythingSettings");
                EditorUtility.SetDirty(AnythingSettings.Instance);
                EditorUtility.SetDirty(TransformSettings.Instance);

                if (closeAfterApply && !dontClose) CloseWindowIfOpen<AnythingSettingsEditor>();
            }
        }

        private void TabButton(SettingsOptions option, string label)
        {
            if (GUILayout.Button(label, landscapeActiveOption == option ? ButtonActiveStyle : ButtonInactiveStyle, GUILayout.Height(20)))
            {
                if (landscapeActiveOption == option) landscapeActiveOption = SettingsOptions.None;
                else landscapeActiveOption = option;
            }
        }
    }
}
