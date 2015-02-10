using UnityEngine;
using System.Collections;
using System;

namespace PreoccupiedGames.SerializeableMethodCall.Editor
{
	public class MethodTargetDrawerAttribute : Attribute
	{
		public Type TargetType;

		public MethodTargetDrawerAttribute(Type type)
		{
			this.TargetType = type;
		}
	}
}	