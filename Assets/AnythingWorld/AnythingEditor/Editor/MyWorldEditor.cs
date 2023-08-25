using AnythingWorld.Networking.Editor;
using AnythingWorld.Utilities.Data;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

namespace AnythingWorld.Editor
{
    public class MyWorldEditor : AnythingCreatorEditor
    {
        #region Fields
        public enum MyWorldTabCategory
        {
            LIKES, LISTS
        }
        public static MyWorldEditor instance;
        public static MyWorldEditor Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GetWindow<MyWorldEditor>();
                }
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        private MyWorldTabCategory myWorldTabCategory;
        private bool modelLevel = true;
        private CollectionResult currentCollection;
        private List<CollectionResult> collections;
        private List<SearchResult> likedObjects;
        #endregion Fields

        #region Textures
        private static Texture2D baseLikesIcon;
        protected static Texture2D BaseLikesIcon
        {
            get
            {
                if (baseLikesIcon == null)
                {
                    baseLikesIcon = Resources.Load("Editor/MyWorld/Icons/likesCategory") as Texture2D;
                }
                return baseLikesIcon;
            }
        }
        private static Texture2D baseListsIcon;
        protected static Texture2D BaseListsIcon
        {
            get
            {
                if (baseListsIcon == null)
                {
                    baseListsIcon = Resources.Load("Editor/MyWorld/Icons/listsCategory") as Texture2D;
                }
                return baseListsIcon;
            }
        }
        private static Texture2D baseDeleteIcon;
        protected static Texture2D BaseDeleteIcon
        {
            get
            {
                if (baseDeleteIcon == null)
                {
                    baseDeleteIcon = Resources.Load("Editor/MyWorld/Icons/delete") as Texture2D;
                }
                return baseDeleteIcon;
            }
        }
        private static Texture2D baseCardDeleteIcon;
        protected static Texture2D BaseCardDeleteIcon
        {
            get
            {
                if (baseCardDeleteIcon == null)
                {
                    baseCardDeleteIcon = Resources.Load("Editor/MyWorld/Icons/CardIcons/delete") as Texture2D;
                }
                return baseCardDeleteIcon;
            }
        }

        private StateTexture2D stateListsIcon;
        protected StateTexture2D StateListsIcon
        {
            get
            {
                if (stateListsIcon == null || !stateListsIcon.TexturesLoadedNoHover)
                {
                    stateListsIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseListsIcon, Color.black, Color.white),
                        TintTextureToEditorTheme(BaseListsIcon, Color.white, Color.black));
                }
                return stateListsIcon;
            }
            set => stateListsIcon = value;
        }
        private StateTexture2D stateLikesIcon;
        protected StateTexture2D StateLikesIcon
        {
            get
            {
                if (stateLikesIcon == null || !stateLikesIcon.TexturesLoadedNoHover)
                {
                    stateLikesIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseLikesIcon, Color.black, Color.white),
                        TintTextureToEditorTheme(BaseLikesIcon, Color.white, Color.black));
                }
                return stateLikesIcon;
            }
            set => stateLikesIcon = value;
        }
        private StateTexture2D stateDeleteIcon;
        protected StateTexture2D StateDeleteIcon
        {
            get
            {
                if (stateDeleteIcon == null || !stateDeleteIcon.TexturesLoadedHover)
                {
                    stateDeleteIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseDeleteIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseDeleteIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseDeleteIcon, HexToColour("606162"), HexToColour("EDEEEC")));
                }
                return stateDeleteIcon;
            }
            set => stateDeleteIcon = value;
        }
        private StateTexture2D stateCardDeleteIcon;
        protected StateTexture2D StateCardDeleteIcon
        {
            get
            {
                if (stateCardDeleteIcon == null || !stateCardDeleteIcon.TexturesLoadedHover)
                {
                    stateCardDeleteIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseCardDeleteIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseCardDeleteIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseCardDeleteIcon, HexToColour("606162"), HexToColour("EDEEEC")));
                }
                return stateCardDeleteIcon;
            }
            set => stateCardDeleteIcon = value;
        }
        #endregion Textures

        #region Initialization
        [MenuItem("Anything World/My World", false, 2)]
        internal static void Initialize()
        {
            AnythingCreatorEditor tabWindow;
            Vector2 windowSize;

            if (AnythingSettings.HasAPIKey)
            {
                windowSize = new Vector2(500, 1200);
                CloseWindowIfOpen<MyWorldEditor>();
                var browser = GetWindow(typeof(MyWorldEditor), false, "My World");
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
                tabWindow = GetWindow<LogInEditor>("Log In | Sign Up", false, typeof(ModelBrowserEditor));
                tabWindow.position = new Rect(EditorGUIUtility.GetMainWindowPosition().center - windowSize / 2, windowSize);
            }
        }

        protected new void Awake()
        {
            base.Awake();
            Instance = this;
            windowTitle = "My World";
            bannerTintA = HexToColour("3CF2F3");
            bannerTintB = HexToColour("4BF841");
            if(AnythingSettings.HasAPIKey) InitializeCollectionsAndLikes();
            UserVoteProcessor.voteChangeDelegate -= RefreshCollectionsAndLikes;
            UserVoteProcessor.voteChangeDelegate += RefreshCollectionsAndLikes;
        }

        private void InitializeCollectionsAndLikes()
        {
            if (searchMode == SearchMode.RUNNING) return;
            if (searchMode == SearchMode.RUNNING_SILENTLY) return;
            if (collections == null || likedObjects == null)
            {
                searchMode = SearchMode.RUNNING;
                if (myWorldTabCategory == MyWorldTabCategory.LIKES) UserVoteProcessor.GetVoteCards(RefreshLikedObjects, Repaint, this);
                if (myWorldTabCategory == MyWorldTabCategory.LISTS && !modelLevel) CollectionProcessor.GetCollections(RefreshCollectionResults, this);
            }
        }
        public void RefreshCollectionsAndLikes()
        {
            if (searchMode == SearchMode.RUNNING) return;
            if (searchMode == SearchMode.RUNNING_SILENTLY) return;
            if (collections == null || likedObjects == null)
            {
                searchMode = SearchMode.RUNNING;
                if (myWorldTabCategory == MyWorldTabCategory.LIKES) UserVoteProcessor.GetVoteCards(RefreshLikedObjects, Repaint, this);
                if (myWorldTabCategory == MyWorldTabCategory.LISTS && !modelLevel) CollectionProcessor.GetCollections(RefreshCollectionResults, this);
            }
            else
            {
                searchMode = SearchMode.RUNNING_SILENTLY;
                if (myWorldTabCategory == MyWorldTabCategory.LIKES) UserVoteProcessor.GetVoteCards(RefreshLikedObjects, Repaint, this);
                if (myWorldTabCategory == MyWorldTabCategory.LISTS && !modelLevel) CollectionProcessor.GetCollections(RefreshCollectionResults, this);
            }
        }
        #endregion Initialization

        #region Editor Drawing
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
                if (selectedResult == null || string.IsNullOrEmpty(selectedResult.data._id)) DrawMyWorld();
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
        private void DrawMyWorld()
        {
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("My Likes", myWorldTabCategory == MyWorldTabCategory.LIKES ? StateLikesIcon.activeTexture : StateLikesIcon.inactiveTexture), myWorldTabCategory == MyWorldTabCategory.LIKES ? TabButtonActiveStyle : TabButtonInactiveStyle, GUILayout.MaxWidth(position.width / 2)))
            {
                modelLevel = true;
                currentCollection = null;
                myWorldTabCategory = MyWorldTabCategory.LIKES;
                searchMode = SearchMode.RUNNING;
                UserVoteProcessor.GetVoteCards(RefreshLikedObjects, Repaint, this);
            }
            if (GUILayout.Button(new GUIContent("My Lists", myWorldTabCategory == MyWorldTabCategory.LISTS ? StateListsIcon.activeTexture : StateListsIcon.inactiveTexture), myWorldTabCategory == MyWorldTabCategory.LISTS ? TabButtonActiveStyle : TabButtonInactiveStyle, GUILayout.MaxWidth(position.width / 2)))
            {
                modelLevel = false;
                currentCollection = null;
                myWorldTabCategory = MyWorldTabCategory.LISTS;
                searchMode = SearchMode.RUNNING;
                CollectionProcessor.GetCollections(RefreshCollectionResults, this);
            }
            EditorGUILayout.EndHorizontal();

            switch (myWorldTabCategory)
            {
                case MyWorldTabCategory.LIKES:
                    DrawToolbar(likedObjects);
                    break;
                case MyWorldTabCategory.LISTS when modelLevel:
                    DrawToolbar(currentCollection.Results, currentCollection.DisplayName);
                    break;
                case MyWorldTabCategory.LISTS:
                    DrawToolbar(collections);
                    break;
            }

            if (modelLevel)
            {
                DrawFilters();
                DrawSettingsIcons();
            }

            GUILayout.Space(16);
            DrawMyContent();
        }

        private Rect miscRect;

        private void DrawMyContent()
        {
            var tempRect = GUILayoutUtility.GetLastRect();
            if (tempRect != new Rect(0, 0, 1, 1)) miscRect = tempRect;

            switch (searchMode)
            {
                case SearchMode.IDLE:
                    DrawError();
                    break;
                case SearchMode.RUNNING:
                    DrawLoading(miscRect);
                    break;
                case SearchMode.RUNNING_SILENTLY:
                case SearchMode.SUCCESS:
                    switch (myWorldTabCategory)
                    {
                        case MyWorldTabCategory.LIKES:
                            DrawGrid(filteredResults, filteredResults.Count, 100, 120, DrawBrowserCard, resultThumbnailMultiplier);
                            break;
                        case MyWorldTabCategory.LISTS when modelLevel:
                            DrawGrid(filteredResults, filteredResults.Count, 100, 120, DrawListItemCard, resultThumbnailMultiplier);
                            break;
                        case MyWorldTabCategory.LISTS:
                            DrawGrid(collections, collections.Count, 100, 120, DrawListCard, resultThumbnailMultiplier);
                            break;
                    }
                    break;
                case SearchMode.FAILURE:
                    DrawError();
                    break;
            }
        }

        private void DrawToolbar<T>(List<T> results, string title = "")
        {
            var headerRect = GUILayoutUtility.GetRect(position.width, 50f);
            
            #region Labels
            var headerLabelContent = new GUIContent(myWorldTabCategory switch
            {
                MyWorldTabCategory.LIKES => "My Likes",
                MyWorldTabCategory.LISTS when !modelLevel => "My Lists",
                MyWorldTabCategory.LISTS when modelLevel => title,
                _ => ""
            });

            var headerLabelWidth = HeaderLabelStyle.CalcSize(headerLabelContent).x;
            var headerLabelRect = new Rect(headerRect.center.x - (headerLabelWidth / 2), headerRect.y, headerLabelWidth, 30);
            GUI.Label(headerLabelRect, headerLabelContent, new GUIStyle(HeaderLabelStyle) { normal = new GUIStyleState { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black } });

            if (searchMode == SearchMode.SUCCESS || searchMode == SearchMode.RUNNING_SILENTLY)
            {
                var countLabelString = myWorldTabCategory switch
                {
                    MyWorldTabCategory.LIKES => $"{results.Count} liked models",
                    MyWorldTabCategory.LISTS when !modelLevel => $"{results.Count} lists",
                    MyWorldTabCategory.LISTS when modelLevel => $"{results.Count} objects in list",
                    _ => ""
                };

                var countLabelContent = new GUIContent(countLabelString);
                var countLabelWidth = HeaderLabelStyle.CalcSize(countLabelContent);
                var countLabelRect = new Rect(headerLabelRect.x, headerLabelRect.yMax, countLabelWidth.x, headerRect.height - headerLabelRect.height);
                GUI.Label(countLabelRect, countLabelString, new GUIStyle(BodyLabelStyle) { alignment = TextAnchor.MiddleCenter, wordWrap=false, stretchWidth = true, fixedWidth = 100});
            
                #endregion Labels

                #region Scale Slider
                var roundedScale = Mathf.RoundToInt(resultThumbnailMultiplier * 10) / 10f;
                var thumbnailMultiplierLabelString = $"{roundedScale}x";
                var thumbnailMultiplierLabelSize = Mathf.Max(BodyLabelStyle.CalcSize(new GUIContent(thumbnailMultiplierLabelString)).x, 30f);

                var thumbnailSizingRect = new Rect(countLabelRect.xMax, countLabelRect.y, headerRect.xMax - countLabelRect.xMax, countLabelRect.height);
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
            }
            #endregion Scale Slider

            #region Icons
            if (modelLevel && currentCollection != null)
            {
                var iconSize = 16;
                var iconsRect = new Rect(headerRect.x, headerRect.y, headerRect.width, headerLabelRect.height);

                var marginY = (iconsRect.height - iconSize) / 2;
                var paddingX = iconSize * (2f / 3f);

                var backIconRect = new Rect(iconsRect.x + paddingX, iconsRect.y + marginY, iconSize, iconSize);
                var deleteIconRect = new Rect(iconsRect.xMax - iconSize - paddingX, iconsRect.y + marginY, iconSize, iconSize);

                if (GUI.Button(deleteIconRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateDeleteIcon.activeTexture), hover = SetStyleState(StateDeleteIcon.hoverTexture) }))
                {
                    AnythingSubwindow.OpenWindow($"Delete {currentCollection.DisplayName}?", new Vector2(300, 150), DrawDeleteCollectionWindow, position);
                }

                if (GUI.Button(backIconRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateBackIcon.activeTexture), hover = SetStyleState(StateBackIcon.hoverTexture) }))
                {
                    modelLevel = false;
                    currentCollection = null;
                    if (searchMode != SearchMode.RUNNING)
                    {
                        searchMode = SearchMode.RUNNING;
                        //searchMode = collections == null || collections.Count == 0 ? SearchMode.RUNNING : SearchMode.RUNNING_SILENTLY;
                        CollectionProcessor.GetCollections(RefreshCollectionResults, this);
                    }

                }
            }
            #endregion Icons
        }

        private void DrawListCard(List<CollectionResult> resultArray, float columnCoord, float rowCoord, float buttonWidth, float buttonHeight, int searchIndex, float scaleMultiplier)
        {
            Event e = Event.current;
            // Set result data
            var result = collections[searchIndex];
            Texture2D[] displayThumbnails;
            if (result.Results != null && result.Results.Any())
            {
                displayThumbnails = result.SearchResultThumbnails;
            }
            else
            {
                displayThumbnails = new[] { BlackAnythingGlobeLogo };
            }

            var collectionName = new GUIContent(result.DisplayName);
            var cardRect = new Rect(columnCoord, rowCoord, buttonWidth, buttonHeight);

            // Initialize padding and sizing 
            var voteIconSizeX = Mathf.Max(BaseFilledHeart.width, BaseFilledHeart.width) / 2.5f * resultThumbnailMultiplier;
            var voteIconSizeY = Mathf.Max(BaseFilledHeart.height, BaseFilledHeart.height) / 2.5f * resultThumbnailMultiplier;

            var infoPaddingX = voteIconSizeX / 2f;
            var infoPaddingY = voteIconSizeY / 2f;

            //Draw elements
            var infoBackdropRect = new Rect(cardRect.x, cardRect.yMax - buttonWidth, buttonWidth, buttonWidth);
            GUI.DrawTexture(infoBackdropRect, StateCardBackdrop.activeTexture);

            var thumbnailBackdropRect = new Rect(cardRect.x, cardRect.y, buttonWidth, buttonHeight * 0.75f);

            if (GUI.Button(thumbnailBackdropRect, new GUIContent()))
            {
                UpdateSearchResults(result.Results.ToArray(), "Sorry, that collection is empty!");
                currentCollection = result;
                modelLevel = true;
            }

            GUI.DrawTexture(thumbnailBackdropRect, BaseCardThumbnailBackdrops[searchIndex % BaseCardThumbnailBackdrops.Length]);
            if (displayThumbnails.Length >= 4)
            {
                var topLeftThumbnailRect = new Rect(thumbnailBackdropRect.x, thumbnailBackdropRect.y, thumbnailBackdropRect.width / 2, thumbnailBackdropRect.height / 2);
                GUI.DrawTexture(topLeftThumbnailRect, displayThumbnails[0], ScaleMode.ScaleAndCrop);

                var topRightThumbnailRect = new Rect(thumbnailBackdropRect.x + thumbnailBackdropRect.width / 2, thumbnailBackdropRect.y, thumbnailBackdropRect.width / 2, thumbnailBackdropRect.height / 2);
                GUI.DrawTexture(topRightThumbnailRect, displayThumbnails[1], ScaleMode.ScaleAndCrop);

                var bottomLeftThumbnailRect = new Rect(thumbnailBackdropRect.x, thumbnailBackdropRect.y + thumbnailBackdropRect.height / 2, thumbnailBackdropRect.width / 2, thumbnailBackdropRect.height / 2);
                GUI.DrawTexture(bottomLeftThumbnailRect, displayThumbnails[2], ScaleMode.ScaleAndCrop);

                var bottomRightThumbnailRect = new Rect(thumbnailBackdropRect.x + thumbnailBackdropRect.width / 2, thumbnailBackdropRect.y + thumbnailBackdropRect.height / 2, thumbnailBackdropRect.width / 2, thumbnailBackdropRect.height / 2);
                GUI.DrawTexture(bottomRightThumbnailRect, displayThumbnails[3], ScaleMode.ScaleAndCrop);
            }
            else
            {
                GUI.DrawTexture(thumbnailBackdropRect, displayThumbnails[0], ScaleMode.ScaleToFit);
            }

            if (cardRect.Contains(e.mousePosition))
            {
                if (e.button == 0 && e.isMouse)
                {
                    GUI.DrawTexture(thumbnailBackdropRect, BaseButtonTint);
                }
                GUI.DrawTexture(thumbnailBackdropRect, BaseButtonTint);
            }

            GUI.Label(new Rect(thumbnailBackdropRect.x, thumbnailBackdropRect.yMax, thumbnailBackdropRect.width, infoBackdropRect.yMax - thumbnailBackdropRect.yMax), collectionName, new GUIStyle(ModelNameStyle) { alignment = TextAnchor.MiddleCenter, fontSize = (int)(12 * resultThumbnailMultiplier) });

            FindCategoryIcon(CreationSearchCategory.COLLECTION, out var categoryIcon);
            DrawCardCategoryIcon(thumbnailBackdropRect, infoPaddingX, infoPaddingY, categoryIcon, scaleMultiplier);
        }

        protected void DrawListItemCard(List<SearchResult> resultArray, float columnCoord, float rowCoord, float buttonWidth, float buttonHeight, int searchIndex, float scaleMultiplier)
        {
            Event e = Event.current;
            // Set result data
            var result = resultArray[searchIndex];
            var displayThumbnail = result.Thumbnail;
            if (displayThumbnail == null)
            {
                displayThumbnail = BlackAnythingGlobeLogo;
            }
            var modelName = new GUIContent(result.DisplayName, result.DisplayName);
            var authorName = new GUIContent(result.data.author, result.data.author);
            var cardRect = new Rect(columnCoord, rowCoord, buttonWidth, buttonHeight);

            // Initialize padding and sizing 
            var voteIconSizeX = Mathf.Max(BaseFilledHeart.width, BaseFilledHeart.width) / 2.5f * resultThumbnailMultiplier;
            var voteIconSizeY = Mathf.Max(BaseFilledHeart.height, BaseFilledHeart.height) / 2.5f * resultThumbnailMultiplier;

            var infoPaddingX = voteIconSizeX / 2f;
            var infoPaddingY = voteIconSizeY / 2f;

            //Draw elements
            var infoBackdropRect = new Rect(cardRect.x, cardRect.yMax - buttonWidth, buttonWidth, buttonWidth);
            GUI.DrawTexture(infoBackdropRect, StateCardBackdrop.activeTexture);

            var thumbnailBackdropRect = new Rect(cardRect.x, cardRect.y, buttonWidth, buttonHeight * 0.75f);

            if (GUI.Button(thumbnailBackdropRect, new GUIContent()))
            {
                selectedResult = result;
            }

            GUI.DrawTexture(thumbnailBackdropRect, BaseCardThumbnailBackdrops[searchIndex % BaseCardThumbnailBackdrops.Length], ScaleMode.ScaleAndCrop);
            GUI.DrawTexture(thumbnailBackdropRect, displayThumbnail, ScaleMode.ScaleAndCrop);
            if (cardRect.Contains(e.mousePosition))
            {
                if (e.button == 0 && e.isMouse)
                {
                    GUI.DrawTexture(thumbnailBackdropRect, BaseButtonTint);
                }
                GUI.DrawTexture(thumbnailBackdropRect, BaseButtonTint);
            }
            DrawCardVoteButton(result, ref infoBackdropRect, voteIconSizeX, voteIconSizeY, infoPaddingX, infoPaddingY, out var voteRect);
            DrawCardVoteCountLabel(infoPaddingX, voteRect, result.data.voteScore, scaleMultiplier);
            DrawDeleteListIcon(result, infoBackdropRect, infoPaddingX, infoPaddingY, scaleMultiplier);

            DrawCardAuthorIconBackground(ref infoBackdropRect, ref thumbnailBackdropRect, infoPaddingX, infoPaddingY, out var authorIconRect, scaleMultiplier);
            DrawCardModelNameLabel(modelName, infoPaddingX, cardRect.xMax - (25 * scaleMultiplier), ref authorIconRect, out var modelNameLabelRect, scaleMultiplier);
            DrawCardAuthorLabel(authorName, infoPaddingX, cardRect.xMax - (25 * scaleMultiplier), authorIconRect, modelNameLabelRect, scaleMultiplier);

            DrawCardAnimationStatusIcon(thumbnailBackdropRect, infoPaddingX, infoPaddingY, scaleMultiplier);
            FindCategoryIcon(CreationSearchCategory.MODELS, out var categoryIcon);
            DrawCardCategoryIcon(thumbnailBackdropRect, infoPaddingX, infoPaddingY, categoryIcon, scaleMultiplier);
        }

        protected void DrawDeleteListIcon(SearchResult result, Rect infoBackdropRect, float infoPaddingX, float infoPaddingY, float scaleMultiplier)
        {
            //Draw List Icon 
            var listIconSizeX = BaseCardDeleteIcon.width / 2.5f * scaleMultiplier;
            var listIconSizeY = BaseCardDeleteIcon.height / 2.5f * scaleMultiplier;
            var listRect = new Rect(infoBackdropRect.xMax - infoPaddingX - listIconSizeX, infoBackdropRect.yMax - infoPaddingY - listIconSizeY, listIconSizeX, listIconSizeY);
            if (GUI.Button(listRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateCardDeleteIcon.activeTexture), hover = SetStyleState(StateCardDeleteIcon.hoverTexture) }))
            {
                AnythingSubwindow.OpenWindow($"Delete {result.DisplayName} from {currentCollection.DisplayName}?", new Vector2(300, 150), DrawRemoveFromCollectionWindow, position, result);
            }
        }

        protected void DrawDeleteCollectionWindow(AnythingEditor window)
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Are you sure you want to delete \"{currentCollection.DisplayName}\"?", new GUIStyle(HeaderLabelStyle) { fontSize = 16 });

            if (GUILayout.Button("Delete Collection", ButtonInactiveStyle))
            {
                CollectionProcessor.DeleteCollection(RefreshCollectionResults, currentCollection.Name, this);
                modelLevel = false;
                currentCollection = null;
                searchMode = SearchMode.RUNNING;
                CollectionProcessor.GetCollections(RefreshCollectionResults, this);
                window.Close();
            }
            GUILayout.FlexibleSpace();
        }

        protected void DrawRemoveFromCollectionWindow(AnythingEditor window, SearchResult result)
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Are you sure you want to remove \"{result.DisplayName}\" from \"{currentCollection.DisplayName}\"?", new GUIStyle(HeaderLabelStyle) { fontSize = 16 });

            if (GUILayout.Button($"Delete {result.DisplayName} from \"{currentCollection.DisplayName}\"", ButtonInactiveStyle))
            {
                CollectionProcessor.RemoveFromCollection(RefreshCollectionResults, result, currentCollection.Name, this);
                modelLevel = true;
                searchMode = SearchMode.RUNNING;
                CollectionProcessor.GetCollection(RefreshCollectionInternalResults, currentCollection, this);
                window.Close();
            }
            GUILayout.FlexibleSpace();
        }
        #endregion Editor Drawing

        #region Helper Functions
        public void RefreshCollectionResults(CollectionResult[] collections)
        {
            if (this.collections == null && collections == null)
            {
                searchMode = SearchMode.FAILURE;
                return;
            }
            this.collections = collections.ToList();
            searchMode = SearchMode.SUCCESS;

            Instance.Repaint();
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }
        public void RefreshCollectionInternalResults(CollectionResult result)
        {
            if (currentCollection == null && result == null)
            {
                searchMode = SearchMode.FAILURE;
                return;
            }

            if (result != null)
            {
                currentCollection = result;
                searchMode = SearchMode.SUCCESS;
                FilterSearchResult(currentCollection.Results);
            }
        }
        public void RefreshLikedObjects(SearchResult[] results)
        {
            if (likedObjects == null && results == null)
            {
                searchMode = SearchMode.FAILURE;
                return;
            }
            likedObjects = results.ToList();
            searchMode = SearchMode.SUCCESS;
            UpdateSearchResults(results, "You don't have any liked models!");
        }
        #endregion Helper Functions
    }
}
