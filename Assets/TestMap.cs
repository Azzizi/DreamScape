using UnityEngine;
using DreamScape;

public class TestMap : MonoBehaviour
{
	TiledMeshMap map;

	// Use this for initialization
	void Start()
	{
		Mesh mapMesh = new Mesh();
		map = new TiledMeshMap();
		map.Load();
		map.BuildMap(mapMesh);
		GetComponent<MeshFilter>().mesh = mapMesh;
	}

	// Update is called once per frame
	void Update()
	{

	}
}