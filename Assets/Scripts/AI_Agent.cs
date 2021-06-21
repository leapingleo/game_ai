using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class AI_Agent : Agent
{
    private Rigidbody2D rb2d;
    public float speedMultiplier = 200.0f;
    public Transform coinParent;
    public Transform AI;
    private int remainingCoins = 0;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();

        this.MaxStep = 5000;
    }

    //public Transform Target;
    public override void OnEpisodeBegin()
    {
        //Debug.Log("Episode began!");
        AI.position = new Vector2(-18.54f, -4.44f);
        //AI.position = new Vector2(-0.23f, -1.35f);
        //Transform coinHolder = transform.parent.Find("TPs");
        remainingCoins = 0;
        foreach (Transform coin in coinParent)
        {
            coin.gameObject.SetActive(true);
            remainingCoins++;
        }
}

    public override void CollectObservations(VectorSensor sensor)
    {
        // TODO: Insert proper code here for collecting the observations!
        // At the moment this code just feeds in 10 observations, all hardcoded to zero, as a placeholder.


        //for (int i = 0; i < 10; i++)
        //{
        //    sensor.AddObservation(0.0f);
        //}

        sensor.AddObservation((float)StepCount / MaxStep);

        base.CollectObservations(sensor);
    }

    public void coinCollected()
    {
        remainingCoins--;

        AddReward(1.0f);

        if (remainingCoins <= 0)
        {
            Debug.Log("no more coins!");
            EndEpisode();
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //Debug.Log("vectorAction[0] = " + vectorAction[0]);

        Vector2 movementDir;

        //Vector2 forceDirection = Vector2.zero;

        switch (actions.DiscreteActions[0])
        {
            case 1:
                movementDir = new Vector2(1.0f, 0.0f);
                break;
            case 2:
                movementDir = new Vector2(-1.0f, 0.0f);
                break;
            case 3:
                movementDir = new Vector2(0.0f, 1.0f);
                break;
            case 4:
                movementDir = new Vector2(0.0f, -1.0f);
                break;
            case 5:
                movementDir = new Vector2(1.0f, 1.0f);
                break;
            case 6:
                movementDir = new Vector2(1.0f, -1.0f);
                break;
            case 7:
                movementDir = new Vector2(-1.0f, 1.0f);
                break;
            case 8:
                movementDir = new Vector2(-1.0f, -1.0f);
                break;
            default:
                movementDir = Vector2.zero;
                break;
        }

        transform.position += (Vector3)movementDir * Time.deltaTime * 2f;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.Keypad8))
        {
            continuousActionsOut[0] = 3;
        }
        if (Input.GetKey(KeyCode.Keypad2))
        {
            continuousActionsOut[0] = 4;
        }
        if (Input.GetKey(KeyCode.Keypad4))
        {
            continuousActionsOut[0] = 2;
        }
        if (Input.GetKey(KeyCode.Keypad6))
        {
            continuousActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.Keypad9))
        {
            continuousActionsOut[0] = 5;
        }
        if (Input.GetKey(KeyCode.Keypad7))
        {
            continuousActionsOut[0] = 6;
        }
        if (Input.GetKey(KeyCode.Keypad1))
        {
            continuousActionsOut[0] = 7;
        }
        if (Input.GetKey(KeyCode.Keypad3))
        {
            continuousActionsOut[0] = 8;
        }
    }

}