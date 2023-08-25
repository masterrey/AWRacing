using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace AnythingWorld
{
    public static class AnythingMaker
    {
        /// <summary>
        /// Request object that's the closest to the search term. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns top level GameObject, model components and additional game objects will be added to this.</returns>
        public static GameObject Make(string name)
        {
            return AnythingWorld.Core.AnythingFactory.RequestModel(name, new Utilities.Data.RequestParamObject());
        }
        /// <summary>
        /// Request object that's the closest to the search term. 
        /// </summary>
        /// <param name="name">String term to find close match to.</param>
        /// <param name="parameters">Array of RequestParameter objects holding parameters to customise make process.</param>
        /// <returns></returns>
        public static GameObject Make(string name, params RequestParameterOption[] parameters )
        {
            if (name == "dog") name = "dog#0001";
            //Fetches data from user input and clears request static variables ready for next request.
            var requestParams = RequestParameter.Fetch();
            return AnythingWorld.Core.AnythingFactory.RequestModel(name, requestParams);
        }

     
    }
}
