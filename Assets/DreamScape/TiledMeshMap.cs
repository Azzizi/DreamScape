using UnityEngine;

namespace DreamScape
{
	public class TiledMeshMap
	{
		public string Name { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }

		private int[][] tiles;
		private int[][] heightmap;
		private Tile[] tileData;

		public void Load()
		{
			Name = "testmap";
			tileData = new Tile[1];
			tileData[0] = new Tile(null);
			Width = 10;
			Height = 10;

			tiles = new int[10][];
			heightmap = new int[10][];

			for (int c = 0; c < 10; c++)
			{
				tiles[c] = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
				heightmap[c] = new int[10] { 0, 0, 0, 0, 1, 1, 0, 0, 0, 0 };
			}
		}

		public void UnLoad()
		{

		}

		public void Update()
		{

		}

		public void BuildMap(Mesh mesh)
		{
			Vector3[] verts = new Vector3[4 * Width * Height];
			Vector2[] uvs = new Vector2[4 * Width * Height];
			int[] tris = new int[6 * Width * Height];

			for (int c = 0; c < tiles.Length; c++)
			{
				for (int i = 0; i < tiles[c].Length; i++)
				{
					verts[(c * Width + i) * 4] = new Vector3(c, heightmap[c][i], i);
					verts[(c * Width + i) * 4 + 1] = new Vector3(c + 1, heightmap[c][i], i);
					verts[(c * Width + i) * 4 + 2] = new Vector3(c + 1, heightmap[c][i], i + 1);
					verts[(c * Width + i) * 4 + 3] = new Vector3(c, heightmap[c][i], i + 1);

					Tile tile = tileData[tiles[c][i]];
					uvs[(c * Width + i) * 4] = tile.uvs[0];
					uvs[(c * Width + i) * 4 + 1] = tile.uvs[1];
					uvs[(c * Width + i) * 4 + 2] = tile.uvs[2];
					uvs[(c * Width + i) * 4 + 3] = tile.uvs[3];

					tris[(c * Width + i) * 6] = (c * Width + i) * 4;
					tris[(c * Width + i) * 6 + 1] = (c * Width + i) * 4 + 3;
					tris[(c * Width + i) * 6 + 2] = (c * Width + i) * 4 + 1;
					tris[(c * Width + i) * 6 + 3] = (c * Width + i) * 4 + 3;
					tris[(c * Width + i) * 6 + 4] = (c * Width + i) * 4 + 2;
					tris[(c * Width + i) * 6 + 5] = (c * Width + i) * 4 + 1;
				}
			}

			mesh.vertices = verts;
			mesh.uv = uvs;
			mesh.triangles = tris;
		}

		public Vector3 MapToWorldCoords(int x, int y)
		{
			return new Vector3( x + 0.5f, heightmap[x][y], y + 0.5f );
		}
	}
}