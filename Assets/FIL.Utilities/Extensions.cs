using UnityEngine;

namespace FIL.Utilities
{
	public static class Extensions
	{
		public static Transform FindInHierarchy(this Transform t, string name)
		{
			for (int c = 0; c < t.childCount; c++)
			{
				Transform temp = t.GetChild(c);
				if (temp.name == name)
					return temp;
				else
				{
					temp = temp.FindInHierarchy(name);
					if (temp != null)
						return temp;
				}
			}
			
			return null;
		}
		
		public static float RoundTo(float number, float roundTo)
		{
			return Mathf.Round(number / roundTo) * roundTo;
		}

		public static float PixelsToWorld(float pixels, Camera cam = null)
		{
			if (cam == null)
				cam = Camera.main;

			return (2f*cam.orthographicSize*(float)pixels)/cam.pixelHeight;
		}

		public static float WorldToPixels(float world, Camera cam = null)
		{
			if (cam == null)
				cam = Camera.main;

			return (cam.pixelHeight*world)/(2*cam.orthographicSize);
		}
	}
}