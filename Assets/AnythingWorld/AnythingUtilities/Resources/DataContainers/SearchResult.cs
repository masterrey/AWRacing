using AnythingWorld.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AnythingWorld.Utilities.Data
{
    /// <summary>
    /// Class holding information about search results for the Object Creator panel.
    /// Holds object names and thumbnails, and handles serialization of of thumbnail images across runtime/editor.
    /// </summary>
    [Serializable]
    public class SearchResult
    {
        [SerializeField]
        public bool ResultHasThumbnail = true;
        [SerializeField]
        public string name;
        [SerializeField]
        public bool isAnimated = false;
        [SerializeField]
        public ModelJson data;
        [SerializeField]
        public byte[] texStream;
        [SerializeField]
        int texHeight;
        [SerializeField]
        int texWidth;
        [SerializeField]
        TextureFormat format;
        [SerializeField]
        int mipMapCount;
        //Thumbnails
        public string thumbnailUrl;
        [SerializeField]
        public Texture2D thumbnail;
        public Texture2D Thumbnail
        {
            get
            {
                if (ResultHasThumbnail)
                {
                    if (thumbnail != null)
                    {
                        return thumbnail;
                    }
                    else
                    {
                        if (texStream != null)
                        {
                            var chain = false;
                            if (mipMapCount > 1) chain = true;
                            try
                            {
                                var texCopy = new Texture2D(texWidth, texHeight, format, chain);
                                texCopy.LoadRawTextureData(texStream);
                                texCopy.Apply();

                                if (texCopy != null)
                                {
                                    thumbnail = texCopy;
                                    return thumbnail;
                                }
                                else
                                {
                                    ResultHasThumbnail = false;
                                    return null;
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Error serializing thumbnail for {this.name}: {e}");
                                texStream = null;
                                ResultHasThumbnail = false;
                                return null;
                            }
                        }
                        else
                        {
                            ResultHasThumbnail = false;
                            return null;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            set
            {
                try
                {
                    if (value == null)
                    {
                        ResultHasThumbnail = false;
                        return;
                    }
                    else
                    {
                        texStream = value.GetRawTextureData();
                        texWidth = value.width;
                        texHeight = value.height;
                        format = value.format;
                        mipMapCount = value.mipmapCount;
                        thumbnail = value;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error setting thumbnail for {this.name}: {e}");
                    ResultHasThumbnail = false;
                    return;
                }
            }
        }
        public SearchResult(ModelJson jsonData)
        {
            //Set AWThing json data
            data = jsonData;
            name = data.name;
            GetThumbnailUrl();
        }
        public SearchResult(ModelJson jsonData, AnimationPipeline pipeline)
        {
            //Set AWThing json data
            data = jsonData;
            name = data.name;
            isAnimated = (pipeline != AnimationPipeline.Static);
            GetThumbnailUrl();
        }
        private void GetThumbnailUrl()
        {
            if (data?.thumbnails != null)
            {
                data.thumbnails.TryGetValue("aw_thumbnail_transparent", out thumbnailUrl);
            }
            else
            {
                thumbnailUrl = data.aw_thumbnail;
            }
        }
        public string DisplayName
        {
            get
            {
                return GetDisplayName();
            }
        }

        private string GetDisplayName()
        {
            if (name != null)
            {
                var disp = Regex.Replace(name, @"\d", "");
                disp = disp.Replace("_", " ");
                disp = Regex.Replace(disp, @"\#.*", "");

                var dispArr = disp.ToCharArray();
                var displayName = new string(dispArr);

                var textInfo = new System.Globalization.CultureInfo("en-US", false).TextInfo;

                return textInfo.ToTitleCase(displayName);
            }
            else
            {
                return null;
            }
        }
    }
    
    [Serializable]
    public class CollectionResult
    {

        public string Name;
        public string DisplayName
        {
            get
            {
                if (Name != null)
                {
                    var disp = Regex.Replace(Name, @"\d", "");
                    disp = disp.Replace("_", " ");
                    disp = Regex.Replace(disp, @"\#.*", "");

                    var dispArr = disp.ToCharArray();
                    var displayName = new string(dispArr);

                    var textInfo = new System.Globalization.CultureInfo("en-US", false).TextInfo;

                    return textInfo.ToTitleCase(displayName);
                }
                return null;
            }
        }
        [SerializeField]
        private List<ModelJson> Jsons;
        [SerializeField]
        private List<SearchResult> results;
        public List<SearchResult> Results
        {
            get
            {
                if (results == null)
                {
                    results = new List<SearchResult>();
                    foreach (var json in Jsons)
                    {
                        var result = new SearchResult(json);
                        var animationPipeline = JsonProcessor.ParseAnimationPipeline(json);
                        //Set if model is animated through our standards, used for filtering.
                        if (!(animationPipeline == AnimationPipeline.Static)) result.isAnimated = true;
                        else result.isAnimated = false;
                        results.Add(result);
                    }
                }
                return results;
            }
        }

        [SerializeField]
        private Texture2D[] searchResultThumbnails;
        public Texture2D[] SearchResultThumbnails
        {
            get
            {
                if(searchResultThumbnails != null)
                {
                    return searchResultThumbnails;
                }

                searchResultThumbnails = new Texture2D[Results.Count];
                for (int i = 0; i < Results.Count; i++)
                {
                    searchResultThumbnails[i] = Results[i].Thumbnail;
                }
                return searchResultThumbnails;
            }
        }
        public CollectionResult(string name, List<ModelJson> results) => (Name, Jsons) = (name, results);
    }
}