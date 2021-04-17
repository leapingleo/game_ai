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

	private void OnDestroy()
	{
		ClearPathMarker();
	}

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

	public Vector3 CenterPosition(Vector3 pos)
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

	public bool SameDest()
	{

		return Mathf.Approximately(lastDestCenterPosition.x, currDestCenterPosition.x) &&
			Mathf.Approximately(lastDestCenterPosition.y, currDestCenterPosition.y) &&
			Mathf.Approximately(lastDestCenterPosition.z, currDestCenterPosition.z);
	}

	public List<Vector2> SetNewPath(Vector3 newDest)
	{
		List<Vector2> p;
		Vector3 newDestCenterPos = CenterPosition(newDest);
		Vector3 currCenterPos = CenterPosition(transform.position);
		currDestCenterPosition = newDestCenterPos;

		ClearPathMarker();
		objPosPair = new Dictionary<Vector2, GameObject>();

		p = JumpPointSearch.SearchPath(currCenterPos, newDestCenterPos);
		DrawPath(p);
		return p;
	}

	void FixedUpdate()
	{
		lastDestCenterPosition = currDestCenterPosition;
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