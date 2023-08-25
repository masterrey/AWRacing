using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnythingWorld.Utilities.Networking;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace AnythingWorld.Utilities 
{
    public class NetworkConfig
    { 
        private static string ApiKey => AnythingSettings.APIKey;
        private static string AppName => AnythingSettings.AppName;
       
        public static string ApiUrlStem
        {
            get
            {
                return AW_API_STEM;
            }
        }
        private const string AW_API_STEM = "https://api.anything.world";
        public static string GetNameEnpointUri(string modelName)
        {
            return $"{AW_API_STEM}/anything?key={ApiKey}&app={Encode(AppName)}&name={Encode(modelName)}";
        }
        public static string SearchUri(string searchTerm, string sortingType)
        {
            return $"{AW_API_STEM}/anything?key={ApiKey}&search={Encode(searchTerm)}{sortingType}";
        }
        public static string FeaturedUri()
        {
            return $"{AW_API_STEM}/featured?key={ApiKey}";
        }
        public static string VoteUri(string voteType, string name, string id)
        {
            return $"{AW_API_STEM}/vote?key={ApiKey}&type={voteType}&name={name}&guid={id}"; 
        }
        public static string MyLikesUri()
        {
            return $"{AW_API_STEM}/voted?key={ApiKey}";
        }
        public static string ReportUri(string name, string id, string reason)
        {
            return $"{AW_API_STEM}/report?key={ApiKey}&name={name}&guid={id}&reason={reason}";
        }

        public static string SpeechToTextUri(string locale = "en-US")
        {
            return $"{AW_API_STEM}/speech-to-text?key={ApiKey}&locale={locale}";
        }
        public static string SpeechToCommandUri(string locale = "en-US")
        {
            return $"{AW_API_STEM}/parse-speech-command?key={ApiKey}&locale={locale}";
        }

        public static string TextToCommandUri()
        {
            return $"{AW_API_STEM}/parse-text-command?key={ApiKey}";
        }
        #region Collections
        public static string UserCollectionsUri(bool namesOnly)
        {
            return $"{AW_API_STEM}/user-collections?key={ApiKey}&onlyName={namesOnly.ToString().ToLower()}";
        }
        public static string AddCollectionUri(string collection)
        {
            return $"{AW_API_STEM}/add-collection?key={ApiKey}&collection={Encode(collection)}";
        }
        public static string RemoveCollectionUri(string collection)
        {
            return $"{AW_API_STEM}/remove-collection?key={ApiKey}&collection={Encode(collection)}";
        }
        public static string AddToCollectionUri(string collection, string name, string id)
        {
            return $"{AW_API_STEM}/add-to-collection?key={ApiKey}&collection={Encode(collection)}&name={name}&guid={id}";
        }
        public static string RemoveFromCollectionUri(string collection, string name, string id)
        {
            return $"{AW_API_STEM}/remove-from-collection?key={ApiKey}&collection={Encode(collection)}&name={name}&guid={id}";
        }
        #endregion Collections

        private static string Encode(string str)
        {
            return UrlEncoder.Encode(str);
        }

    }
}
