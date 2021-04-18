using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomer : Character
{
	public bool turned = false;

	public override void Start()
	{
		base.Start();
	}

	private void Update()
	{
		visionDistance = 0.6f;
		/**
		//	�Ӿ���㶨��Ϊ��ɫ��ǰλ�ó�����ת������ǰ0.41fΪ���
		Vector3 detectVisionStartAt = transform.position + transform.up * 0.5f;
		//	�Ӿ���ǰ���յ�Ϊ�Ӿ���㳯����ת���򳤶�Ϊ�Ӿ�����
		Vector3 endVisionAt = detectVisionStartAt + transform.up * visionDistance;
		//	�Ӿ����������յ�Ϊ�Ӿ���������ӽ�Ϊ�Ƕȣ��Ӿ�����Ϊ�߳�������
		Vector3 leftVisionEnd = detectVisionStartAt + Quaternion.Euler(0, 0, visionAngle) * transform.up * visionDistance;
		Vector3 rightVisionEnd = detectVisionStartAt + Quaternion.Euler(0, 0, -visionAngle) * transform.up * visionDistance;
		frontVision = Physics2D.Raycast(detectVisionStartAt, transform.up, visionDistance);
		RaycastHit2D leftVision = Physics2D.Raycast(detectVisionStartAt, Quaternion.Euler(0, 0, visionAngle) * transform.up, visionDistance);
		RaycastHit2D rightVision = Physics2D.Raycast(detectVisionStartAt, Quaternion.Euler(0, 0, -visionAngle) * transform.up, visionDistance);
		Debug.DrawLine(detectVisionStartAt, endVisionAt, Color.green);
		//left vision ray
		Debug.DrawLine(detectVisionStartAt, leftVisionEnd, Color.yellow);
		//right vision ray
		Debug.DrawLine(detectVisionStartAt, rightVisionEnd, Color.black);
		
		if (Input.GetMouseButtonDown(0))
		{
			target = cam.ScreenToWorldPoint(Input.mousePosition);
			storeTarget = target;
			visionDistance = 0f;
		}

		float distToDest = Vector2.Distance(transform.position, storeTarget);
	
		if (rightVision && rightVision.collider.CompareTag("Obstacle") && visionDistance > 0f && distToDest > 0.5f)
		{
			Vector2 targetTurningPoint = rightVision.point + rightVision.normal * (visionDistance);
			//nextWallFollow.transform.position = targetTurningPoint;
			wallFollow = targetTurningPoint;
			target = targetTurningPoint;
		}
		else
		{
			if (Vector2.Distance(transform.position, target) < 0.02f)
			{
				target = storeTarget;
			}
		}

		if (leftVision && leftVision.collider.CompareTag("Obstacle")
			&& visionDistance > 0f && distToDest > 0.5f)
		{
			Vector2 targetTurningPoint = leftVision.point + leftVision.normal * (visionDistance);
			//nextWallFollow.transform.position = targetTurningPoint;
			wallFollow = targetTurningPoint;
			target = targetTurningPoint;
		}
		else
		{
			if (Vector2.Distance(transform.position, target) < 0.02f)
			{
				target = storeTarget;
			}
		}
		**/
		if (Input.GetMouseButtonDown(0))
		{
			target = cam.ScreenToWorldPoint(Input.mousePosition);
			storeTarget = target;
			visionDistance = 0f;
		}
		Vector3 detectVisionStartAt = transform.position + transform.up * 0.5f;
		Vector3 endVisionAt = detectVisionStartAt + transform.up * visionDistance;
		RaycastHit2D frontVision = Physics2D.Raycast(detectVisionStartAt, transform.up * 0.5f, visionDistance);
		Debug.DrawLine(detectVisionStartAt, endVisionAt, Color.green);

		float dist = Vector2.Distance(transform.position, storeTarget);


		//wall following by single front vision
		if (frontVision.collider != null && frontVision.collider.tag == "Obstacle")
		{
			//Debug.Log(frontVision.collider.name);
			Vector2 targetTurningPoint = frontVision.point + frontVision.normal * 0.6f;
			GameObject wallFollow = Instantiate(wallFollowMarker, targetTurningPoint, Quaternion.identity);
			Destroy(wallFollow, 1f);
			//nextWallFollow.transform.position = targetTurningPoint;

			target = targetTurningPoint;
		}
		else
		{
			if (Vector2.Distance(transform.position, target) < 0.02f)
			{
				target = storeTarget;

			}
		}

		Vector2 targetForce = Seek(target, 1, dist);
		ApplyForce(targetForce);
		UpdateMovement();

		if (Input.GetKeyDown(KeyCode.A))
		{
			state = State.FETCH_ROLL;
		}
		if (Input.GetKeyDown(KeyCode.S))
		{
			state = State.STEAL;
		}
		if (Input.GetKeyDown(KeyCode.F))
        {
			state = State.FOLLOW;
        }
	}

}