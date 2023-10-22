using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Xml;
// using System;

public class newAgent : Agent
{
    Rigidbody agent;
    private int currentStep = 0;
    private int maxSteps = 50000;
    private bool inTarget1 = false;
    private bool inTarget2 = false;

    public Transform Block1;
    public Transform Block2;
    public Transform Target1;
    public Transform Target2;
    public Vector3 oldBlock1Pos;
    public Vector3 oldBlock2Pos;
    void Start()
    {
        agent = GetComponent<Rigidbody>();
        oldBlock1Pos = Block1.localPosition;
        oldBlock2Pos = Block2.localPosition;
    }
    
    private Vector3 calcBlockSpawn(int half){
        int xval = Random.Range(-8,-20);
        int zval;
        if(half == 1){
            zval = Random.Range(1, 10);
        }
        else{
            zval = Random.Range(-1, -10);
        }
        return new Vector3(xval, 0.8f, zval);
    }
     public override void OnEpisodeBegin()
    {
        this.agent.angularVelocity = Vector3.zero;
        this.agent.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3(-21, 0.8f, 0);
        Block1.localPosition = calcBlockSpawn(1);
        Block2.localPosition = calcBlockSpawn(2);
    }

     public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(Target1.localPosition);
        sensor.AddObservation(Target2.localPosition);
    }

    public float forceMultiplier = 60;

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];

        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
            case 5:
                dirToGo = transform.right * -0.75f;
                break;
            case 6:
                dirToGo = transform.right * 0.75f;
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        agent.AddForce(dirToGo * forceMultiplier);
    }

    public float calculateMovementReward(Transform Block, Transform Target){
        float distanceToTarget = Vector3.Distance(Block.transform.localPosition,Target.transform.localPosition);
        float oldDistancetoTarget = Vector3.Distance(Block.transform.localPosition, Target.transform.localPosition);
        if(distanceToTarget < oldDistancetoTarget){
            return 1f* (oldDistancetoTarget - distanceToTarget);
        }
        else{
            return 0f;
        }
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        currentStep++;
        MoveAgent(actionBuffers.DiscreteActions);
        SetReward(-1f/MaxStep);
        //For block1
        SetReward(calculateMovementReward(Block1, Target1));
        SetReward(calculateMovementReward(Block2, Target2));

        if(currentStep == maxSteps){
            currentStep = 0;
            EndEpisode();
        }

        if(inTarget1 && inTarget2){
            SetReward(100);
            EndEpisode();
        }
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }

    public void targetEntry(GameObject obj){
        if(obj.name == "Target1"){
            inTarget1 = true;
        }
        else{
            inTarget2=true;
        }
        SetReward(30);
    }

    public void targetExit(GameObject obj){
        if(obj.name == "Target1"){
            inTarget1 = false;
        }
        else{
            inTarget2=false;
        }
        SetReward(-30);
    }
}
