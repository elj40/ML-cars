using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointSetup : MonoBehaviour
{
    void Start()
    {
        for (int i = 0; i < transform.childCount-1; i++) {
            Transform child = transform.GetChild(i);

            WaypointBehaviour wpB = child.GetComponent<WaypointBehaviour>();
            wpB.nextWaypoint = transform.GetChild(i+1);
            wpB.findNextPoint();
        }
    }

}
