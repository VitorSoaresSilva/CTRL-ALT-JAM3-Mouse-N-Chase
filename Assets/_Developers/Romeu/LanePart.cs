using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class LanePart : MonoBehaviour
{
    public LanePart NextLane;
    public LanePart PreviousLane;
    public LanePart LeftLane;
    public LanePart RightLane;
    [field: SerializeField] public Transform[] waypoints { get; private set; }

    private void OnDrawGizmos()
    {
        if (waypoints.Length == 0) return;

        Gizmos.color = Color.cyan;
        foreach (Transform wp in waypoints)
        {
            if(wp == null) break;
            Gizmos.DrawSphere(wp.transform.position, 0.3f);
            if(Array.IndexOf(waypoints, wp) < waypoints.Length - 1)
                Gizmos.DrawLine(wp.position, waypoints[Array.IndexOf(waypoints, wp) + 1].position);    
        }
    }
}
