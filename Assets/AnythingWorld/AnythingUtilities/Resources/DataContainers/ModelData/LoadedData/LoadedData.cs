using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace AnythingWorld.Utilities.Data
{
    [Serializable]
    public class LoadedData
    {
        public LoadedData()
        {
            obj = new LoadedDataObj();
            gltf = new LoadedDataGltf();
        }
        public Vector3 bounds = Vector3.one;
        public LoadedDataObj obj;
        public LoadedDataGltf gltf;
        public Vector3 dbDimensionsVector;
        public float boundsYOffset = 0;
    }
}
