using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PreoccupiedGames.SerializeableMethodCall.Example
{
	public class Character : MonoBehaviour 
	{
		private List<GameObject> touching = new List<GameObject>();

		public void OnCollisionEnter(Collision collision)
		{
			touching.Add (collision.gameObject);
		}

		public void OnCollisionExit(Collision collision)
		{
			touching.Remove (collision.gameObject);
		}

		public bool IsTouching(GameObject gameObject)
		{
			return touching.Contains(gameObject);
		}

		public void Update()
		{
			if (Input.GetKey(KeyCode.UpArrow))
			{
				this.transform.position += Vector3.forward * 3 * Time.deltaTime;
			}

			if (Input.GetKey(KeyCode.DownArrow))
			{
				this.transform.position += Vector3.back * 3 * Time.deltaTime;
			}

			if (Input.GetKey(KeyCode.LeftArrow))
			{
				this.transform.position += Vector3.left * 3 * Time.deltaTime;
			}

			if (Input.GetKey(KeyCode.RightArrow))
			{
				this.transform.position += Vector3.right * 3 * Time.deltaTime;
			}
		}
	}
}