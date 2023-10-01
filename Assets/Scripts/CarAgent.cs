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
    public Material failMaterial;
    public Material winMaterial; 

    public float speedRewardMultiplier = 1;

    private int previousSpacePressed;

    public override void Initialize()
    {
        // Initialize your agent here, called when the Agent is first created.
    }

    public override void OnEpisodeBegin()
    {
        // Reset the state of the agent for a new episode here.
        float ranX = Random.Range(-30f, 30f);
        float ranZ = Random.Range(-30f, 30f);
        float ranAngle = Random.Range(0f, 360f);

        Car.transform.position = new Vector3(ranX, 0, ranZ);
        Car.transform.rotation = Quaternion.Euler(0, ranAngle, 0);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Define what observations the agent should collect here.
        Vector3 toTarget = targetWaypoint.position - transform.position;
        Vector3 toTargetNorm = toTarget.normalized;
        sensor.AddObservation(toTargetNorm);

        WaypointBehaviour wpB = targetWaypoint.gameObject.GetComponent<WaypointBehaviour>();
        sensor.AddObservation(wpB.toNextWayPoint.normalized);

        PrometeoCarController controller = Car.GetComponent<PrometeoCarController>();
        sensor.AddObservation(controller.carSpeed);
        sensor.AddObservation(Car.transform.eulerAngles.y/360f);  
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
        //AddReward(speedReward);
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
        SetReward(-1f);
        EndEpisode();
      }
    }

}

