using UnityEngine;
using System.Collections;
using System.IO;
using VRM;

public class CharacterBehaviour : MonoBehaviour
{

	public float LookAtSpeed = 10.0f;   // 頭の追従速度係数 [1/s]
	private float BlinkTime = 0.1f; // まばたきで閉じるまたは開く時間 [s]

	private float lastBlinkTime = 0f;
	private float nextBlinkTime = 0f;
	private int blinkState = 0; // まばたきの状態管理。 0:なし, 1:閉じ中, 2:開き中

	private VRMLookAtHead lookAtHead;
	private VRMBlendShapeProxy blendShapeProxy;

	private GameObject targetObject;    // 視線目標オブジェクト
	private Transform headTransform;    // Head transform
	private bool isNewTargetObject = false; // 新規に目標オブジェクトを作成したらtrue

	private Vector3 targetPosition;
	private CharacterController characterController;
	private Animator animator;
	private VrmSample vrmsample;
	private VrmUiController vrmui;

	public float HeadSpeed = 0.2f;
	public float CenterHight = 1.0f;
	

	// Use this for initialization
	void Start()
	{
		if (!targetObject)
		{
			targetObject = new GameObject("LookAtTarget");
			isNewTargetObject = true;
		}

		lookAtHead = GetComponent<VRMLookAtHead>();
		blendShapeProxy = GetComponent<VRMBlendShapeProxy>();

		if (lookAtHead)
		{
			lookAtHead.Target = targetObject.transform;
			lookAtHead.UpdateType = UpdateType.LateUpdate;

			headTransform = lookAtHead.Head;
		}
		if (!headTransform)
		{
			headTransform = this.transform;
		}

		targetPosition = transform.position;
		characterController = GetComponent<CharacterController>();
		characterController.radius = 0.05f;
		characterController.skinWidth = 0.01f;
		characterController.height = 0.05f;
		Vector3 pos = characterController.center;
		pos.y = CenterHight;
		characterController.center = pos;

		velocity = Vector3.zero;
		Me = GameObject.FindWithTag("Me");

		animator = GetComponent<Animator>();
		animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animations/Animators/UnityChanLocomotions");
		vrmsample = FindObjectOfType<VrmSample>();
		vrmui = FindObjectOfType<VrmUiController>();

		//animator.enabled = false;
	}


	/// <summary>
	/// Destroy created target object
	/// </summary>
	void OnDestroy()
	{
		if (isNewTargetObject)
		{
			GameObject.Destroy(targetObject);
		}
	}

	/// <summary>
	/// 毎フレーム呼ばれる
	/// </summary>
	void Update()
	{

		if (vrmui.head_flag)
		{
			UpdateLookAtTarget();
		}
		Blink();

		MouseMove();
		//Randommove();
		if (vrmsample.motion_flag)
		{
			animator.enabled = false;
		}
		Motion();
		ScaleChange();
	}

	/// <summary>
	/// Update()より後で呼ばれる
	/// </summary>
	void LateUpdate()
	{
		if (vrmui.head_flag)
		{
			UpdateHead();
		}
	}

	/// <summary>
	/// 目線目標座標を更新
	/// </summary>
	private void UpdateLookAtTarget()
	{
		Vector3 mousePos = Input.mousePosition;
		// モデル座標から 1[m] 手前に設定
		//mousePos.z = (Camera.main.transform.position - headTransform.position).magnitude - 1f;
		mousePos.z = 5f;
		Vector3 pos = Camera.main.ScreenToWorldPoint(mousePos);
		targetObject.transform.position = pos;
	}

	/// <summary>
	/// マウスカーソルの方を見る動作
	/// </summary>
	private void UpdateHead()
	{
		//Quaternion rot = Quaternion.Euler(-lookAtHead.Pitch, lookAtHead.Yaw, 0f);
		//Debug.Log(rot);

		Quaternion rot = Quaternion.LookRotation(targetObject.transform.position - headTransform.position, Vector3.up);

		headTransform.rotation = Quaternion.Slerp(headTransform.rotation, rot, HeadSpeed);
	}

	/// <summary>
	/// 
	/// </summary>
	private void Blink()
	{
		if (!blendShapeProxy) return;

		float now = Time.timeSinceLevelLoad;
		float span;

		switch (blinkState)
		{
			case 1:
				span = now - lastBlinkTime;
				if (span > BlinkTime)
				{
					blinkState = 2;
					blendShapeProxy.SetValue(BlendShapePreset.Blink, 1f);
				}
				else
				{
					blendShapeProxy.SetValue(BlendShapePreset.Blink, (span / BlinkTime));
				}
				break;
			case 2:
				span = now - lastBlinkTime - BlinkTime;
				if (span > BlinkTime)
				{
					blinkState = 0;
					blendShapeProxy.SetValue(BlendShapePreset.Blink, 0f);
				}
				else
				{
					blendShapeProxy.SetValue(BlendShapePreset.Blink, (1f - span) / BlinkTime);
				}
				break;
			default:
				if (now >= nextBlinkTime)
				{
					lastBlinkTime = now;
					if (Random.value < 0.2f)
					{
						nextBlinkTime = now;    // 20%の確率で連続まばたき
					}
					else
					{
						nextBlinkTime = now + Random.Range(1f, 10f);
					}
					blinkState = 1;
				}
				break;
		}
	}

	//　レイを飛ばす距離
	private float rayRange = 100f;
	//　速度
	private Vector3 velocity;
	//　移動スピード
	[SerializeField]
	private float moveSpeed = 2.0f;
	//　マウスクリックで移動する位置を決定するかどうか

	//　回転度合い
	[SerializeField]
	private float smoothRotateSpeed = 180f;

	private GameObject Me;

	private bool TargetFlag = true;
	private bool rotate_flag = true;


	private void MouseMove()
	{
		velocity = Vector3.zero;
		//　マウスクリックまたはisMouseDownModeがOffの時マウスの位置を移動する位置にする
		if (Input.GetButton("Fire1") && vrmui.menu_flag && vrmui.moveable)
		{
			animator.enabled = true;
			vrmsample.motion_flag = false;
			rotate_flag = false;
			timecount = 0;

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			Debug.DrawRay(ray.origin, ray.direction * rayRange, Color.green, 5, false);

			if (Physics.Raycast(ray, out hit, rayRange, LayerMask.GetMask("Field")))
			{
				Vector3 pos = hit.point;
				pos.y -= transform.localScale.y;
				hit.point = pos;
				targetPosition = hit.point;
			}
		}
		//　移動の目的地と0.1mより距離がある時は速度を計算


		//Debug.Log(transform.position);
		//Debug.Log(targetPosition);

		if (Vector2.Distance(transform.position, targetPosition) > 0.01f * transform.localScale.z)
		{
			var moveDirection = (targetPosition - transform.position).normalized;
			velocity = new Vector3(moveDirection.x * moveSpeed * transform.localScale.x, moveDirection.y * moveSpeed * transform.localScale.y, moveDirection.z * moveSpeed * transform.localScale.z);
			
			//　徐々にキャラクターの向きを変更する
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(new Vector3(moveDirection.x, 0, 0)), smoothRotateSpeed * Time.deltaTime);
			
			//　アニメーションパラメータの設定
			animator.SetFloat("Speed", moveDirection.magnitude);
		}
		else
		{
			animator.SetFloat("Speed", 0f);

			if (Input.GetMouseButton(2))
			{
				Model_rotate();
				rotate_flag = true;
			}
			else if(!rotate_flag)
			{
				Quaternion lockRotation = Quaternion.LookRotation(Me.transform.position - transform.position, Vector3.up);

				lockRotation.x = 0;
				lockRotation.z = 0;

				transform.rotation = Quaternion.Slerp(transform.rotation, lockRotation, 5f * Time.deltaTime);
			}
		}

		characterController.Move(velocity * Time.deltaTime);
	}

	private int wait_time = 0;
	private float timecount = 0;

	private void Motion()
	{
		AnimatorStateInfo anim = animator.GetCurrentAnimatorStateInfo(0);

		/*if (Mathf.Abs((transform.rotation * Quaternion.Inverse(lockRotation)).w) > 0.95)
		{
			if (anim.IsName("Idle"))
			{
				timecount += (int)Time.deltaTime;
			}
		}*/

		if (anim.IsName("Idle"))
		{
			timecount += Time.deltaTime;
		}
		//Debug.Log(timecount);
		if (timecount >= 12 + wait_time && anim.IsName("Idle") && vrmui.randommove_flag)
		{
			int waitstate = Random.Range(0, 3);

			switch (waitstate)
			{
				case 0:
					animator.SetBool("Rest", true);
					break;
				case 1:
					animator.SetBool("JUMP01", true);
					break;
				case 2:
					animator.SetBool("WAIT02", true);
					break;
			}
			//Debug.Log(waitstate);
			timecount = 0;
			wait_time = Random.Range(0, 15);
		}
		else
		{
			animator.SetBool("Rest", false);
			animator.SetBool("JUMP01", false);
			animator.SetBool("WAIT02", false);
		}
	}


	public Vector3 scale = new Vector3(1,1,1);
	private float scroll;

	private void ScaleChange()
	{
		scroll = Input.GetAxis("Mouse ScrollWheel");
		transform.localScale += new Vector3(scale.x*scroll, scale.y*scroll, scale.z*scroll);
	}

	private float dragSensitivity = 5.0f;

	private void Model_rotate()
	{
			// 中ボタンドラッグで回転
		Vector3 screenVector = new Vector3(
			Input.GetAxis("Mouse X") * dragSensitivity,
			0f,
			0f
			);
		transform.Rotate(0, -screenVector.x, 0);
			
	}

	public float Randommove_x = 7f;
	public float Randommove_y = 4f;

	private void Randommove()
	{
		velocity = Vector3.zero;
		//　マウスクリックまたはisMouseDownModeがOffの時マウスの位置を移動する位置にする
		if (timecount >= 20 && vrmui.menu_flag && vrmui.moveable)
		{
			animator.enabled = true;
			vrmsample.motion_flag = false;
			timecount = 0;

			Vector3 pos = new Vector3(Random.Range(-Randommove_x, Randommove_x), Random.Range(-Randommove_y, Randommove_y), 0f);
			targetPosition = pos;
		}

		if (Vector2.Distance(transform.position, targetPosition) > 0.01f * transform.localScale.z)
		{
			var moveDirection = (targetPosition - transform.position).normalized;
			velocity = new Vector3(moveDirection.x * moveSpeed * transform.localScale.x, moveDirection.y * moveSpeed * transform.localScale.y, moveDirection.z * moveSpeed * transform.localScale.z);

			//　徐々にキャラクターの向きを変更する
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(new Vector3(moveDirection.x, 0, 0)), smoothRotateSpeed * Time.deltaTime);

			//　アニメーションパラメータの設定
			animator.SetFloat("Speed", moveDirection.magnitude);
		}
		else
		{
			animator.SetFloat("Speed", 0f);

			Quaternion lockRotation = Quaternion.LookRotation(Me.transform.position - transform.position, Vector3.up);

			lockRotation.x = 0;
			lockRotation.z = 0;

			transform.rotation = Quaternion.Slerp(transform.rotation, lockRotation, 5f * Time.deltaTime);
		}

		characterController.Move(velocity * Time.deltaTime);
	}
}
