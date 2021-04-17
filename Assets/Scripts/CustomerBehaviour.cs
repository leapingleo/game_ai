using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerBehaviour : MonoBehaviour
{

    float speed = 3f;

    float rotSpeed = 100f;

    public Rigidbody2D rb;
    //[SerializeField]
    //float range;
    //[SerializeField]
    //float maxDistance;

    private bool isWandering = false;
    private bool isRotatingLeft = false;
    private bool isRotatingRight = false;
    private bool isWalking = false;
    public Collision2D collision;
    public bool collide = false;

    Vector2 wayPoint;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = Vector2.MoveTowards(transform.position, wayPoint, speed * Time.deltaTime);
        //if (Vector2.Distance(transform.position, wayPoint) < range)
        //{
        //    SetNewDestiation();
        //}
        if (isWandering == false)
        {
            StartCoroutine(Wander());
        }
        if (isRotatingRight == true) 
        {
            transform.Rotate(new Vector3(0, 0, 1) * Time.deltaTime * rotSpeed);

        }
        if (isRotatingLeft == true) 
        {
            transform.Rotate(new Vector3(0, 0, -1) * Time.deltaTime * rotSpeed);
        }
        if (isWalking == true)
        {
            transform.position += transform.up * speed * Time.deltaTime;
        }
        //if (collide == true) //NEEDS TO BE FIXED
        //{
        //    transform.Rotate(new Vector3(0, 0, -1) * Time.deltaTime * rotSpeed);
        //}

    }
    public void OnCollisionEnter2D(Collision2D collision)//NEEDS TO BE FIXED
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            this.transform.Rotate(transform.rotation.x, transform.rotation.y, transform.rotation.z - 90);
        }
    }

    IEnumerator Wander()
    { 
        int rotTime = Random.Range(1, 3);
        int rotateWait = Random.Range(1, 3);
        int rotateLorR = Random.Range(0, 3);
        int walkWait = Random.Range(1, 3);
        int WalkTime = Random.Range(1, 5);

        isWandering = true;
        //collide = false;//NEEDS TO BE FIXED

        yield return new WaitForSeconds(walkWait);
        isWalking = true;
        yield return new WaitForSeconds(WalkTime);
        isWalking = false;
        yield return new WaitForSeconds(rotateWait);
        if (rotateLorR == 1)
        {
            isRotatingRight = true;
            yield return new WaitForSeconds(rotTime);
            isRotatingRight = false;
        }
        if (rotateLorR == 2)
        {
            isRotatingLeft = true;
            yield return new WaitForSeconds(rotTime);
            isRotatingLeft = false;
        }
        
        //if (collide == true)//NEEDS TO BE FIXED
        //{
        //    isWalking = false;
        //    yield return new WaitForSeconds(rotTime);
        //}
        isWandering = false;
    }
}
//    void SetNewDestiation()
//{
//    wayPoint = new Vector2(Random.Range(-maxDistance, maxDistance), Random.Range(-maxDistance, maxDistance));
//}


