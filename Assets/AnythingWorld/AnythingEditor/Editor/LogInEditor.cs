using AnythingWorld.Networking.Editor;
using AnythingWorld.Utilities;

using System;

using UnityEditor;
using UnityEngine;

namespace AnythingWorld.Editor
{
    public class LogInEditor : AnythingCreatorEditor
    {
        #region Fields

        private enum LoginTabCategory
        {
            LOGIN,
            SIGNUP
        }

        private LoginTabCategory loginTabCategory;

        private string loginEmailField = "";
        private string loginPasswordField = "";

        private string signupEmailField = "";
        private string signupFullNameField = "";
        private string signupPasswordField = "";
        private string signupPasswordCheckField = "";
        private bool signupTermsAccepted = false;

        private bool showPassword = false;
        private bool showPasswordCheck = false;
        private bool successfulLogin = false;
        private string loginErrorMessage;

        #region Textures

        #region Base Textures
        private static Texture2D baseLoginTabIcon;
        protected static Texture2D BaseLoginTabIcon
        {
            get
            {
                if (baseLoginTabIcon == null)
                {
                    baseLoginTabIcon = Resources.Load("Editor/SettingsPanel/Icons/loginTab") as Texture2D;
                }
                return baseLoginTabIcon;
            }
        }
        private static Texture2D baseSignupTabIcon;
        protected static Texture2D BaseSignupTabIcon
        {
            get
            {
                if (baseSignupTabIcon == null)
                {
                    baseSignupTabIcon = Resources.Load("Editor/SettingsPanel/Icons/signupTab") as Texture2D;
                }

                return baseSignupTabIcon;
            }
        }
        private static Texture2D baseLoginLabelIcon;
        protected static Texture2D BaseLoginLabelIcon
        {
            get
            {
                if (baseLoginLabelIcon == null)
                {
                    baseLoginLabelIcon = Resources.Load("Editor/SettingsPanel/loginLabel") as Texture2D;
                }
                return baseLoginLabelIcon;
            }
        }
        private static Texture2D baseSignupLabelIcon;
        protected static Texture2D BaseSignupLabelIcon
        {
            get
            {
                if (baseSignupLabelIcon == null)
                {
                    baseSignupLabelIcon = Resources.Load("Editor/SettingsPanel/signupLabel") as Texture2D;
                }
                return baseSignupLabelIcon;
            }
        }
        private static Texture2D baseConfirmButton;
        protected static Texture2D BaseConfirmButton
        {
            get
            {
                if (baseConfirmButton == null)
                {
                    baseConfirmButton = Resources.Load("Editor/SettingsPanel/button") as Texture2D;
                }
                return baseConfirmButton;
            }
        }
        private static Texture2D baseHidePasswordIcon;
        protected static Texture2D BaseHidePasswordIcon
        {
            get
            {
                if (baseHidePasswordIcon == null)
                {
                    baseHidePasswordIcon = Resources.Load("Editor/SettingsPanel/Icons/hidePassword") as Texture2D;
                }
                return baseHidePasswordIcon;
            }
        }
        private static Texture2D baseShowPasswordIcon;
        protected static Texture2D BaseShowPasswordIcon
        {
            get
            {
                if (baseShowPasswordIcon == null)
                {
                    baseShowPasswordIcon = Resources.Load("Editor/SettingsPanel/Icons/showPassword") as Texture2D;
                }

                return baseShowPasswordIcon;
            }
        }
        #endregion Base Textures

        #region Tinted Textures
        private Texture2D tintedLoginLabelIcon;
        protected Texture2D TintedLoginLabelIcon
        {
            get
            {
                if (tintedLoginLabelIcon == null)
                {
                    tintedLoginLabelIcon = TintTextureToEditorTheme(BaseLoginLabelIcon, Color.white, Color.black);
                }

                return tintedLoginLabelIcon;
            }
            set => tintedLoginLabelIcon = value;
        }
        private Texture2D tintedSignupLabelIcon;
        protected Texture2D TintedSignupLabelIcon
        {
            get
            {
                if (tintedSignupLabelIcon == null)
                {
                    tintedSignupLabelIcon = TintTextureToEditorTheme(BaseSignupLabelIcon, Color.white, Color.black);
                }

                return tintedSignupLabelIcon;
            }
            set => tintedSignupLabelIcon = value;
        }
        #endregion Tinted Textures

        #region State Textures
        private StateTexture2D stateLoginTabIcon;
        protected StateTexture2D StateLoginTabIcon
        {
            get
            {
                if (stateLoginTabIcon == null || !stateLoginTabIcon.TexturesLoadedNoHover)
                {
                    stateLoginTabIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseLoginTabIcon, Color.black, Color.white),
                        TintTextureToEditorTheme(BaseLoginTabIcon, Color.white, Color.black));
                }
                return stateLoginTabIcon;
            }
            set => stateLoginTabIcon = value;
        }
        private StateTexture2D stateSignupTabIcon;
        protected StateTexture2D StateSignupTabIcon
        {
            get
            {
                if (stateSignupTabIcon == null || !stateSignupTabIcon.TexturesLoadedNoHover)
                {
                    stateSignupTabIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseSignupTabIcon, Color.black, Color.white),
                        TintTextureToEditorTheme(BaseSignupTabIcon, Color.white, Color.black));
                }
                return stateSignupTabIcon;
            }
            set => stateSignupTabIcon = value;
        }
        private StateTexture2D stateConfirmButton;
        protected StateTexture2D StateConfirmButton
        {
            get
            {
                if (stateConfirmButton == null || !stateConfirmButton.TexturesLoadedHover)
                {
                    stateConfirmButton = new StateTexture2D(
                        TintTextureToEditorTheme(BaseConfirmButton, HexToColour("98999A"), HexToColour("979797")),
                        TintTextureToEditorTheme(BaseConfirmButton, HexToColour("575859"), HexToColour("E4E5E3")),
                        TintTextureToEditorTheme(BaseConfirmButton, HexToColour("606162"), HexToColour("EDEEEC")));
                }
                return stateConfirmButton;
            }
            set => stateConfirmButton = value;
        }
        private StateTexture2D statePasswordVisibilityIcon;
        protected StateTexture2D StatePasswordVisibilityIcon
        {
            get
            {
                if (statePasswordVisibilityIcon == null || statePasswordVisibilityIcon.TexturesLoadedNoHover)
                {
                    statePasswordVisibilityIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseShowPasswordIcon),
                        TintTextureToEditorTheme(BaseHidePasswordIcon));
                }
                return statePasswordVisibilityIcon;
            }
            set => statePasswordVisibilityIcon = value;
        }
        #endregion State Textures
        #endregion Textures
        protected GUIStyle ConfirmButtonStyle;
        #endregion Fields

        protected new void Awake()
        {
            base.Awake();
            windowTitle = "Settings";
            bannerTintA = HexToColour("FFF500");
            bannerTintB = HexToColour("FF4DEE");
        }

        protected override void DefineCustomStyles()
        {
            base.DefineCustomStyles();
            
            ConfirmButtonStyle = new GUIStyle(ButtonStyle)
            {
                normal =
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black,
                    background = StateConfirmButton.inactiveTexture
                },
                hover =
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black,
                    background = StateConfirmButton.hoverTexture
                },
                active =
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.black : Color.white,
                    background = StateConfirmButton.activeTexture
                },
                fixedHeight = 25
            };
        }

        protected new void OnGUI()
        {
            base.OnGUI();

            try
            {
                EditorGUILayout.BeginVertical();
                DrawLogin();
                EditorGUILayout.EndVertical();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void DrawLogin()
        {
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Log In", loginTabCategory == LoginTabCategory.LOGIN ? StateLoginTabIcon.activeTexture : StateLoginTabIcon.inactiveTexture), loginTabCategory == LoginTabCategory.LOGIN ? TabButtonActiveStyle : TabButtonInactiveStyle, GUILayout.MaxWidth(position.width / 2)))
            {
                ResetDetails();
                loginTabCategory = LoginTabCategory.LOGIN;
            }
            if (GUILayout.Button(new GUIContent("Sign Up", loginTabCategory == LoginTabCategory.SIGNUP ? StateSignupTabIcon.activeTexture : StateSignupTabIcon.inactiveTexture), loginTabCategory == LoginTabCategory.SIGNUP ? TabButtonActiveStyle : TabButtonInactiveStyle, GUILayout.MaxWidth(position.width / 2)))
            {
                ResetDetails();
                loginTabCategory = LoginTabCategory.SIGNUP;
            }
            EditorGUILayout.EndHorizontal();
            if (successfulLogin)
            {
                var signedInText = new GUIContent($"You've successfully {(loginTabCategory == LoginTabCategory.LOGIN ? "logged in" : "signed up")}!");
                var textHeight = HeaderLabelStyle.CalcHeight(signedInText, position.width);

                var labelRect = GUILayoutUtility.GetRect(position.width, textHeight);
                GUI.Label(labelRect, signedInText, new GUIStyle(HeaderLabelStyle) { wordWrap = true });
            }
            else
            {
                DrawHeaderLabel();
                if (loginTabCategory == LoginTabCategory.LOGIN) DrawLoginDetails();
                else if (loginTabCategory == LoginTabCategory.SIGNUP) DrawSignupDetails();

                if (!string.IsNullOrEmpty(loginErrorMessage))
                {
                    var errorRect = GUILayoutUtility.GetRect(new GUIContent(loginErrorMessage), BodyLabelStyle);
                    GUI.Label(errorRect, loginErrorMessage, new GUIStyle(BodyLabelStyle) { alignment = TextAnchor.MiddleCenter, normal = SetStyleState(Color.red) });
                }

                var confirmRect = GUILayoutUtility.GetRect(BaseConfirmButton.width, BaseConfirmButton.height);
                confirmRect.width = BaseConfirmButton.width;
                confirmRect.x = (position.width / 2) - (BaseConfirmButton.width / 2);

                if (GUI.Button(confirmRect, loginTabCategory == LoginTabCategory.LOGIN ? "Login" : "Sign Up", ConfirmButtonStyle) || Event.current.keyCode == KeyCode.Return)
                {
                    if (loginTabCategory == LoginTabCategory.LOGIN) SignupLoginProcessor.LogIn(loginEmailField, loginPasswordField, SignupLoginErrorHandler, LoginSuccessHandler, this);
                    else if (loginTabCategory == LoginTabCategory.SIGNUP) SignupLoginProcessor.SignUp(signupEmailField, signupPasswordField, signupPasswordCheckField, signupFullNameField, signupTermsAccepted, SignupLoginErrorHandler, LoginSuccessHandler, this);
                }
            }
        }

        private void DrawHeaderLabel()
        {
            var labelContent = new GUIContent(loginTabCategory == LoginTabCategory.LOGIN ? "Log In" : "Sign Up for API Key",
                                              loginTabCategory == LoginTabCategory.LOGIN ? TintedLoginLabelIcon : TintedSignupLabelIcon);

            var headerRect = GUILayoutUtility.GetRect(position.width, 50f);
            var labelWidth = HeaderLabelStyle.CalcSize(labelContent).x;
            var labelRect = new Rect(headerRect.center.x - (labelWidth / 2), headerRect.y, labelWidth, headerRect.height);
            GUI.Label(labelRect, labelContent, new GUIStyle(HeaderLabelStyle) { normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black } });
        }

        private void DrawLoginDetails()
        {
            Event e = Event.current;
            DrawInputField("Email", e, ref loginEmailField);
            DrawPasswordField("Password", e, ref loginPasswordField, ref showPassword);
        }

        private void DrawSignupDetails()
        {
            Event e = Event.current;
            DrawInputField("Email", e, ref signupEmailField);
            DrawInputField("Full Name", e, ref signupFullNameField);
            DrawPasswordField("Password", e, ref signupPasswordField, ref showPassword);
            DrawPasswordField("Confirm Password", e, ref signupPasswordCheckField, ref showPasswordCheck);
            DrawToggleFieldWithLink("I have read and agree with the terms in the user agreement.", "https://anything.world/terms-of-use", ref signupTermsAccepted, 0.115f);
        }

        private void DrawInputField(string title, Event e, ref string fieldToSet, float marginSize = 0.1f )
        {
            GUI.SetNextControlName(title);

            var titleHeight = BodyLabelStyle.CalcSize(new GUIContent(title)).y;

            var titleRect = GUILayoutUtility.GetRect(position.width, titleHeight, BodyLabelStyle, GUILayout.Height(titleHeight));
            titleRect.width = position.width - (position.width * (marginSize * 2));
            titleRect.x = position.width * marginSize;
            GUI.Label(titleRect, new GUIContent(title), new GUIStyle(BodyLabelStyle) { fontSize = 12, margin = new RectOffset(10, 10, 0, 0), padding = new RectOffset(10, 10, 0, 0), contentOffset = new Vector2(6, 0) });

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
            fieldToSet = GUI.TextField(fieldRect, fieldToSet, InputFieldStyle);
            GUI.enabled = true;

            if (GUI.Button(clearIconRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateClearIcon.activeTexture), hover = SetStyleState(StateClearIcon.hoverTexture) }))
            {
                fieldToSet = "";
            }
        }

        private void DrawPasswordField(string title, Event e, ref string fieldToSet, ref bool showPassword, float marginSize = 0.1f)
        {
            GUI.SetNextControlName(title);

            var titleHeight = BodyLabelStyle.CalcSize(new GUIContent(title)).y;

            var titleRect = GUILayoutUtility.GetRect(position.width, titleHeight, BodyLabelStyle, GUILayout.Height(titleHeight));
            titleRect.width = position.width - (position.width * (marginSize * 2));
            titleRect.x = position.width * marginSize;
            GUI.Label(titleRect, new GUIContent(title), new GUIStyle(BodyLabelStyle) { fontSize = 12, margin = new RectOffset(10, 10, 0, 0), padding = new RectOffset(10, 10, 0, 0), contentOffset = new Vector2(6, 0) });

            var fieldRect = GUILayoutUtility.GetRect(position.width, 30, InputFieldStyle);
            fieldRect.width = position.width - (position.width * (marginSize * 2));
            fieldRect.x = position.width * marginSize;

            var iconPadding = (fieldRect.height - BaseClearIcon.height) / 2f;

            var clearIconRect = new Rect(fieldRect.xMax - BaseClearIcon.width - iconPadding, fieldRect.y + iconPadding, BaseClearIcon.width, BaseClearIcon.height);

            var inputFieldLeftEdgeRect = new Rect(fieldRect.xMin, fieldRect.yMin, BaseInputFieldLeft.width, BaseInputFieldLeft.height);
            var inputFieldRightEdgeRect = new Rect(fieldRect.xMax - BaseInputFieldRight.width, fieldRect.yMin, BaseInputFieldRight.width, BaseInputFieldRight.height);
            var inputFieldMainRect = new Rect(inputFieldLeftEdgeRect.xMax, fieldRect.y, fieldRect.width - inputFieldLeftEdgeRect.width - inputFieldRightEdgeRect.width, BaseInputFieldMain.height);

            var passwordVisibilityWidth = showPassword ? BaseShowPasswordIcon.width : BaseHidePasswordIcon.width;
            var passwordVisibilityHeight = showPassword ? BaseShowPasswordIcon.height : BaseHidePasswordIcon.height;

            var passwordVisibilityRect = new Rect(fieldRect.xMax + iconPadding, fieldRect.y + ((fieldRect.height - passwordVisibilityHeight) / 2f), passwordVisibilityWidth, passwordVisibilityHeight);

            if (GUI.Button(passwordVisibilityRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(showPassword ? StatePasswordVisibilityIcon.activeTexture : StatePasswordVisibilityIcon.inactiveTexture) }))
            {
                showPassword = !showPassword;
            }

            GUI.enabled = !(e.isMouse && clearIconRect.Contains(e.mousePosition));
            GUI.DrawTexture(inputFieldLeftEdgeRect, TintedInputFieldLeft);
            GUI.DrawTexture(inputFieldMainRect, TintedInputFieldMain);
            GUI.DrawTexture(inputFieldRightEdgeRect, TintedInputFieldRight);
            if (showPassword) fieldToSet = GUI.TextField(fieldRect, fieldToSet, InputFieldStyle);
            else fieldToSet = GUI.PasswordField(fieldRect, fieldToSet, '*', InputFieldStyle);
            GUI.enabled = true;

            if (GUI.Button(clearIconRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateClearIcon.activeTexture), hover = SetStyleState(StateClearIcon.hoverTexture) }))
            {
                fieldToSet = "";
            }
        }

        private void DrawToggleFieldWithLink(string label, string link, ref bool boolToSet, float marginSize = 0.1f)
        {
            GUI.SetNextControlName(label);

            var labelHeight = BodyLabelStyle.CalcSize(new GUIContent(label)).y;
            GUILayoutUtility.GetRect(position.width, labelHeight / 2, BodyLabelStyle, GUILayout.Height(labelHeight));

            var toggleRect = GUILayoutUtility.GetRect(position.width, 30, ToggleStyle);
            toggleRect.width = position.width - (position.width * (marginSize * 2));
            toggleRect.x = position.width * marginSize;

            var toggleBoxRect = new Rect(toggleRect.x, toggleRect.y, toggleRect.height, toggleRect.height);
            boolToSet = GUI.Toggle(toggleBoxRect, boolToSet, "");

            var labelRect = new Rect(toggleBoxRect.xMax, toggleRect.y, toggleRect.width - toggleBoxRect.width, toggleRect.height);
            if (GUI.Button(labelRect, label, new GUIStyle(BodyLabelStyle)
            {
                font = GetPoppinsFont(PoppinsStyle.Regular),
                fontSize = 12,
                normal = SetStyleState(EditorGUIUtility.isProSkin ? HexToColour("7588FF") : HexToColour("0E2EFF")),
                focused = SetStyleState(EditorGUIUtility.isProSkin ? HexToColour("B987E8") : HexToColour("551A8B")),
                hover = SetStyleState(EditorGUIUtility.isProSkin ? HexToColour("B987E8") : HexToColour("551A8B")),
                active = SetStyleState(EditorGUIUtility.isProSkin ? HexToColour("B987E8") : HexToColour("551A8B"))
            }))
            {
                Application.OpenURL(link);
            }

            GUILayoutUtility.GetRect(position.width, labelHeight / 2, BodyLabelStyle, GUILayout.Height(labelHeight));
        }
        #region Helper Functions
        private void ResetDetails()
        {
            loginEmailField = "";
            loginPasswordField = "";

            signupEmailField = "";
            signupFullNameField = "";
            signupPasswordField = "";
            signupPasswordCheckField = "";
            signupTermsAccepted = false;
            loginErrorMessage = "";
            showPassword = false;
            showPasswordCheck = false;
        }
     
        private void SignupLoginErrorHandler(SignupLoginProcessor.SignupLoginError signupError)
        {
            loginErrorMessage = $"{signupError.message}";
            Repaint();
        }

        private void LoginSuccessHandler()
        {
            loginErrorMessage = "";
            //UAS
            AnythingWorld.Utilities.Editor.VSAttribution.SendAttributionEvent("Login", "AnythingWorld", AnythingSettings.APIKey);
            CoroutineExtension.StartEditorCoroutine(CoroutineExtension.WaitThen(1f, PresentSuccess, RestartAnythingWorld), this);
        }

        private void PresentSuccess()
        {
            successfulLogin = true;
            Repaint();
        }

        private void RestartAnythingWorld()
        {
            ModelBrowserEditor.Initialize(position);
            Close();
        }
        #endregion Helper Functions
    }
}
