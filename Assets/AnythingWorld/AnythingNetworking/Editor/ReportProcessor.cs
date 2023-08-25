using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using AnythingWorld.Utilities.Networking;

using System.Collections;

using UnityEngine;
using UnityEngine.Networking;

namespace AnythingWorld.Networking.Editor
{
    public static class ReportProcessor
    {
        public enum ReportReason
        {
            COPYRIGHT, EMPTY, INAPPROPRIATE, QUALITY, OTHER
        }

        public delegate void ReportSentDelegate();
        private static ReportSentDelegate reportDelegate;

        public static void SendReport(ReportSentDelegate reportSent, SearchResult searchResult, ReportReason reason, object owner) 
        {
            CoroutineExtension.StartEditorCoroutine(SendReportCoroutine(reportSent, searchResult, reason), owner);
        }

        public static IEnumerator SendReportCoroutine(ReportSentDelegate delegateFunc, SearchResult searchResult, ReportReason reason)
        {
            reportDelegate += delegateFunc;
            var reasonString = reason switch
            {
                ReportReason.COPYRIGHT => "copyright",
                ReportReason.EMPTY => "empty",
                ReportReason.INAPPROPRIATE => "inappropriate",
                ReportReason.QUALITY => "poor-quality",
                _ => "other"
            };
            var nameSplit = searchResult.data.name.Split('#');

            UnityWebRequest www;
            var apiCall = NetworkConfig.ReportUri(nameSplit[0], nameSplit[1], reasonString);
#if UNITY_2022
            www = UnityWebRequest.PostWwwForm(apiCall, "");
#else
            www = UnityWebRequest.Post(apiCall, "");
#endif
            www.timeout = 5;
            yield return www.SendWebRequest();
            
            if(www.result == UnityWebRequest.Result.Success)
            {
                if (AnythingSettings.Instance.showDebugMessages) Debug.Log($"Report ({searchResult.data.name} | {reason}) succeeded!");
                reportDelegate?.Invoke();
            }
            else
            {
                try
                {
                    //If supported error format process
                    var error = new NetworkErrorMessage(www);
                    NetworkErrorHandler.HandleError(error);
                }
                catch
                {
                    //Else just debug as not handled by server properly
                    Debug.Log($"Couldn't parse error: {www.downloadHandler.text}");
                }
            }
            reportDelegate -= delegateFunc;
        }
    }
}
