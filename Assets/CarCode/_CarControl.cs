using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class _CarControl : MonoBehaviour
{
    // SerializeFields
    [Header("Components")]
    [SerializeField] WheelCollider[] wheelColliderF;
    [SerializeField] WheelCollider[] wheelColliderR;
    [SerializeField] AudioSource engineSound;

    [Header("Parameters")]
    [SerializeField] float motorPower;
    [SerializeField] float motorMaxRpm = 9000f;
    [SerializeField] float maxSteerAngle = 30f;
    [SerializeField] float brakeTorque = 1000f;
    [SerializeField] Vector3 centerMass;
    [SerializeField] float[] gearRatio;
    [SerializeField] public float maxSpeed;

    [Header("Reverse")]
    [SerializeField] float reverseHoldTime = 2f;  
    private float reverseTimer = 0f;  
    private bool isReversing = false;

    [Header("lights")]
    [SerializeField] 
    Light[] BrakeLights;


    // Private variables
    private Rigidbody rb;
    private int gear = 1;
    private int lastGear = 1;
    private float Rpms;
    private WheelFrictionCurve wfc;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerMass;
        wfc = wheelColliderR[0].sidewaysFriction;
    }

    private void Update()
    {
        HandleInputs();
    }

    void FixedUpdate()
    {
        HandleCarMovement();
    }

    void HandleInputs()
    {
        if (!Input.GetButton("Jump"))  // Check if brake is NOT pressed
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
                gear++;

            if (Input.GetKeyDown(KeyCode.LeftControl))
                gear--;

            gear = Mathf.Clamp(gear, 0, gearRatio.Length - 1);
        }

        if (Input.GetButtonDown("Jump"))
        {
            ApplyBrake(true);
            lastGear = gear;
            gear = 0;
        }

        if (Input.GetButtonUp("Jump"))
        {
            ApplyBrake(false);
            gear = lastGear;
        }

    }

    void ApplyBrake(bool brakeState)
    {
        foreach (WheelCollider wheel in wheelColliderR)
        {
            wheel.brakeTorque = brakeState ? brakeTorque : 0;
        }
        rb.angularDrag = brakeState ? 0.005f : 1f;
        wfc.stiffness = brakeState ? 0.1f : 1.5f;
        wheelColliderR[0].sidewaysFriction = wfc;
        wheelColliderR[1].sidewaysFriction = wfc;
    }

    void HandleCarMovement()
    {
        float steer = Input.GetAxis("Horizontal");
        float accel = Input.GetAxis("Vertical");
        float steerAngle = steer * maxSteerAngle;
        float brake = 0;
        if(accel<0 &&rb.velocity.sqrMagnitude>0.1f)
        {
            brake=brakeTorque*Mathf.Abs(accel);
            BrakeLights[0].intensity = 1;
            BrakeLights[1].intensity = 1;
        }else
        {
            BrakeLights[0].intensity = 0;
            BrakeLights[1].intensity = 0;
        }

        foreach (WheelCollider w in wheelColliderF)
        {
            w.motorTorque = accel * motorPower/2 * gearRatio[gear];
            w.steerAngle = steerAngle;
            w.brakeTorque = brake;
        }

        float wheelRpmsTotal = 0;
        foreach (WheelCollider w in wheelColliderR)
        {
            if (gear > 0 && Rpms < motorMaxRpm - 1000)
            {
                w.motorTorque = accel * motorPower * gearRatio[gear];
            }
            else
            {
                w.motorTorque = 0;
            }
            wheelRpmsTotal += w.rpm;
            w.brakeTorque = brake*0.05f;
        }

        // Calculate average RPM of the rear wheels
        Rpms = Mathf.Lerp(Rpms, (wheelRpmsTotal * 4) * gearRatio[gear], Time.deltaTime * 10);
        engineSound.pitch = Mathf.Lerp(0.5f, 2f, Rpms / motorMaxRpm) * 1.5f;
    }
}
