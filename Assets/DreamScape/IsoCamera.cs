using UnityEngine;
using System.Collections;

namespace DreamScape
{
	[RequireComponent(typeof(Camera))]
	public class IsoCamera : MonoBehaviour
	{
		public int pixelSize = 2;

		private Camera sceneCamera;
		private Camera finalCamera;
		private RenderTexture sceneTarget;
		private GameObject finalSceneQuad;
		private bool dirty = true;

		private Vector3 cameraTarget;
		private float cameraRotation;

		public Vector3 CameraTarget { get { return cameraTarget; } set { cameraTarget = value; dirty = true; } }
		public float CameraRotation { get { return cameraRotation; } set { cameraRotation = value; dirty = true; } }
		public Vector3 CameraForward { get { return transform.forward; } }

		// Use this for initialization
		void Start()
		{
			if (Camera.main == null)
				sceneCamera = gameObject.AddComponent<Camera>();
			else
				sceneCamera = GetComponent<Camera>();

			sceneCamera.clearFlags = CameraClearFlags.SolidColor;
			// sceneCamera.backgroundColor = Color.black;
			sceneCamera.orthographic = true;
			CameraRotation = 45f;
			CameraTarget = new Vector3(5f, 0f, 5f);
			sceneCamera.tag = "MainCamera";

			if (pixelSize > 1)
			{
				sceneTarget = new RenderTexture(Screen.width / pixelSize, Screen.height / pixelSize, 24);
				sceneTarget.filterMode = FilterMode.Point;

				sceneCamera.cullingMask = ~(1 << 8);
				sceneCamera.targetTexture = sceneTarget;
				sceneCamera.orthographicSize = (float)sceneTarget.height / (2f * 32f);

				finalCamera = new GameObject("FinalCamera").AddComponent<Camera>();
				finalCamera.clearFlags = CameraClearFlags.SolidColor;
				finalCamera.backgroundColor = Color.black;
				finalCamera.orthographic = true;
				finalCamera.orthographicSize =  0.5f;
				finalCamera.cullingMask = 1 << 8;
				finalCamera.nearClipPlane = 0.0625f;
				finalCamera.farClipPlane = 8.0f;

				finalSceneQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
				finalSceneQuad.name = "FinalScene";
				finalSceneQuad.layer = 8;
				finalSceneQuad.transform.parent = finalCamera.transform;
				finalSceneQuad.transform.position = new Vector3(0f, 0f, 8f);
				finalSceneQuad.transform.localScale = new Vector3(finalCamera.aspect, 1f, 1f);
				((MeshRenderer)finalSceneQuad.GetComponent<Renderer>()).material = new Material(Shader.Find("Unlit/Texture"));
				((MeshRenderer)finalSceneQuad.GetComponent<Renderer>()).material.mainTexture = sceneTarget;
			}
			else
			{
				sceneCamera.orthographicSize = (float)Screen.height / (4f * 32f);

			}
		}

		// Update is called once per frame
		void Update()
		{
			CameraRotation += 30 * Time.deltaTime;

			if (dirty)
			{
				sceneCamera.transform.position = (Quaternion.Euler(30f, CameraRotation, 0f) * new Vector3(0f, 0f, -20f)) + CameraTarget;
				sceneCamera.transform.LookAt(CameraTarget);
				dirty = false;
			}
		}
	}
}