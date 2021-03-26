using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 velocity;
    private float maxSpeed;
    private float maxForce;
    public GameObject target;
    public FlowField flowField;
    


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        maxSpeed = Random.Range(4f, 6f);
        maxForce = Random.Range(0.08f, 0.15f);
    }

    public void SetFlowField(FlowField flowField)
    {
        this.flowField = flowField;
    }

    // Update is called once per frame
    void Update()
    {
        BoardWarp();

        //Vector3 desired = target.transform.position - transform.position;
        Vector3 desired = flowField.GetCurrentDesiredVelocity(transform.position);

        desired = desired.normalized * maxSpeed;

        Vector3 steer = desired - velocity;
        Vector3 steerClamp = Vector3.ClampMagnitude(steer, maxForce);
        
        //apply steer force to current velocity
        velocity += steerClamp;

        transform.position += velocity * Time.deltaTime;
        transform.right = velocity.normalized;
    }

    private void BoardWarp()
    {
        float newX = transform.position.x, newY = transform.position.y;

        if (transform.position.x > 8.8)
            newX = -8.8f;
        if (transform.position.x < -8.8)
            newX = 8.8f;

        if (transform.position.y > 4.9)
            newY = -4.9f;
        if (transform.position.y < -4.9)
            newY = 4.9f;


        transform.position = new Vector3(newX, newY, 0);
    }
}
