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
    private int maxSteps = 5000;
    void Start()
    {
        ball = GetComponent<Rigidbody>();
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
        return new Vector3(blockPos[0], 0.1f, blockPos[1]);
    }
    
     public override void OnEpisodeBegin()
    {
        currentStep = 0;
        // If the Agent fell from the platform, zero its momentum
        if (this.transform.localPosition.y < 0)
        {
            this.ball.angularVelocity = Vector3.zero;
            this.ball.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 1.8f, 0);
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

    public float forceMultiplier = 10;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        currentStep++;
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        ball.AddForce(controlSignal * forceMultiplier);

        // Rewards
        float distanceToTarget = Vector3.Distance(Block.transform.localPosition, MiniPlane.localPosition);

        // Reached target
        if (distanceToTarget < 5.64f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        if(currentStep == maxSteps){
            currentStep = 0;
            EndEpisode();
        }

        // Fell off platform
        // else if (this.transform.localPosition.y < 0)
        // {
        //     SetReward(-0.1f);
        //     EndEpisode();
        // }
        // else{
        //     SetReward(distanceToTarget*0.01f);
        //     Debug.Log(distanceToTarget*0.01f);
        // }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}
