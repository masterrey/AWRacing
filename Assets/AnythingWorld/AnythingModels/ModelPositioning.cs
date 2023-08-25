using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using System;
using UnityEngine;

namespace AnythingWorld.Models
{
    public static class ModelPositioning
    {
        public static void ApplyTransforms(ModelData data, Action<ModelData> onSuccess)
        {
            ApplyParentTransform(data.model, data.parameters.parentTransform);

            if (data.parameters.position.value != Vector3.zero)
            {
                ApplyPositionAccordingToSpace(data);
              
            }
            else if (data.parameters.placeOnGrid)
            {
                ApplyGridPositionAccordingToSpace(data);
            }
            else
            {
                ApplyPositionAccordingToSpace(data);
            }

            ApplyRotationAccordingToSpace(data);
            AdjustIntraModelPositioning(data);
            onSuccess?.Invoke(data);
        }

        private static void AdjustIntraModelPositioning(ModelData data)
        {
            if (data.parameters.placeOnGround)
            {
                //Debug.Log($"Bounds ({data.loadedData.boundsYOffset}) * localScale ({data.model.transform.localScale.y}) = {data.loadedData.boundsYOffset/2 * data.model.transform.localScale.y}");
                data.model.transform.localPosition += new Vector3(0, data.loadedData.boundsYOffset, 0);
            }

        }

        private static void ApplyGridPositionAccordingToSpace(ModelData data)
        {
            var gridCoords = SimpleGrid.AddCell();
            var gridPosition = new Vector3(gridCoords.x, gridCoords.y, gridCoords.z);
        
            switch (data.parameters.transformSpace)
            {
                case Utilities.TransformSpace.World:
                    ApplyWorldSpacePosition(data.model, gridPosition);
                    break;

                case Utilities.TransformSpace.Local:
                    ApplyLocalSpacePosition(data.model, gridPosition);
                    break;
            }
        }
        private static void ApplyPositionAccordingToSpace(ModelData data)
        {
            switch (data.parameters.transformSpace)
            {
                case Utilities.TransformSpace.World:
                    ApplyWorldSpacePosition(data.model, data.parameters.position.value);
                    break;

                case Utilities.TransformSpace.Local:
                    ApplyLocalSpacePosition(data.model, data.parameters.position.value);
                    break;
            }
        }

        private static void ApplyRotationAccordingToSpace(ModelData data)
        {
            
            switch (data.parameters.transformSpace)
            {
                case Utilities.TransformSpace.World:
                    ApplyWorldSpaceRotation(data.model, data.parameters.rotation);
                    break;

                case Utilities.TransformSpace.Local:
                    ApplyLocalSpaceRotation(data.model, data.parameters.rotation);
                    break;
            }
        }

        private static void ApplyLocalSpacePosition(GameObject model, Vector3 position)
        {
            model.transform.localPosition += position;
        }

        private static void ApplyLocalSpaceRotation(GameObject model, Quaternion rotation)
        {
            model.transform.localRotation = rotation;
        }

        private static void ApplyWorldSpacePosition(GameObject model, Vector3 position)
        {
            model.transform.position += position;
        }

        private static void ApplyWorldSpaceRotation(GameObject model, Quaternion rotation)
        {
            model.transform.rotation = rotation;
        }

        private static void ApplyParentTransform(GameObject model, Transform parentTransform)
        {
            model.transform.parent = parentTransform;
        }
    }
}
