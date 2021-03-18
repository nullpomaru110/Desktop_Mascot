using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameRate : MonoBehaviour
{
	static int targetFrameRate;

	private void Awake()
	{
		Application.targetFrameRate = 60;
	}
}
