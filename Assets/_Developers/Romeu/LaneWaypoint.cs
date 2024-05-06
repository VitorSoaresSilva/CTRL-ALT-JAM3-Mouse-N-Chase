using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneWaypoint : MonoBehaviour
{
    public LaneWaypoint Next;
    public LaneWaypoint Previous;

    [Header("Lanes")]
    public LaneWaypoint LeftLaneNext;
    public LaneWaypoint RightLaneNext;

    [Header("Turns")]
    public LaneWaypoint LeftTurn;
    public LaneWaypoint RightTurn;

    protected bool selected = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = selected ? Color.green : Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.3f);

        if (Next != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, Next.transform.position);
        }

        if (Previous != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, Previous.transform.position);
        }

        if (LeftTurn != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, LeftTurn.transform.position);
        }

        if (RightTurn != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, RightTurn.transform.position);
        }

        selected = false;
    }

    private void OnDrawGizmosSelected()
    {
        selected = true;
    }
}
