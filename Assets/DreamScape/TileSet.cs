using UnityEngine;
using System.Collections;

namespace DreamScape
{
	public struct Tile
	{
		public int id;
		public Texture2D atlas;
		public Vector2[] uvs;

		public Tile(Texture2D tex)
		{
			id = 0;
			atlas = tex;
			uvs = new Vector2[4] { new Vector2(0,0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
		}
	}
}
