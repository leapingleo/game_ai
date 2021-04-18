using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupManager : MonoBehaviour
{
	public static GroupManager Instance;
	public Character leaderPrefab;
	public Character followerPrefab;

	[Range(1, 10)]
	public int GroupSize = 1;
	public float GroupDensity = 0.1f;

	protected List<Character> _members;
	public List<Character> Members { get { return _members; } }

	protected Character _leader;
	public Character Leader { get { return _leader; } }
	public int groupTotalRolls;
	public float gameDuration;
	public float timer;
	public GameObject gameEnvironmentInitializer;
	public GameObject HUD;
	public GameObject screenTitle;
	public GameObject AISpawner;
	public List<Transform> aICustomerTransforms;

	void Awake()
    {
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	public void StartGame()
    {
		screenTitle.SetActive(false);
		HUD.SetActive(true);
		gameEnvironmentInitializer.SetActive(true);
		//AISpawner.SetActive(true);
		SetupGroup();
	}



    void Start()
	{
		timer = gameDuration;
		StartGame();
	}

	void SetupGroup()
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
		if (timer > 0)
			timer -= Time.deltaTime; 


		int rollCount = 0;
        foreach (var member in _members)
        {
			member.SetState(_leader.state);
			rollCount += member.transform.childCount;
        }
		groupTotalRolls = rollCount;


		List<Transform> allAICustomer = new List<Transform>();

		for (int i = 0; i < AISpawner.transform.childCount; i++)
        {
			allAICustomer.Add(AISpawner.transform.GetChild(i).transform);
		}
		aICustomerTransforms = allAICustomer;
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