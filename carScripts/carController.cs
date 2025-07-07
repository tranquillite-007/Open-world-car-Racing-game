using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriftCarController : MonoBehaviour
{
    [SerializeField] private WheelCollider FrontRightCollider;
    [SerializeField] private WheelCollider FrontLeftCollider;
    [SerializeField] private WheelCollider RearRightCollider;
    [SerializeField] private WheelCollider RearLeftCollider;

    [SerializeField] private Transform FrontRightWheelMesh;
    [SerializeField] private Transform FrontLeftWheelMesh;
    [SerializeField] private Transform RearRightWheelMesh;
    [SerializeField] private Transform RearLeftWheelMesh;

    [Header("Car Settings")]
    public float acceleration = 8000f; 
    public float maxSpeed = 180f; 
    public float steeringAngle = 35f; 
    public float driftFactor = 0.95f; 
    public float gripFactor = 0.5f;
    public float brakeForce = 5000f; 
    public float driftSpeedReduction = 5f;
    public float nitroBoostMultiplier = 2.0f;
    public float nitroDuration = 3f; 
    public float driftThresholdSpeed = 10f; 
    public float reverseThresholdSpeed = 5f;
    [Header("Anti-Roll Settings")]
    public float antiRollForce = 6000f;

    private Rigidbody rb;
    private float inputVertical;
    private float inputHorizontal;
    private bool isDrifting;
    private bool isBraking;
    private bool isNitroActive;
    private float originalMaxSpeed;

    //[Header("Audio Settings")]
    //[SerializeField] private AudioSource engineStartAudioSource;
    //[SerializeField] private AudioSource engineMovingAudioSource;

    //[Header("skid Settings")]
    //private SkidMarkManager skidMarkManager;
    //public Transform rearLeftWheel;
    //public Transform rearRightWheel;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0); 
        originalMaxSpeed = maxSpeed; 

        //skidMarkManager = GetComponent<SkidMarkManager>();
        //engineStartAudioSource.Play();
    }

    void Update()
    {
        // Input for acceleration, steering, and braking
        inputVertical = Input.GetAxis("Vertical"); // W/S or Up/Down Arrow
        inputHorizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow

        // Check Spacebar for drifting
        isDrifting = Input.GetKey(KeyCode.Space); // Spacebar to drift

        // Nitro activation
        if (Input.GetKeyDown(KeyCode.N) && !isNitroActive)
        {
            StartCoroutine(NitroBoost());
        }

        UpdateWheelMeshes();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleDrift();
        ApplyAntiRoll();
        LimitSpeed();

        //if (GetCurrentSpeed() > 0 && !engineMovingAudioSource.isPlaying)
        //{
        //    engineMovingAudioSource.Play();
        //}
        //else if (GetCurrentSpeed() <= 0 && engineMovingAudioSource.isPlaying)
        //{
        //    engineMovingAudioSource.Stop();
        //}
    }

    void HandleMovement()
    {
        // Apply motor torque based on input
        float currentSpeed = rb.linearVelocity.magnitude * 3.6f; // Current speed in km/h

        // Apply motor torque with a speed factor for better acceleration
        if (inputVertical > 0) // Accelerating
        {
            RearLeftCollider.motorTorque = acceleration * inputVertical * (1 - currentSpeed / maxSpeed);
            RearRightCollider.motorTorque = acceleration * inputVertical * (1 - currentSpeed / maxSpeed);
        }
        else if (inputVertical < 0) // Reversing
        {
            // Apply reverse torque directly
            RearLeftCollider.motorTorque = -acceleration * Mathf.Abs(inputVertical); // Apply reverse torque
            RearRightCollider.motorTorque = -acceleration * Mathf.Abs(inputVertical); // Apply reverse torque
        }
        else // No input
        {
            RearLeftCollider.motorTorque = 0;
            RearRightCollider.motorTorque = 0;
        }

        // Apply steering
        if (isDrifting)
        {
            FrontLeftCollider.steerAngle = steeringAngle * inputHorizontal * 0.5f; // Reduce steering angle while drifting
            FrontRightCollider.steerAngle = steeringAngle * inputHorizontal * 0.5f;
        }
        else
        {
            FrontLeftCollider.steerAngle = steeringAngle * inputHorizontal;
            FrontRightCollider.steerAngle = steeringAngle * inputHorizontal;
        }

        // Apply braking if needed
        if (isBraking)
        {
            ApplyBrakes(brakeForce);
        }
        else
        {
            ApplyBrakes(0f);
        }
    }

    void ApplyBrakes(float brakeTorque)
    {
        FrontLeftCollider.brakeTorque = brakeTorque;
        FrontRightCollider.brakeTorque = brakeTorque;
        RearLeftCollider.brakeTorque = brakeTorque;
        RearRightCollider.brakeTorque = brakeTorque;
    }

    void HandleDrift()
    {
        if (isDrifting)
        {
            // Reduce rear wheel grip for drift
            ModifyWheelGrip(RearLeftCollider, gripFactor);
            ModifyWheelGrip(RearRightCollider, gripFactor);
            ApplyDriftForce();
            ReduceSpeedAfterDrift(); // Reduce speed after each drift
        }
        else
        {
            // Restore normal grip
            ModifyWheelGrip(RearLeftCollider, 1f);
            ModifyWheelGrip(RearRightCollider, 1f);
        }
    }

    void ApplyDriftForce()
    {
        Vector3 driftForceVector = -transform.forward * driftFactor * rb.linearVelocity.magnitude;
        rb.AddForce(driftForceVector, ForceMode.Acceleration);
    }

    public bool IsDrifting()
    {
        return isDrifting; // Return the drifting state
    }
    void ReduceSpeedAfterDrift()
    {
        // Reduce the car's speed after each drift
        if (rb.linearVelocity.magnitude > 0)
        {
            rb.linearVelocity *= (1 - driftSpeedReduction * Time.fixedDeltaTime / rb.linearVelocity.magnitude);
        }
    }

    void ModifyWheelGrip(WheelCollider wheel, float grip)
    {
        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
        sidewaysFriction.stiffness = grip;
        wheel.sidewaysFriction = sidewaysFriction;
    }

    void ApplyAntiRoll()
    {
        ApplyAntiRollForce(FrontLeftCollider, FrontRightCollider);
        ApplyAntiRollForce(RearLeftCollider, RearRightCollider);
    }

    void ApplyAntiRollForce(WheelCollider leftWheel, WheelCollider rightWheel)
    {
        WheelHit hit;
        float leftCompression = 1.0f;
        float rightCompression = 1.0f;

        bool leftGrounded = leftWheel.GetGroundHit(out hit);
        if (leftGrounded)
        {
            leftCompression = (-leftWheel.transform.InverseTransformPoint(hit.point).y - leftWheel.radius) / leftWheel.suspensionDistance;
        }

        bool rightGrounded = rightWheel.GetGroundHit(out hit);
        if (rightGrounded)
        {
            rightCompression = (-rightWheel.transform.InverseTransformPoint(hit.point).y - rightWheel.radius) / rightWheel.suspensionDistance;
        }

        float antiRoll = (leftCompression - rightCompression) * antiRollForce;

        if (leftGrounded)
        {
            rb.AddForceAtPosition(leftWheel.transform.up * -antiRoll, leftWheel.transform.position);
        }

        if (rightGrounded)
        {
            rb.AddForceAtPosition(rightWheel.transform.up * antiRoll, rightWheel.transform.position);
        }
    }

    void LimitSpeed()
    {
        float speed = rb.linearVelocity.magnitude * 3.6f; // Convert to km/h
        if (speed > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed / 3.6f;
        }
    }

    void UpdateWheelMeshes()
    {
        UpdateWheelMesh(FrontRightCollider, FrontRightWheelMesh);
        UpdateWheelMesh(FrontLeftCollider, FrontLeftWheelMesh);
        UpdateWheelMesh(RearRightCollider, RearRightWheelMesh);
        UpdateWheelMesh(RearLeftCollider, RearLeftWheelMesh);
    }

    void UpdateWheelMesh(WheelCollider collider, Transform mesh)
    {
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
        mesh.position = position;
        mesh.rotation = rotation;
    }

    private void OnDrawGizmos()
    {
        if (rb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + rb.centerOfMass, 0.1f);
        }
    }

    private IEnumerator NitroBoost()
    {
        isNitroActive = true;
        maxSpeed *= nitroBoostMultiplier; // Increase max speed
        yield return new WaitForSeconds(nitroDuration); // Wait for the duration of the nitro
        maxSpeed = originalMaxSpeed; // Reset max speed
        isNitroActive = false;
    }

    public float GetCurrentSpeed()
    {
        return rb.linearVelocity.magnitude * 3.6f; // Convert from m/s to km/h
    }
}