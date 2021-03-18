/**
 * VrmSample
 * 
 * Author: Kirurobo http://twitter.com/kirurobo
 * License: CC0 https://creativecommons.org/publicdomain/zero/1.0/
 */

using System;
using System.IO;
using UniHumanoid;
using UnityEngine;
using VRM;

public class VrmSample : MonoBehaviour {

	private WindowController windowController;

	private VRMImporterContext context;
	private HumanPoseTransfer model;
	private HumanPoseTransfer motion;
	private VRMMetaObject meta;

	public VrmUiController uiController;
	public CameraController cameraController;
	public Transform cameraTransform;

	private CameraController.ZoomMode originalWheelMode;

	public AudioSource audioSource;

	public bool motion_flag = false;



	// Use this for initialization
	void Start () {
		if (!uiController)
		{
			uiController = FindObjectOfType<VrmUiController>();
		}

		if (!cameraController)
		{
			cameraController = FindObjectOfType<CameraController>();
			if (cameraController)
			{
				originalWheelMode = cameraController.zoomMode;
			}
		}

		if (!audioSource)
		{
			audioSource = FindObjectOfType<AudioSource>();
		}

		// Load the initial model.
		string[] cmdArgs = System.Environment.GetCommandLineArgs();
		if (cmdArgs.Length > 1)
		{
			LoadModel(cmdArgs[1]);
		} else
		{
			LoadModel(Application.streamingAssetsPath + "/default_vrm.vrm");
		}

		// Load the initial motion.
		//LoadMotion(Application.streamingAssetsPath + "/default_bvh.txt");

		// Initialize window manager
		windowController = FindObjectOfType<WindowController>();
		if (windowController)
		{
			// Add a file drop handler.
			windowController.OnFilesDropped += Window_OnFilesDropped;
		}
	}

	void Update()
	{
		// 透明なところではホイール操作は受け付けなくする
		if (windowController && cameraController)
		{
			Vector2 pos = Input.mousePosition;
			bool inScreen = (pos.x >= 0 && pos.x < Screen.width && pos.y >= 0 && pos.y < Screen.height);
			if (!windowController.isClickThrough && inScreen)
			{
				cameraController.zoomMode = originalWheelMode;
			} else
			{
				cameraController.zoomMode = CameraController.ZoomMode.None;
			}
		}

		// End を押すとウィンドウ透過切替
		if (Input.GetKeyDown(KeyCode.End))
		{
			windowController.SetTransparent(!windowController.isTransparent);
		}

		// Home を押すと最前面切替
		if (Input.GetKeyDown(KeyCode.Home))
		{
			windowController.SetTopmost(!windowController.isTopmost);
		}
		// F11 を押すと最大化切替
		if (Input.GetKeyDown(KeyCode.F11))
		{
			windowController.SetMaximized(!windowController.isMaximized);
		}

		// Insert を押すと最小化切替
		if (Input.GetKeyDown(KeyCode.Insert))
		{
			windowController.SetMinimized(!windowController.isMinimized);
		}
	}

	/// <summary>
	/// A handler for file dropping.
	/// </summary>
	/// <param name="files"></param>
	private void Window_OnFilesDropped(string[] files)
	{
		foreach (string path in files)
		{
			string ext = path.Substring(path.Length - 4).ToLower();

			// Open the VRM file if its extension is ".vrm".
			if (ext == ".vrm")
			{
				LoadModel(path);
				continue;
			}

			// Open the motion file if its extension is ".bvh" or ".txt".
			if (ext == ".bvh" || ext == ".txt")
			{
				LoadMotion(path);
				continue;
			}

			// Open the audio file.
			// mp3はライセンスの関係でWindowsスタンドアローンでは読み込めないよう。
			// 参考 https://docs.unity3d.com/jp/460/ScriptReference/WWW.GetAudioClip.html
			// 参考 https://answers.unity.com/questions/433428/load-mp3-from-harddrive-on-pc-again.html
			if (ext == ".ogg" || ext == ".wav")
			{
				LoadAudio(path);
				continue;
			}
		}
	}

	/// <summary>
	/// Apply the motion to the model.
	/// </summary>
	/// <param name="motion"></param>
	/// <param name="model"></param>
	/// <param name="meta"></param>
	private void SetMotion(HumanPoseTransfer motion, HumanPoseTransfer model, VRMMetaObject meta)
	{
		if (!model || !motion || !meta) return;

		// Apply the motion if AllowedUser is equal to "Everyone".
		if (meta.AllowedUser == AllowedUser.Everyone)
		{
			model.Source = motion;
			model.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseTransfer;
		}
	}


	/// <summary>
	/// Load the motion from a BVH file.
	/// </summary>
	/// <param name="path"></param>
	private void LoadMotion(string path)
	{
		motion_flag = true;

		if (!File.Exists(path)) return;

		GameObject newMotionObject = null;

		try
		{
			BvhImporterContext context = new BvhImporterContext();
			//Debug.Log("Loading motion : " + path);

			context.Parse(path);
			context.Load();
			newMotionObject = context.Root;

			// Hide the motion model
			Renderer renderer = newMotionObject.GetComponent<Renderer>();
			if (renderer)
			{
				renderer.enabled = false;
			}

		} catch (Exception ex)
		{
			if (uiController) uiController.SetWarning("Motion load failed.");
			Debug.LogError("Failed loading " + path);
			Debug.LogError(ex);
			return;
		}
		
		if (newMotionObject)
		{
			if (motion)
			{
				GameObject.Destroy(motion.gameObject);
			}
			motion = newMotionObject.GetComponent<HumanPoseTransfer>();
			SetMotion(motion, model, meta);

			// Play loaded audio if available
			if (audioSource && audioSource.clip && audioSource.clip.loadState == AudioDataLoadState.Loaded)
			{
				audioSource.Stop();
				audioSource.Play();
			}
		}
	}

	/// <summary>
	/// Unload the old model and load the new model from VRM file.
	/// </summary>
	/// <param name="path"></param>
	private void LoadModel(string path)
	{
		if (!File.Exists(path)) return;
		GameObject newModelObject = null;

		try
		{
			// Load from a VRM file.
			context = new VRMImporterContext();
			//Debug.Log("Loading model : " + path);

			context.Load(path);
			newModelObject = context.Root;
			meta = context.ReadMeta();

			context.ShowMeshes();
		}
		catch (Exception ex)
		{
			if (uiController) uiController.SetWarning("Model load failed.");
			Debug.LogError("Failed loading " + path);
			Debug.LogError(ex);
			return;
		}

		if (newModelObject)
		{
			if (model)
			{
				GameObject.Destroy(model.gameObject);
			}

			model = newModelObject.AddComponent<HumanPoseTransfer>();
			SetMotion(motion, model, meta);
			// 位置をplaneより前に
			model.transform.position = new Vector3(0,0,0.01f);

			model.gameObject.AddComponent<CharacterBehaviour>();
			model.gameObject.AddComponent<CharacterController>();


			model.gameObject.tag = "Model";
			
			model.gameObject.AddComponent<OVRLipSyncContext>();
			model.gameObject.AddComponent<OVRLipSyncMicInput>();
			model.gameObject.AddComponent<VRMLipSyncContextMorphTarget>();

			if (uiController)
			{
				uiController.Show(meta);
			}
		}
	}

	/// <summary>
	/// Load the audio clip
	/// Reference: http://fantom1x.blog130.fc2.com/blog-entry-299.html
	/// </summary>
	/// <param name="path"></param>
	private void LoadAudio(string path)
	{
		StartCoroutine(LoadAudioCoroutine(path));
	}

	private System.Collections.IEnumerator LoadAudioCoroutine(string path)
	{
		if (!File.Exists(path)) yield break;

		using (WWW www = new WWW("file://" + path))
		{
			while (!www.isDone) {
				yield return null;
			}

			AudioClip audioClip = www.GetAudioClip(false, false);
			if (audioClip.loadState != AudioDataLoadState.Loaded)
			{
				Debug.Log("Failed to load audio: " + path);
				yield break;
			}

			audioSource.clip = audioClip;
			audioSource.Play();
			Debug.Log("Audio: " + path);
		}
	}
}
