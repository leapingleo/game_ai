using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Security : MonoBehaviour
{
    public float speed;
    public float duration;
    public bool move;
    private float fraction = 0;
    float d;
    public Vector3[] targets;
    private int counter = 0;

    void Start()
    {
        d = Vector3.Distance(transform.localPosition, Vector3.zero); 
    }

    // Update is called once per frame
    void Update()
    {
        if (!move)
            return;

        if (Vector3.SqrMagnitude(transform.localPosition - targets[counter]) < 0.001f * 0.001f)
        {
            Debug.Log("It's working!");
            counter++;
            if (counter > targets.Length - 1)
                counter = 0;
        }
        else
        {
            transform.up = targets[counter] - transform.localPosition;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targets[counter], Time.deltaTime * speed);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
       // if (collision.transform.CompareTag("Obstacle"))
        //    speed *= -1f;
    }

    
}
