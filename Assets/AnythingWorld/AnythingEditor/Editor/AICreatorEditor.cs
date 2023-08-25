using AnythingWorld.Voice;

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace AnythingWorld.Editor
{
    public class AICreatorEditor : AnythingCreatorEditor
    {
        private bool micActive = false;
        private bool showLog = true;
        private string writtenPrompt = "";
        #region Wave Form Variables
        private int waveCount = 5;
        private float maxWaveHeight = 40f;
        private float wavePadding = 8f;
        #region Processing Variables
        private bool processing;
        private float processingAnimationAmplitude = 0.15f;
        private float processingAnimationSpeed = 5f;
        public bool autoMake;
        public ParsedSpeechCommand lastMadeCommand;
        #endregion Processing Variables
        #endregion Wave Form Variables

        #region Textures
        private static Texture2D baseButton;
        protected static Texture2D BaseButton
        {
            get
            {
                if (baseButton == null)
                {
                    baseButton = Resources.Load("Editor/BuilderBot/builderBotButton") as Texture2D;
                }
                return baseButton;
            }
        }

        private static Texture2D baseMicrophone;
        protected static Texture2D BaseMicrophone
        {
            get
            {
                if (baseMicrophone == null)
                {
                    baseMicrophone = Resources.Load("Editor/BuilderBot/microphone") as Texture2D;
                }
                return baseMicrophone;
            }
        }

        private static Texture2D baseWaveFormTopCap;
        protected static Texture2D BaseWaveFormTopCap
        {
            get
            {
                if (baseWaveFormTopCap == null)
                {
                    baseWaveFormTopCap = Resources.Load("Editor/BuilderBot/waveformTopCap") as Texture2D;
                }
                return baseWaveFormTopCap;
            }
        }

        private static Texture2D baseWaveFormBottomCap;
        protected static Texture2D BaseWaveFormBottomCap
        {
            get
            {
                if (baseWaveFormBottomCap == null)
                {
                    baseWaveFormBottomCap = Resources.Load("Editor/BuilderBot/waveformBottomCap") as Texture2D;
                }
                return baseWaveFormBottomCap;
            }
        }

        private static Texture2D baseWaveFormCore;
        protected static Texture2D BaseWaveFormCore
        {
            get
            {
                if (baseWaveFormCore == null)
                {
                    baseWaveFormCore = Resources.Load("Editor/BuilderBot/waveformFill") as Texture2D;
                }
                return baseWaveFormCore;
            }
        }


        private Texture2D tintedButton;
        protected Texture2D TintedButton
        {
            get
            {
                if (tintedButton == null)
                {
                    tintedButton = TintTextureToEditorTheme(BaseButton, HexToColour("575859"), HexToColour("E4E5E3"));
                }

                return tintedButton;
            }
            set => tintedButton = value;
        }

        private Texture2D tintedMicrophone;
        protected Texture2D TintedMicrophone
        {
            get
            {
                if (tintedMicrophone == null)
                {
                    tintedMicrophone = TintTextureToEditorTheme(BaseMicrophone, Color.white, Color.black);
                }

                return tintedMicrophone;
            }
            set => tintedMicrophone = value;
        }

        private Texture2D tintedWaveFormTopCap;
        protected Texture2D TintedWaveFormTopCap
        {
            get
            {
                if (tintedWaveFormTopCap == null)
                {
                    tintedWaveFormTopCap = TintTextureToEditorTheme(BaseWaveFormTopCap, Color.white, Color.black);
                }

                return tintedWaveFormTopCap;
            }
            set => tintedWaveFormTopCap = value;
        }

        private Texture2D tintedWaveFormBottomCap;
        protected Texture2D TintedWaveFormBottomCap
        {
            get
            {
                if (tintedWaveFormBottomCap == null)
                {
                    tintedWaveFormBottomCap = TintTextureToEditorTheme(BaseWaveFormBottomCap, Color.white, Color.black);
                }

                return tintedWaveFormBottomCap;
            }
            set => tintedWaveFormBottomCap = value;
        }

        private Texture2D tintedWaveFormCore;
        protected Texture2D TintedWaveFormCore
        {
            get
            {
                if (tintedWaveFormCore == null)
                {
                    tintedWaveFormCore = TintTextureToEditorTheme(BaseWaveFormCore, Color.white, Color.black);
                }

                return tintedWaveFormCore;
            }
            set => tintedWaveFormCore = value;
        }
        #endregion Textures

        #region Initialization
        /// <summary>
        /// Initializes and shows window, called from Anything World top bar menu.
        /// </summary>
        [MenuItem("Anything World/AI Creator", false, 3)]
        internal static void Initialize()
        {
            AnythingCreatorEditor tabWindow;
            Vector2 windowSize;
            if (AnythingSettings.HasAPIKey)
            {
                windowSize = new Vector2(500, 800);
                CloseWindowIfOpen<AICreatorEditor>();
                var browser = GetWindow(typeof(AICreatorEditor), false, "AI Creator");
                browser.position = new Rect(EditorGUIUtility.GetMainWindowPosition().center - windowSize / 2, windowSize);
                browser.Show();
                browser.Focus();
            }
            else
            {
                CloseWindowIfOpen<ModelBrowserEditor>();
                CloseWindowIfOpen<MyWorldEditor>();
                CloseWindowIfOpen<AICreatorEditor>();
                windowSize = new Vector2(450, 800);
                tabWindow = GetWindow<LogInEditor>("Log In | Sign Up", false);
                tabWindow.position = new Rect(EditorGUIUtility.GetMainWindowPosition().center - windowSize / 2, windowSize);
            }
        }

        protected new void Awake()
        {
            base.Awake();
            windowTitle = "AI Creator";
            bannerTintA = HexToColour("EB00FF");
            bannerTintB = HexToColour("FFB800");
        }
        #endregion Initialization

        #region Editor Drawing
        protected new void OnGUI()
        {
            base.OnGUI();
            if (Event.current.type == EventType.Repaint && !AnythingSettings.HasAPIKey) Close();
            #region Overwriting Editor Styles
            var backupLabelStyle = new GUIStyle(EditorStyles.label);
            var backupObjectStyle = new GUIStyle(EditorStyles.objectField);
            var backupNumberStyle = new GUIStyle(EditorStyles.numberField);
            var backupFoldoutStyle = new GUIStyle(EditorStyles.foldout);

            EditorStyles.label.font = GetPoppinsFont(PoppinsStyle.Bold);
            EditorStyles.objectField.font = GetPoppinsFont(PoppinsStyle.Medium);
            EditorStyles.numberField.font = GetPoppinsFont(PoppinsStyle.Medium);
            EditorStyles.foldout.font = GetPoppinsFont(PoppinsStyle.Bold);
            EditorStyles.foldout.fontSize = 16;
            #endregion Overwriting Editor Styles

            if (!EditorGUIUtility.wideMode)
            {
                EditorGUIUtility.wideMode = true;
                EditorGUIUtility.labelWidth = 170;
            }

            try
            {
                DrawBuilderBot();

                #region Resetting Editor Styles
                EditorStyles.label.font = backupLabelStyle.font;
                EditorStyles.objectField.font = backupObjectStyle.font;
                EditorStyles.numberField.font = backupNumberStyle.font;
                EditorStyles.foldout.font = backupFoldoutStyle.font;
                EditorStyles.foldout.fontSize = backupFoldoutStyle.fontSize;
                #endregion Resetting Editor Styles
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                #region Resetting Editor Styles
                EditorStyles.label.font = backupLabelStyle.font;
                EditorStyles.objectField.font = backupObjectStyle.font;
                EditorStyles.numberField.font = backupNumberStyle.font;
                EditorStyles.foldout.font = backupFoldoutStyle.font;
                EditorStyles.foldout.fontSize = backupFoldoutStyle.fontSize;
                #endregion Resetting Editor Styles
            }

            if (searchMode == SearchMode.RUNNING)
            {
                Repaint();
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
        }

        private void DrawBuilderBot()
        {
            Event e = Event.current;

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            Rect buttonRect = DrawMicrophoneButton();

            Repaint();
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();

            GUI.DrawTexture(buttonRect, AnythingVoice.isRecording ? TintTexture(BaseMicrophone, HexToColour("FFB800")) : TintedMicrophone);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawWaveForm(AnythingVoice.GetCurrentVolumeRange());
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);

            float marginSize = 0.1f;
            DrawTextInputBox(e, marginSize);

            GUILayout.Space(10f);
            DrawSubmitButton();
            GUILayout.Space(20);

            DrawResults(marginSize);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool gridWindowOpen = HasOpenInstances<GridSettingsEditor>();
            if (GUILayout.Button(new GUIContent("Grid Options", gridWindowOpen ? StateTransformIcon.activeTexture : StateTransformIcon.inactiveTexture), gridWindowOpen ? ButtonActiveStyle : ButtonInactiveStyle, GUILayout.MaxWidth((position.width / 3) - DropdownStyle.margin.horizontal - 8)))
            {
                if (gridWindowOpen)
                {
                    CloseWindowIfOpen<GridSettingsEditor>();
                }
                else
                {
                    GridSettingsEditor.Initialize();
                }
            }
            DrawMakeButton();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            processing = AnythingVoice.uploadInProgress;
        }

        private void DrawResults(float marginSize)
        {
            var results = "(0)";
            if (AnythingVoice.parsedCommand != null)
            {
                results = $"({AnythingVoice.parsedCommand?.actions.Count.ToString()})";
            }
            GUILayout.BeginHorizontal();
            GUILayout.Space(position.width * marginSize);
            if (results != "(0)")
            {
                showLog = EditorGUILayout.Foldout(showLog, $"Results {results}");
            }
            else
            {
                EditorGUILayout.Foldout(false, $"Results {results}");
            }

            GUILayout.EndHorizontal();
            if (showLog)
            {
                DrawOutputLog(marginSize);
            }
            GUILayout.Space(5);
        }

        private void DrawSubmitButton()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(position.width / 3);

            if (DrawRoundedButton("Submit", 1.5f, 8f, 1f, true, PoppinsStyle.Bold))
            {
                SubmitStringCommand();
            }
            GUILayout.Space(position.width / 3);
            GUILayout.EndHorizontal();
        }

        private void SubmitStringCommand()
        {
            modelFoldouts = null;
            AnythingVoice.RequestCommandsFromString(writtenPrompt);
            writtenPrompt = "";
        }

        private void DrawMakeButton()
        {
            GUILayout.BeginVertical();
            autoMake = GUILayout.Toggle(autoMake, "Auto", GUILayout.Height(30));

            if (autoMake)
            {
                if (AnythingVoice.parsedCommand != null && AnythingVoice.parsedCommand.result == CommandResult.Success)
                {
                    if (lastMadeCommand == null || lastMadeCommand != AnythingVoice.parsedCommand)
                    {
                        Voice.CommandHandler.ParseCommand(AnythingVoice.parsedCommand);
                        lastMadeCommand = AnythingVoice.parsedCommand;
                    }
                }

                if (lastMadeCommand == AnythingVoice.parsedCommand && lastMadeCommand != null)
                {
                    if (DrawRoundedButton("Make Again", 1f, 8f, 1f, true, PoppinsStyle.Bold))
                    {
                        if (AnythingVoice.parsedCommand != null && AnythingVoice.parsedCommand.result == CommandResult.Success)
                        {
                            Voice.CommandHandler.ParseCommand(AnythingVoice.parsedCommand);
                        }
                    }
                }
                else
                {
                    GUI.enabled = !autoMake;
                    if (DrawRoundedButton("Make All", 1f, 8f, 1f, true, PoppinsStyle.Bold))
                    {
                        if (AnythingVoice.parsedCommand != null && AnythingVoice.parsedCommand.result == CommandResult.Success)
                        {
                            Voice.CommandHandler.ParseCommand(AnythingVoice.parsedCommand);
                        }
                    }
                    GUI.enabled = true;
                }
            }
            else
            {
                GUI.enabled = !autoMake && AnythingVoice.parsedCommand != null && AnythingVoice.parsedCommand.actions.Count > 0;
                if (DrawRoundedButton("Make All", 1f, 8f, 1f, true, PoppinsStyle.Bold))
                {
                    if (AnythingVoice.parsedCommand != null && AnythingVoice.parsedCommand.result == CommandResult.Success)
                    {
                        Voice.CommandHandler.ParseCommand(AnythingVoice.parsedCommand);
                    }
                }
                GUI.enabled = true;

            }
            GUILayout.EndVertical();
        }

        private Rect DrawMicrophoneButton()
        {
            var buttonRect = GUILayoutUtility.GetRect(TintedButton.width, TintedButton.height);
            var activeStyle = new GUIStyle(iconStyle) { normal = SetStyleState(TintTexture(TintedButton, HexToColour("CCCCCC"))), hover = SetStyleState(TintTexture(TintedButton, HexToColour("BBBBBB"))) };
            var inactiveStyle = new GUIStyle(iconStyle) { normal = SetStyleState(TintedButton), hover = SetStyleState(TintTexture(TintedButton, HexToColour("CCCCCC"))) };

            GUI.enabled = !AnythingVoice.uploadInProgress;
            if (!AnythingVoice.isRecording)
            {
                if (GUI.Button(buttonRect, "", inactiveStyle))
                {
                    micActive = !micActive;
                    AnythingVoice.StartRecording();
                    actionFoldouts = null;
                }
            }
            else
            {
                if (GUI.Button(buttonRect, "", activeStyle))
                {
                    AnythingVoice.isRecording = false;
                    var clip = AnythingVoice.StopRecording();
                    AnythingVoice.ExtractBytesAndProcess(clip);

                }
            }
            GUI.enabled = true;
            return buttonRect;
        }

        private void DrawTextInputBox(Event e, float marginSize = 0.1f)
        {
            var fieldRect = GUILayoutUtility.GetRect(position.width, 30, InputFieldStyle);
            fieldRect.width = position.width - (position.width * (marginSize * 2));
            fieldRect.x = position.width * marginSize;

            var iconPadding = (fieldRect.height - BaseClearIcon.height) / 2f;
            var clearIconRect = new Rect(fieldRect.xMax - BaseClearIcon.width - iconPadding, fieldRect.y + iconPadding, BaseClearIcon.width, BaseClearIcon.height);

            var inputFieldLeftEdgeRect = new Rect(fieldRect.xMin, fieldRect.yMin, BaseInputFieldLeft.width, BaseInputFieldLeft.height);
            var inputFieldRightEdgeRect = new Rect(fieldRect.xMax - BaseInputFieldRight.width, fieldRect.yMin, BaseInputFieldRight.width, BaseInputFieldRight.height);
            var inputFieldMainRect = new Rect(inputFieldLeftEdgeRect.xMax, fieldRect.y, fieldRect.width - inputFieldLeftEdgeRect.width - inputFieldRightEdgeRect.width, BaseInputFieldMain.height);

            GUI.enabled = !(e.isMouse && clearIconRect.Contains(e.mousePosition));
            GUI.DrawTexture(inputFieldLeftEdgeRect, TintedInputFieldLeft);
            GUI.DrawTexture(inputFieldMainRect, TintedInputFieldMain);
            GUI.DrawTexture(inputFieldRightEdgeRect, TintedInputFieldRight);
            GUI.SetNextControlName("TextInput");
            writtenPrompt = GUI.TextField(fieldRect, writtenPrompt, InputFieldStyle);
            GUI.enabled = !AnythingVoice.uploadInProgress;

            if (Event.current.Equals(Event.KeyboardEvent("return")) && GUI.GetNameOfFocusedControl() == "TextInput")
            {
                SubmitStringCommand();
            }

            if (GUI.Button(clearIconRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateClearIcon.activeTexture), hover = SetStyleState(StateClearIcon.hoverTexture) }))
            {
                writtenPrompt = "";
            }
            if (string.IsNullOrEmpty(writtenPrompt)) GUI.Label(inputFieldMainRect, "Write what you want to create!", new GUIStyle(BodyLabelStyle) { font = GetPoppinsFont(PoppinsStyle.Bold), normal = SetStyleState(HexToColour("575859")) });
        }

        ParsedSpeechCommand foldoutCommand;
        List<bool> actionFoldouts;
        List<List<bool>> modelFoldouts;
        private void DrawOutputLog(float marginSize)
        {
            if (AnythingVoice.parsedCommand != null && AnythingVoice.parsedCommand.actions != null)
            {
                if (AnythingVoice.parsedCommand != foldoutCommand)
                {
                    actionFoldouts = new List<bool>();
                    modelFoldouts = new List<List<bool>>();
                    foreach (var action in AnythingVoice.parsedCommand.actions)
                    {

                        actionFoldouts.Add(false);
                        var temp = new List<bool>();
                        foreach (var model in action.models)
                        {
                            temp.Add(false);
                        }
                        modelFoldouts.Add(temp);
                    }
                    foldoutCommand = AnythingVoice.parsedCommand;
                }

                int actionIndent = 15;
                int textIndent = 15;
                int modelIndent = actionIndent + textIndent;
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(position.width * marginSize + textIndent);


                GUILayout.Label($"Text: {AnythingVoice.parsedCommand.text}", BodyLabelStyle);
                GUILayout.EndHorizontal();

                var actions = AnythingVoice.parsedCommand.actions.ToArray();
                //Foreach action in parsed actions draw a drop down
                for (int actionIndex = 0; actionIndex < actions.Length; actionIndex++)
                {
                    var action = actions[actionIndex];
                    var actionModels = action.models.ToArray();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(position.width * marginSize + actionIndent);
                    //Action foldout

                    actionFoldouts[actionIndex] = EditorGUILayout.Foldout(actionFoldouts[actionIndex], $"{GetHumanReadableAction(action.actionEnum)} ({action.models.Count})");

                    if (GUILayout.Button("x", GUILayout.Width(20)))
                    {
                        AnythingVoice.parsedCommand.actions.RemoveAt(actionIndex);
                        if (AnythingVoice.parsedCommand.actions.Count == 0) AnythingVoice.parsedCommand = null;
                    }



                    GUILayout.Space(5);
                    EditorGUILayout.EndHorizontal();
                    if (actionFoldouts[actionIndex])
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(position.width * marginSize + modelIndent);
                        
                        if (action.actionEnum == CommandSchema.ActionSchema.change_habitat)
                        {
                            GUILayout.Label($"Habitat: {action.habitat}", BodyLabelStyle);
                        }
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(5);

                        for (int modelIndex = 0; modelIndex < actionModels.Length; modelIndex++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(position.width * marginSize + modelIndent);
                            //Model foldout with label
                            modelFoldouts[actionIndex][modelIndex] = EditorGUILayout.Foldout(modelFoldouts[actionIndex][modelIndex], $" {action.models[modelIndex].name}");
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Make", GUILayout.ExpandWidth(false)))
                            {
                                if (AnythingVoice.parsedCommand != null && AnythingVoice.parsedCommand.result == CommandResult.Success)
                                {
                                    Voice.CommandHandler.ParseActionModel(AnythingVoice.parsedCommand.actions[actionIndex], AnythingVoice.parsedCommand.actions[actionIndex].models[modelIndex]);
                                }
                            }
                            if (GUILayout.Button("x", GUILayout.Width(20)))
                            {
                                if (AnythingVoice.parsedCommand.actions[actionIndex].models.Count > modelIndex)
                                {
                                    AnythingVoice.parsedCommand.actions[actionIndex].models.RemoveAt(modelIndex);
                                    if (AnythingVoice.parsedCommand.actions[actionIndex].models.Count == 0)
                                    {

                                        AnythingVoice.parsedCommand.actions.RemoveAt(actionIndex);
                                        if (AnythingVoice.parsedCommand.actions.Count == 0) AnythingVoice.parsedCommand = null;

                                    }
                                    modelFoldouts[actionIndex].RemoveAt(modelIndex);
                                    continue;
                                }
                                else
                                {
                                    Debug.LogWarning("Tried to remove out of range index." + modelIndex);
                                }

                            }
                            GUILayout.Space(5);
                            EditorGUILayout.EndHorizontal();

                            if (modelFoldouts[actionIndex][modelIndex])
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(position.width * marginSize + modelIndent + textIndent);
                                var model = action.models[modelIndex];
                                GUILayout.BeginVertical();

                                GUILayout.Label($"Quantity: {model.number}", BodyLabelStyle);
                                
                                if (model.location != null)
                                {
                                    GUILayout.Label($"Location: {model.location.relative} relative to {model.location.model_name}", BodyLabelStyle);
                                }
                                GUILayout.EndVertical();
                                GUILayout.EndHorizontal();
                            }

                        }
                        GUILayout.Space(5);
                    }


                }
            }
        }
        private string GetHumanReadableAction(CommandSchema.ActionSchema schema)
        {
            var stringSchema = schema.ToString();

            string returnStr = "";
            foreach (var str in schema.ToString().Split('_'))
            {
                returnStr = string.Join(" ", new[] { returnStr, (str[0].ToString().ToUpper() + str.Substring(1)) });
            }

            return returnStr;
        }

        private void DrawWaveForm(float waveIntensity)
        {
            var waveFormRect = GUILayoutUtility.GetRect(waveCount * (BaseWaveFormCore.width + wavePadding) + wavePadding, maxWaveHeight);

            for (int i = 0; i < waveCount; i++)
            {
                var minWaveHeight = BaseWaveFormBottomCap.height + BaseWaveFormTopCap.height;
                float localWaveHeight;
                float localWavePos;

                if (!processing)
                {
                    localWavePos = 0;

                    #region Double-Sided Sine
                    var positionFromMiddle = (i - Mathf.Floor(waveCount / 2));
                    //localWaveHeight = minWaveHeight + Mathf.Abs((waveIntensity * Mathf.Cos((Mathf.PI * positionFromMiddle) / waveCount)) * (maxWaveHeight - minWaveHeight));
                    #endregion Double-Sided Sine
                    #region Noise
                    localWaveHeight = minWaveHeight + (UnityEngine.Random.Range(0f, 1f) * ((maxWaveHeight - minWaveHeight) * waveIntensity));
                    #endregion Noise
                }
                else
                {
                    localWaveHeight = minWaveHeight;
                    localWavePos = Mathf.Sin(processingAnimationSpeed * (i + (float)EditorApplication.timeSinceStartup)) * ((minWaveHeight - maxWaveHeight) * processingAnimationAmplitude);
                }

                var waveRect = new Rect(waveFormRect.x + wavePadding + (i * (BaseWaveFormCore.width + wavePadding)), waveFormRect.yMin + ((waveFormRect.height - localWaveHeight) / 2) + localWavePos, BaseWaveFormCore.width, localWaveHeight);

                var waveTopCapRect = new Rect(waveRect.xMin, processing ? (waveRect.center.y - BaseWaveFormTopCap.height) : waveRect.yMin, BaseWaveFormTopCap.width, BaseWaveFormTopCap.height);
                var waveBottomCapRect = new Rect(waveRect.xMin, processing ? waveRect.center.y : (waveRect.yMax - BaseWaveFormBottomCap.height), BaseWaveFormBottomCap.width, BaseWaveFormBottomCap.height);
                var waveCoreRect = new Rect(waveRect.xMin, waveTopCapRect.yMax, BaseWaveFormCore.width, waveRect.height - waveBottomCapRect.height - waveTopCapRect.height + 2);

                GUI.DrawTexture(waveTopCapRect, TintedWaveFormTopCap);
                GUI.DrawTexture(waveBottomCapRect, TintedWaveFormBottomCap);
                GUI.DrawTexture(waveCoreRect, TintedWaveFormCore, ScaleMode.StretchToFill);
            }
        }
        #endregion Editor Drawing
    }
}
