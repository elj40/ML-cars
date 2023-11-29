using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAgent : Agent
{
    // Define variables for your agent's behavior and state here.
    public float totalReward = 0;

    public float timeSpent = 0f;

    public float timeLimit = 50f;
    //[HideInInspector]
    public float speedRewardMultiplier = 1;
    [HideInInspector]
    public float reversePenalty = 0.1f;
    //[HideInInspector]
    public float timePenalty = -0.01f;
    
    public Vector3[] spawnPoints;


    [HideInInspector]
    public int[] outputs = new int[5];
    [HideInInspector]
    private int previousSpacePressed;

    private PrometeoCarController controller;

    public override void Initialize()
    {
        // Initialize your agent here, called when the Agent is first created.
        controller = GetComponent<PrometeoCarController>();
        controller.useKeyboardControls = false;
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

        EndEpisode();
      }

      float speed = controller.carSpeed/controller.maxSpeed;
      if (speed < 0.1f) {
        //AddReward(-0.2f);
      }

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float speed = controller.carSpeed/controller.maxSpeed;
        sensor.AddObservation(speed);

        float drifting = controller.isDrifting ? 1.0f: 0.0f;
        sensor.AddObservation(drifting);
    }

    public override void OnActionReceived(ActionBuffers vectorAction)
    {   
        outputs = new int[5];

        // Define the agent's behavior based on the actions received here.
        var discreteActions = vectorAction.DiscreteActions;
        
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
        totalReward += timePenalty;

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

    void OnCollisonEnter(Collision collision) {
      Debug.Log("Collision");
      Debug.Log(collision.gameObject.tag );
      if (collision.gameObject.tag == "Wall") {
          SetReward(-1f);
          spawnAtStart();
          EndEpisode();
        }
    }

    void OnTriggerEnter(Collider other) {
      Debug.Log("Collision");
      Debug.Log(other.tag );
      if (other.tag == "Wall") {
          SetReward(-1f);
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

        transform.localPosition = newPos;
        transform.rotation = Quaternion.Euler(0, newAngle, 0);

        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }
}