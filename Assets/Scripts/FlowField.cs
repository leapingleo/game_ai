using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowField : MonoBehaviour
{
    public int rows, cols;
    public GameObject agentPrefab;
    private Vector3[,] flowArr;
    private int rowInterval;
    private int colInterval;

    private float timer;
    public float spawnInterval;
    private float nextSpawnTime;
    

    // Start is called before the first frame update
    void Start()
    {
        SetupFlows();
    }

    // Update is called once per frame
    void Update()
    {
        DrawFlow();

        if (Input.GetMouseButton(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Time.time > nextSpawnTime)
            {
                nextSpawnTime = Time.time + spawnInterval;
                GameObject agent = Instantiate(agentPrefab, worldPos, agentPrefab.transform.rotation);
                agent.GetComponent<Triangle>().SetFlowField(this);
            }
        }
		if (Input.GetKeyDown(KeyCode.R)) {
            SetupFlows();
		}
    }

    public void SetupFlows()
    {
        int seed = Random.Range(0, 10000);
        flowArr = new Vector3[rows, cols];
        rowInterval = Screen.height / rows;
        colInterval = Screen.width / cols;
        float xoff = 0.1f;
        for (int i = 0; i < rows; i++)
        {
            float yoff = 0.01f;
            for (int j = 0; j < cols; j++)
            {
                float theta = Mathf.PerlinNoise(xoff + seed, yoff + seed);
                //  theta = Mathf.PI;
                flowArr[i, j] = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0);
                //  flowArr[i, j] = flowArr[i, j].normalized;
                yoff += 0.1f;
                // Debug.Log("xx " + xx + ", yy " + yy );
            }
            xoff += 0.1f;
        }
    }

    void DrawFlow()
    {
        float colInt = 18f / cols;
        float rowInt = 10f / rows;
        float startX = -9f;
        float startY = -5f;
        Debug.Log("colint " + colInt);
        Debug.Log("rowint " + rowInt);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                float x = startX + j * colInt + colInt * 0.5f;
                float y = startY + i * rowInt + rowInt * 0.5f;
                Vector3 point = new Vector3(x, y, 0);
                Vector3 dir = GetCurrentDesiredVelocity(point);
                //point = Camera.main.WorldToScreenPoint(point);
                Debug.DrawLine(point, point + dir * 0.3f, Color.green);

                //no need to shift to center of each box
                Vector3 a = new Vector3(startX + j * colInt, startY + i * rowInt, 0);
                Vector3 b = a + new Vector3(colInt, 0, 0);
                Vector3 c = b + new Vector3(0, rowInt, 0);
                Vector3 d = c - new Vector3(colInt, 0, 0);
                Debug.DrawLine(a, b, Color.yellow);
                Debug.DrawLine(b, c, Color.yellow);
                Debug.DrawLine(c, d, Color.yellow);
                Debug.DrawLine(d, a, Color.yellow);
            }
        }

        //float r = 3f;
        //float thetaStep = 2 * Mathf.PI / 20;

        //for (int i = 0; i < 20; i++)
        //{
        //    float theta = thetaStep * i;
        //    Vector3 vec = new Vector3(Mathf.Cos(theta + Mathf.PI * 0.25f),
        //        Mathf.Sin(theta + Mathf.PI * 0.25f), 0f);

        //    float x = r * Mathf.Cos(theta);
        //    float y = r * Mathf.Sin(theta);

        //    //point step on circle
        //    Vector3 pos = new Vector3(x, y, 0);
        //    Vector3 screenPos = Camera.main.WorldToScreenPoint(new Vector3(x, y, 0));

        //    int row = (int)screenPos.y / rowInterval;
        //    int col = (int)screenPos.x / colInterval;

        //    flowArr[row, col] = new Vector3(vec.x, vec.y, 0);
        //    Debug.DrawLine(pos, pos + flowArr[row, col], Color.green);

        //}

        //for (int i = 0; i < rows; i++)
        //{
        //    for (int j = 0; j < cols; j++)
        //    {
        //        if (flowArr[i, j] == null)
        //        {
        //            flowArr[i, j] = new Vector3(1, 0, 0);
        //        }
        //    }
        //}

        /**
        float rStep = 9.0f / cols;
        int n = 4;
        float thetaStep = 0;

        for (int j = 1; j < 9; j++)
        {
            rStep = rStep * j;
            for (int i = 0; i < n; i++)
            {
                float theta = (2 * Mathf.PI / n) * i;
                float x = rStep * Mathf.Cos(theta);
                float y = -0.5f + rStep * Mathf.Sin(theta);
                Vector3 vec = new Vector3(2*x, 2*y, 0f);

                //point step on circle
                Vector3 pos = new Vector3(x, y, 0);
                Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);

                int row = (int)screenPos.y / rowInterval;
                int col = (int)screenPos.x / colInterval;

               // Debug.Log("row + " + row + ", " + col);
               // if (screenPos.x < Screen.width && screenPos.y < Screen.height)
                    flowArr[row, col] = new Vector3(vec.x, vec.y, 0);
             //   Debug.DrawLine(pos, pos + flowArr[row, col] * 0.3f, Color.green);
            }
            n += 8;
            Debug.Log("n " + n);
        }
        **/

    }

    public Vector3 GetCurrentDesiredVelocity(Vector3 pos)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);

        // Debug.Log(Screen.width + ", " + Screen.height);
        int row = (int)screenPos.y / rowInterval;
        int col = (int)screenPos.x / colInterval;

        return flowArr[row, col];
    }
}
