using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VrmUiController : MonoBehaviour {

	public WindowController windowController;

	public RectTransform panel;
	public Text informationText;
	public Text warningText;
	public Button closeButton;
	public Toggle transparentToggle;
	public Toggle topmostToggle;
	public Toggle maximizeToggle;
	public Toggle randommoveToggle;
	public Toggle updateheadToggle;
	public Toggle shadowToggle;
	public Toggle cameraToggle;
	public Button quitButton;
	public Text titleText;

	private float mouseMoveSS = 0f;				// Sum of mouse trajectory squares. [px^2]
	private float mouseMoveSSThreshold = 16f;	// Threshold to be regarded as not moving. [px^2]
	private Vector3 lastMousePosition;

	private bool isDebugMode = false;   // Show debug Info.

	public bool menu_flag = false;
	public bool randommove_flag = true;
	public bool head_flag = true;
	public bool moveable = true;

	private GameObject Shadow;

	private GameObject SubCam;
	


	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Start () {

		Shadow = GameObject.FindGameObjectWithTag("Shadow");

		SubCam = GameObject.FindGameObjectWithTag("SubCamera");
		SubCam.SetActive(false);


		windowController = FindObjectOfType<WindowController>();
		windowController.OnStateChange += windowController_OnStateChanged;

		if (!panel)
		{
			panel = GetComponentInChildren<RectTransform>();
		}
		if (!informationText)
		{
			informationText = GetComponentInChildren<Text>();
		}

		windowController.SetMaximized(true);
		// Initialize toggles.
		UpdateUI();

		// Set event listeners.
		if (closeButton) { closeButton.onClick.AddListener(Close); }
		if (quitButton) { quitButton.onClick.AddListener(Quit); }
		if (transparentToggle) { transparentToggle.onValueChanged.AddListener(windowController.SetTransparent); }
		if (maximizeToggle) { maximizeToggle.onValueChanged.AddListener(windowController.SetMaximized); }
		if (topmostToggle) { topmostToggle.onValueChanged.AddListener(windowController.SetTopmost); }
		if (randommoveToggle) { randommoveToggle.onValueChanged.AddListener(changerandommoveToggle); }
		if (updateheadToggle) { updateheadToggle.onValueChanged.AddListener(changeupdateheadToggle); }

		if (shadowToggle) { shadowToggle.onValueChanged.AddListener(changeshadowToggle); }

		if (cameraToggle) { cameraToggle.onValueChanged.AddListener(changecameraToggle); }

		// Show menu on startup.
		Show(null);
	}

	private void changecameraToggle(bool isActive)
	{
		SubCam.SetActive(isActive);
		moveable = !isActive;

		windowController.SetMaximized(!isActive);
	}

	private void changeshadowToggle(bool isActive)
	{
		Debug.Log(isActive);
		Shadow.SetActive(isActive);
	}

	private void changerandommoveToggle(bool isActive)
	{
		randommove_flag = isActive;
	}

	private void changeupdateheadToggle(bool isActive)
	{
		head_flag = isActive;
	}

	private void windowController_OnStateChanged()
	{
		UpdateUI();
	}

	/// <summary>
	/// UIの状況を現在のウィンドウ状態に合わせて更新
	/// </summary>
	public void UpdateUI()
	{
		if (!windowController) return;

		if (transparentToggle) { transparentToggle.isOn = windowController.isTransparent; }
		if (maximizeToggle) { maximizeToggle.isOn = windowController.isMaximized; }
		if (topmostToggle) { topmostToggle.isOn = windowController.isTopmost; }
		if (randommoveToggle) { randommoveToggle.isOn = randommove_flag; }
		if (shadowToggle) { shadowToggle.isOn = Shadow.activeSelf; }
		if (cameraToggle) { cameraToggle.isOn = SubCam.activeSelf; }
	}


	/// <summary>
	/// メニューを閉じる
	/// </summary>
	private void Close()
	{
		panel.gameObject.SetActive(false);
		menu_flag = true;
}

	/// <summary>
	/// 終了ボタンが押された時の処理。エディタ上であれば再生停止とする。
	/// </summary>
	private void Quit()
	{
#if UNITY_EDITOR
		// Stop playing for the editor
		UnityEditor.EditorApplication.isPlaying = false;
#else
		// Quit application for the standalone player
		Application.Quit();
#endif
	}

	/// <summary>
	/// Update is called once per frame
	/// </summary>
	void Update () {
		// マウス右ボタンクリックでメニューを表示させる。閾値以下の移動ならクリックとみなす。
		if (Input.GetMouseButtonDown(1))
		{
			lastMousePosition = Input.mousePosition;
		}
		if (Input.GetMouseButton(1))
		{
			mouseMoveSS += (Input.mousePosition - lastMousePosition).sqrMagnitude;
		}
		if (Input.GetMouseButtonUp(1))
		{
			if (mouseMoveSS < mouseMoveSSThreshold)
			{
				Show();
			}
			mouseMoveSS = 0f;
		}

		// [ESC] でメニューを閉じる
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Close();
		}

		// 裏機能
		// メニューのタイトルがマウスカーソル直下の色になる
		if (isDebugMode &&  windowController && titleText)
		{
			titleText.color = windowController.pickedColor;
			//Debug.Log(windowController.pickedColor);
		}
	}

	/// <summary>
	/// フォーカスが外れたときの処理
	/// </summary>
	/// <param name="focus"></param>
	private void OnApplicationFocus(bool focus)
	{
		// フォーカスが外れたらメニューを閉じる
		if (!focus)
		{
			Close();
		}
	}

	/// <summary>
	/// メニューを表示する
	/// </summary>
	public void Show()
	{
		menu_flag = false;

		if (panel)
		{
			panel.gameObject.SetActive(true);
		}

		// 裏機能
		// メニューが表示されるとき、[Shift]が押されていればデバッグ表示を有効にする
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			isDebugMode = true;
		} else
		{
			isDebugMode = false;
		}
	}

	/// <summary>
	/// Show the meta information
	/// </summary>
	/// <param name="meta"></param>
	public void Show(VRM.VRMMetaObject meta)
	{
		if (meta)
		{
			if (informationText)
			{
				string text = string.Format(
					"Title:{0}\nVer.:{1}\nAuthor:{2}\nAllowedUser:{3}\nLicense:{4} {5}\n",
					meta.Title,
					meta.Version,
					meta.Author,
					meta.AllowedUser,
					meta.LicenseType,
					meta.OtherLicenseUrl
					);
				informationText.text = text;
			}
			else
			{
				informationText.text = "Drop a VRM file here!";
			}

			if (warningText)
			{
				if (meta.AllowedUser == VRM.AllowedUser.Everyone)
				{
					warningText.text = "";
				} else if (meta.AllowedUser == VRM.AllowedUser.OnlyAuthor)
				{
					warningText.text = "Only the author is permitted to perform.";
				} else
				{
					warningText.text = "Only the explicitly licensed person is permitted to perform.";
				}
			}
		}

		Show();
	}

	/// <summary>
	/// Set the warning text
	/// </summary>
	/// <param name="message"></param>
	public void SetWarning(string message)
	{
		if (warningText)
		{
			warningText.text = message;
		}
	}
}
