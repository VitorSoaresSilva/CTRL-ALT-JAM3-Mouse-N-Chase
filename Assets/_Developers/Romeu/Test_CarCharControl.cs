using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_CarCharControl : MonoBehaviour
{
    public float maxSpeed = 100;
    public float acceleration = 20;
    public float turnSpeed = 1;
    
    [HideInInspector] public CharacterController charControl;
    private Vector3 direction, velocity;

    private WheelControl[] wheels;
    private float moveSpeed;

    // Start is called before the first frame update
    void Start()
    {
        charControl = GetComponent<CharacterController>();
        wheels = GetComponentsInChildren<WheelControl>();
    }

    // Update is called once per frame
    void Update()
    {
        float hInput = -Input.GetAxis("Left") + Input.GetAxis("Right");
        float vInput = Input.GetAxis("Debug Vertical");

        moveSpeed = acceleration * vInput;

        if (charControl.isGrounded)
        {
            direction.x = hInput * turnSpeed;
            direction.z = moveSpeed;
            velocity = direction * turnSpeed;
        }
        else
        {
            velocity.y += Physics.gravity.y * Time.deltaTime;
        }

        charControl.Move(velocity * Time.deltaTime);

        //foreach(WheelControl wheel in wheels)
        //{
        //    wheel.Steer(hInput);
        //}
    }
}
