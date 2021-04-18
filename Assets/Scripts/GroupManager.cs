using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GroupManager : MonoBehaviour
{
	public static GroupManager Instance;
	public Character leaderPrefab;
	public Character followerPrefab;

	[Range(2, 8)]
	public int GroupSize = 1;
	public float GroupDensity = 0.1f;

	protected List<Character> _members;
	public List<Character> Members { get { return _members; } }

	protected Character _leader;
	public Character Leader { get { return _leader; } }
	public int groupTotalRolls;
	public float gameDuration;
	public float timer;
	public GameObject aISpawner;
	public GameObject gameEnvironmentInitializer;
	public GameObject HUD;
	public GameObject screenTitle;
	public GameObject gameOverScreen;
	public GameObject gameWonScreen;

	//public List<Transform> aICustomerTransforms;
	public bool gameOver;
	public bool won = false;
    public int targetNumberOfRolls;
	private bool gameStarted;
	public bool debugMode = false;

    void Awake()
    {
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	public void StartGame()
    {
		Instantiate(aISpawner, Vector3.zero, Quaternion.identity);
		gameStarted = true;
		screenTitle.SetActive(false);
		HUD.SetActive(true);
		gameEnvironmentInitializer.SetActive(true);
		
		SetupGroup();

		targetNumberOfRolls = GroupSize * 2;
	}

	public void GetGroupNumberFromSlider(float value)
    {
		GroupSize = (int)value;

	}

    void Start()
	{
		timer = gameDuration;
		//StartGame();
	}

	public void RestartGame()
    {
		timer = gameDuration;
		won = false;
		gameOverScreen.SetActive(false);
		gameWonScreen.SetActive(false);

		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
			follower.transform.parent = transform;
			_members.Add(follower);
		}
	}

	void Update()
    {
		if (Input.GetKeyDown(KeyCode.R))
        {
			debugMode = !debugMode;
        }

		if (timer > 0 && gameStarted && !gameOver)
			timer -= Time.deltaTime;

		//check number of rolls the group collected
		int rollCount = 0;
		foreach (var member in _members)
		{
			member.SetState(_leader.state);
			rollCount += member.transform.childCount;
		}
		groupTotalRolls = rollCount;

		//game over when no more group members, or times up and target number not reached
		if (transform.childCount < 1 || (timer <= 1f && groupTotalRolls < targetNumberOfRolls))
        {
			gameOver = true;
        }
		if (timer <= 1f && groupTotalRolls >= targetNumberOfRolls)
		{
			won = true;
		}
		



		if (gameOver)
        {
			Cursor.lockState = CursorLockMode.None;
			gameOverScreen.SetActive(true);
			return;
		}

		if (won)
        {
			Cursor.lockState = CursorLockMode.None;
			gameWonScreen.SetActive(true);
			return;
		}

		/**
		List<Transform> allAICustomer = new List<Transform>();

		for (int i = 0; i < AISpawner.transform.childCount; i++)
        {
			allAICustomer.Add(AISpawner.transform.GetChild(i).transform);
		}
		aICustomerTransforms = allAICustomer;
		**/
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