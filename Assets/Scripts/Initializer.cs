using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initializer : MonoBehaviour
{
	public GameObject emptyShelfPrefab_0;
	public GameObject paperRollPrefab;
	public SecurityController securityPrefab;
	public GameObject shelfLocationManager;

	void Start()
	{
		InitializeEmptyShelves();
		//InitializePaperRolls();
		//InitializeSecurities();
	}

	void InitializeEmptyShelves()
	{
        for (int i = 0; i < shelfLocationManager.transform.childCount; i++)
        {
			Instantiate(emptyShelfPrefab_0, shelfLocationManager.transform.GetChild(i).transform.position,
				Quaternion.Euler(Vector3.zero)).name = "Empty Shelf 0";
		}
		
	}

	void InitializePaperRolls()
	{
		int rolls_per_layer = 5;
		float x_offset = (6f - 3.7f) / 5;

		for (int i = 0; i < rolls_per_layer; i++)
		{
			Instantiate(paperRollPrefab, new Vector3(3.7f + i * x_offset, 2.35f, 0), Quaternion.Euler(Vector3.zero)).name = "Paper_Roll_U_" + i;
			Instantiate(paperRollPrefab, new Vector3(3.7f + i * x_offset, 1.57f, 0), Quaternion.Euler(Vector3.zero)).name = "Paper_Roll_U_" + i;
		}
	}

	void InitializeSecurities()
	{
		for (int i = 0; i < 1; i++)
		{
			Vector3 start = new Vector3(15.7f, 0f, 0);
			Vector3 end = new Vector3(15.7f, -7f, 0);
			SecurityController security = Instantiate(securityPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero));
			security.name = "Security_" + i;
			security.Initialize(start, end);
		}
	}
}