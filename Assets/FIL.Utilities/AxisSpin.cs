using UnityEngine;
using System.Collections;

namespace FIL.Utilities
{
	public class AxisSpin : MonoBehaviour
	{
		public Vector3 rotationAxis;
		public float speed;

		// Update is called once per frame
		void Update()
		{
			transform.Rotate(rotationAxis, speed * Time.deltaTime);
		}
	}
}