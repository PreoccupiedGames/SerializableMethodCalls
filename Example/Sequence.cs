using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PreoccupiedGames.SerializeableMethodCall.Example
{
	public class Sequence : ScriptableObject {

		[SerializeField, HideInInspector, MethodCallEditor(typeof(IEnumerator), typeof(ComponentTarget))]
		private List<MethodCall> sequence;

		public Sequence()
		{
			sequence = new List<MethodCall>();
		}

		public void Add(MethodCall methodCall)
		{
			sequence.Add(methodCall);
		}

		public void AddRange(IEnumerable<MethodCall> methodCallEnumerable)
		{
			sequence.AddRange(methodCallEnumerable);
		}

		public IEnumerator Run()
		{
			foreach (var call in sequence)
			{
				var steps = (IEnumerator)call.Invoke();
				while (steps.MoveNext())
				{
					yield return steps.Current;
				}
			}
			yield break;
		}
	}
}