using AnythingWorld.Animation;
using AnythingWorld.Behaviour;
using AnythingWorld.Models;
using AnythingWorld.Networking;
using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using System;
using UnityEngine;

namespace AnythingWorld.Core
{
    public static class AnythingFactory
    {
        /// <summary>
        /// Request model that matches seach term.
        /// </summary>
        /// <param name="searchTerm">Search term to find closest match to.</param>
        /// <returns></returns>
        public static GameObject RequestModel(string searchTerm, RequestParamObject userParams)
        {
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline) { }
            else
            {
                Debug.LogWarning("Warning: Standard RP detected, HDRP or URP must be installed to use Anything World.");
                return null;
            }
            var data = ConstructModelDataContainer(searchTerm, userParams);
            var anchorGameObject = CreateAnchorGameObject(data);
            FactoryCallbacks.Subscribe(data);
            UserCallbacks.Subscribe(data);
            BeginModelRequest(data);
            return anchorGameObject;
        }

        /// <summary>
        /// Request model using pre-fetched JSON.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="userParams"></param>
        /// <returns></returns>
        public static GameObject RequestModel(ModelJson json, RequestParamObject userParams)
        {
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline) { }
            else
            {
                Debug.LogWarning("Warning: Standard RP detected, HDRP or URP must be installed to use Anything World.");
                return null;
            }
            var data = ConstructModelDataContainer(json, userParams);
            var anchorGameObject = CreateAnchorGameObject(data);
            FactoryCallbacks.Subscribe(data);
            UserCallbacks.Subscribe(data);
            BeginModelRequest(data);
            return anchorGameObject;
        }

        /// <summary>
        /// Constructs model data container, sets search term.
        /// </summary>
        /// <param name="searchTerm">Name of search tem linked to this ModelData container.</param>
        /// <returns></returns>
        private static ModelData ConstructModelDataContainer(string searchTerm, RequestParamObject userParams)
        {
            ModelData data = new ModelData();
            data.searchTerm = searchTerm;
            data.requestType = RequestType.Search;
            if (userParams == null) data.parameters = new RequestParamObject();
            else
            {
                data.parameters = userParams;
            }

            return data;
        }

        private static ModelData ConstructModelDataContainer(ModelJson json, RequestParamObject userParams)
        {
            ModelData data = new ModelData();
            data.searchTerm = json.name;
            data.json = json;
            data.requestType = RequestType.Json;
            if (userParams == null) data.parameters = new RequestParamObject();
            else data.parameters = userParams;

            return data;
        }

        /// <summary>
        /// Create "anchor" game object returned to user immediately,
        /// all components and meshes will be added onto or as a child of this GameObject.
        /// </summary>
        /// <param name="data">Model data that will be linked to this GameObject.</param>
        /// <returns>Anchor GameObject</returns>
        private static GameObject CreateAnchorGameObject(ModelData data)
        {
            data.model = new GameObject(data.searchTerm);
            data.loadingScript = data.model.AddComponent<LoadingObject>();
            return data.model;
        }

        /// <summary>
        /// Sets up request pipeline depending on type of request and invokes start process.
        /// </summary>
        /// <param name="data">Model data for this request.</param>
        private static void BeginModelRequest(ModelData data)
        {
            switch (data.requestType)
            {
                case RequestType.Search:
                    SetupRequestFromSearchPipeline(data);
                    break;

                case RequestType.Json:
                    SetupRequestFromJsonPipeline(data);
                    break;
            }
            data.actions.startPipeline?.Invoke(data);
        }

        // Assign methods to the loading pipeline delegates within ModelData
        private static void SetupRequestFromSearchPipeline(ModelData data)
        {
            //Fetch json data
            data.actions.startPipeline = JsonRequester.FetchJson;
            //Action to load JSON
            data.actions.loadJsonDelegate = JsonRequester.FetchJson;
            //Do some post processing on JSON to calculate secondary variables, (animation pipeline, model loading pipeline, inspector script) 
            data.actions.processJsonDelegate = JsonProcessor.ProcessData;
            //Action to load Model + extract animations if needed
            data.actions.loadModelDelegate = ModelLoader.Load;

            data.actions.loadAnimationDelegate = AnimationFactory.Load;

            data.actions.addBehavioursDelegate = BehaviourHandler.AddBehaviours;

            data.actions.postProcessingDelegate = ModelPostProcessing.FinishMakeProcess;
        }

        private static void SetupRequestFromJsonPipeline(ModelData data)
        {
            //Skip fetching json and go straight to processing data
            data.actions.startPipeline = JsonProcessor.ProcessData;
            //Action to load JSON
            //Assign in case we cannot load data from the passed in json for any reason
            data.actions.loadJsonDelegate = JsonRequester.FetchJson;
            //Do some post processing on JSON to calculate secondary variables, (animation pipeline, model loading pipeline, inspector script) 
            data.actions.processJsonDelegate = JsonProcessor.ProcessData;
            //Action to load Model + extract animations if needed
            data.actions.loadModelDelegate = ModelLoader.Load;

            data.actions.loadAnimationDelegate = AnimationFactory.Load;

            data.actions.addBehavioursDelegate = BehaviourHandler.AddBehaviours;

            data.actions.postProcessingDelegate = ModelPostProcessing.FinishMakeProcess;
        }
    }
}