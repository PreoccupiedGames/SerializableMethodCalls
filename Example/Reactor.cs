using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace PreoccupiedGames.SerializeableMethodCall.Example
{
	public class Reactor : MonoBehaviour {

		[MethodCallEditorAttribute(typeof(bool), typeof(ComponentTarget))]
		public List<MethodCall> Conditions;
		public Sequence Response;
		
		// Update is called once per frame
		void Update() {
			if (Conditions.All(x => (bool)x.Invoke()))
			{
				this.StartCoroutine(Response.Run());
			}
		}

		public IEnumerator PrintDebug(string text)
		{
			Debug.Log (text);
			yield return new WaitForEndOfFrame();
			yield break;
		}
	}
}