using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using UnityEngine;

namespace AnythingWorld.Core
{
    public static class ModelPostProcessing
    {
        public static void FinishMakeProcess(ModelData data)
        {

            // Invoke factory actions stored for successful creation of model.
            foreach (var action in data.actions.onSuccess)
            {
                action?.Invoke(data, "Successfully made");
            }
            // If serializing parameter passed, attempt to serialize.
            if (data.parameters.serializeAsset)
            {
                AssetSaver.CreateAssetFromData(new CallbackInfo(data));
            }


            // If collider specified add collider
            if (data.parameters.addCollider)
            {
                AddCollider(data);
            }
            // If rigidbody specified, add rigibody

            if (data.parameters.addRigidbody)
            {
                AddRigidbody(data);
            }
            //dirty scene on succesful completion
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }

#endif

        }
        private static void AddCollider(ModelData data)
        {
            var skinnedRenderer = data.model.GetComponentInChildren<SkinnedMeshRenderer>();
            Bounds bounds;
            if (skinnedRenderer != null)
            {
                bounds = GetBoundsSkinnedMeshRenderer(data);
            }
            else
            {
                bounds = GetBoundsMeshFilter(data);
            }

            if (data.animationPipeline == AnimationPipeline.Static)
            {
                foreach(var mf in data.model.GetComponentsInChildren<MeshFilter>())
                {
                    var mfRb = mf.gameObject.AddComponent<MeshCollider>();
                    mfRb.convex = true;
                }
            }
            else
            {
                var modelCollider = data.model.AddComponent<CapsuleCollider>();
                bounds = DetermineAxis(bounds, modelCollider);

                switch (modelCollider.direction)
                {
                    case 0:
                        modelCollider.height = bounds.extents.x *2;
                        modelCollider.radius = bounds.extents.y;
                        break;
                    case 1:
                        modelCollider.height = bounds.extents.y*2;
                        modelCollider.radius = bounds.extents.x;
                        break;
                    case 2:
                        modelCollider.height = bounds.extents.z*2;
                        modelCollider.radius = bounds.extents.y;
                        break;
                }
            }
        }

        private static Bounds DetermineAxis(Bounds bounds, CapsuleCollider modelCollider)
        {
            if (bounds.extents.y > bounds.extents.x)
            {
                if (bounds.extents.y > bounds.extents.z)
                {
                    modelCollider.direction = 1;
                }
                else
                {
                    modelCollider.direction = 2;
                }
            }
            else if (bounds.extents.x > bounds.extents.z)
            {
                modelCollider.direction = 0;
            }
            else modelCollider.direction = 2;
            return bounds;
        }

        private static Bounds GetBoundsSkinnedMeshRenderer(ModelData data)
        {
            // Have to do a complex process because the root bone is rotated 
            // which means the automatically generated bounds for the SMR 
            // are also rotated, causing them to be inaccurate.
            var skinnedRenderer = data.model.GetComponentInChildren<SkinnedMeshRenderer>();
            Bounds bounds;
            var rot = skinnedRenderer.rootBone.localRotation;
            skinnedRenderer.rootBone.localRotation = Quaternion.Euler(0, 0, 0);
            skinnedRenderer.updateWhenOffscreen = true;
            skinnedRenderer.sharedMesh.RecalculateBounds();

            bounds = new Bounds();
            Vector3 center = skinnedRenderer.localBounds.center;
            Vector3 extents = new Vector3(skinnedRenderer.localBounds.extents.x, skinnedRenderer.localBounds.extents.y, skinnedRenderer.localBounds.extents.z);

            
            bounds = skinnedRenderer.bounds;
            bounds.center = center;
            bounds.extents = extents;

            
            skinnedRenderer.updateWhenOffscreen = false;
            skinnedRenderer.rootBone.localRotation = rot;
            return bounds;
        }
        private static Bounds GetBoundsMeshFilter(ModelData data)
        {
            Bounds bounds;
            var meshfilters = data.model.GetComponentsInChildren<MeshFilter>();
            bounds = GetObjectBounds(meshfilters);
            return bounds;
        }
        private static void AddRigidbody(ModelData data)
        {
            var rb = data.model.AddComponent<Rigidbody>();
            rb.mass = data.json.mass;
        }

        private static Vector3 GetObjectBounds(Renderer[] renderers)
        {
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (var objRenderer in renderers)
            {
                bounds.Encapsulate(objRenderer.bounds);
            }
            return bounds.size;
        }

        private static Bounds GetObjectBounds(MeshFilter[] meshFilters)
        {
            var totalBounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (var mFilter in meshFilters)
            {
                var mMesh = mFilter.sharedMesh;
                totalBounds.Encapsulate(mMesh.bounds);
            }
            return totalBounds;
        }
    }

}
