using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneFollower : MonoBehaviour
{
    public LanePart StartingLane;
    public float speed = 2f;
    public float rotSpeed = 2f;

    private LanePart currentLane;
    private int currentWaypointIndex = 0;
    private Transform targetWaypoint => currentLane.waypoints[currentWaypointIndex];

    public bool loop;

    private void Start()
    {
        currentLane = StartingLane;
    }

    void FixedUpdate()
    {
        float distance = Vector3.Distance(targetWaypoint.position, transform.position);
        if(distance < 0.1f)
        {
            if(currentWaypointIndex < currentLane.waypoints.Length - 1)
            {
                currentWaypointIndex++;
            }
            else if(currentLane.NextLane != null)
            {
                currentLane = currentLane.NextLane;
                currentWaypointIndex = 0;
            } else if(loop) currentWaypointIndex = 0;
        }

        Quaternion lookWp = Quaternion.LookRotation(targetWaypoint.position - transform.position);

        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookWp, rotSpeed * Time.deltaTime);        
        Physics.SyncTransforms();    
    }
}
