using AnythingWorld.Networking;
using AnythingWorld.Networking.Editor;
using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace AnythingWorld.Editor
{
    public class ModelBrowserEditor : AnythingCreatorEditor
    {
        #region Fields
        private CreationSearchCategory selectedSearchCategory;

        private string searchBarInput = "";
        private string searchTerm;

        #region Textures
        #region Search Bar
        private static Texture2D baseSearchIcon;
        protected static Texture2D BaseSearchIcon
        {
            get
            {
                if (baseSearchIcon == null)
                {
                    baseSearchIcon = Resources.Load("Editor/AnythingBrowser/Icons/search") as Texture2D;
                }

                return baseSearchIcon;
            }
        }
        #endregion Search Bar
        #region Category Icons
        private static Texture2D baseModelsCategoryIcon;
        protected static Texture2D BaseModelsCategoryIcon
        {
            get
            {
                if (baseModelsCategoryIcon == null)
                {
                    baseModelsCategoryIcon = Resources.Load("Editor/AnythingBrowser/Icons/modelsCategory") as Texture2D;
                }

                return baseModelsCategoryIcon;
            }
        }

        private static Texture2D baseWorldsCategoryIcon;
        protected static Texture2D BaseWorldsCategoryIcon
        {
            get
            {
                if (baseWorldsCategoryIcon == null)
                {
                    baseWorldsCategoryIcon = Resources.Load("Editor/AnythingBrowser/Icons/worldsCategory") as Texture2D;
                }

                return baseWorldsCategoryIcon;
            }
        }

        private static Texture2D baseLightingCategoryIcon;
        protected static Texture2D BaseLightingCategoryIcon
        {
            get
            {
                if (baseLightingCategoryIcon == null)
                {
                    baseLightingCategoryIcon =
                        Resources.Load("Editor/AnythingBrowser/Icons/lightingCategory") as Texture2D;
                }

                return baseLightingCategoryIcon;
            }
        }
        #endregion Category Icons
        #region Settings Icons
        private static Texture2D baseFilterIcon;
        protected static Texture2D BaseFilterIcon
        {
            get
            {
                if (baseFilterIcon == null)
                {
                    baseFilterIcon = Resources.Load("Editor/AnythingBrowser/Icons/SettingsIcons/filter") as Texture2D;
                }

                return baseFilterIcon;
            }
        }

        private static Texture2D baseMicIcon;
        protected static Texture2D BaseMicIcon
        {
            get
            {
                if (baseMicIcon == null)
                {
                    baseMicIcon = Resources.Load("Editor/AnythingBrowser/Icons/SettingsIcons/mic") as Texture2D;
                }

                return baseMicIcon;
            }
        }
        #endregion Settings Icons

        private Texture2D tintedSearchIcon;
        protected Texture2D TintedSearchIcon
        {
            get
            {
                if (tintedSearchIcon == null)
                {
                    tintedSearchIcon = TintTextureToEditorTheme(BaseSearchIcon, Color.white, Color.black);
                }

                return tintedSearchIcon;
            }
            set => tintedSearchIcon = value;
        }

        private StateTexture2D stateModelsCategoryIcon;
        protected StateTexture2D StateModelsCategoryIcon
        {
            get
            {
                if (stateModelsCategoryIcon == null || !stateModelsCategoryIcon.TexturesLoadedNoHover)
                {
                    stateModelsCategoryIcon = new StateTexture2D(TintTextureToEditorTheme(BaseModelsCategoryIcon, Color.black, Color.white), TintTextureToEditorTheme(BaseModelsCategoryIcon, Color.white, Color.black));
                }
                return stateModelsCategoryIcon;
            }
            set => stateModelsCategoryIcon = value;
        }
        private StateTexture2D stateWorldsCategoryIcon;
        protected StateTexture2D StateWorldsCategoryIcon
        {
            get
            {
                if (stateWorldsCategoryIcon == null || !stateWorldsCategoryIcon.TexturesLoadedNoHover)
                {
                    stateWorldsCategoryIcon = new StateTexture2D(TintTextureToEditorTheme(BaseWorldsCategoryIcon, Color.black, Color.white), TintTextureToEditorTheme(BaseWorldsCategoryIcon, Color.white, Color.black));
                }
                return stateWorldsCategoryIcon;
            }
            set => stateWorldsCategoryIcon = value;
        }
        private StateTexture2D stateLightingCategoryIcon;
        protected StateTexture2D StateLightingCategoryIcon
        {
            get
            {
                if (stateLightingCategoryIcon == null || !stateLightingCategoryIcon.TexturesLoadedNoHover)
                {
                    stateLightingCategoryIcon = new StateTexture2D(TintTextureToEditorTheme(BaseLightingCategoryIcon, Color.black, Color.white), TintTextureToEditorTheme(BaseLightingCategoryIcon, Color.white, Color.black));
                }
                return stateLightingCategoryIcon;
            }
            set => stateLightingCategoryIcon = value;
        }
        #endregion Textures
        #region Styles
        protected GUIStyle SearchBarStyle;
        #endregion Styles
        #region Colours

        #endregion Colours
        #endregion Fields

        #region Initialization
        /// <summary>
        /// Initializes and shows window, called from Anything World top bar menu.
        /// </summary>
        [MenuItem("Anything World/Anything Browser", false, 1)]
        internal static void Initialize()
        {
            AnythingCreatorEditor tabWindow;
            Vector2 windowSize;
            if (AnythingSettings.HasAPIKey)
            {
                windowSize = new Vector2(500, 800);
                CloseWindowIfOpen<ModelBrowserEditor>();
                var browser = GetWindow(typeof(ModelBrowserEditor), false, "Anything Browser");
                browser.position = new Rect(EditorGUIUtility.GetMainWindowPosition().center - windowSize / 2, windowSize);
                browser.Show();
                browser.Focus();

                CloseWindowIfOpen<MyWorldEditor>();
                GetWindow<MyWorldEditor>("My World", false, typeof(ModelBrowserEditor));

                browser.Focus();
            }
            else
            {
                CloseWindowIfOpen<ModelBrowserEditor>();
                CloseWindowIfOpen<MyWorldEditor>();
                CloseWindowIfOpen<AICreatorEditor>();
                windowSize = new Vector2(450, 800);
                tabWindow = GetWindow<LogInEditor>("Log In | Sign Up", false, typeof(ModelBrowserEditor));
                tabWindow.position = new Rect(EditorGUIUtility.GetMainWindowPosition().center - windowSize / 2, windowSize);
            }
        }

        internal static void Initialize(Rect windowPosition)
        {
            AnythingCreatorEditor tabWindow;
            Vector2 windowSize;
            Vector2 windowDiff;

            if (AnythingSettings.HasAPIKey)
            {
                windowSize = new Vector2(500, 1200);
                windowDiff = windowPosition.size - windowSize;
                CloseWindowIfOpen<ModelBrowserEditor>();
                var browser = GetWindow(typeof(ModelBrowserEditor), false, "Anything Browser");
                browser.position = new Rect(windowPosition.position + (windowDiff / 2), windowSize);
                browser.Show();
                browser.Focus();

                CloseWindowIfOpen<MyWorldEditor>();
                GetWindow<MyWorldEditor>("My World", false, typeof(ModelBrowserEditor));

                browser.Focus();
            }
            else
            {
                windowSize = new Vector2(450, 800);
                windowDiff = windowPosition.size - windowSize;

                tabWindow = GetWindow<LogInEditor>("Log In | Sign Up", false, typeof(ModelBrowserEditor));
                tabWindow.position = new Rect(windowPosition.position + (windowDiff / 2), windowSize);
            }
        }

        protected new void Awake()
        {
            base.Awake();
            windowTitle = "Browser";
            bannerTintA = HexToColour("00FDFF");
            bannerTintB = HexToColour("FF00E7");
            searchResults = new List<SearchResult>();
            filteredResults = new List<SearchResult>();

            OnFeatured();
        }
        protected override void DefineCustomStyles()
        {
            base.DefineCustomStyles();

            SearchBarStyle = new GUIStyle(InputFieldStyle)
            {
                contentOffset = new Vector2(32, 0)
            };
        }

        #endregion Initialisation
        
        #region Editor Drawing
        protected new void OnGUI()
        {
            base.OnGUI();
            if (Event.current.type == EventType.Repaint && !AnythingSettings.HasAPIKey) Close();
            #region Overwriting Editor Styles
            var backupLabelStyle = new GUIStyle(EditorStyles.label);
            var backupObjectStyle = new GUIStyle(EditorStyles.objectField);
            var backupNumberStyle = new GUIStyle(EditorStyles.numberField);

            EditorStyles.label.font = GetPoppinsFont(PoppinsStyle.Bold);
            EditorStyles.objectField.font = GetPoppinsFont(PoppinsStyle.Medium);
            EditorStyles.numberField.font = GetPoppinsFont(PoppinsStyle.Medium);
            #endregion Overwriting Editor Styles

            if (!EditorGUIUtility.wideMode)
            {
                EditorGUIUtility.wideMode = true;
                EditorGUIUtility.labelWidth = 170;
            }

            try
            {
                EditorGUILayout.BeginVertical();
                if (selectedResult == null || string.IsNullOrEmpty(selectedResult.data._id)) DrawBrowser(); //BUG: Investigate why result is set on reload
                else DrawDetails();
                EditorGUILayout.EndVertical();

                #region Resetting Editor Styles
                EditorStyles.label.font = backupLabelStyle.font;
                EditorStyles.objectField.font = backupObjectStyle.font;
                EditorStyles.numberField.font = backupNumberStyle.font;
                #endregion Resetting Editor Styles
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                #region Resetting Editor Styles
                EditorStyles.label.font = backupLabelStyle.font;
                EditorStyles.objectField.font = backupObjectStyle.font;
                EditorStyles.numberField.font = backupNumberStyle.font;
                #endregion Resetting Editor Styles
            }

            if (searchMode == SearchMode.RUNNING)
            {
                Repaint();
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
        }
        private void DrawBrowser()
        {
            DrawSearchBar();
            DrawFilters();
            DrawSettingsIcons();
            GUILayout.Space(16);
            DrawBrowserContent();
        }

        Rect miscRect;

        private void DrawBrowserContent()
        {
            var tempRect = GUILayoutUtility.GetLastRect();
            if (tempRect != new Rect(0, 0, 1, 1)) miscRect = tempRect;

            switch (searchMode)
            {
                case SearchMode.IDLE:
                    DrawLandingPage();
                    break;
                case SearchMode.RUNNING:
                    DrawLoading(miscRect);
                    break;
                case SearchMode.SUCCESS:
                    if (filteredResults.Count > 0)
                    {
                        DrawGrid(filteredResults, filteredResults.Count, 100, 120, DrawBrowserCard, resultThumbnailMultiplier);
                    }
                    else
                    {
                        EmptyResults(searchTerm);
                    }

                    break;
                case SearchMode.FAILURE:
                    DrawError();
                    break;
            }
        }

        private void DrawLandingPage()
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(EditorGUIUtility.isProSkin ? BaseAnythingGlobeLogo : BlackAnythingGlobeLogo, new GUIStyle(), GUILayout.MaxWidth(100), GUILayout.MaxHeight(100));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Try searching for a model... eg. \"cat\", \"flower\", \"car\"  ", new GUIStyle(BodyLabelStyle) { alignment = TextAnchor.MiddleCenter });
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }
        protected void EmptyResults(string searchTerm)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Sorry, no results found for \"{searchTerm}\".", BodyLabelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawSearchBar()
        {
            GUILayout.Space(16);

            if (Event.current.Equals(Event.KeyboardEvent("return")))
            {
                OnSearch(searchBarInput);
            }

            GUI.SetNextControlName("SearchBar");
            Event e = Event.current;

            var searchBarRect = GUILayoutUtility.GetRect(position.width, 30, SearchBarStyle);
            var iconPadding = (searchBarRect.height - TintedSearchIcon.height) / 2f;

            var searchIconRect = new Rect(searchBarRect.xMin + iconPadding, searchBarRect.yMin + iconPadding, TintedSearchIcon.width, TintedSearchIcon.height);
            var clearIconRect = new Rect(searchBarRect.xMax - BaseClearIcon.width - iconPadding, searchBarRect.y + iconPadding, BaseClearIcon.width, BaseClearIcon.height);

            var searchBarLeftEdgeRect = new Rect(searchBarRect.xMin, searchBarRect.yMin, BaseInputFieldLeft.width, BaseInputFieldLeft.height);
            var searchBarRightEdgeRect = new Rect(searchBarRect.xMax - BaseInputFieldRight.width, searchBarRect.yMin, BaseInputFieldRight.width, BaseInputFieldRight.height);
            var searchBarMainRect = new Rect(searchBarLeftEdgeRect.xMax, searchBarRect.y, searchBarRect.width - searchBarLeftEdgeRect.width - searchBarRightEdgeRect.width, BaseInputFieldMain.height);

            GUI.enabled = !(e.isMouse && clearIconRect.Contains(e.mousePosition));
            GUI.DrawTexture(searchBarLeftEdgeRect, TintedInputFieldLeft);
            GUI.DrawTexture(searchBarMainRect, TintedInputFieldMain);
            GUI.DrawTexture(searchBarRightEdgeRect, TintedInputFieldRight);
            searchBarInput = GUI.TextField(searchBarRect, searchBarInput, SearchBarStyle);
            GUI.DrawTexture(searchIconRect, TintedSearchIcon);
            GUI.enabled = true;

            if (GUI.Button(clearIconRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateClearIcon.activeTexture), hover = SetStyleState(StateClearIcon.hoverTexture) }))
            {
                searchBarInput = "";
            }
            if (searchMode == SearchMode.SUCCESS) DrawSearchOptionsBar(searchBarRect);
        }
        private void DrawSearchOptionsBar(Rect searchBarSize)
        {
            var optionsRect = new Rect(searchBarSize.x, searchBarSize.yMax + 8, searchBarSize.width, 20);

            var resultsLabelRect = new Rect(optionsRect.x, optionsRect.y, optionsRect.width / 2, optionsRect.height);
            if (filteredResults != null)
            {
                var pluraliser = "s";
                if (filteredResults.Count == 1) pluraliser = "";
                GUI.Label(resultsLabelRect, $"{filteredResults.Count} result{pluraliser}", VoteStyle);
            }

            var roundedScale = Mathf.RoundToInt(resultThumbnailMultiplier * 10) / 10f;
            var thumbnailMultiplierLabelString = $"{roundedScale}x";
            var thumbnailMultiplierLabelSize = Mathf.Max(BodyLabelStyle.CalcSize(new GUIContent(thumbnailMultiplierLabelString)).x, 30f);

            var thumbnailSizingRect = new Rect(resultsLabelRect.xMax, optionsRect.y, optionsRect.width / 2, optionsRect.height);
            var padding = (thumbnailSizingRect.height - TintedThumbnailGridIcon.height) / 2;
            var thumbnailMultiplierLabelRect = new Rect(thumbnailSizingRect.xMax - thumbnailMultiplierLabelSize, thumbnailSizingRect.y, thumbnailMultiplierLabelSize, thumbnailSizingRect.height);

            var thumbnailMultiplierSliderWidth = 100;
            var thumbnailMultiplierSliderHeight = thumbnailSizingRect.height - 20;
            var thumbnailMultiplierSliderRect = new Rect(thumbnailMultiplierLabelRect.x - thumbnailMultiplierSliderWidth - padding, thumbnailSizingRect.y + (thumbnailMultiplierSliderHeight / 2), thumbnailMultiplierSliderWidth, thumbnailMultiplierSliderHeight);

            var thumbnailMultiplierButtonRect = new Rect(thumbnailMultiplierSliderRect.x - TintedThumbnailGridIcon.width - padding, thumbnailSizingRect.y + padding, TintedThumbnailGridIcon.width, thumbnailSizingRect.height);

            if (GUI.Button(thumbnailMultiplierButtonRect, TintedThumbnailGridIcon, iconStyle))
            {
                resultThumbnailMultiplier = 1f;
            }

            resultThumbnailMultiplier = GUI.HorizontalSlider(thumbnailMultiplierSliderRect, resultThumbnailMultiplier, 0.5f, 2.5f);
            GUI.Label(thumbnailMultiplierLabelRect, thumbnailMultiplierLabelString, new GUIStyle(BodyLabelStyle) { alignment = TextAnchor.MiddleCenter });

            GUILayoutUtility.GetRect(position.width, optionsRect.height, GUILayout.MinWidth(500));
            GUILayout.Space(20);
        }
        #endregion Editor Drawing

        #region Mechanics
        private void OnSearch(string termToSearch)
        {
            searchTerm = termToSearch;
            searchResults = new List<SearchResult>();
            searchMode = SearchMode.RUNNING;
            SearchResultRequester.RequestCategorySearchResults(searchTerm, UpdateSearchResults, Repaint, this);
        }

        private void OnFeatured()
        {
            searchResults = new List<SearchResult>();
            searchMode = SearchMode.RUNNING;
            SearchResultRequester.RequestFeaturedResults(UpdateSearchResults, Repaint, this);
        }

        protected override void ResetAnythingWorld(ResetMode resetMode)
        {
            base.ResetAnythingWorld(resetMode);
            if (resetMode != ResetMode.Scene)
            {
                searchBarInput = "";
                searchTerm = "";
                searchResults = new List<SearchResult>();
                filteredResults = new List<SearchResult>();
            }
        }
        #endregion Mechanics
        #region Draw Scene GUI
        void OnFocus()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
            // Add (or re-add) the delegate.
            SceneView.duringSceneGui += this.OnSceneGUI;
        }
        protected void OnSceneGUI(SceneView sceneView)
        {
            if (TransformSettings.ShowGridHandles)
            {
                Handles.Label(SimpleGrid.origin, "Grid Origin");
            }
        }
        private void OnDestroy()
        {
            SceneView.duringSceneGui -= this.OnSceneGUI;
        }
        #endregion
    }
}
