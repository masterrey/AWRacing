using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnythingWorld;
namespace AnythingWorld
{
    public class MakeObjectsExample : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            //Examples of how to use the make object method to create objects at runtime.
            AnythingWorld.AnythingMaker.Make("cat", RequestParameter.AddCollider(), RequestParameter.AddRigidbody());
            AnythingWorld.AnythingMaker.Make("car", RequestParameter.OnSuccessAction(AdjustCarSpeed));






            //Make 5 sharks of differing sizes 
            int numberOfSharks = 5;
            for(int i=0; i<numberOfSharks; i++)
            {
                var scale = Random.Range(0.5f,2f);
                AnythingWorld.AnythingMaker.Make("shark", RequestParameter.ScaleMultiplier(scale));

            }
            //Make 10 bees and spawn them randomly in different y coordinates in center of plane. 
            int numberOfBees = 10;
            for (int i = 0; i < numberOfBees; i++)
            {
                var yCoordinate = Random.Range(1f, 3f);
                AnythingWorld.AnythingMaker.Make("bee", RequestParameter.Position(new Vector3(0,yCoordinate,0)));

            }
        }

        /// <summary>
        /// Callback to adjust speed values in the attached vehicle movement script after it has been created.
        /// </summary>
        /// <param name="modelInfo"></param>
        public void AdjustCarSpeed(CallbackInfo modelInfo)
        {
            var movementScript = modelInfo.linkedObject.GetComponent<AnythingWorld.Behaviour.RandomMovementWheeledVehicle>();
            if (movementScript == null) return;
            movementScript.speed = movementScript.speed * 2;
            movementScript.turnSpeed = movementScript.turnSpeed * 2;
        }
    }
}
