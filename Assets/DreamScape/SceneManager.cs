using UnityEngine;
using System.Collections;

namespace DreamScape
{
	public class SceneManager : MonoBehaviour
	{
		public int pixelSize = 2;

		private Camera sceneCamera;
		private Camera finalCamera;
		private RenderTexture sceneTarget;
		private GameObject finalSceneQuad;

		// Use this for initialization
		void Start()
		{
			if (Camera.main == null)
				sceneCamera = new GameObject("SceneCamera").AddComponent<Camera>();
			else
				sceneCamera = Camera.main;

			sceneCamera.clearFlags = CameraClearFlags.SolidColor;
			// sceneCamera.backgroundColor = Color.black;
			sceneCamera.orthographic = true;
			sceneCamera.transform.position = Quaternion.Euler(30f, 45f, 0f) * new Vector3(0f, 0f, -20f);
			sceneCamera.transform.LookAt(Vector3.zero);

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
				sceneCamera.tag = "MainCamera";
			}

		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}