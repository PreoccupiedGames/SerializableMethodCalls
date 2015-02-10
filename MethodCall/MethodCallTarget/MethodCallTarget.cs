using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace PreoccupiedGames.SerializeableMethodCall
{
	public abstract class MethodCallTarget : ScriptableObject, ICloneable
	{
		public abstract object ResolveTarget(Dictionary<string, object> Context = null);

		public abstract Type TargetType { get; }

		public abstract bool ValidTarget { get; }

		public abstract object Clone();
	}
}