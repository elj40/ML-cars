using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAgent : Agent
{
    // Define variables for your agent's behavior and state here.
    public GameObject Car;
    public Transform targetWaypoint;
    public MeshRenderer groundMesh;
    public Material failMaterial;
    public Material winMaterial; 


    public float speedRewardMultiplier = 1;
    public float reversePenalty = 0.1f;
    public float timePenalty = -0.01f;
    public float maxWaypointDistance = 100f;
    
    public Vector3[] spawnPoints;

    [HideInInspector]
    public float[] inputs = new float[6];
    public float[] outputs = new float[5];
    private int previousSpacePressed;

    public override void Initialize()
    {
        // Initialize your agent here, called when the Agent is first created.
    }

    public override void OnEpisodeBegin()
    {
        // Reset the state of the agent for a new episode here.
        //float ranAngle = Random.Range(90f, 270f);
        int ranI = Random.Range(0, spawnPoints.Length);

        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        Car.transform.localPosition = spawnPoints[ranI];
        Car.transform.rotation = Quaternion.Euler(0, 180f, 0);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Define what observations the agent should collect here.
        Vector3 toTarget = targetWaypoint.localPosition - transform.localPosition;
        Vector3 toTargetNorm = toTarget.normalized;


        float targetDot = Vector3.Dot(Vector3.forward, toTargetNorm);
        float targetDotPerpendicular = Vector3.Dot(Vector3.right, toTargetNorm);
        float toTargetMag = toTarget.magnitude/maxWaypointDistance; 

        sensor.AddObservation(map(targetDot,-1,1,0,1));
        sensor.AddObservation(map(targetDotPerpendicular,-1,1,0,1));
        sensor.AddObservation(toTargetMag);

        WaypointBehaviour wpB = targetWaypoint.gameObject.GetComponent<WaypointBehaviour>();
        float toNextWPDot = Vector3.Dot(Vector3.forward, wpB.toNextWayPoint.normalized);
        float toNextWPDot90 = Vector3.Dot(Vector3.right, wpB.toNextWayPoint.normalized);
        sensor.AddObservation(toNextWPDot);
        sensor.AddObservation(toNextWPDot90);

        PrometeoCarController controller = Car.GetComponent<PrometeoCarController>();
        sensor.AddObservation(controller.carSpeed/controller.maxSpeed);
    }

    public override void OnActionReceived(ActionBuffers vectorAction)
    {
        // Define the agent's behavior based on the actions received here.
        var discreteActions = vectorAction.DiscreteActions;
        PrometeoCarController controller = Car.GetComponent<PrometeoCarController>();
        
        //W: Forwards
        if(discreteActions[0] == 1){
          controller.CancelInvoke("DecelerateCar");
          controller.deceleratingCar = false;
          controller.GoForward();
        }
        //S: Backwards
        if(discreteActions[1] == 1){
          controller.CancelInvoke("DecelerateCar");
          controller.deceleratingCar = false;
          controller.GoReverse();
        }
        //A: Left
        if(discreteActions[2] == 1){
          controller.TurnLeft();

        }
        //D: Right
        if(discreteActions[3] == 1){
          controller.TurnRight();

        }
        //SPACE: Handbrake
        if(discreteActions[4] == 1){
          controller.CancelInvoke("DecelerateCar");
          controller.deceleratingCar = false;
          controller.Handbrake();
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

        AddReward(timePenalty);


    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {   
        var discreteActionsOut = actionsOut.DiscreteActions;
        // Implement a heuristic policy to control the agent during training here.
        if(Input.GetKey(KeyCode.W)){
          discreteActionsOut[0] = 1;
        }
        if(Input.GetKey(KeyCode.S)){
          discreteActionsOut[1] = 1;
        }

        if(Input.GetKey(KeyCode.A)){
          discreteActionsOut[2] = 1;
        }
        if(Input.GetKey(KeyCode.D)){
          discreteActionsOut[3] = 1;
        }
        if(Input.GetKey(KeyCode.Space)){
          discreteActionsOut[4] = 1;
        }

    }

    void OnTriggerEnter(Collider other) {
      if (other.tag == "Waypoint" && other.transform == targetWaypoint) {
        AddReward(10f);
        groundMesh.sharedMaterial = winMaterial;
        Debug.Log("Hit waypoint");
        
        WaypointBehaviour wpB = other.GetComponent<WaypointBehaviour>();
        if (wpB.nextWaypoint != null) {
          targetWaypoint = wpB.nextWaypoint;
        }else {
          Debug.Log("Finished Track");
          EndEpisode();
        }
      }
      else if (other.tag == "Wall") {
        SetReward(-10f);
        groundMesh.sharedMaterial = failMaterial;
        EndEpisode();
      }
    }

    //From unity forum, by mgear
    float map(float s, float a1, float a2, float b1, float b2)
  {
      return b1 + (s-a1)*(b2-b1)/(a2-a1);
  } 

}

