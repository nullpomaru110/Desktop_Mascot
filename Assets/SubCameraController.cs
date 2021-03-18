using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubCameraController : MonoBehaviour
{

	private GameObject Model;   //プレイヤー情報格納用
	private float dragSensitivity = 0.1f;
	public Vector3 offset;      //相対距離取得用
	public float bodysize = 1.3f;

	private void Start()
	{
		offset = new Vector3(0,0,10.0f);
	}

	// Update is called once per frame
	void Update()
	{
		if (!Model)
		{
			Model = GameObject.FindGameObjectWithTag("Model");
			offset = new Vector3(0, 0 , 10.0f);
			if (!Model)
			{
				Debug.LogError("Model is not imported");
				return;
			}
		}

		if (Model)
		{
			// CameraとModelとの相対距離を求める

			if (Input.GetMouseButton(1))
			{
				Vector3 screenVector = new Vector3(
					Input.GetAxis("Mouse X") * -dragSensitivity,
					Input.GetAxis("Mouse Y") * -dragSensitivity,
					0
					);
				offset += screenVector;
			}

			Vector3 pos = offset;
			pos.y += Model.transform.localScale.y*bodysize;

			//新しいトランスフォームの値を代入する
			transform.position = Model.transform.position + pos;

			Quaternion lockRotation = Quaternion.LookRotation(Model.transform.position - transform.position, Vector3.up);

			lockRotation.x = 0;
			lockRotation.z = 0;

			transform.rotation = lockRotation;
		}
	}	
}