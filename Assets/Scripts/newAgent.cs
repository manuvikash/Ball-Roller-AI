using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


// using System;

public class newAgent : Agent
{
    Rigidbody agent;
    private int currentStep = 0;
    // private int maxSteps = 35000;

    public Transform Block1;
    public Transform Block2;
    public Transform Block3;
    public Transform Block4;
    public Transform Block5;

    private bool block1Status;
    private bool block2Status;
    private bool block3Status;
    private bool block4Status;
    private bool block5Status;
  
    public Transform Target;
    public Transform Wall;
    public Vector3 oldBlock1Pos;
    public Vector3 oldBlock2Pos;
    public Vector3 oldBlock3Pos;
    public Vector3 oldBlock4Pos;
    public Vector3 oldBlock5Pos;
    private int activeBlocks;

    private float[] randomTargetPos(){
        float [][] targetPosArr = new float[][]{
            new float[] {-30, -12, 90, 10, -12, 90 },
            new float[] {-12, -12, 90, -12, -12, 90 },
            new float[] {0, 0, 0, 0, 0, 0},
            new float[] {0, -18.6f, 0, 0, 21.8f, 0}
        };
        System.Random random = new();
        float[] targetPos = targetPosArr[random.Next(0, targetPosArr.Length)];
        return targetPos;
    }
    void Start()
    {
        activeBlocks = 5;
        agent = GetComponent<Rigidbody>();
        oldBlock1Pos = Block1.localPosition;
        oldBlock2Pos = Block2.localPosition;
        oldBlock3Pos = Block3.localPosition;
        oldBlock4Pos = Block4.localPosition;
        oldBlock5Pos = Block5.localPosition;
    }
    
    private Vector3 calcBlockSpawn(){
        float xval = Random.Range(-6f,-18f);
        float zval = Random.Range(-7f,5f);
        return new Vector3(xval, 0, zval);
    }
     public override void OnEpisodeBegin()
    {
        currentStep = 0;
        this.agent.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3(-12.5f, 0.6f, 0);
        // activeBlocks = 5;
        Block1.localPosition = calcBlockSpawn();
        Block2.localPosition = calcBlockSpawn();
        Block3.localPosition = calcBlockSpawn();
        Block4.localPosition = calcBlockSpawn();
        Block5.localPosition = calcBlockSpawn();
        oldBlock1Pos = Block1.localPosition;
        oldBlock2Pos = Block2.localPosition;
        oldBlock3Pos = Block3.localPosition;
        oldBlock4Pos = Block4.localPosition;
        oldBlock5Pos = Block5.localPosition;
        block1Status = false;
        block2Status = false;
        block3Status = false;
        block4Status = false;
        block5Status = false;
        float[] targetPos = randomTargetPos();
        Target.localPosition = new Vector3(targetPos[0], 0, targetPos[1]);
        Target.eulerAngles = new Vector3(0, targetPos[2], 0);

        Wall.localPosition = new Vector3(targetPos[3], 0, targetPos[4]);
        Wall.eulerAngles = new Vector3(0, targetPos[5], 0);
    }

    public Vector3 avgBlockPos(){
        float xval = (Block1.localPosition.x + Block2.localPosition.x + Block3.localPosition.x + Block4.localPosition.x + Block5.localPosition.x)/5;
        float zval = (Block1.localPosition.z + Block2.localPosition.z + Block3.localPosition.z + Block4.localPosition.z + Block5.localPosition.z)/5;
        return new Vector3(xval, 0, zval);
    }

     public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        // sensor.AddObservation(this.transform.localPosition);
        // sensor.AddObservation(Target.localPosition);
        // sensor.AddObservation(avgBlockPos());
        // // sensor.AddObservation(Target2.localPosition);
        // sensor.AddObservation(Block1.localPosition);
        // sensor.AddObservation(Block2.localPosition);
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
        float oldDistancetoTarget;
        if(Block.name == "Block1" && !block1Status){
            oldDistancetoTarget = Vector3.Distance(oldBlock1Pos, Target.transform.localPosition);
        }
        else if(Block.name == "Block2" && !block2Status){
            oldDistancetoTarget = Vector3.Distance(oldBlock2Pos, Target.transform.localPosition);
        }
        else if(Block.name == "Block3" && !block3Status){
            oldDistancetoTarget = Vector3.Distance(oldBlock3Pos, Target.transform.localPosition);
        }
        else if(Block.name == "Block4" && !block4Status){
            oldDistancetoTarget = Vector3.Distance(oldBlock4Pos, Target.transform.localPosition);
        }
        else if(Block.name == "Block5" && !block5Status){
            oldDistancetoTarget = Vector3.Distance(oldBlock5Pos, Target.transform.localPosition);
        }
        else{
            return 0f;
        }
        
        if(distanceToTarget < oldDistancetoTarget){
            // Debug.Log(Block.name + " --- " + 1f* (oldDistancetoTarget - distanceToTarget));
            return 10f* (oldDistancetoTarget - distanceToTarget);
        }
        else{
            return 0;
        }
    }
    public void targetEntry(string name){
        if(name == "Block1"){
            block1Status = true;
        }
        else if(name == "Block2"){
            block2Status = true;
        }
        else if(name == "Block3"){
            block3Status = true;
        }
        else if(name == "Block4"){
            block4Status = true;
        }
        else if(name == "Block5"){
            block5Status = true;
        }
        activeBlocks--;
        SetReward(30);
        // Debug.Log(20);
    }

    public void targetExit(string name){
        if(name == "Block1"){
            block1Status = false;
        }
        else if(name == "Block2"){
            block2Status = false;
        }
        else if(name == "Block3"){
            block3Status = false;
        }
        else if(name == "Block4"){
            block4Status = false;
        }
        else if(name == "Block5"){
            block5Status = false;
        }
        activeBlocks++;
        SetReward(-30);
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        currentStep++;
        if(currentStep > 30){
            MoveAgent(actionBuffers.DiscreteActions);
            SetReward(-10f/MaxStep);
            //For block1
            float block1MoveReward = calculateMovementReward(Block1, Target);
            float block2MoveReward = calculateMovementReward(Block2, Target);
            float block3MoveReward = calculateMovementReward(Block3, Target);
            float block4MoveReward = calculateMovementReward(Block4, Target);
            float block5MoveReward = calculateMovementReward(Block5, Target);
            SetReward(block1MoveReward);
            SetReward(block2MoveReward);
            SetReward(block3MoveReward);
            SetReward(block4MoveReward);
            SetReward(block5MoveReward);

            // if(block1MoveReward > 0){
            //     Debug.Log(block1MoveReward + " :BLOCK 1");
            // }
            // if(block2MoveReward > 0){
            //     Debug.Log(block2MoveReward+ " :BLOCK 2");
            // }
            // if(block3MoveReward > 0){
            //     Debug.Log(block3MoveReward+ " :BLOCK 3");
            // }
            // if(block4MoveReward > 0){
            //     Debug.Log(block4MoveReward+ " :BLOCK 4");
            // }
            // if(block5MoveReward > 0){
            //     Debug.Log(block5MoveReward+ " :BLOCK 5");
            // }

            oldBlock1Pos = Block1.transform.localPosition;
            oldBlock2Pos = Block2.transform.localPosition;
            oldBlock3Pos = Block3.transform.localPosition;
            oldBlock4Pos = Block4.transform.localPosition;
            oldBlock5Pos = Block5.transform.localPosition;

            // if(currentStep == maxSteps){
            //     currentStep = 0;
            //     // EndEpisode();
            // }

            if(activeBlocks == 0){
                SetReward(200);
                Debug.Log(200);
                EndEpisode();
            }

            if(this.transform.localPosition.y < -1f){
                EndEpisode();
            }
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
}
