using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoolBar : MonoBehaviour
{
    [SerializeField]
    WheelCollider WheelL;
    [SerializeField]
    WheelCollider WheelR ;
    [SerializeField]
    float AntiRoll = 5000.0f;
    [SerializeField]
    Rigidbody rigidbody;

    void FixedUpdate()
    {
        WheelHit hit;
        var travelL = 1.0f;
        var travelR = 1.0f;

        var groundedL = WheelL.GetGroundHit(out hit);
        if (groundedL)
            travelL = (-WheelL.transform.InverseTransformPoint(hit.point).y - WheelL.radius) / WheelL.suspensionDistance;

        var groundedR = WheelR.GetGroundHit(out hit);
        if (groundedR)
            travelR = (-WheelR.transform.InverseTransformPoint(hit.point).y - WheelR.radius) / WheelR.suspensionDistance;

        float antiRollForce = (travelL - travelR) * AntiRoll;

        if (groundedL)
            rigidbody.AddForceAtPosition(WheelL.transform.up * -antiRollForce,
                   WheelL.transform.position);
        if (groundedR)
            rigidbody.AddForceAtPosition(WheelR.transform.up * antiRollForce,
                   WheelR.transform.position);
    }
}
