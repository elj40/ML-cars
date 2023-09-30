using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointBehaviour : MonoBehaviour
{
    public GameObject nextWaypoint;
    public bool reached = false;

    public Vector3 toNextWayPoint =  Vector3.zero;

    void Start() {
        if (nextWaypoint != null) {
            Vector3 diff = this.transform.position - nextWaypoint.transform.position;
            Vector3 diffNorm = diff.normalized;

            toNextWayPoint = diffNorm;
        }

        
    }
}
