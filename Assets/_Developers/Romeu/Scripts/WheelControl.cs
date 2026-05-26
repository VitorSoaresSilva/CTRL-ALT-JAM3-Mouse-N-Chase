using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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

        // Validar posição antes de aplicar
        if (!_Developers.Vitor.ValidationUtility.IsValidVector3(position))
        {
            Debug.LogWarning($"[WheelControl] Posição inválida da roda: {position}");
            return;
        }

        // Validar rotação antes de aplicar
        if (!_Developers.Vitor.ValidationUtility.IsValidQuaternion(rotation))
        {
            Debug.LogWarning($"[WheelControl] Rotação inválida da roda: {rotation}");
            return;
        }

        wheelModel.transform.position = position;
        wheelModel.transform.rotation = rotation;

        //float steerFactor = -Input.GetAxis("Left") + Input.GetAxis("Right");
        //Steer(steerFactor);

        //Steer(-Input.GetAxis("Left") + Input.GetAxis("Right"));
        //Accelerate(Input.GetAxis("Debug Vertical"));
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
