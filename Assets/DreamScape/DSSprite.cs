using UnityEngine;
using System.Collections;

namespace DreamScape
{
	public class DSSprite : MonoBehaviour
	{
		private Sprite sprite;
		private Character data;

		public DSSprite(Character charData)
		{
			data = charData;
		}

		// Use this for initialization
		void Start()
		{
			sprite = GetComponent<Sprite>();
		}

		// Update is called once per frame
		void Update()
		{
			transform.LookAt(transform.position + Camera.main.GetComponent<IsoCamera>().CameraForward);
		}
	}
}