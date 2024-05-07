using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_CarCharControl : MonoBehaviour
{
    [SerializeField, Header("Waypoints")] 
    public LaneWaypoint startWaypoint;

    [Header("Vehicle")]
    public bool autoAccel = true;
    public float maxSpeed = 100;
    public float acceleration = 20;
    public float turnSpeed = 1;
    
    [HideInInspector] public CharacterController charControl;
    private Vector3 direction, velocity;

    private WheelControl[] wheels;
    private float moveSpeed;

    private LaneWaypoint currentWaypoint, nextWaypoint;

    // Start is called before the first frame update
    void Start()
    {
        charControl = GetComponent<CharacterController>();
        wheels = GetComponentsInChildren<WheelControl>();
        currentWaypoint = startWaypoint;
    }

    private void Update()
    {
        if(Vector3.Distance(transform.position, currentWaypoint.transform.position) < .5)
        {
            currentWaypoint = currentWaypoint.Next;
        }
    }

    void FixedUpdate()
    {
        float vInput = autoAccel ? 1 : Input.GetAxis("Debug Vertical");
        float hInput = -Input.GetAxis("Left") + Input.GetAxis("Right");

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

        foreach (WheelControl wheel in wheels)
        {
            wheel.Accelerate(moveSpeed);
        }
    }
}
