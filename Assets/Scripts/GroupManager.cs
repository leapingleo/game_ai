using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupManager : MonoBehaviour
{
	public Character leaderPrefab;
	public Character followerPrefab;

	[Range(1, 10)]
	public int GroupSize = 1;
	public float GroupDensity = 0.1f;

	protected List<Character> _members;
	public List<Character> Members { get { return _members; } }

	protected Character _leader;
	public Character Leader { get { return _leader; } }

	void Start()
	{
		_members = new List<Character>();

		_leader = Instantiate(leaderPrefab, transform.position, Quaternion.Euler(Vector3.zero));
		_leader.name = "Leader";
		_leader.SetGroupManager(this);

		_members.Add(_leader);

		for (int i = 0; i < GroupSize - 1; i++)
		{
			Character follower = Instantiate(followerPrefab, Leader.transform.position + Random.insideUnitSphere * GroupDensity * GroupSize, Quaternion.Euler(Vector3.zero));
			follower.name = "Follower " + i;
			follower.SetGroupManager(this);
			follower.SetLeader(_leader);
			_members.Add(follower);
		}
	}

	void Update()
    {
        foreach (var member in _members)
        {
			member.SetState(_leader.state);
        }
    }

	public List<Transform> GetGroupMembers()
	{
		List<Transform> context = new List<Transform>();

		context.Add(Leader.transform);

		foreach (Character shopper in _members)
		{
			context.Add(shopper.transform);
		}

		return context;
	}

	public void UnRegister(Character destroyed)
	{
		if (_leader != destroyed)
		{
			_members.Remove(destroyed);
		}
	}
}