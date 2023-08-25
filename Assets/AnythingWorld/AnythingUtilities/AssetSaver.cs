using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AnythingWorld.Utilities
{
    public static class AssetSaver
    {
        public static string rootPath = "Assets/SavedAssets";

        public static void CreateAssetFromData(CallbackInfo callbackData)
        {
#if UNITY_EDITOR
            CreateAssetFromGameObject(callbackData.linkedObject, callbackData);
#endif
        }

        public static void CreateAssetFromGameObject(GameObject streamedObject, CallbackInfo callbackData)
        {
#if UNITY_EDITOR
            SerializeAnimations(streamedObject, callbackData);

            SerializeSkinnedMeshRenderers(streamedObject);

            SerializeMeshRenderers(streamedObject);
            Debug.Log($"Saved asset to {rootPath}/{streamedObject.name}");
#endif
        }
#if UNITY_EDITOR
        private static void SerializeAnimations(GameObject streamedObject, CallbackInfo callbackData)
        {
            if (streamedObject.GetComponentInChildren<Animation>())
            {
                var animationComponent = streamedObject.GetComponentInChildren<Animation>();
                var animationContainer = animationComponent.gameObject;
                GameObject.DestroyImmediate(animationComponent);

                List<string> paths = new List<string>();
                foreach (var kvp in callbackData.data.loadedData.gltf.animationClips)
                {
                    if (kvp.Value == null) continue;
                    kvp.Value.legacy = true;
                    TryCreateAsset<AnimationClip>(kvp.Value, kvp.Key, streamedObject.name, out var path, out var serializedAnimationClip);
                    if (path != null) paths.Add(path);
                }


                animationComponent = animationContainer.AddComponent<Animation>();
                foreach (string path in paths)
                {

                    var clip = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;
                    animationComponent.AddClip(clip, clip.name);
                }

                EditorUtility.SetDirty(animationComponent);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static void SerializeMeshRenderers(GameObject streamedObject)
        {
            if (streamedObject.GetComponentsInChildren<MeshRenderer>()?.Length > 0)
            {
                foreach (var meshRenderer in streamedObject.GetComponentsInChildren<MeshRenderer>())
                {
                    var gameObject = meshRenderer.gameObject;

                    List<Material> serializedSharedMaterials = new List<Material>();
                    foreach (Material mat in meshRenderer.sharedMaterials)
                    {

                        SerializeMaterialTextures(streamedObject, mat);
                        TryCreateAsset<Material>(mat, mat.name, streamedObject.name, out var materialPath, out var material);
                        serializedSharedMaterials.Add(material);

                    }
                    meshRenderer.sharedMaterials = serializedSharedMaterials.ToArray();

                    if (meshRenderer.TryGetComponent<MeshFilter>(out var meshFilter) && meshFilter.sharedMesh != null)
                    {
                        var mesh = meshFilter.sharedMesh;
                        TryCreateAsset<Mesh>(mesh, mesh.name, streamedObject.name, out var meshPath, out var serializedMesh);
                        meshFilter.sharedMesh = serializedMesh;
                    }
                    else
                    {
                        Debug.LogWarning("Could not find mesh filter for mesh renderer:", meshRenderer.gameObject);
                    }
                }

            }

        }

        private static void SerializeSkinnedMeshRenderers(GameObject streamedObject)
        {
            if (streamedObject.GetComponentInChildren<SkinnedMeshRenderer>())
            {
                var smRenderer = streamedObject.GetComponentInChildren<SkinnedMeshRenderer>();
                List<Material> serializedSharedMaterials = new List<Material>();
                foreach (Material mat in smRenderer.sharedMaterials)
                {
                    SerializeMaterialTextures(streamedObject, mat);
                    TryCreateAsset<Material>(mat, mat.name, streamedObject.name, out var path, out var serializedMaterial);
                    serializedSharedMaterials.Add(serializedMaterial);

                }

                smRenderer.sharedMaterials = serializedSharedMaterials.ToArray();



                if (streamedObject.GetComponentInChildren<SkinnedMeshRenderer>() &&
             streamedObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh != null)
                {
                    var mesh = streamedObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
                    TryCreateAsset<Mesh>(mesh, mesh.name, streamedObject.name, out var path, out var serializedMesh);
                    streamedObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh = serializedMesh;
                }
            }
        }

        private static void SerializeMaterialTextures(GameObject streamedObject, Material mat)
        {

            List<Tuple<Texture, string>> allTexture = GetTextures(mat);
            for (int i = 0; i < allTexture.Count; i++)
            {
                if (allTexture[i].Item1 == null) continue;
                TryCreateAsset<Texture>(allTexture[i].Item1, allTexture[i].Item2, streamedObject.name, out var texturePath, out var texture);
                mat.SetTexture(allTexture[i].Item2, texture);

            }

        }


        private static List<Tuple<Texture, string>> GetTextures(Material mat)
        {
            List<Tuple<Texture, string>> allTexture = new List<Tuple<Texture, string>>();
            Shader shader = mat.shader;
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    var textureName = ShaderUtil.GetPropertyName(shader, i);
                    Texture texture = mat.GetTexture(textureName);

                    allTexture.Add(new Tuple<Texture, string>(texture, textureName));
                }
            }
            return allTexture;
        }

        public static bool TryCreateAsset<T>(UnityEngine.Object asset, string name, string guid, out string path, out T loadedAsset) where T : UnityEngine.Object
        {
            path = "";
            loadedAsset = null;
            if (AssetDatabase.Contains(asset))
            {

                path = AssetDatabase.GetAssetPath(asset);
                loadedAsset = (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
                Debug.Log($"{asset} already serialized within database.");
                return true;
            }

            CreateDefaultFolder();
            CreateFolder(rootPath, guid);

            var safeFilterName = GenerateSafeFilePath(name);
            string extension = ".asset";
            if (typeof(T) == typeof(AnimationClip)) extension = ".anim";
            var assetPath = $"{rootPath}/{guid}/{safeFilterName}{extension}";

            if (!AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)))
            {
                try
                {

                    AssetDatabase.CreateAsset(asset, assetPath);
                    EditorUtility.SetDirty(asset);
                    path = assetPath;
                    loadedAsset = (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
                    return true;
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                    return false;
                }
            }
            else
            {
                path = assetPath;
                loadedAsset = (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
                return true;
            }
        }
        private static string CreateDefaultFolder()
        {
            if (AssetDatabase.IsValidFolder(rootPath)) return AssetDatabase.AssetPathToGUID(rootPath);
            else return CreateFolder("Assets", "SavedAssets");
        }

        static string CreateFolder(string rootDirectory, string name)
        {
            string newDirectory = rootDirectory + "/" + name;
            if (AssetDatabase.IsValidFolder(newDirectory)) return AssetDatabase.AssetPathToGUID(newDirectory);
            string guid = AssetDatabase.CreateFolder(rootDirectory, name);
            string newFolderPath = AssetDatabase.GUIDToAssetPath(guid);
            return newFolderPath;
        }
        private static string GenerateSafeFilePath(string inputPath)
        {
            string illegalChars = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(illegalChars)));
            var safePath = r.Replace(inputPath, "");
            return safePath;
        }

        private static bool TryGetPath<T>(T asset, string name, string guid, out string path) where T : UnityEngine.Object
        {
            //CreateDefaultFolder();
            //CreateFolder(rootPath, guid);

            var safeFilterName = GenerateSafeFilePath(name);
            string extension = ".asset";
            if (typeof(T) == typeof(AnimationClip)) extension = ".anim";
            var assetPath = $"{rootPath}/{guid}/{safeFilterName}{extension}";


            Debug.Log(assetPath);
            if (AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)))
            {
                path = AssetDatabase.GetAssetPath(asset);
                return true;
            }
            else
            {
                path = null;
                return false;
            }

        }


#endif
    }
}


