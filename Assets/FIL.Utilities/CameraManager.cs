using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
	public int pixelSize = 2;

	public Camera sceneCamera = null;
	private Camera finalCamera;
	private RenderTexture sceneTarget;
    private GameObject finalSceneQuad;

	// Use this for initialization
	void Start()
	{
		if (sceneCamera == null)
			sceneCamera = new GameObject("SceneCamera").AddComponent<Camera>();

		sceneCamera.clearFlags = CameraClearFlags.SolidColor;
		sceneCamera.backgroundColor = Color.black;
		sceneCamera.orthographic = true;

		if (pixelSize > 1)
		{
            sceneTarget = new RenderTexture(Screen.width / pixelSize, Screen.height / pixelSize, 24);
			sceneCamera.orthographicSize = Screen.height / pixelSize / 2;
            sceneTarget.filterMode = FilterMode.Point;

			sceneCamera.cullingMask = ~(1 << 8);
            sceneCamera.targetTexture = sceneTarget;

			finalCamera = new GameObject("FinalCamera").AddComponent<Camera>();
			finalCamera.clearFlags = CameraClearFlags.SolidColor;
			finalCamera.backgroundColor = Color.black;
			finalCamera.orthographic = true;
			finalCamera.orthographicSize = 0.5f;
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
