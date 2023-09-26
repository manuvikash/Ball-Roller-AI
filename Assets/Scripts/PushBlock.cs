using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Runtime.CompilerServices;

public class PushBlock : Agent
{
    Rigidbody ball;
    private int currentStep = 0;
    private int maxSteps;
    private Vector3 lastBlockPosition;
    void Start()
    {
        ball = GetComponent<Rigidbody>();
        lastBlockPosition = Block.localPosition;
    }

    public Transform Block;
    public Transform MiniPlane;
    public Transform Plane;

     private int[][] miniPlaneLocations = new int[][] {
        new int[] { 5, -5 },
        new int[] { 5, 5 },
        new int[] { -5, 5 },
        new int[] { -5, -5 }
    };

    private Vector3 calcBlockPos(int x, int y){
        int[][] blockPosArr;
        if(x > 0 && y > 0){
            blockPosArr = new int[][] {
                new int[] {Random.Range(0,-4), Random.Range(0,-4)},
                new int[] {Random.Range(0,4), Random.Range(0,-4)},
                new int[] {Random.Range(0,-4), Random.Range(0,4)},
            };
        }
        else if(x > 0 && y < 0){
            blockPosArr = new int[][] {
                new int[] {Random.Range(0,4), Random.Range(0,4)},
                new int[] {Random.Range(0,-4), Random.Range(0,4)},
                new int[] {Random.Range(0,-4), Random.Range(0,-4)},
            };
        }
        else if(x < 0 && y > 0){
            blockPosArr = new int[][] {
                new int[] {Random.Range(0,4), Random.Range(0,4)},
                new int[] {Random.Range(0,4), Random.Range(0,-4)},
                new int[] {Random.Range(0,-4), Random.Range(0,-4)},
            };
        }
        else{
            blockPosArr = new int[][] {
                new int[] {Random.Range(0,4), Random.Range(0,4)},
                new int[] {Random.Range(0,4), Random.Range(0,-4)},
                new int[] {Random.Range(0,-4), Random.Range(0,4)},
            };
        }
        System.Random random = new System.Random();
        int[] blockPos = blockPosArr[random.Next(0, blockPosArr.Length)];
        return new Vector3(blockPos[0], 0.76f, blockPos[1]);
    }
    
     public override void OnEpisodeBegin()
    {
        currentStep = 0;
        // If the Agent fell from the platform, zero its momentum
        if (this.transform.localPosition.y < 0)
        {
            this.ball.angularVelocity = Vector3.zero;
            this.ball.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.6f, 0);
        }

        // Move the target to a new spot
        System.Random random = new System.Random();
        int[] miniPlanePos = miniPlaneLocations[random.Next(0, miniPlaneLocations.Length)];
        MiniPlane.localPosition = new Vector3(miniPlanePos[0], 0.0001f, miniPlanePos[1]);
        Block.localPosition = this.calcBlockPos(miniPlanePos[0], miniPlanePos[1]);

    }

     public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(Block.localPosition);
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(MiniPlane.localPosition);
        // Agent velocity
        sensor.AddObservation(ball.velocity.x);
        sensor.AddObservation(ball.velocity.z);
    }

    public float forceMultiplier;

    // private void OnCollisionEnter(Collision col) {
    //     if(col.gameObject.name == "Cube"){
    //         // float collisionForce = col.relativeVelocity.magnitude;
    //         SetReward(0.1f);
    //         // Debug.Log("Collision with cube, reward = " + 0.01f  * collisionForce);
    //     }
    // }

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
        ball.AddForce(dirToGo * forceMultiplier);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        currentStep++;
        MoveAgent(actionBuffers.DiscreteActions);
        SetReward(-0.1f/MaxStep);
        // Debug.Log("Delay reward = " + -0.1/MaxStep);
        // Vector3 controlSignal = Vector3.zero;
        // controlSignal.x = actionBuffers.ContinuousActions[0];
        // controlSignal.z = actionBuffers.ContinuousActions[1];
        // ball.AddForce(controlSignal * forceMultiplier);

        // Rewards
        float distanceToTarget = Vector3.Distance(Block.transform.localPosition, MiniPlane.localPosition);

        float oldDistancetoTarget = Vector3.Distance(lastBlockPosition, MiniPlane.localPosition);
        // Reached target

        if(distanceToTarget < oldDistancetoTarget){
            SetReward(0.1f * oldDistancetoTarget - distanceToTarget);
        }
        lastBlockPosition = Block.transform.localPosition;
        if (distanceToTarget < 5.64f)
        {
            SetReward(5.0f);
            Debug.Log("Success Reward");
            EndEpisode();
        }

        if(currentStep == maxSteps){
            currentStep = 0;
            EndEpisode();
        }

        // Fell off platform
        else if (this.transform.localPosition.y < 0)
        {
            SetReward(-1f);
            EndEpisode();
        }
        // else{
        //     SetReward(distanceToTarget*0.01f);
        //     Debug.Log(distanceToTarget*0.01f);
        // }
    }

    // public override void Heuristic(in ActionBuffers actionsOut)
    // {
    //     var continuousActionsOut = actionsOut.ContinuousActions;
    //     continuousActionsOut[0] = Input.GetAxis("Horizontal");
    //     continuousActionsOut[1] = Input.GetAxis("Vertical");
    // }

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
