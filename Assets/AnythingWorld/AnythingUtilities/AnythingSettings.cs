                                                                                                                                                                                                                                                                        using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AnythingWorld
{
    public class AnythingSettings : ScriptableObject
    {
        public static AnythingSettings Instance
        {
            get
            {
                var instance = Resources.Load<AnythingSettings>("Settings/AnythingSettings");
                if (instance == null)
                {
                    Debug.Log("Instance is null, making new Settings file");
#if UNITY_EDITOR
                    var asset = CreateInstance<AnythingSettings>();
                    AssetDatabase.CreateAsset(asset, "Assets/AnythingWorld/Resources/Settings/AnythingSettings.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    return asset;
#endif
                }
                return instance;
            }
        }

        [SerializeField, Tooltip("Your Anything World API key goes here.")]
        private string apiKey = "";
        [SerializeField, Tooltip("Name of application. Please ensure this value is different for each app.")]
        private string appName = "My Anything World App";
        [SerializeField, Tooltip("Your address for email communications.")]
        public string email = "";
        [Tooltip("Show AW Debug Messages")]
        public bool showDebugMessages = false;
        public static string PackageVersion
        {
            get
            {
                return versionNumber;
            }
        }

        private const string versionNumber = "v1.0.2.4";

        public static string APIKey
        {
            get
            {
                return Instance.apiKey;
            }
            set
            {
                Instance.apiKey = value;
            }
        }
        public static string AppName
        {

            get
            {
                return Instance.appName;
            }
            set
            {
                Instance.appName = value;
            }
        }

        public static string Email
        {
            get
            {
                return Instance.email;
            }
            set
            {
                Instance.email = value;
            }
        }

        public static bool DebugEnabled
        {
            get
            {
                return Instance.showDebugMessages;
            }
            set
            {
                Instance.showDebugMessages = value;
            }
        }

      

        public void ClearSettings()
        {
            apiKey = "";
            appName = "My App";
            email = "";
            showDebugMessages = false;
        }

        public static bool HasEmail()
        {
            if (Instance.email != "")
            {
                return true;
            }
            else
            {
                return false;
            };
        }

        public static bool HasAPIKey
        {
            get
            {
                if (Instance.apiKey != "")
                {
                    return true;
                }
                else
                {
                    return false;
                };
            }
          
        }
    }
}
