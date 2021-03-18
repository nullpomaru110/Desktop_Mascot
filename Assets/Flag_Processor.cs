using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag_Processor : MonoBehaviour
{
	public bool isClickThrough
	{
		get { return _isClickThrough; }
	}
	private bool _isClickThrough = false;


	public VrmSample desktopmascot;

	// Start is called before the first frame update
	void Start()
    {
		desktopmascot = FindObjectOfType<VrmSample>();
	}

    // Update is called once per frame
    void Update()
    {
		desktopmascot.motion_flag = true;
    }
}
