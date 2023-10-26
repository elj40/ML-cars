using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAgent : Agent
{
    // Define variables for your agent's behavior and state here.
    public GameObject Car;
    public Transform waypointManager;
    public Material failMaterial;
    public Material winMaterial; 

    private Transform targetWaypoint;

    public float totalReward = 0;

    public float timeSpent = 0f;

    public float timeLimit = 80f;
    //[HideInInspector]
    public float speedRewardMultiplier = 1;
    [HideInInspector]
    public float reversePenalty = 0.1f;
    //[HideInInspector]
    public float timePenalty = -0.01f;
    public float maxWaypointDistance = 100f;
    
    public Vector3[] spawnPoints;

    [HideInInspector]
    public Vector3[] inputs = new Vector3[2];
    [HideInInspector]
    public int[] outputs = new int[5];
    [HideInInspector]
    private int previousSpacePressed;

    public override void Initialize()
    {
        // Initialize your agent here, called when the Agent is first created.
        targetWaypoint = waypointManager.GetChild(0);
    }

    public override void OnEpisodeBegin()
    {
        // Reset the state of the agent for a new episode here.

        timeSpent = 0f;
        totalReward = 0f;

    }

    public void Update() {
      timeSpent += Time.deltaTime;
      if (timeSpent > timeLimit) {
        SetReward(-1f);
        spawnAtStart();
        setWaypointMaterials();

        EndEpisode();
      }

      PrometeoCarController controller = Car.GetComponent<PrometeoCarController>();
      float speed = controller.carSpeed/controller.maxSpeed;
      if (speed < 0.1f) {
        AddReward(-0.2f);
      }

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Define what observations the agent should collect here.
        Vector3 toTarget = targetWaypoint.localPosition - transform.localPosition;
        Vector3 toTargetNorm = toTarget.normalized;


        float targetDot = Vector3.Dot(transform.rotation * Vector3.forward, toTargetNorm);
        float targetDotPerpendicular = Vector3.Dot(transform.rotation * Vector3.right, toTargetNorm);
        float toTargetMag = toTarget.magnitude/maxWaypointDistance; 

        sensor.AddObservation(map(targetDot,-1,1,0,1));
        sensor.AddObservation(map(targetDotPerpendicular,-1,1,0,1));
        sensor.AddObservation(toTargetMag);

        WaypointBehaviour wpB = targetWaypoint.gameObject.GetComponent<WaypointBehaviour>();
        float toNextWPMag = wpB.toNextWayPoint.magnitude/maxWaypointDistance;
        float toNextWPDot = Vector3.Dot(transform.rotation * Vector3.forward, wpB.toNextWayPoint.normalized);
        float toNextWPDot90 = Vector3.Dot(transform.rotation * Vector3.right, wpB.toNextWayPoint.normalized);
        sensor.AddObservation(map(toNextWPDot,-1,1,0,1));
        sensor.AddObservation(map(toNextWPDot90,-1,1,0,1));
        sensor.AddObservation(toNextWPMag);

        PrometeoCarController controller = Car.GetComponent<PrometeoCarController>();
        sensor.AddObservation(controller.carSpeed/controller.maxSpeed);

        inputs[0] = new Vector3(map(targetDot,-1,1,0,1), map(targetDotPerpendicular,-1,1,0,1), toTargetMag);
        inputs[1] = new Vector3(map(toNextWPDot,-1,1,0,1), map(toNextWPDot90,-1,1,0,1), controller.carSpeed/controller.maxSpeed);

    }

    public override void OnActionReceived(ActionBuffers vectorAction)
    {   
        outputs = new int[5];

        // Define the agent's behavior based on the actions received here.
        var discreteActions = vectorAction.DiscreteActions;
        PrometeoCarController controller = Car.GetComponent<PrometeoCarController>();
        
        //W: Forwards
        if(discreteActions[0] == 1){
          controller.CancelInvoke("DecelerateCar");
          controller.deceleratingCar = false;
          controller.GoForward();
          outputs[0] = 1;
        }
        //S: Backwards
        if(discreteActions[1] == 1){
          controller.CancelInvoke("DecelerateCar");
          controller.deceleratingCar = false;
          controller.GoReverse();
          outputs[1] = 1;
        }
        //A: Left
        if(discreteActions[2] == 1){
          controller.TurnLeft();
          outputs[2] = 1;

        }
        //D: Right
        if(discreteActions[3] == 1){
          controller.TurnRight();

          outputs[3] = 1;
        }
        //SPACE: Handbrake
        if(discreteActions[4] == 1){
          controller.CancelInvoke("DecelerateCar");
          controller.deceleratingCar = false;
          controller.Handbrake();
          outputs[4] = 1;
        }
        //SPACE up
        if(previousSpacePressed == 1 && discreteActions[4] == 0){
          controller.RecoverTraction();
        }
        //not S and not W
        if((discreteActions[1] == 0 && discreteActions[0] == 0)){
          controller.ThrottleOff();
        }
        //not S and not W and not SPACE and not decelerating
        if((discreteActions[1] == 0 && discreteActions[0] == 0) && discreteActions[4] == 0 && !controller.deceleratingCar){
          controller.InvokeRepeating("DecelerateCar", 0f, 0.1f);
          controller.deceleratingCar = true;
        }
        // not A and not D and 0 steering-axis
        if(discreteActions[2] == 0 && discreteActions[3] == 0 && controller.steeringAxis != 0f){
          controller.ResetSteeringAngle();
        }

        previousSpacePressed = discreteActions[4];

        //Add speed reward
        float speedReward = (controller.carSpeed/controller.maxSpeed) * speedRewardMultiplier;
        AddReward(speedReward);

        totalReward += speedReward;

        AddReward(timePenalty);
        //totalReward += timePenalty;

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {   
        var discreteActionsOut = actionsOut.DiscreteActions;
        // Implement a heuristic policy to control the agent during training here.
        if(Input.GetKey(KeyCode.W)){
          discreteActionsOut[0] = 1;
          outputs[0] = 1;
        }
        if(Input.GetKey(KeyCode.S)){
          discreteActionsOut[1] = 1;
          outputs[1] = 1;
        }

        if(Input.GetKey(KeyCode.A)){
          discreteActionsOut[2] = 1;
          outputs[2] = 1;
        }
        if(Input.GetKey(KeyCode.D)){
          discreteActionsOut[3] = 1;
          outputs[3] = 1;
        }
        if(Input.GetKey(KeyCode.Space)){
          discreteActionsOut[4] = 1;
          outputs[4] = 1;
        }

    }

    void OnTriggerEnter(Collider other) {
      if (other.tag == "Waypoint" && other.transform == targetWaypoint) {
        
        other.GetComponent<MeshRenderer>().sharedMaterial = winMaterial;

        AddReward(1f);
        Transform nextWp = other.transform.gameObject.GetComponent<WaypointBehaviour>().nextWaypoint;
        if (nextWp != null)
          targetWaypoint = nextWp;
        else {
          spawnAtStart();
          SetReward(5f);
          EndEpisode();
        }

        //EndEpisode();
      }
      else if (other.tag == "Wall") {
        SetReward(-1f);
        //totalReward -= 20f;
        setWaypointMaterials();
        spawnAtStart();
        EndEpisode();
      }
    }

    //From unity forum, by mgear
    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s-a1)*(b2-b1)/(a2-a1);
    } 

    void spawnAtStart() {
        Vector3 newPos = spawnPoints[0];
        float newAngle = 180f;

        // if (trackChoice == Track.Left) {
        //   newPos = spawnPoints[0];
        //   newAngle = 180f;
        // }
        // if (trackChoice == Track.Right) {
        //   newPos = spawnPoints[1];
        //   newAngle = 0f;
        // }         
        
        Car.transform.localPosition = newPos;
        Car.transform.rotation = Quaternion.Euler(0, newAngle, 0);

        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        targetWaypoint = waypointManager.GetChild(0);
    }

    void setWaypointMaterials() {
      foreach (Transform child in waypointManager) {
        child.GetComponent<MeshRenderer>().sharedMaterial = failMaterial;
      }
    }

}

