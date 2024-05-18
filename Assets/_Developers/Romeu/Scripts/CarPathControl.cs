using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPathControl : MonoBehaviour
{
    [Header("Path")]
    public PathCreator PathCreator;
    public EndOfPathInstruction endOfPathInstruction = EndOfPathInstruction.Stop;
    
    [Header("Car")]
    public Rigidbody rigidBody;
    public WheelCollider[] wheelColliders;
    public float motorTorque = 2000;
    public float brakeTorque = 2000;
    public float maxSpeed = 20;
    public float steeringRange = 30;
    public float steeringRangeAtMaxSpeed = 10;
    public float centreOfGravityOffset = -1f;

    // runtime
    public float speed;
    private float distanceTravelled;

    void Start()
    {
        if(!rigidBody) rigidBody = GetComponent<Rigidbody>();
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;
        if(wheelColliders.Length == 0) wheelColliders = GetComponentsInChildren<WheelCollider>();

        if (PathCreator != null)
        {
            // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
            PathCreator.pathUpdated += OnPathChanged;
        }
    }

    void Update()
    {
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.velocity);
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);
        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
        float currentSteeringRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        foreach(WheelCollider wheel in wheelColliders)
        {
            wheel.motorTorque = currentMotorTorque;
            wheel.brakeTorque = brakeTorque;
        }

        if (PathCreator != null)
        {
            distanceTravelled = PathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }
    }

    void OnPathChanged()
    {
        distanceTravelled = PathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }
}
