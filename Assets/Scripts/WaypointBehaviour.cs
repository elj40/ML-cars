using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointBehaviour : MonoBehaviour
{
    public Transform nextWaypoint;
    public bool reached = false;

    public Vector3 toNextWayPoint =  Vector3.zero;
    
    public void findNextPoint() {
        if (nextWaypoint != null) {
            Vector3 diff = nextWaypoint.position - transform.position;
            Vector3 diffNorm = diff.normalized;

            toNextWayPoint = diff;

            transform.rotation = Quaternion.LookRotation(diffNorm) * Quaternion.Euler(0,90f,0);
        }
    }
}

public enum Track {
    Left,
    Right
}
