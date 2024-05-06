using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class WheelControl : MonoBehaviour
{
    public GameObject wheelModel;

    [Header("Motor")]
    public bool motorized;
    
    [Header("Steering")]
    public bool steerable;
    public float SteerAngle = 30;
    
    [HideInInspector] public WheelCollider wheelCollider;

    private Vector3 position;
    private Quaternion rotation;

    // Start is called before the first frame update
    void Start()
    {
        wheelCollider = GetComponent<WheelCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelModel.transform.position = position;
        wheelModel.transform.rotation = rotation;

        float steerFactor = -Input.GetAxis("Left") + Input.GetAxis("Right");
        Steer(steerFactor);
    }

    public void Steer(float steer)
    {
        if (!steerable) return;
        
        wheelCollider.steerAngle = steer * SteerAngle;
    }

    public void Accelerate(float torque)
    {   
        wheelCollider.motorTorque = torque;
    }

    public void Brake(float torque)
    {
        wheelCollider.motorTorque = torque;
    }
}
