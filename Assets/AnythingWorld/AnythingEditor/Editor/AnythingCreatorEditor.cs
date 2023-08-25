using AnythingWorld.Networking.Editor;
using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;

using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AnythingWorld.Editor
{
    [Serializable]
    public class StateTexture2D
    {

        public Texture2D activeTexture;
        public Texture2D inactiveTexture;
        public Texture2D hoverTexture;

        public bool TexturesLoadedNoHover => activeTexture != null && inactiveTexture != null;
        public bool TexturesLoadedHover => activeTexture != null && inactiveTexture != null && hoverTexture != null;

        public StateTexture2D(Texture2D activeTexture, Texture2D inactiveTexture, Texture2D hoverTexture = null)
        {
            this.activeTexture = activeTexture;
            this.inactiveTexture = inactiveTexture;
            this.hoverTexture = hoverTexture;
        }
    }

    public class AnythingCreatorEditor : AnythingEditor
    {
        #region Fields
        public enum CreationSearchCategory
        {
            MODELS, WORLDS, LIGHTING, COLLECTION
        }
        protected string windowTitle;
        private float searchRingAngle = 0;
        private double lastEditorTime = 0;
        protected static float resultThumbnailMultiplier = 1f;
        protected List<SearchResult> searchResults = null;
        protected List<SearchResult> filteredResults = null;
        protected SearchResult selectedResult;

        protected enum SearchMode { IDLE, RUNNING, RUNNING_SILENTLY, FAILURE, SUCCESS }

        protected SearchMode searchMode = SearchMode.IDLE;
        protected string searchModeFailReason = "";

        protected Color bannerTintA, bannerTintB;

        public static Dictionary<DefaultBehaviourType, Type> DefaultBehaviourDictionary
        {
            get
            {
                var temp = new Dictionary<DefaultBehaviourType, Type>();
                if (TransformSettings.RiggedBehaviourFieldEnabled && TransformSettings.RiggedAnimalScript != null) temp.Add(DefaultBehaviourType.WalkingAnimal, TransformSettings.RiggedAnimalScript?.GetClass());
                if (TransformSettings.VehicleBehaviourFieldEnabled && TransformSettings.GroundVehicleScript != null) temp.Add(DefaultBehaviourType.WheeledVehicle, TransformSettings.GroundVehicleScript?.GetClass());
                if (TransformSettings.FlyingVehicleBehaviourFieldEnabled && TransformSettings.FlyingVehicleScript != null) temp.Add(DefaultBehaviourType.FlyingVehicle, TransformSettings.FlyingVehicleScript?.GetClass());
                if (TransformSettings.StaticBehaviourFieldEnabled && TransformSettings.StaticBehaviourScript != null) temp.Add(DefaultBehaviourType.Static, TransformSettings.StaticBehaviourScript?.GetClass());
                if (TransformSettings.FlyingAnimalBehaviourFieldEnabled && TransformSettings.FlyingAnimalScript != null) temp.Add(DefaultBehaviourType.FlyingAnimal, TransformSettings.FlyingAnimalScript?.GetClass());
                if (TransformSettings.SwimmingAnimalBehaviourFieldEnabled && TransformSettings.SwimmingAnimalScript != null) temp.Add(DefaultBehaviourType.SwimmingAnimal, TransformSettings.SwimmingAnimalScript?.GetClass());
                return temp;
            }
        }
        #region Filters
        protected DropdownOption[] CategoryFilter
        {
            get
            {
                string[] categoryLabels = {
                    "All", "Animals & Pets", "Architecture", "Art & Abstract",
            "Cars & Vehicles", "Characters & Creatures", "Cultural Heritage & History",
            "Electronics & Gadgets","Fashion & Style","Food & Drink","Furniture & Home",
            "Music","Nature & Plants","News & Politics","People","Places & Travel",
            "Science & Technology","Sports & Fitness","Weapons & Military"};
                if (categoryFilter == null)
                {
                    var dropdownList = new List<DropdownOption>();
                    foreach (var (label, index) in categoryLabels.WithIndex())
                    {
                        var option = new DropdownOption()
                        {
                            dataEndpoint = (CategoryDropdownOption)index,
                            label = categoryLabels[index],
                            function = () =>
                            {
                                currentCategory = (CategoryDropdownOption)index;
                                FilterSearchResult(searchResults);
                            }
                        };

                        dropdownList.Add(option);
                    }
                    categoryFilter = dropdownList.ToArray();
                }
                return categoryFilter;
            }
        }
        protected DropdownOption[] categoryFilter;

        protected DropdownOption[] AnimationFilter
        {
            get
            {
                if (animationFilter == null)
                {
                    string[] animationLabels = { "Animated & Still", "Animated Only", "Still Only" };
                    var dropdownList = new List<DropdownOption>();
                    foreach (var (label, index) in animationLabels.WithIndex())
                    {
                        var option = new DropdownOption()
                        {
                            dataEndpoint = (AnimatedDropdownOption)index,
                            label = animationLabels[index],
                            function = () =>
                            {
                                currentAnimationFilter = (AnimatedDropdownOption)index;
                                FilterSearchResult(searchResults);
                            }
                        };

                        dropdownList.Add(option);
                    }
                    animationFilter = dropdownList.ToArray();
                }
                return animationFilter;
            }
        }
        protected DropdownOption[] animationFilter;

        protected DropdownOption[] SortingFilter
        {
            get
            {
                if (sortingFilter == null)
                {
                    string[] sortingLabels = { "Most Relevant",/*"Most Used",*/ "Most Liked", "Liked Models", "A-Z", "Z-A" };
                    var dropdownList = new List<DropdownOption>();
                    foreach (var (label, index) in sortingLabels.WithIndex())
                    {
                        var option = new DropdownOption()
                        {
                            dataEndpoint = (SortingDropdownOption)index,
                            label = sortingLabels[index],
                            function = () =>
                            {
                                currentSortingMethod = (SortingDropdownOption)index;
                                FilterSearchResult(searchResults);
                            }
                        };

                        dropdownList.Add(option);
                    }
                    sortingFilter = dropdownList.ToArray();
                }
                return sortingFilter;
            }
        }
        protected DropdownOption[] sortingFilter;

        protected enum CategoryDropdownOption { ALL, ANIMAL, ARCHITECTURE, ART, VEHICLE, CHARACTER, CULTURE, ELECTRONIC, FASHION, FOOD, FURNITURE, MUSIC, NATURE, NEWS, PEOPLE, PLACE, SCIENCE, SPORTS, WEAPON }
        protected CategoryDropdownOption currentCategory = CategoryDropdownOption.ALL;

        protected enum AnimatedDropdownOption { BOTH, ANIMATED, STILL }
        protected AnimatedDropdownOption currentAnimationFilter = AnimatedDropdownOption.BOTH;

        protected SortingDropdownOption currentSortingMethod = SortingDropdownOption.MostRelevant;
        #endregion Filters
        #region Styles
        protected GUIStyle iconStyle;

        protected GUIStyle ModelNameStyle;
        protected GUIStyle AuthorNameStyle;
        protected GUIStyle VoteStyle;
        #endregion Styles
        #region Textures
        #region Base Textures
        private static Texture2D baseWebsiteIcon;
        protected static Texture2D BaseWebsiteIcon
        {
            get
            {
                if (baseWebsiteIcon == null)
                {
                    baseWebsiteIcon = Resources.Load("Editor/Shared/SocialIcons/website") as Texture2D;
                }
                return baseWebsiteIcon;
            }
        }
        private static Texture2D baseDiscordIcon;
        protected static Texture2D BaseDiscordIcon
        {
            get
            {
                if (baseDiscordIcon == null)
                {
                    baseDiscordIcon = Resources.Load("Editor/Shared/SocialIcons/discord") as Texture2D;
                }
                return baseDiscordIcon;
            }
        }
        private static Texture2D baseLoginIcon;
        protected static Texture2D BaseLoginIcon
        {
            get
            {
                if (baseLoginIcon == null)
                {
                    baseLoginIcon = Resources.Load("Editor/Shared/SocialIcons/login") as Texture2D;
                }
                return baseLoginIcon;
            }
        }
        private static Texture2D baseLogoutIcon;
        protected static Texture2D BaseLogoutIcon
        {
            get
            {
                if (baseLogoutIcon == null)
                {
                    baseLogoutIcon = Resources.Load("Editor/Shared/SocialIcons/logout") as Texture2D;
                }
                return baseLogoutIcon;
            }
        }

        private static Texture2D baseLoadingCircle;
        protected static Texture2D BaseLoadingCircle
        {
            get
            {
                if (baseLoadingCircle == null)
                {
                    baseLoadingCircle = Resources.Load("Editor/AnythingBrowser/loadingCircle") as Texture2D;
                }
                return baseLoadingCircle;
            }
        }

        private static Texture2D baseResetIcon;
        protected static Texture2D BaseResetIcon
        {
            get
            {
                if (baseResetIcon == null)
                {
                    baseResetIcon = Resources.Load("Editor/AnythingBrowser/Icons/SettingsIcons/reset") as Texture2D;
                }
                return baseResetIcon;
            }
        }
        private static Texture2D baseTransformIcon;
        protected static Texture2D BaseTransformIcon
        {
            get
            {
                if (baseTransformIcon == null)
                {
                    baseTransformIcon = Resources.Load("Editor/AnythingBrowser/Icons/SettingsIcons/transform") as Texture2D;
                }
                return baseTransformIcon;
            }
        }
        private static Texture2D baseGridIcon;
        protected static Texture2D BaseGridIcon
        {
            get
            {
                if (baseGridIcon == null)
                {
                    baseGridIcon = Resources.Load("Editor/AnythingBrowser/Icons/SettingsIcons/grid") as Texture2D;
                }
                return baseGridIcon;
            }
        }
        private static Texture2D baseClearIcon;
        protected static Texture2D BaseClearIcon
        {
            get
            {
                if (baseClearIcon == null)
                {
                    baseClearIcon = Resources.Load("Editor/AnythingBrowser/Icons/clear") as Texture2D;
                }
                return baseClearIcon;
            }
        }
        private static Texture2D baseBackIcon;
        protected static Texture2D BaseBackIcon
        {
            get
            {
                if (baseBackIcon == null)
                {
                    baseBackIcon = Resources.Load("Editor/MyWorld/Icons/back") as Texture2D;
                }
                return baseBackIcon;
            }
        }
        private static Texture2D baseInputFieldLeft;
        protected static Texture2D BaseInputFieldLeft
        {
            get
            {
                if (baseInputFieldLeft == null)
                {
                    baseInputFieldLeft = Resources.Load("Editor/AnythingBrowser/roundedEdgeLeft") as Texture2D;
                }
                return baseInputFieldLeft;
            }
        }
        private static Texture2D baseInputFieldRight;
        protected static Texture2D BaseInputFieldRight
        {
            get
            {
                if (baseInputFieldRight == null)
                {
                    baseInputFieldRight = Resources.Load("Editor/AnythingBrowser/roundedEdgeRight") as Texture2D;
                }
                return baseInputFieldRight;
            }
        }
        private static Texture2D baseInputFieldMain;
        protected static Texture2D BaseInputFieldMain
        {
            get
            {
                if (baseInputFieldMain == null)
                {
                    baseInputFieldMain = Resources.Load("Editor/AnythingBrowser/roundedFrameMiddle") as Texture2D;
                }
                return baseInputFieldMain;
            }
        }

        private static Texture2D baseRoundButtonLeft;
        protected static Texture2D BaseRoundButtonLeft
        {
            get
            {
                if (baseRoundButtonLeft == null)
                {
                    baseRoundButtonLeft = Resources.Load("Editor/AnythingBrowser/roundedButtonEdgeLeft") as Texture2D;
                }
                return baseRoundButtonLeft;
            }
        }
        private static Texture2D baseRoundButtonRight;
        protected static Texture2D BaseRoundButtonRight
        {
            get
            {
                if (baseRoundButtonRight == null)
                {
                    baseRoundButtonRight = Resources.Load("Editor/AnythingBrowser/roundedButtonEdgeRight") as Texture2D;
                }
                return baseRoundButtonRight;
            }
        }
        private static Texture2D baseRoundButtonMain;
        protected static Texture2D BaseRoundButtonMain
        {
            get
            {
                if (baseRoundButtonMain == null)
                {
                    baseRoundButtonMain = Resources.Load("Editor/AnythingBrowser/roundedButtonMiddle") as Texture2D;
                }
                return baseRoundButtonMain;
            }
        }

        private static Texture2D baseThumbnailGridIcon;
        protected static Texture2D BaseThumbnailGridIcon
        {
            get
            {
                if (baseThumbnailGridIcon == null)
                {
                    baseThumbnailGridIcon = Resources.Load("Editor/AnythingBrowser/Icons/thumbnailGrid") as Texture2D;
                }

                return baseThumbnailGridIcon;
            }
        }
        #region Details
        private static Texture2D baseDetailsBackdrop;
        protected static Texture2D BaseDetailsBackdrop
        {
            get
            {
                if (baseDetailsBackdrop == null)
                {
                    baseDetailsBackdrop = Resources.Load("Editor/DetailsView/detailsBackground") as Texture2D;
                }
                return baseDetailsBackdrop;
            }
        }

        private static Texture2D baseReportIcon;
        protected static Texture2D BaseReportIcon
        {
            get
            {
                if (baseReportIcon == null)
                {
                    baseReportIcon = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/report") as Texture2D;
                }
                return baseReportIcon;
            }
        }

        private static Texture2D baseReportGraphic;
        protected static Texture2D BaseReportGraphic
        {
            get
            {
                if (baseReportGraphic == null)
                {
                    baseReportGraphic = Resources.Load("Editor/DetailsView/Icons/report") as Texture2D;
                }
                return baseReportGraphic;
            }
        }

        private static Texture2D baseTypeIcon;
        protected static Texture2D BaseTypeIcon
        {
            get
            {
                if (baseTypeIcon == null)
                {
                    baseTypeIcon = Resources.Load("Editor/DetailsView/Icons/type") as Texture2D;
                }
                return baseTypeIcon;
            }
        }

        private static Texture2D basePolyCountIcon;
        protected static Texture2D BasePolyCountIcon
        {
            get
            {
                if (basePolyCountIcon == null)
                {
                    basePolyCountIcon = Resources.Load("Editor/DetailsView/Icons/polygonCount") as Texture2D;
                }
                return basePolyCountIcon;
            }
        }

        private static Texture2D baseLicenseIcon;
        protected static Texture2D BaseLicenseIcon
        {
            get
            {
                if (baseLicenseIcon == null)
                {
                    baseLicenseIcon = Resources.Load("Editor/DetailsView/Icons/license") as Texture2D;
                }
                return baseLicenseIcon;
            }
        }
        #endregion Details
        #region Card
        private static Texture2D baseCardBackdrop;
        protected static Texture2D BaseCardBackdrop
        {
            get
            {
                if (baseCardBackdrop == null)
                {
                    baseCardBackdrop = Resources.Load("Editor/AnythingBrowser/Cards/cardBackdrop") as Texture2D;
                }
                return baseCardBackdrop;
            }
        }
        private static Texture2D[] baseCardThumbnailBackdrops;
        protected static Texture2D[] BaseCardThumbnailBackdrops
        {
            get
            {

                if (baseCardThumbnailBackdrops == null)
                {
                    baseCardThumbnailBackdrops = new[]
                    {
                        Resources.Load("Editor/AnythingBrowser/Cards/cardGradientBackdrop1") as Texture2D,
                        Resources.Load("Editor/AnythingBrowser/Cards/cardGradientBackdrop2") as Texture2D,
                        Resources.Load("Editor/AnythingBrowser/Cards/cardGradientBackdrop3") as Texture2D,
                        Resources.Load("Editor/AnythingBrowser/Cards/cardGradientBackdrop4") as Texture2D
                    };
                }
                return baseCardThumbnailBackdrops;
            }
        }
        private static Texture2D baseUserIcon;
        protected static Texture2D BaseUserIcon
        {
            get
            {
                if (baseUserIcon == null)
                {
                    baseUserIcon = Resources.Load("Editor/AnythingBrowser/Cards/cardProfile") as Texture2D;
                }
                return baseUserIcon;
            }
        }

        private static Texture2D baseFilledHeart;
        protected static Texture2D BaseFilledHeart
        {
            get
            {
                if (baseFilledHeart == null)
                {
                    baseFilledHeart = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/filledHeart") as Texture2D;
                }
                return baseFilledHeart;
            }
        }
        private static Texture2D baseEmptyHeart;
        protected static Texture2D BaseEmptyHeart
        {
            get
            {
                if (baseEmptyHeart == null)
                {
                    baseEmptyHeart = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/emptyHeart") as Texture2D;
                }
                return baseEmptyHeart;
            }
        }

        private static Texture2D baseCollectionIcon;
        protected static Texture2D BaseCollectionIcon
        {
            get
            {
                if (baseCollectionIcon == null)
                {
                    baseCollectionIcon = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/addToList") as Texture2D;
                }
                return baseCollectionIcon;
            }
        }
        private static Texture2D baseInfoIcon;
        protected static Texture2D BaseInfoIcon
        {
            get
            {
                if (baseInfoIcon == null)
                {
                    baseInfoIcon = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/info") as Texture2D;
                }
                return baseInfoIcon;
            }
        }

        private static Texture2D baseAnimatedIcon;
        protected static Texture2D BaseAnimatedIcon
        {
            get
            {
                if (baseAnimatedIcon == null)
                {
                    baseAnimatedIcon = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/animated") as Texture2D;
                }
                return baseAnimatedIcon;
            }
        }

        private static Texture2D baseCardObjectIcon;
        protected static Texture2D BaseCardObjectIcon
        {
            get
            {
                if (baseCardObjectIcon == null)
                {
                    baseCardObjectIcon = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/object") as Texture2D;
                }
                return baseCardObjectIcon;
            }
        }
        private static Texture2D baseCardWorldIcon;
        protected static Texture2D BaseCardWorldIcon
        {
            get
            {
                if (baseCardWorldIcon == null)
                {
                    baseCardWorldIcon = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/world") as Texture2D;
                }
                return baseCardWorldIcon;
            }
        }
        private static Texture2D baseCardLightingIcon;
        protected static Texture2D BaseCardLightingIcon
        {
            get
            {
                if (baseCardLightingIcon == null)
                {
                    baseCardLightingIcon = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/lighting") as Texture2D;
                }
                return baseCardLightingIcon;
            }
        }
        private static Texture2D baseCardCollectionIcon;
        protected static Texture2D BaseCardCollectionIcon
        {
            get
            {
                if (baseCardCollectionIcon == null)
                {
                    baseCardCollectionIcon = Resources.Load("Editor/AnythingBrowser/Icons/CardIcons/collections") as Texture2D;
                }
                return baseCardCollectionIcon;
            }
        }

        private static Texture2D baseButtonTint;
        protected static Texture2D BaseButtonTint
        {
            get
            {
                if (baseButtonTint == null)
                {
                    baseButtonTint = Resources.Load("Editor/AnythingBrowser/thumbnailBackgroundActive") as Texture2D;
                }
                return baseButtonTint;
            }
        }

        private static Texture2D baseCardObjectThumbnail;
        protected static Texture2D BaseCardObjectThumbnail
        {
            get
            {
                if (baseCardObjectThumbnail == null)
                {
                    baseCardObjectThumbnail = Resources.Load("Editor/AnythingBrowser/Cards/model") as Texture2D;
                }
                return baseCardObjectThumbnail;
            }
        }
        private static Texture2D baseCardWorldThumbnail;
        protected static Texture2D BaseCardWorldThumbnail
        {
            get
            {
                if (baseCardWorldThumbnail == null)
                {
                    baseCardWorldThumbnail = Resources.Load("Editor/AnythingBrowser/Cards/worlds") as Texture2D;
                }
                return baseCardWorldThumbnail;
            }
        }
        private static Texture2D baseCardLightingThumbnail;
        protected static Texture2D BaseCardLightingThumbnail
        {
            get
            {
                if (baseCardLightingThumbnail == null)
                {
                    baseCardLightingThumbnail = Resources.Load("Editor/AnythingBrowser/Cards/lighting") as Texture2D;
                }
                return baseCardLightingThumbnail;
            }
        }
        #endregion Card
        #endregion Base Textures
        #region Tinted Textures
        private Texture2D tintedUserIcon;
        protected Texture2D TintedUserIcon
        {
            get
            {
                if (tintedUserIcon == null)
                {
                    tintedUserIcon = TintTextureToEditorTheme(BaseUserIcon);
                }
                return tintedUserIcon;
            }
            set => tintedUserIcon = value;
        }

        private Texture2D blackAnythingGlobeLogo;
        protected Texture2D BlackAnythingGlobeLogo
        {
            get
            {
                if (blackAnythingGlobeLogo == null)
                {
                    blackAnythingGlobeLogo = TintTexture(BaseAnythingGlobeLogo, Color.black);
                }
                return blackAnythingGlobeLogo;
            }
            set => blackAnythingGlobeLogo = value;
        }

        private Texture2D tintedGradientBanner;
        protected Texture2D TintedGradientBanner
        {
            get
            {
                if (tintedGradientBanner == null)
                {
                    tintedGradientBanner = TintGradient(BaseGradientBanner, bannerTintA, bannerTintB);
                }
                return tintedGradientBanner;
            }
            set => tintedGradientBanner = value;
        }

        private Texture2D tintedLoadingCircle;
        protected Texture2D TintedLoadingCircle
        {
            get
            {
                if (tintedLoadingCircle == null)
                {
                    tintedLoadingCircle = TintTextureToEditorTheme(BaseLoadingCircle);
                }
                return tintedLoadingCircle;
            }
            set => tintedLoadingCircle = value;
        }

        private Texture2D tintedInputFieldMain;
        protected Texture2D TintedInputFieldMain
        {
            get
            {
                if (tintedInputFieldMain == null)
                {
                    tintedInputFieldMain = TintTextureToEditorTheme(BaseInputFieldMain, HexToColour("292A2B"), HexToColour("E4E5E3"));
                }

                return tintedInputFieldMain;
            }
            set => tintedInputFieldMain = value;
        }
        private Texture2D tintedInputFieldLeft;
        protected Texture2D TintedInputFieldLeft
        {
            get
            {
                if (tintedInputFieldLeft == null)
                {
                    tintedInputFieldLeft = TintTextureToEditorTheme(BaseInputFieldLeft, HexToColour("292A2B"), HexToColour("E4E5E3"));
                }

                return tintedInputFieldLeft;
            }
            set => tintedInputFieldLeft = value;
        }
        private Texture2D tintedInputFieldRight;
        protected Texture2D TintedInputFieldRight
        {
            get
            {
                if (tintedInputFieldRight == null)
                {
                    tintedInputFieldRight = TintTextureToEditorTheme(BaseInputFieldRight, HexToColour("292A2B"), HexToColour("E4E5E3"));
                }

                return tintedInputFieldRight;
            }
            set => tintedInputFieldRight = value;
        }

        private Texture2D tintedThumbnailGridIcon;
        protected Texture2D TintedThumbnailGridIcon
        {
            get
            {
                if (tintedThumbnailGridIcon == null)
                {
                    tintedThumbnailGridIcon = TintTextureToEditorTheme(BaseThumbnailGridIcon);
                }
                return tintedThumbnailGridIcon;
            }
            set => tintedThumbnailGridIcon = value;
        }

        private Texture2D tintedCardObjectThumbnail;
        protected Texture2D TintedCardObjectThumbnail
        {
            get
            {
                if (tintedCardObjectThumbnail == null)
                {
                    tintedCardObjectThumbnail = TintTextureToEditorTheme(BaseCardObjectThumbnail);
                }
                return tintedCardObjectThumbnail;
            }
        }
        private Texture2D tintedCardWorldThumbnail;
        protected Texture2D TintedCardWorldThumbnail
        {
            get
            {
                if (tintedCardWorldThumbnail == null)
                {
                    tintedCardWorldThumbnail = TintTextureToEditorTheme(BaseCardWorldThumbnail);
                }
                return tintedCardWorldThumbnail;
            }
        }
        private Texture2D tintedCardLightingThumbnail;
        protected Texture2D TintedCardLightingThumbnail
        {
            get
            {
                if (tintedCardLightingThumbnail == null)
                {
                    tintedCardLightingThumbnail = TintTextureToEditorTheme(BaseCardLightingThumbnail);
                }
                return tintedCardLightingThumbnail;
            }
        }

        private Texture2D tintedReportGraphic;
        protected Texture2D TintedReportGraphic
        {
            get
            {
                if(tintedReportGraphic == null)
                {
                    tintedReportGraphic = TintTextureToEditorTheme(BaseReportGraphic);
                }
                return tintedReportGraphic;
            }
        }
        #endregion Tinted Textures
        #region State Textures
        private StateTexture2D stateLogoutIcon;
        protected StateTexture2D StateLogoutIcon
        {
            get
            {
                if (stateLogoutIcon == null || !stateLogoutIcon.TexturesLoadedHover)
                {
                    stateLogoutIcon = new StateTexture2D(BaseLogoutIcon, BaseLogoutIcon, TintTexture(BaseLogoutIcon, HexToColour("EEEEEE")));
                }
                return stateLogoutIcon;
            }
            set => stateLogoutIcon = value;
        }
        private StateTexture2D stateLoginIcon;
        protected StateTexture2D StateLoginIcon
        {
            get
            {
                if (stateLoginIcon == null || !stateLoginIcon.TexturesLoadedHover)
                {
                    stateLoginIcon = new StateTexture2D(BaseLoginIcon, BaseLoginIcon, TintTexture(BaseLoginIcon, HexToColour("EEEEEE")));
                }
                return stateLoginIcon;
            }
            set => stateLoginIcon = value;
        }
        private StateTexture2D stateDiscordIcon;
        protected StateTexture2D StateDiscordIcon
        {
            get
            {
                if (stateDiscordIcon == null || !stateDiscordIcon.TexturesLoadedHover)
                {
                    stateDiscordIcon = new StateTexture2D(BaseDiscordIcon, BaseDiscordIcon, TintTexture(BaseDiscordIcon, HexToColour("EEEEEE")));
                }
                return stateDiscordIcon;
            }
            set => stateDiscordIcon = value;
        }
        private StateTexture2D stateWebsiteIcon;
        protected StateTexture2D StateWebsiteIcon
        {
            get
            {
                if (stateWebsiteIcon == null || !stateWebsiteIcon.TexturesLoadedHover)
                {
                    stateWebsiteIcon = new StateTexture2D(BaseWebsiteIcon, BaseWebsiteIcon, TintTexture(BaseWebsiteIcon, HexToColour("EEEEEE")));
                }
                return stateWebsiteIcon;
            }
            set => stateWebsiteIcon = value;
        }

        private StateTexture2D stateRoundButtonMain;
        protected StateTexture2D StateRoundButtonMain
        {
            get
            {
                if (stateRoundButtonMain == null || !stateRoundButtonMain.TexturesLoadedHover)
                {
                    stateRoundButtonMain = new StateTexture2D(
                        TintTextureToEditorTheme(BaseRoundButtonMain, HexToColour("98999A"), HexToColour("979797")),
                        TintTextureToEditorTheme(BaseRoundButtonMain, HexToColour("575859"), HexToColour("E4E5E3")),
                        TintTextureToEditorTheme(BaseRoundButtonMain, HexToColour("606162"), HexToColour("EDEEEC")));
                }

                return stateRoundButtonMain;
            }
            set => stateRoundButtonMain = value;
        }
        private StateTexture2D stateRoundButtonLeft;
        protected StateTexture2D StateRoundButtonLeft
        {
            get
            {
                if (stateRoundButtonLeft == null || !stateRoundButtonLeft.TexturesLoadedHover)
                {
                    stateRoundButtonLeft = new StateTexture2D(
                        TintTextureToEditorTheme(BaseRoundButtonLeft, HexToColour("98999A"), HexToColour("979797")),
                        TintTextureToEditorTheme(BaseRoundButtonLeft, HexToColour("575859"), HexToColour("E4E5E3")),
                        TintTextureToEditorTheme(BaseRoundButtonLeft, HexToColour("606162"), HexToColour("EDEEEC")));
                }

                return stateRoundButtonLeft;
            }
            set => stateRoundButtonLeft = value;
        }
        private StateTexture2D stateRoundButtonRight;
        protected StateTexture2D StateRoundButtonRight
        {
            get
            {
                if (stateRoundButtonRight == null || !stateRoundButtonRight.TexturesLoadedHover)
                {
                    stateRoundButtonRight = new StateTexture2D(
                        TintTextureToEditorTheme(BaseRoundButtonRight, HexToColour("98999A"), HexToColour("979797")),
                        TintTextureToEditorTheme(BaseRoundButtonRight, HexToColour("575859"), HexToColour("E4E5E3")),
                        TintTextureToEditorTheme(BaseRoundButtonRight, HexToColour("606162"), HexToColour("EDEEEC")));
                }

                return stateRoundButtonRight;
            }
            set => stateRoundButtonRight = value;
        }

        private StateTexture2D stateResetIcon;
        protected StateTexture2D StateResetIcon
        {
            get
            {
                if (stateResetIcon == null || !stateResetIcon.TexturesLoadedHover)
                {
                    stateResetIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseResetIcon, Color.white, Color.black),
                        TintTexture(BaseResetIcon, HexToColour("979797")),
                        TintTextureToEditorTheme(BaseResetIcon, HexToColour("606162"), HexToColour("EDEEEC")));

                }
                return stateResetIcon;
            }
            set => stateResetIcon = value;
        }
        private StateTexture2D stateTransformIcon;
        protected StateTexture2D StateTransformIcon
        {
            get
            {
                if (stateTransformIcon == null || !stateTransformIcon.TexturesLoadedNoHover)
                {
                    stateTransformIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseTransformIcon, Color.black, Color.white),
                        TintTextureToEditorTheme(BaseTransformIcon, Color.white, Color.black));
                }
                return stateTransformIcon;
            }
            set => stateTransformIcon = value;
        }
        private StateTexture2D stateGridIcon;
        protected StateTexture2D StateGridIcon
        {
            get
            {
                if (stateGridIcon == null || !stateGridIcon.TexturesLoadedHover)
                {
                    stateGridIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseGridIcon, Color.white, Color.black),
                        TintTexture(BaseGridIcon, HexToColour("979797")),
                        TintTextureToEditorTheme(BaseGridIcon, HexToColour("606162"), HexToColour("EDEEEC")));
                }
                return stateGridIcon;
            }
            set => stateGridIcon = value;
        }
        private StateTexture2D stateClearIcon;
        protected StateTexture2D StateClearIcon
        {
            get
            {
                if (stateClearIcon == null || !stateClearIcon.TexturesLoadedHover)
                {
                    stateClearIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseClearIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseClearIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseClearIcon, HexToColour("606162"), HexToColour("EDEEEC")));
                }
                return stateClearIcon;
            }
            set => stateClearIcon = value;
        }
        private StateTexture2D stateBackIcon;
        protected StateTexture2D StateBackIcon
        {
            get
            {
                if (stateBackIcon == null || !stateBackIcon.TexturesLoadedHover)
                {
                    stateBackIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseBackIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseBackIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseBackIcon, HexToColour("606162"), HexToColour("EDEEEC")));
                }
                return stateBackIcon;
            }
            set => stateBackIcon = value;
        }
        private StateTexture2D stateHeartIcon;
        protected StateTexture2D StateHeartIcon
        {
            get
            {
                if (stateHeartIcon == null || !stateHeartIcon.TexturesLoadedHover)
                {
                    stateHeartIcon = new StateTexture2D(
                        EditorGUIUtility.isProSkin ? BaseFilledHeart : TintTextureWhite(BaseFilledHeart, Color.black),
                        TintTextureToEditorTheme(BaseEmptyHeart, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseEmptyHeart, HexToColour("606162"), HexToColour("EDEEEC")));
                }
                return stateHeartIcon;
            }
            set => stateHeartIcon = value;
        }
        private StateTexture2D stateCollectionIcon;
        protected StateTexture2D StateCollectionIcon
        {
            get
            {
                if (stateCollectionIcon == null || !stateCollectionIcon.TexturesLoadedHover)
                {
                    stateCollectionIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseCollectionIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseCollectionIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseCollectionIcon, HexToColour("606162"), HexToColour("EDEEEC")));
                }
                return stateCollectionIcon;
            }
            set => stateCollectionIcon = value;
        }
        private StateTexture2D stateInfoIcon;
        protected StateTexture2D StateInfoIcon
        {
            get
            {
                if (stateInfoIcon == null || !stateInfoIcon.TexturesLoadedHover)
                {
                    stateInfoIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseInfoIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseInfoIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseInfoIcon, HexToColour("606162"), HexToColour("EDEEEC")));
                }
                return stateInfoIcon;
            }
            set => stateInfoIcon = value;
        }
        private StateTexture2D stateReportIcon;
        protected StateTexture2D StateReportIcon
        {
            get
            {
                if (stateReportIcon == null || !stateReportIcon.TexturesLoadedHover)
                {
                    stateReportIcon = new StateTexture2D(
                        TintTextureToEditorTheme(BaseReportIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseReportIcon, Color.white, Color.black),
                        TintTextureToEditorTheme(BaseReportIcon, HexToColour("606162"), HexToColour("EDEEEC")));
                }
                return stateReportIcon;
            }
            set => stateReportIcon = value;
        }
        private StateTexture2D stateCardBackdrop;
        protected StateTexture2D StateCardBackdrop
        {
            get
            {
                if (stateCardBackdrop == null || !stateCardBackdrop.TexturesLoadedHover)
                {
                    stateCardBackdrop = new StateTexture2D(
                        TintTextureToEditorTheme(BaseCardBackdrop, HexToColour("3F4041"), HexToColour("E4E5E3")),
                        TintTextureToEditorTheme(BaseCardBackdrop, HexToColour("3F4041"), HexToColour("E4E5E3")),
                        TintTextureToEditorTheme(BaseCardBackdrop, HexToColour("575859"), HexToColour("EDEEEC")));
                }
                return stateCardBackdrop;
            }
            set => stateCardBackdrop = value;
        }
        #endregion State Textures
        #endregion Textures


        public static Transform objectParent;

        protected Vector2 newScrollPosition;
        protected bool copiedToKeyboard = false;
        protected Rect copiedRect;
        protected int copiedResult = 0;
        #endregion

        #region Initialization
        protected new void Awake()
        {
            base.Awake();
            AssignDefaultBehavioursFromScriptable();
        }
        protected void AssignDefaultBehavioursFromScriptable()
        {
            if (ScriptableObjectExtensions.TryGetInstance<DefaultBehaviourEditorScriptable>(out var script))
            {
                TransformSettings.RiggedAnimalScript = script.defaultRigged;
                TransformSettings.GroundVehicleScript = script.defaultWheeledVehicle;
                TransformSettings.FlyingVehicleScript = script.defaultPropellorVehicle;
                TransformSettings.FlyingAnimalScript = script.defaultFlyingAnimal;
            }
        }
        protected override void DefineCustomStyles()
        {
            base.DefineCustomStyles();
            iconStyle = new GUIStyle
            {
                normal =
                {
                    background = null
                },
                hover =
                {
                    background = null
                },
                stretchWidth = true,
                clipping = TextClipping.Overflow
            };

            ModelNameStyle = new GUIStyle(BodyLabelStyle)
            {
                font = GetPoppinsFont(PoppinsStyle.Medium),
                clipping = TextClipping.Clip,
                wordWrap = false
            };

            AuthorNameStyle = new GUIStyle(BodyLabelStyle)
            {
                font = GetPoppinsFont(PoppinsStyle.Regular),
                clipping = TextClipping.Clip,
                wordWrap = false
            };

            VoteStyle = new GUIStyle(BodyLabelStyle)
            {
                font = GetPoppinsFont(PoppinsStyle.Regular)
            };
        }
        #endregion Initialization

        protected new void OnGUI()
        {
            base.OnGUI();

            InitializeResources();
            DrawWindowBanner();
        }
        #region Editor Drawing
        /// <summary>
        /// Draws Anything World logo and social buttons for the Anything Creator windows.
        /// </summary>
        private void DrawWindowBanner()
        {
            var globeRect = new Rect(10, 10, 64, 64);
            var bannerRect = new Rect(0, 0, position.width, globeRect.yMax + 10);
            GUI.DrawTexture(bannerRect, TintedGradientBanner);
            GUI.DrawTexture(globeRect, BlackAnythingGlobeLogo);
            var textHeight = 50;
            var textPadding = 15;
            var titleRect = new Rect(globeRect.xMax + 10, bannerRect.height - textHeight - textPadding, position.width - globeRect.xMax, textHeight);
            var anythingWorld = new GUIContent("ANYTHING WORLD");

            GUI.Label(titleRect, anythingWorld, new GUIStyle(EditorStyles.label) { font = GetPoppinsFont(PoppinsStyle.Bold), fontSize = 22, alignment = TextAnchor.UpperLeft, normal = new GUIStyleState() { textColor = Color.black } });
            GUI.Label(titleRect, new GUIContent(windowTitle), new GUIStyle(EditorStyles.label) { font = GetPoppinsFont(PoppinsStyle.Bold), fontSize = 22, alignment = TextAnchor.LowerLeft, normal = new GUIStyleState() { textColor = Color.white } });

            var iconSize = 16f;
            var iconPadding = 4f;
            var iconMargin = (bannerRect.height - ((iconSize * 3) + (iconPadding * 2))) / 2;

            var iconsXPos = bannerRect.xMax - iconSize - iconMargin;
            var iconsYPos = bannerRect.yMin + iconMargin;

            var versionRect = new Rect(bannerRect.center.x, iconsYPos, (bannerRect.width / 2) - iconSize - iconMargin - iconPadding, iconSize);
            var version = new GUIContent(AnythingSettings.PackageVersion);
            GUI.Label(versionRect, version, new GUIStyle(EditorStyles.label) { font = GetPoppinsFont(PoppinsStyle.Medium), fontSize = 12, alignment = TextAnchor.MiddleRight, normal = new GUIStyleState() { textColor = Color.white } });

            var logoutIconRect = new Rect(iconsXPos, iconsYPos, iconSize, iconSize);
            var discordIconRect = new Rect(iconsXPos, logoutIconRect.yMax + iconPadding, iconSize, iconSize);
            var websiteIconRect = new Rect(iconsXPos, discordIconRect.yMax + iconPadding, iconSize, iconSize);

            if (AnythingSettings.HasAPIKey)
            {
                if (GUI.Button(logoutIconRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateLogoutIcon.activeTexture), hover = SetStyleState(StateLogoutIcon.hoverTexture) }))
                {
                    var settingsSerializedObject = new SerializedObject(AnythingSettings.Instance);
                    settingsSerializedObject.FindProperty("apiKey").stringValue = "";
                    settingsSerializedObject.FindProperty("email").stringValue = "";
                    settingsSerializedObject.ApplyModifiedProperties();

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Undo.RecordObject(AnythingSettings.Instance, "Logged out");
                    EditorUtility.SetDirty(AnythingSettings.Instance);

                    while (HasOpenInstances<AnythingCreatorEditor>())
                    {
                        var window = GetWindow(typeof(AnythingCreatorEditor));
                        window.Close();
                    }
                }
            }
            if (GUI.Button(discordIconRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateDiscordIcon.activeTexture), hover = SetStyleState(StateDiscordIcon.hoverTexture) })) System.Diagnostics.Process.Start("https://discord.gg/anythingworld");
            if (GUI.Button(websiteIconRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateWebsiteIcon.activeTexture), hover = SetStyleState(StateWebsiteIcon.hoverTexture) })) System.Diagnostics.Process.Start("https://www.anything.world/");
            //Mask banner in layouting
            GUILayoutUtility.GetRect(position.width, bannerRect.yMax, GUILayout.MinWidth(500));
        }

        protected void DrawLoading(Rect miscRect)
        {
            var thisTime = EditorApplication.timeSinceStartup;
            var workArea = GUILayoutUtility.GetRect(position.width, position.height - (miscRect.y + miscRect.height));
            var logoSize = workArea.height / 4;
            var spinningRect = new Rect((workArea.width / 2) - (logoSize / 2), workArea.y + (workArea.height / 2) - (logoSize / 2), logoSize, logoSize);
            var logoRect = new Rect(spinningRect.x + (spinningRect.width / 6), spinningRect.y + (spinningRect.height / 6), spinningRect.width * (2f / 3f), spinningRect.height * (2f / 3f));
            var dt = EditorApplication.timeSinceStartup - lastEditorTime;
            var matrixBack = GUI.matrix;
            searchRingAngle += 75f * (float)dt;
            GUIUtility.RotateAroundPivot(searchRingAngle, spinningRect.center);
            GUI.DrawTexture(spinningRect, TintedLoadingCircle);
            GUI.matrix = matrixBack;
            GUI.DrawTexture(logoRect, EditorGUIUtility.isProSkin ? BaseAnythingGlobeLogo : BlackAnythingGlobeLogo);
            lastEditorTime = thisTime;
        }
        protected void DrawError(string searchTerm)
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Error searching for that term, please try again.", new GUIStyle(HeaderLabelStyle) { wordWrap = true });
            GUILayout.FlexibleSpace();
        }
        protected void DrawError()
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label(searchModeFailReason, new GUIStyle(HeaderLabelStyle) { wordWrap = true });

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset Creator", TabButtonInactiveStyle))
            {
                ResetAnythingWorld(ResetMode.Creator);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }

        protected void DrawSettingsIcons()
        {
            GUILayout.BeginHorizontal();
            bool transformWindowOpen = HasOpenInstances<TransformSettingsEditor>();
            if (GUILayout.Button(new GUIContent("Make Options", transformWindowOpen ? StateTransformIcon.activeTexture : StateTransformIcon.inactiveTexture), transformWindowOpen ? ButtonActiveStyle : ButtonInactiveStyle, GUILayout.MaxWidth((position.width / 3) - DropdownStyle.margin.horizontal - 8)))
            {
                if(transformWindowOpen)
                {
                    CloseWindowIfOpen<TransformSettingsEditor>();
                }
                else
                {
                    TransformSettingsEditor.Initialize();
                }
            }

            GUILayout.FlexibleSpace();

            var iconSize = 16;
            var settingsIconsRect = GUILayoutUtility.GetRect((position.width / 3) - DropdownStyle.margin.horizontal - 8, iconSize * 2);
            GUILayout.EndHorizontal();

            var marginY = (settingsIconsRect.height - iconSize) / 2;
            var paddingX = iconSize * (2f / 3f);

            var resetIconRect = new Rect(settingsIconsRect.xMax - iconSize - paddingX, settingsIconsRect.y + marginY, iconSize, iconSize);
            
            //Dropdown to Reset Grid, Reset Creator, and Reset All
            if (GUI.Button(resetIconRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateResetIcon.activeTexture), hover = SetStyleState(StateResetIcon.hoverTexture) }))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Reset Scene"), false, () => SetupResetEditorWindow(ResetMode.Scene));
                menu.AddItem(new GUIContent("Reset Creator"), false, () => SetupResetEditorWindow(ResetMode.Creator));
                menu.AddItem(new GUIContent("Reset All"), false, () => SetupResetEditorWindow(ResetMode.All));
                menu.DropDown(resetIconRect);
            }
        }

        protected void DrawFilters()
        {
            GUILayout.Label("FILTER", new GUIStyle(BodyLabelStyle) { alignment = TextAnchor.MiddleCenter });
            GUILayout.BeginHorizontal();
            DrawDropdown(new Vector3(0, 20), CategoryFilter, currentCategory, "CATEGORY");
            DrawDropdown(new Vector3(0, 20), AnimationFilter, currentAnimationFilter, "ANIMATED");
            DrawDropdown(new Vector3(0, 20), SortingFilter, currentSortingMethod, "SORT BY");
            GUILayout.EndHorizontal();
        }

        protected void DrawBrowserCard(List<SearchResult> resultArray, float columnCoord, float rowCoord, float buttonWidth, float buttonHeight, int searchIndex, float resultScaleMultiplier)
        {
            try
            {
                Event e = Event.current;
                // Set result data
                var result = resultArray[searchIndex];
                var displayThumbnail = result.Thumbnail;
                if (displayThumbnail == null)
                {
                    displayThumbnail = BaseCardObjectThumbnail;
                    //displayThumbnail = category switch
                    //{
                    //    CreationSearchCategory.MODELS => BaseCardObjectThumbnail,
                    //    CreationSearchCategory.WORLDS => BaseCardWorldThumbnail,
                    //    CreationSearchCategory.LIGHTING => BaseCardLightingThumbnail,
                    //    CreationSearchCategory.COLLECTION => BaseCardCollectionThumbnail,
                    //    _ => null
                    //};
                }
                var modelName = new GUIContent(result.DisplayName, result.DisplayName);
                var authorName = new GUIContent(result.data.author, result.data.author);
                var cardRect = new Rect(columnCoord, rowCoord, buttonWidth, buttonHeight);

                // Initialize padding and sizing 
                var voteIconSizeX = Mathf.Max(BaseFilledHeart.width, BaseFilledHeart.width) / 2.5f * resultScaleMultiplier;
                var voteIconSizeY = Mathf.Max(BaseFilledHeart.height, BaseFilledHeart.height) / 2.5f * resultScaleMultiplier;

                var infoPaddingX = voteIconSizeX / 2f;
                var infoPaddingY = voteIconSizeY / 2f;

                //Draw elements
                var infoBackdropRect = new Rect(cardRect.x, cardRect.yMax - buttonWidth, buttonWidth, buttonWidth);
                GUI.DrawTexture(infoBackdropRect, StateCardBackdrop.activeTexture);

                var thumbnailBackdropRect = new Rect(cardRect.x, cardRect.y, buttonWidth, buttonHeight * 0.75f);

                if (GUI.Button(thumbnailBackdropRect, new GUIContent()))
                    MakeResult(result);

                GUI.DrawTexture(thumbnailBackdropRect, BaseCardThumbnailBackdrops[searchIndex % BaseCardThumbnailBackdrops.Length], ScaleMode.ScaleAndCrop);

                try
                {
                    if (displayThumbnail != null)
                    {
                        GUI.DrawTexture(thumbnailBackdropRect, displayThumbnail, ScaleMode.ScaleAndCrop);
                    }

                }
                catch { }
                if (cardRect.Contains(e.mousePosition))
                {
                    if (e.button == 0 && e.isMouse)
                    {
                        GUI.DrawTexture(thumbnailBackdropRect, BaseButtonTint);
                    }
                    GUI.DrawTexture(thumbnailBackdropRect, BaseButtonTint);
                }
                DrawCardVoteButton(result, ref infoBackdropRect, voteIconSizeX, voteIconSizeY, infoPaddingX, infoPaddingY, out var voteRect);
                DrawCardVoteCountLabel(infoPaddingX, voteRect, result.data.voteScore, resultScaleMultiplier);
                DrawCardListIcon(result, infoBackdropRect, infoPaddingX, infoPaddingY, resultScaleMultiplier);
                DrawInfoIcon(result, thumbnailBackdropRect, infoPaddingX, infoPaddingY, resultScaleMultiplier);

                DrawCardAuthorIconBackground(ref infoBackdropRect, ref thumbnailBackdropRect, infoPaddingX, infoPaddingY, out var authorIconRect, resultScaleMultiplier);
                DrawCardModelNameLabel(modelName, infoPaddingX, cardRect.xMax-(25*resultScaleMultiplier), ref authorIconRect, out var modelNameLabelRect, resultScaleMultiplier);
                DrawCardAuthorLabel(authorName, infoPaddingX, cardRect.xMax-(25 * resultScaleMultiplier), authorIconRect, modelNameLabelRect, resultScaleMultiplier);

                if (result.isAnimated) DrawCardAnimationStatusIcon(thumbnailBackdropRect, infoPaddingX, infoPaddingY, resultScaleMultiplier);
                FindCategoryIcon(CreationSearchCategory.MODELS, out var categoryIcon);
                DrawCardCategoryIcon(thumbnailBackdropRect, infoPaddingX, infoPaddingY, categoryIcon, resultScaleMultiplier);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void MakeResult(SearchResult result, bool forceSerialize = false)
        {
            var inputParams = new List<RequestParameterOption>();

            if (TransformSettings.PositionFieldEnabled) inputParams.Add(RequestParameter.Position(TransformSettings.PositionField));
            if (TransformSettings.RotationFieldEnabled) inputParams.Add(RequestParameter.Rotation(TransformSettings.RotationField));
            if (TransformSettings.ScaleFieldEnabled) inputParams.Add(RequestParameter.ScaleMultiplier(TransformSettings.ScaleField));
            if (TransformSettings.ParentFieldEnabled) inputParams.Add(RequestParameter.Parent(objectParent));
            inputParams.Add(RequestParameter.PlaceOnGrid(TransformSettings.PlaceOnGrid));
            inputParams.Add(RequestParameter.AddRigidbody(TransformSettings.AddRigidbody));
            inputParams.Add(RequestParameter.AddCollider(TransformSettings.AddCollider));
            inputParams.Add(RequestParameter.SetDefaultBehaviourPreset(DefaultBehavioursUtility.CreateNewTemporaryInstance(DefaultBehaviourDictionary)));
            inputParams.Add(RequestParameter.IsAnimated(TransformSettings.AnimateModel));
            inputParams.Add(RequestParameter.PlaceOnGround(TransformSettings.PlaceOnGround));
            inputParams.Add(RequestParameter.SerializeAsset(TransformSettings.SerializeAsset || forceSerialize));
            AnythingMaker.Make(result.data.name, inputParams.ToArray());
        }
        #region Browser Card Draw Methods
        protected void DrawCardVoteButton(SearchResult result, ref Rect infoBackdropRect, float voteIconSizeX, float voteIconSizeY, float infoPaddingX, float infoPaddingY, out Rect voteRect)
        {
            voteRect = new Rect(infoBackdropRect.x + infoPaddingX, infoBackdropRect.yMax - infoPaddingY - voteIconSizeY, voteIconSizeX, voteIconSizeY);
            if (GUI.Button(voteRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(result.data.userVote == "upvote" ? StateHeartIcon.activeTexture : StateHeartIcon.inactiveTexture), hover = SetStyleState(StateHeartIcon.hoverTexture) }))
            {
                UserVoteProcessor.FlipUserVote(result, this);
            }
        }

        protected void DrawCardVoteCountLabel(float infoPaddingX, Rect voteRect, int voteCount, float scaleMultiplier)
        {
            var voteStyle = new GUIStyle(VoteStyle) { fontSize = (int)(12 * scaleMultiplier) };
            var voteContent = new GUIContent(TruncateNumber(voteCount));
            var voteLabelWidth = voteStyle.CalcSize(voteContent).x;
            var voteLabelRect = new Rect(voteRect.xMax + (infoPaddingX / 2), voteRect.y, voteLabelWidth, voteRect.height);
            GUI.Label(voteLabelRect, voteContent, voteStyle);
        }

        protected void DrawCardAuthorIconBackground(ref Rect infoBackdropRect, ref Rect thumbnailBackdropRect, float infoPaddingX, float infoPaddingY, out Rect userIconRect, float scaleMultiplier)
        {
            var userIconSize = BaseUserIcon.width / 2.5f * scaleMultiplier;
            userIconRect = new Rect(infoBackdropRect.x + infoPaddingX, thumbnailBackdropRect.yMax + infoPaddingY / 1.5f, userIconSize, userIconSize);
            GUI.DrawTexture(userIconRect, TintedUserIcon);
        }


        protected void DrawCardListIcon(SearchResult result, Rect infoBackdropRect, float infoPaddingX, float infoPaddingY, float scaleMultiplier)
        {
            //Draw List Icon 
            var listIconSizeX = BaseCollectionIcon.width / 2.5f * scaleMultiplier;
            var listIconSizeY = BaseCollectionIcon.height / 2.5f * scaleMultiplier;
            var listRect = new Rect(infoBackdropRect.xMax - infoPaddingX - listIconSizeX, infoBackdropRect.yMax - infoPaddingY - listIconSizeY, listIconSizeX, listIconSizeY);
            if (GUI.Button(listRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateCollectionIcon.activeTexture), hover = SetStyleState(StateCollectionIcon.hoverTexture) }))
            {
                CollectionProcessor.GetCollectionNames(CollectionReceived, this);
                void CollectionReceived(string[] results)
                {
                    existingCollections = results;
                    AnythingSubwindow.OpenWindow("Add to Collection", new Vector2(Mathf.Max(position.width - 80, 450), 260), DrawCollectionWindow, position, result);
                }
            }
        }

        protected void DrawInfoIcon(SearchResult result, Rect thumbnailBackdropRect, float infoPaddingX, float infoPaddingY, float scaleMultiplier)
        {
            //Draw List Icon 
            var infoIconSizeX = BaseInfoIcon.width / 2.5f * scaleMultiplier;
            var infoIconSizeY = BaseInfoIcon.height / 2.5f * scaleMultiplier;
            var infoRect = new Rect(thumbnailBackdropRect.xMax - infoPaddingX - infoIconSizeX, thumbnailBackdropRect.yMax + infoPaddingY, infoIconSizeX, infoIconSizeY);
            if (GUI.Button(infoRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateInfoIcon.activeTexture), hover = SetStyleState(StateInfoIcon.hoverTexture) }))
            {
                selectedResult = result;
            }
        }

        protected void DrawCardAnimationStatusIcon(Rect thumbnailBackdropRect, float infoPaddingX, float infoPaddingY, float scaleMultiplier)
        {
            //Top Left Animated Icon Indicator
            var animatedIconSizeX = BaseAnimatedIcon.width / 2.5f * scaleMultiplier;
            var animatedIconSizeY = BaseAnimatedIcon.height / 2.5f * scaleMultiplier;
            var animatedRect = new Rect(thumbnailBackdropRect.x + infoPaddingX, thumbnailBackdropRect.y + infoPaddingY, animatedIconSizeX, animatedIconSizeY);
            GUI.DrawTexture(animatedRect, BaseAnimatedIcon);
        }

        protected static void FindCategoryIcon(CreationSearchCategory category, out Texture2D categoryIcon)
        {
            categoryIcon = category switch
            {
                CreationSearchCategory.MODELS => BaseCardObjectIcon,
                CreationSearchCategory.WORLDS => BaseCardWorldIcon,
                CreationSearchCategory.LIGHTING => BaseCardLightingIcon,
                CreationSearchCategory.COLLECTION => BaseCardCollectionIcon,
                _ => null
            };
        }

        protected void DrawCardCategoryIcon(Rect thumbnailBackdropRect, float infoPaddingX, float infoPaddingY, Texture2D categoryIcon, float scaleMultiplier)
        {
            var categoryIconSizeX = categoryIcon.width / 2.5f * scaleMultiplier;
            var categoryIconSizeY = categoryIcon.height / 2.5f * scaleMultiplier;
            var categoryRect = new Rect(thumbnailBackdropRect.xMax - infoPaddingX - categoryIconSizeX, thumbnailBackdropRect.y + infoPaddingY, categoryIconSizeX, categoryIconSizeY);
            if (categoryIcon != null) GUI.DrawTexture(categoryRect, categoryIcon);
        }

        protected void DrawCardModelNameLabel(GUIContent modelName, float infoPaddingX, float buttonRightEdge, ref Rect userIconRect, out Rect modelNameLabelRect, float scaleMultiplier)
        {
            var modelNameStyle = new GUIStyle(ModelNameStyle) { fontSize = (int)(12 * scaleMultiplier) };
            var modelNameXPos = userIconRect.xMax + (infoPaddingX / 2);
            var modelNameLabelWidth = Mathf.Min(modelNameStyle.CalcSize(modelName).x, buttonRightEdge - modelNameXPos);
            modelNameLabelRect = new Rect(modelNameXPos, userIconRect.y, modelNameLabelWidth, userIconRect.height / 2);
            GUI.Label(modelNameLabelRect, modelName, modelNameStyle);
        }

        protected void DrawCardAuthorLabel(GUIContent authorName, float infoPaddingX, float buttonRightEdge, Rect userIconRect, Rect modelNameLabelRect, float scaleMultiplier)
        {
            //Draw Author Label
            var authorStyle = new GUIStyle(AuthorNameStyle) { fontSize = (int)(10 * scaleMultiplier) };
            var authorNameXPos = userIconRect.xMax + (infoPaddingX / 2);
            var authorNameLabelWidth = Mathf.Min(authorStyle.CalcSize(authorName).x, buttonRightEdge - authorNameXPos);
            var authorNameLabelRect = new Rect(authorNameXPos, modelNameLabelRect.yMax, authorNameLabelWidth, userIconRect.height / 2);
            GUI.Label(authorNameLabelRect, authorName, authorStyle);
        }
        #endregion
        protected void DrawGrid<T>(List<T> results, int cellCount, float cellWidth, float cellHeight, Action<List<T>, float, float, float, float, int, float> drawCellFunction, float scaleMultiplier = 1f)
        {
            if (cellCount == 0) return;

            var internalMultiplier = 1.5f;
            var buttonWidth = cellWidth * internalMultiplier * scaleMultiplier;
            var buttonHeight = cellHeight * internalMultiplier * scaleMultiplier;
            var aspectRatio = cellHeight / cellWidth;

            var verticalMargin = 5 * internalMultiplier;
            var horizontalMargin = 5 * internalMultiplier;
            float scrollBarAllowance = 6;
            var buttonWidthWithMargin = buttonWidth + horizontalMargin;
            var resultsPerLine = Mathf.Floor((position.width - horizontalMargin) / buttonWidthWithMargin);
            if (resultsPerLine == 0)
            {
                resultsPerLine = 1;
                var scalingFix = scaleMultiplier;
                if (buttonWidth > position.width)
                {
                    scalingFix = (position.width / cellWidth) / internalMultiplier;
                    buttonWidth = position.width;
                    buttonHeight = buttonWidth * aspectRatio;
                    buttonWidthWithMargin = buttonWidth + horizontalMargin;
                }
                scaleMultiplier = scalingFix;
            }
            var rows = (int)Math.Ceiling(cellCount / resultsPerLine);
            var actualBlockWidth = (resultsPerLine * buttonWidthWithMargin) + horizontalMargin;
            var outerRemainder = position.width - actualBlockWidth;
            var remainderMargin = outerRemainder / 2;

            var cardIndex = 0;

            var lastRect = GUILayoutUtility.GetLastRect();
            var gridArea = new Rect(0, lastRect.yMax, position.width + scrollBarAllowance, (buttonHeight * rows) + (verticalMargin * rows));
            var view = new Rect(0, lastRect.yMax, position.width, position.height - lastRect.yMax);
            newScrollPosition = GUI.BeginScrollView(view, newScrollPosition, gridArea, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

            if (copiedToKeyboard)
            {
                if (!copiedRect.Contains(Event.current.mousePosition))
                {
                    copiedToKeyboard = false;
                }
            }
            var scrollViewRect = new Rect(new Vector2(view.x, view.y + newScrollPosition.y), view.size);


            //It through rows and draw 
            for (var yPos = 0; yPos < rows; yPos++)
            {

                var rowCoord = view.yMin + (yPos * buttonHeight) + (verticalMargin * yPos);

                if (rowCoord > scrollViewRect.yMax) continue;
                if (rowCoord + buttonHeight < scrollViewRect.yMin) continue;
                GUI.DrawTexture(lastRect, TintedUserIcon);
                // if (rowCoord * buttonHeight > gridArea.height) break;
                for (var xPos = 0; xPos < resultsPerLine; xPos++)
                {
                    var columnCoord = (xPos * buttonWidthWithMargin) + horizontalMargin + (remainderMargin - scrollBarAllowance);
                    var index = (yPos * (int)resultsPerLine) + xPos;

                    if (results.Count > index)
                    {
                        drawCellFunction(results, columnCoord, rowCoord, buttonWidth, buttonHeight, (yPos * (int)Mathf.FloorToInt(resultsPerLine)) + xPos, scaleMultiplier);
                    }
                    else
                    {
                        break;
                    }
                    cardIndex++;
                }

            }
            GUI.EndScrollView();
        }
        private Vector2 tagScroll = Vector2.zero;
        protected void DrawDetails()
        {
            Event e = Event.current;

            #region Back Button
            var backButtonStyle = new GUIStyle(iconStyle)
            {
                font = GetPoppinsFont(PoppinsStyle.Medium),
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                wordWrap = true,
                normal = new GUIStyleState
                {
                    textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                }
            };

            var backText = new GUIContent("Back", StateBackIcon.activeTexture);
            var backTextSize = backButtonStyle.CalcSize(backText);

            var backRect = GUILayoutUtility.GetRect(position.width, backTextSize.y * 1.5f);

            var margin = (backRect.height - backTextSize.y) / 2;

            var backIconRect = new Rect(backRect.x + margin, backRect.y + margin, backTextSize.x, backTextSize.y);
            if (backIconRect.Contains(e.mousePosition))
            {
                backText.image = StateBackIcon.hoverTexture;
                backButtonStyle.normal = SetStyleState(EditorGUIUtility.isProSkin ? HexToColour("606162") : HexToColour("EDEEEC"));
            }

            if (GUI.Button(backIconRect, backText, backButtonStyle))
            {
                selectedResult = null;
                return;
            }
            #endregion Back Button

            #region Thumbnail
            var thumbnailRect = GUILayoutUtility.GetRect(position.width, position.width * (3f / 5f));
            GUI.DrawTexture(thumbnailRect, BaseDetailsBackdrop, ScaleMode.StretchToFill);
            GUI.DrawTexture(thumbnailRect, selectedResult.Thumbnail, ScaleMode.ScaleToFit);
            #endregion Thumbnail

            #region Intro
            var introRect = GUILayoutUtility.GetRect(position.width, 60f);

            var userIconSize = introRect.height * (2f / 3f);
            var padding = (introRect.height - userIconSize) / 2;
            var userIconRect = new Rect(introRect.x + padding, introRect.y + padding, userIconSize, userIconSize);
            GUI.DrawTexture(userIconRect, TintedUserIcon);


            var modelName = new GUIContent($"{selectedResult.name}");
            var modelNameStyle = new GUIStyle(ModelNameStyle) { fontSize = 16 };
            var modelNameXPos = userIconRect.xMax + padding / 2;
            var modelNameLabelWidth = modelNameStyle.CalcSize(modelName).x;
            var modelNameLabelRect = new Rect(modelNameXPos, userIconRect.y, modelNameLabelWidth, userIconRect.height / 2);
            GUI.Label(modelNameLabelRect, modelName, modelNameStyle);


            var authorName = new GUIContent(selectedResult.data.author);
            var authorStyle = new GUIStyle(AuthorNameStyle) { fontSize = 10 };
            var authorNameXPos = userIconRect.xMax + padding / 2;
            var authorNameLabelWidth = authorStyle.CalcSize(authorName).x;
            var authorNameLabelRect = new Rect(authorNameXPos, modelNameLabelRect.yMax, authorNameLabelWidth, userIconRect.height / 2);
            GUI.Label(authorNameLabelRect, authorName, authorStyle);


            var iconSize = BaseReportIcon.height;
            padding = (introRect.height - iconSize) / 2;
            var reportRect = new Rect(introRect.xMax - (iconSize * 1.5f), introRect.y + padding, iconSize, iconSize);
            if (GUI.Button(reportRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateReportIcon.activeTexture), hover = SetStyleState(StateReportIcon.hoverTexture) }))
            {
                AnythingSubwindow.OpenWindow($"Report {selectedResult.DisplayName}", new Vector2(Mathf.Max(position.width - 80, 450), 260), DrawReportWindow, position, selectedResult);
            }

            var listRect = new Rect(reportRect.x - (iconSize * 1.5f), introRect.y + padding, iconSize, iconSize);
            if (GUI.Button(listRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(StateCollectionIcon.activeTexture), hover = SetStyleState(StateCollectionIcon.hoverTexture) }))
            {
                CollectionProcessor.GetCollectionNames(CollectionReceived, this);
                void CollectionReceived(string[] results)
                {
                    existingCollections = results;
                    AnythingSubwindow.OpenWindow("Add to Collection", new Vector2(Mathf.Max(position.width - 80, 450), 260), DrawCollectionWindow, position, selectedResult);
                }
            }

            var voteStyle = new GUIStyle(VoteStyle) { fontSize = 14 };
            var voteContent = new GUIContent(TruncateNumber(selectedResult.data.voteScore));
            var voteLabelSize = voteStyle.CalcSize(voteContent);
            var voteLabelRect = new Rect(listRect.x - voteLabelSize.x - (iconSize * 0.5f), introRect.y + (introRect.height - voteLabelSize.y) / 2, voteLabelSize.x, voteLabelSize.y);
            GUI.Label(voteLabelRect, voteContent, voteStyle);

            var voteRect = new Rect(voteLabelRect.x - (iconSize * 1.5f), introRect.y + padding, iconSize, iconSize);
            if (GUI.Button(voteRect, "", new GUIStyle(iconStyle) { normal = SetStyleState(selectedResult.data.userVote == "upvote" ? StateHeartIcon.activeTexture : StateHeartIcon.inactiveTexture), hover = SetStyleState(StateHeartIcon.hoverTexture) }))
            {
                UserVoteProcessor.FlipUserVote(selectedResult, this);
            }
            #endregion Intro

            #region Details
            var detailsRect = GUILayoutUtility.GetRect(position.width, 30f);
            padding = (detailsRect.height - iconSize) / 2;
            float nextXPosStart = detailsRect.x;

            if (selectedResult.data.type != null)
            {
                var typeRect = new Rect(nextXPosStart + padding, detailsRect.y + padding, iconSize, iconSize);
                GUI.DrawTexture(typeRect, BaseTypeIcon, ScaleMode.ScaleToFit);

                var typeContent = new GUIContent($"{selectedResult.data.type.DeepClean().CapitaliseAll()}");
                var typeLabelSize = voteStyle.CalcSize(typeContent);
                var typeLabelRect = new Rect(typeRect.xMax + padding, detailsRect.y + (detailsRect.height - typeLabelSize.y) / 2, typeLabelSize.x, typeLabelSize.y);
                GUI.Label(typeLabelRect, typeContent, VoteStyle);
                nextXPosStart = typeLabelRect.xMax;
            }

            if (selectedResult.data.detail != null)
            {
                var polyCountRect = new Rect(nextXPosStart + padding, detailsRect.y + padding, iconSize, iconSize);
                GUI.DrawTexture(polyCountRect, BasePolyCountIcon, ScaleMode.ScaleToFit);

                var polyCountContent = new GUIContent($"{selectedResult.data.detail.DeepClean().CapitaliseAll()} Poly Count");
                var polyCountLabelSize = voteStyle.CalcSize(polyCountContent);
                var polyCountLabelRect = new Rect(polyCountRect.xMax + padding, detailsRect.y + (detailsRect.height - polyCountLabelSize.y) / 2, polyCountLabelSize.x, polyCountLabelSize.y);
                GUI.Label(polyCountLabelRect, polyCountContent, VoteStyle);
                nextXPosStart = polyCountLabelRect.xMax;
            }

            var licenseRect = new Rect(nextXPosStart + padding, detailsRect.y + padding, iconSize, iconSize);
            GUI.DrawTexture(licenseRect, BaseLicenseIcon, ScaleMode.ScaleToFit);

            var licenseContent = new GUIContent($"CC BY 4.0", "You must give appropriate credit, provide a link to the license, and indicate if changes were made. You may do so in any reasonable manner, but not in any way that suggests the licensor endorses you or your use. [https://creativecommons.org/licenses/by/4.0/]");
            var licenseLabelSize = voteStyle.CalcSize(licenseContent);
            var licenseLabelRect = new Rect(licenseRect.xMax + padding, detailsRect.y + (detailsRect.height - licenseLabelSize.y) / 2, licenseLabelSize.x, licenseLabelSize.y);
            GUI.Label(licenseLabelRect, licenseContent, VoteStyle);

            if (selectedResult.data.themeCategories != null && selectedResult.data.themeCategories.Any())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUILayout.Label("CATEGORY", new GUIStyle(BodyLabelStyle) { alignment = TextAnchor.UpperLeft }, GUILayout.Width(position.width / 6));
                foreach (string category in selectedResult.data.themeCategories)
                {
                    DrawRoundedLabel(category.CapitaliseAll());
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            if (selectedResult.data.tags != null && selectedResult.data.tags.Any())
            {
                GUILayout.BeginHorizontal(); // TODO: Wrap tags overflowing
                GUILayout.Space(10);
                GUILayout.Label("TAGS", new GUIStyle(BodyLabelStyle) { alignment = TextAnchor.UpperLeft }, GUILayout.Width(position.width / 6));

                tagScroll = GUILayout.BeginScrollView(tagScroll, GUILayout.ExpandHeight(false));
                GUILayout.BeginHorizontal();
                foreach (string tag in selectedResult.data.tags)
                {
                    DrawRoundedLabel(tag.CapitaliseAll());
                    GUILayout.Space(2);
                    var rect = GUILayoutUtility.GetLastRect();
                }
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
            }

            if (selectedResult.data.habitats != null && selectedResult.data.habitats.Any())
            {
                GUILayout.BeginHorizontal(); // TODO: Wrap tags overflowing
                GUILayout.Space(10);
                GUILayout.Label("HABITATS", new GUIStyle(BodyLabelStyle) { alignment = TextAnchor.UpperLeft }, GUILayout.Width(position.width / 6));
                foreach (string habitat in selectedResult.data.habitats)
                {
                    DrawRoundedLabel(habitat.CapitaliseAll());
                    GUILayout.Space(2);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            #endregion Details

            GUILayout.Space(32);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            if (DrawRoundedButton("Add Model to Scene", 1.2f, 8f, 1f, true, PoppinsStyle.Bold))
            {
                MakeResult(selectedResult);
            }
            GUILayout.Space(10);
            if (DrawRoundedButton("Add Model and Save Assets", 1.2f, 8f, 1f, true, PoppinsStyle.Bold))
            {
                MakeResult(selectedResult, true);
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
        }



        public string[] existingCollections;
        public int existingCollectionIndex = 0;
        public string collectionSearchTerm;

        public void SetExistingIndex(int i)
        {
            existingCollectionIndex=i;
        }

        protected void DrawCollectionWindow(AnythingEditor window, SearchResult result)
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (existingCollections.Any())
            {
                GUILayout.BeginVertical(GUILayout.MaxWidth(window.position.width / 2));
                GUILayout.Label("Add to a pre-existing collection?", new GUIStyle(HeaderLabelStyle) { fontSize = 12 });
                
                List<DropdownOption> newCollections = new List<DropdownOption>();

                for (int i = 0; i < existingCollections.Length; i++)
                {
                    string collectionName = existingCollections[i];
                    int index = i;
                    var dropdown = new DropdownOption()
                    {
                        dataEndpoint = index,
                        label = collectionName,
                        function = () => existingCollectionIndex = index
                    };
                        newCollections.Add(dropdown);
                }
                DropdownOption[] createdCollections = newCollections.ToArray();


                DrawDropdown(new Vector2(0, 20), createdCollections, existingCollectionIndex);
                GUILayout.Space(10);

                if (DrawRoundedButton("Add to Collection", 1f, 8f, 2f / 3f, true))
                {
                    CollectionProcessor.AddToCollection(MyWorldEditor.Instance.RefreshCollectionResults, result, existingCollections[existingCollectionIndex], this);
                    window.Close();
                }
                GUILayout.EndVertical();
            }
            GUILayout.BeginVertical(GUILayout.MaxWidth(window.position.width / 2));
            GUILayout.Label("...or make a new collection?", new GUIStyle(HeaderLabelStyle) { fontSize = 12 });
            collectionSearchTerm = GUILayout.TextField(collectionSearchTerm);
            GUILayout.Space(10);

            if (DrawRoundedButton("Create Collection", 1f, 8f, 2f / 3f, true))
            {
                CollectionProcessor.AddToCollection(MyWorldEditor.Instance.RefreshCollectionResults, result, collectionSearchTerm, this);

                collectionSearchTerm = "";
                window.Close();
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }

        protected void SetupResetEditorWindow(ResetMode resetMode)
        {
            editorResetMode = resetMode;
            AnythingSubwindow.OpenWindow($"Reset {editorResetMode}?", new Vector2(300f, 150f), DrawResetEditorWindow, position);
        }

        private ResetMode editorResetMode;
        protected void DrawResetEditorWindow(AnythingEditor window)
        {
            string inlineResetText = editorResetMode switch
            {
                ResetMode.Scene => "the scene",
                ResetMode.Creator => "the creator",
                ResetMode.All => "everything",
                _ => ""
            };

            GUILayout.FlexibleSpace();
            GUILayout.Label($"Are you sure you want to reset {inlineResetText}?", new GUIStyle(HeaderLabelStyle) { fontSize = 16 });
            
            if (DrawRoundedButton($"Reset {editorResetMode}", 1f, 8f, 2f / 3f, true))
            {
                ResetAnythingWorld(editorResetMode);
                window.Close();
            }
            GUILayout.FlexibleSpace();
        }

        bool reportSent;
        ReportProcessor.ReportReason reportReason = ReportProcessor.ReportReason.COPYRIGHT;
        protected void DrawReportWindow(AnythingEditor window, SearchResult result)
        {
            GUILayout.FlexibleSpace();
            var flagRect = GUILayoutUtility.GetRect(window.position.width, 30f);
            GUI.DrawTexture(flagRect, TintedReportGraphic, ScaleMode.ScaleToFit);
            GUILayout.Label(reportSent ? "Thank you for your report! :)" : "Please select one of the options below:", new GUIStyle(BodyLabelStyle) { alignment = TextAnchor.MiddleCenter });

            if (!reportSent)
            {
                DropdownOption[] reportOptions = new DropdownOption[]
                {
                new DropdownOption
                {
                    dataEndpoint = ReportProcessor.ReportReason.COPYRIGHT,
                    label = "Copyright",
                    function = () => SetReportStatus(ReportProcessor.ReportReason.COPYRIGHT)
                },
                new DropdownOption
                {
                    dataEndpoint = ReportProcessor.ReportReason.EMPTY,
                    label = "Empty Model",
                    function = () => SetReportStatus(ReportProcessor.ReportReason.EMPTY)
                },
                new DropdownOption
                {
                    dataEndpoint = ReportProcessor.ReportReason.INAPPROPRIATE,
                    label = "Inappropriate",
                    function = () => SetReportStatus(ReportProcessor.ReportReason.INAPPROPRIATE)
                },
                new DropdownOption
                {
                    dataEndpoint = ReportProcessor.ReportReason.QUALITY,
                    label = "Poor Quality",
                    function = () => SetReportStatus(ReportProcessor.ReportReason.QUALITY)
                },
                new DropdownOption
                {
                    dataEndpoint = ReportProcessor.ReportReason.OTHER,
                    label = "Other",
                    function = () => SetReportStatus(ReportProcessor.ReportReason.OTHER)
                }
                };
                DrawDropdown(new Vector2(0, 20), reportOptions, reportReason);

                GUILayout.Space(10);
                if (DrawRoundedButton("Send Report", 1f, 8f, 2f / 3f, true))
                {
                    ReportProcessor.SendReport(DrawPostReportSent, result, reportReason, this);
                }
            }

            void DrawPostReportSent()
            {
                CoroutineExtension.StartEditorCoroutine(CoroutineExtension.WaitThen(3f, () => reportSent = true, () => {
                    window.Close();
                    reportSent = false;
                }), this);
            }

            void SetReportStatus(ReportProcessor.ReportReason reason)
            {
                reportReason = reason;
            }
            GUILayout.FlexibleSpace();
        }

        protected bool DrawRoundedButton(string buttonContent, float fontSizeModifier = 1f, float heightPadding = 0f, float widthModifier = 1f, bool centre = false, PoppinsStyle fontOverride = PoppinsStyle.Regular)
        {
            Event e = Event.current;
            GUIContent content = new GUIContent(buttonContent);
            GUIStyle buttonStyle = new GUIStyle(BodyLabelStyle) { fontSize = (int)(BodyLabelStyle.fontSize * fontSizeModifier), alignment = TextAnchor.MiddleCenter, normal = SetStyleState(EditorGUIUtility.isProSkin ? Color.white : Color.black), hover = SetStyleState(EditorGUIUtility.isProSkin ? Color.white : Color.black), font = GetPoppinsFont(fontOverride) };

            var buttonHeight = buttonStyle.CalcSize(content).y + heightPadding;
            var buttonRect = GUILayoutUtility.GetRect((buttonStyle.CalcSize(content).x + 16) * widthModifier, buttonHeight);
            var initialWidth = buttonRect.width;
            var initialPosition = buttonRect.position;
            buttonRect.width *= widthModifier;
            if (centre) { buttonRect.x = initialPosition.x + (initialWidth - buttonRect.width) / 2; }

            var textureScalar = buttonHeight / BaseRoundButtonMain.height;
            var buttonLeftEdgeRect = new Rect(buttonRect.xMin, buttonRect.yMin, BaseRoundButtonLeft.width * textureScalar, buttonHeight);
            var buttonRightEdgeRect = new Rect(buttonRect.xMax - (BaseInputFieldRight.width * textureScalar), buttonRect.yMin, BaseRoundButtonRight.width * textureScalar, buttonHeight);
            var buttonMainRect = new Rect(buttonLeftEdgeRect.xMax, buttonRect.y, buttonRect.width - buttonLeftEdgeRect.width - buttonRightEdgeRect.width, buttonHeight);

            GUI.DrawTexture(buttonLeftEdgeRect, buttonRect.Contains(e.mousePosition) ? StateRoundButtonLeft.hoverTexture : StateRoundButtonLeft.inactiveTexture);
            GUI.DrawTexture(buttonMainRect, buttonRect.Contains(e.mousePosition) ? StateRoundButtonMain.hoverTexture : StateRoundButtonMain.inactiveTexture);
            GUI.DrawTexture(buttonRightEdgeRect, buttonRect.Contains(e.mousePosition) ? StateRoundButtonRight.hoverTexture : StateRoundButtonRight.inactiveTexture);
            return GUI.Button(buttonRect, buttonContent, buttonStyle);
        }

        protected void DrawRoundedLabel(string labelContent, float fontSizeModifier = 1f, float heightPadding = 0f, float widthModifier = 1f, bool centre = false, PoppinsStyle fontOverride = PoppinsStyle.Regular)
        {
            GUIContent content = new GUIContent(labelContent);
            GUIStyle labelStyle = new GUIStyle(BodyLabelStyle) { fontSize = (int)(BodyLabelStyle.fontSize * fontSizeModifier), alignment = TextAnchor.MiddleCenter, normal = SetStyleState(EditorGUIUtility.isProSkin ? Color.white : Color.black), font = GetPoppinsFont(fontOverride) };

            var labelHeight = labelStyle.CalcSize(content).y + heightPadding;
            var labelRect = GUILayoutUtility.GetRect(labelStyle.CalcSize(content).x + 16, labelHeight);
            var initialWidth = labelRect.width;
            var initialPosition = labelRect.position;
            labelRect.width *= widthModifier;
            if (centre) { labelRect.x = initialPosition.x + (initialWidth - labelRect.width) / 2; }

            var textureScalar = labelHeight / BaseRoundButtonMain.height;
            var labelLeftEdgeRect = new Rect(labelRect.xMin, labelRect.yMin, BaseRoundButtonLeft.width * textureScalar, labelHeight);
            var labelRightEdgeRect = new Rect(labelRect.xMax - (BaseInputFieldRight.width * textureScalar), labelRect.yMin, BaseRoundButtonRight.width * textureScalar, labelHeight);
            var labelMainRect = new Rect(labelLeftEdgeRect.xMax, labelRect.y, labelRect.width - labelLeftEdgeRect.width - labelRightEdgeRect.width, labelHeight);

            GUI.DrawTexture(labelLeftEdgeRect, StateRoundButtonLeft.inactiveTexture);
            GUI.DrawTexture(labelMainRect, StateRoundButtonMain.inactiveTexture);
            GUI.DrawTexture(labelRightEdgeRect, StateRoundButtonRight.inactiveTexture);
            GUI.Label(labelRect, labelContent, labelStyle);
        }
        #endregion Editor Drawing

        #region Helper Functions
        protected enum ResetMode
        {
            Scene, Creator, All
        }

        protected virtual void ResetAnythingWorld(ResetMode resetMode)
        {
            if (resetMode != ResetMode.Scene)
            {
                resultThumbnailMultiplier = 1f;
                searchModeFailReason = "";
                searchMode = SearchMode.IDLE;

                currentAnimationFilter = AnimatedDropdownOption.BOTH;
                currentCategory = CategoryDropdownOption.ALL;
                currentSortingMethod = SortingDropdownOption.MostRelevant;


                var browser = GetWindow(typeof(TransformSettingsEditor), false, "Transform Settings") as TransformSettingsEditor;
                browser.Close();

                TransformSettings.ClearSettings();

                AssignDefaultBehavioursFromScriptable();
            }

            if (resetMode != ResetMode.Creator)
            {
                ModelDataInspector[] createdModels = FindObjectsOfType<ModelDataInspector>();
                for (int i = createdModels.Length - 1; i >= 0; i--)
                {
                    Utilities.Destroy.GameObject(createdModels[i].gameObject);
                }
                SimpleGrid.Reset();
            }
        }

        public void UpdateSearchResults(SearchResult[] results, string onEmpty)
        {
            searchResults = new List<SearchResult>();
            searchMode = SearchMode.SUCCESS;

            if (results == null || results.Length == 0)
            {
                searchMode = SearchMode.FAILURE;
                searchModeFailReason = onEmpty;
                return;
            }

            if (results.Length > 0)
            {
                searchResults = results.ToList();
                FilterSearchResult(searchResults);
            }
        }

        public void UpdateSearchResults(ref List<SearchResult> unfiltered, ref List<SearchResult> filtered, string onEmpty)
        {
            unfiltered = new List<SearchResult>();

            if (unfiltered == null || unfiltered.Count == 0)
            {
                return;
            }

            if (unfiltered.Count > 0)
            {
                searchResults = unfiltered.ToList();
                filtered = FilterAndReturnResults(searchResults);
            }
        }
        public List<SearchResult> FilterAndReturnResults(List<SearchResult> results)
        {
            var filtered = FilterByAnimation(results);
            filtered = FilterByCategory(filtered);
            filtered = SortResults(filtered);
            filteredResults = filtered;
            return filteredResults;
        }
        public void FilterSearchResult(List<SearchResult> results)
        {
            var filtered = FilterByAnimation(results);
            filtered = FilterByCategory(filtered);
            filtered = SortResults(filtered);
            filteredResults = filtered;
            if (filteredResults == null)
            {
                searchMode = SearchMode.FAILURE;
                searchModeFailReason = "We couldn't find any models matching those filters.";
            }
            Repaint();
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }

        /// <summary>
        /// Filters a list of search results by category.
        /// </summary>
        /// <param name="results">The list of search results to be filtered.</param>
        /// <returns>A filtered list of search results based on the current category.</returns>
        private List<SearchResult> FilterByCategory(List<SearchResult> results)
        {
            var categoryFilter = new List<SearchResult>();

            if (currentCategory == CategoryDropdownOption.ALL) return results;

            string categoryWord = currentCategory.ToString().ToLower();
            categoryFilter = (from result in results where result.data.themeCategories.Contains(categoryWord) select result).ToList();
          
            return categoryFilter;
        }

        private List<SearchResult> FilterByAnimation(List<SearchResult> results)
        {
            List<SearchResult> animationFilter = new List<SearchResult>();
            switch (currentAnimationFilter)
            {
                case AnimatedDropdownOption.BOTH:
                    animationFilter = results;
                    break;
                case AnimatedDropdownOption.ANIMATED:
                    animationFilter = (from result in results where result.isAnimated select result).ToList();
                    break;
                case AnimatedDropdownOption.STILL:
                    animationFilter = (from result in results where !result.isAnimated select result).ToList();
                    break;
            }
            return animationFilter;
        }

        private List<SearchResult> SortResults(List<SearchResult> results)
        {
            List<SearchResult> sortedResults = new List<SearchResult>();
            switch (currentSortingMethod)
            {
                case SortingDropdownOption.MostRelevant:
                    sortedResults = results;
                    break;
                /* case SortingDropdownOption.MostPopular:
                     sortedResults = (from result in results orderby result.data.popularity select result).ToList();
                     break;
                */
                case SortingDropdownOption.MostLiked:
                    sortedResults = (from result in results orderby result.data.voteScore select result).ToList();
                    sortedResults.Reverse();
                    break;
                case SortingDropdownOption.MyList:
                    sortedResults = (from result in results where result.data.userVote == "upvote" select result).ToList();
                    break;
                case SortingDropdownOption.AtoZ:
                    sortedResults = (from result in results orderby result.data.name select result).ToList();
                    break;
                case SortingDropdownOption.ZtoA:
                    sortedResults = (from result in results orderby result.data.name select result).ToList();
                    sortedResults.Reverse();
                    break;
            }

            return sortedResults;
        }

        protected string TruncateNumber(int number)
        {
            switch (number)
            {
                case var _ when number >= 100000000:
                    return (number / 1000000).ToString("#,0M");
                case var _ when number >= 10000000:
                    return (number / 1000000).ToString("0.#") + "M";
                case var _ when number >= 100000:
                    return (number / 1000).ToString("#,0K");
                case var _ when number >= 10000:
                    return (number / 1000).ToString("0.#") + "K";
                default:
                    return number.ToString("#,0");
            };
        }
        #endregion Helper Functions
    }

    public class AnythingSubwindow : AnythingEditor
    {
        protected static Rect windowPosition;
        protected static Rect callingWindowScreenPosition;

        protected static bool invokeWithParameter;
        protected static bool resetWindowPosition = true;

        protected static string windowTitle;
        protected static Vector2 windowSize;

        protected static Action<AnythingEditor> windowAction;
        protected static Action<AnythingEditor, SearchResult> windowActionSR;
        protected static SearchResult searchResult;

        public static void OpenWindow(string title, Vector2 size, Action<AnythingEditor, SearchResult> guiAction, Rect callingWindow, SearchResult result)
        {
            callingWindowScreenPosition = GUIUtility.GUIToScreenRect(callingWindow);
            windowTitle = title;
            windowSize = size;
            windowActionSR = guiAction;
            searchResult = result;

            invokeWithParameter = true;
            ShowWindow();
        }

        public static void OpenWindow(string title, Vector2 size, Action<AnythingEditor> guiAction, Rect callingWindow)
        {
            callingWindowScreenPosition = GUIUtility.GUIToScreenRect(callingWindow);
            windowTitle = title;
            windowSize = size;
            windowAction = guiAction;

            invokeWithParameter = false;
            ShowWindow();
        }

        protected static void ShowWindow()
        {
            var window = GetWindow<AnythingSubwindow>(true);
            window.titleContent = new GUIContent(windowTitle);
            window.minSize = window.maxSize = windowSize;

            if (resetWindowPosition)
            {
                resetWindowPosition = false;
                windowPosition = GUIUtility.ScreenToGUIRect(new Rect(callingWindowScreenPosition.x + ((callingWindowScreenPosition.width - window.minSize.x) / 2), callingWindowScreenPosition.y + ((callingWindowScreenPosition.height - window.minSize.y) / 2), 0, 0));
            }
            else
            {
                windowPosition = window.position;
            }
            //If failed to find width give default subvalue;
            if (windowPosition.width == 0) windowPosition.width = 450;
            if (windowPosition.height == 0) windowPosition.height = 260;
            window.position = windowPosition;
        }

        protected new void OnGUI()
        {
            base.OnGUI();
            if (invokeWithParameter) windowActionSR?.Invoke(this, searchResult);
            else windowAction?.Invoke(this);

        }

        protected void OnDestroy()
        {
            resetWindowPosition = true;
            // When the window is destroyed, remove the delegate
            // so that it will no longer do any drawing.
        }
    }
}
