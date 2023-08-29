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

    [Header("lights")]
    [SerializeField] 
    Light[] BrakeLights;


    // Private variables
    private Rigidbody rb;
    private int gear = 1;
   
    private float Rpms;
    private WheelFrictionCurve wfc;
    [SerializeField]
    private bool isAutomaticGear;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerMass;
        wfc = wheelColliderR[0].sidewaysFriction;
    }

    public float GetSpeed()
    {
        return rb.velocity.magnitude * 3.6f * Time.timeScale;
    }
    public float GetRPM()
    {
        return Rpms;
    }   
    public int GetGear()
    {
        return gear;
    }

    private void Update()
    {
        HandleInputs();
       
    }

    void FixedUpdate()
    {
        HandleCarMovement();
        if (isAutomaticGear)
        {
            AutoGearShift();
        }
    }

    void AutoGearShift()
    {
        // Auto shift up
        if (Rpms > (motorMaxRpm - 1000) && gear < gearRatio.Length - 1)
        {
            gear++;
        }
        // Auto shift down
        else if (Rpms < (motorMaxRpm - 5000) && gear > 1)
        {
            gear--;
        }
    }

    Vector3 vel;
    Vector3 angvel;
    bool drift = false;
    void HandleInputs()
    {
        if (!Input.GetButton("Jump"))  // Check if brake is NOT pressed
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
                gear++;

            if (Input.GetKeyDown(KeyCode.LeftControl))
                gear--;

            gear = Mathf.Clamp(gear, -1, gearRatio.Length - 1);
        }

        if (Input.GetButtonDown("Jump"))
        {
            ApplyBrake(true);
           
           
        }

        if (Input.GetButton("Jump"))
        {

            Drift();
        }

        if (Input.GetButtonUp("Jump"))
        {
            ApplyBrake(false);
            drift = false;
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
            transform.rotation = Quaternion.identity;
        }

    }

    void Drift()
    {
        var locVel = transform.InverseTransformDirection(rb.velocity);
        
        if (Mathf.Abs(locVel.x) > 9)
        {
            if (!drift)
            {
                vel = rb.velocity;
                angvel = rb.angularVelocity;
            }
            drift = true;
            Vector3 newvel = (vel + transform.forward * vel.magnitude) * 0.6f;
            rb.velocity = new Vector3(newvel.x, rb.velocity.y, newvel.z);
            transform.Rotate(Vector3.up, Input.GetAxis("Horizontal") * Time.deltaTime * 30);
            rb.angularVelocity = angvel * 0.5f;
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
        float power=0;
        float wheelRpmsTotal = 0;

        if (accel<0)
        {
            brake=brakeTorque*Mathf.Abs(accel);
            BrakeLights[0].intensity = 1;
            BrakeLights[1].intensity = 1;
        }else
        {
            BrakeLights[0].intensity = 0;
            BrakeLights[1].intensity = 0;
 
        }
       

      
        if (Rpms < motorMaxRpm - 1000)
        {
            if(gear>0)
                power = accel * motorPower * gearRatio[gear];
        }
        else
        {
            power = 0;
            brake = Mathf.Abs(motorMaxRpm-Rpms);
        }
        if (gear == 0)
            Rpms = Mathf.Lerp(Rpms, accel * motorMaxRpm, Time.deltaTime * 10);

        if (gear == -1)
            power = accel * -motorPower;
       

        foreach (WheelCollider w in wheelColliderF)
        {
            w.motorTorque = power/2;
            w.steerAngle = steerAngle;
            w.brakeTorque = brake;
            wheelRpmsTotal += w.rpm;
        }

        
        foreach (WheelCollider w in wheelColliderR)
        {
            w.motorTorque = power;
            wheelRpmsTotal += w.rpm;
            w.brakeTorque = brake*0.05f;
        }

        if (gear > 0)
            Rpms = Mathf.Lerp(Rpms,1.2f* wheelRpmsTotal * gearRatio[gear], Time.fixedDeltaTime * 10);
        else if (gear == 0)
            Rpms = Mathf.Lerp(Rpms, accel * motorMaxRpm, Time.fixedDeltaTime * 10);
        else if (gear == -1)
            Rpms = Mathf.Lerp(Rpms, -wheelRpmsTotal, Time.fixedDeltaTime * 10);  // Negative RPMs for reverse

        engineSound.pitch = Mathf.Lerp(0.5f, 2f, Rpms / motorMaxRpm) * 1.5f;
        engineSound.volume = Mathf.Clamp(accel,0.3f,0.8f)+Rpms*0.0001f;
    }
}
