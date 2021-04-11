using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
// using UnityEngine.Tilemaps;

public class TestMove : MonoBehaviour
{
    public GameObject markerPrefab;
    public GameObject bush;
    public Grid grid;
    public bool reroute;

    // float horizontal;
    // float vertical;
    private List<Vector2> path = new List<Vector2>();
    private Vector3 currDestCenterPosition;
    private Vector3 lastDestCenterPosition;
    private Dictionary<Vector2, GameObject> objPosPair = new Dictionary<Vector2, GameObject>();
    public bool reset;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 sourceCenterPosition = CenterPosition(transform.position);
        currDestCenterPosition = CenterPosition(bush.transform.position);

        if (path == null || path.Count == 0)
        {
            path = JumpPointSearch.SearchPath(sourceCenterPosition, currDestCenterPosition);
            DrawPath(path);
        }

        lastDestCenterPosition = currDestCenterPosition;
    }

    // Update is called once per frame
    void Update()
    {

    }

    Vector3 CenterPosition(Vector3 pos)
    {
        Vector3Int cellPosition = grid.WorldToCell(pos);
        Vector3 centerPosition = grid.GetCellCenterWorld(cellPosition);

        return centerPosition;
    }

    void DrawPath(List<Vector2> paths)
    {
        if (paths == null || paths.Count == 0)
            return;

        foreach (var p in paths)
        {
            GameObject marker = Instantiate(markerPrefab, p, quaternion.identity);
            objPosPair.Add(p, marker);
        }

        foreach (var item in objPosPair)
        {
            item.Value.transform.position = item.Key;
        }
    }

    void ClearPathMarker()
    {
        if (objPosPair == null || objPosPair.Count == 0)
            return;

        foreach (var item in objPosPair)
        {
            Destroy(item.Value);
        }
    }

    public bool sameDest()
    {

        return Mathf.Approximately(lastDestCenterPosition.x, currDestCenterPosition.x) &&
            Mathf.Approximately(lastDestCenterPosition.y, currDestCenterPosition.y) &&
            Mathf.Approximately(lastDestCenterPosition.z, currDestCenterPosition.z);
    }

    public List<Vector2> SetNewPath(Vector3 newDest)
    {
        Vector3 newDestCenterPos = CenterPosition(newDest);
        currDestCenterPosition = newDestCenterPos;
        path = new List<Vector2>();
        ClearPathMarker();
        objPosPair = new Dictionary<Vector2, GameObject>();
        path = JumpPointSearch.SearchPath(CenterPosition(transform.position), newDestCenterPos);
        DrawPath(path);
        return path;
    }

    void FixedUpdate()
    {
      //  Vector2 position = transform.position;
       // position = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y));
        //  Vector3Int sourceCellPosition = grid.WorldToCell(position);
        //  Vector3 sourceCenterPosition = grid.GetCellCenterWorld(sourceCellPosition);
      //  Vector3 sourceCenterPosition = CenterPosition(position);

        // Rigidbody2D destRigidbody2d = bush.GetComponent<Rigidbody2D>();
        //  Vector2 destPosition = destRigidbody2d.position;
        //   Vector3Int destCellPosition = grid.WorldToCell(destPosition);
        //  Vector3 destCenterPosition = grid.GetCellCenterWorld(destCellPosition);
      //  currDestCenterPosition = CenterPosition(bush.transform.position);

        //if (!sameDest() || reset)
        //{
        //    path = new List<Vector2>();
        //    ClearPathMarker();
        //    objPosPair = new Dictionary<Vector2, GameObject>();
        //    path = JumpPointSearch.SearchPath(sourceCenterPosition, currDestCenterPosition);
        //    DrawPath(path);
        //    //  lastDestCenterPosition = currDestCenterPosition;
        //}
        lastDestCenterPosition = currDestCenterPosition;


        /**
        if (path != null && path.Count > 0)
        {
            Vector2 nextStep = path[path.Count - 1];
            float delta = 0.01f;
            if (Math.Abs(position.x - nextStep.x) < delta && Math.Abs(position.y - nextStep.y) < delta)
            {
                path.RemoveAt(path.Count - 1);
                if (path.Count > 0)
                {
                    nextStep = path[path.Count - 1];
                }
                else
                {
                    nextStep = new Vector2(float.NaN, float.NaN);
                }
            }
            if (!float.IsNaN(nextStep.x) && !float.IsNaN(nextStep.y))
            {
                position.x += Math.Sign(nextStep.x - position.x) * speed * Time.deltaTime;
                position.y += Math.Sign(nextStep.y - position.y) * speed * Time.deltaTime;
                // rigidbody2d.MovePosition(position);
            //    transform.position = position;
            }
        }
        **/
    }

    public List<Vector2> GetPath()
    {
        return path;
    }

    public Vector3 FinalTarget()
    {
        return currDestCenterPosition;
    }
}