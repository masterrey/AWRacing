using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnythingWorld.Utilities.Data;

namespace AnythingWorld
{
    public class CallbackInfo
    {
        public CallbackInfo(ModelData data)
        {
            guid = data.guid;
            linkedObject = data.model;
            searchTerm = data.searchTerm;
            this.data = data;
        }
        public CallbackInfo(ModelData data, string message = null)
        {
            guid = data.guid;
            linkedObject = data.model;
            searchTerm = data.searchTerm;
            this.data = data;
            this.logOutput = message;
        }
        public string guid;
        public GameObject linkedObject;
        public string searchTerm;
        public string logOutput;
        public ModelData data;
    }
}
