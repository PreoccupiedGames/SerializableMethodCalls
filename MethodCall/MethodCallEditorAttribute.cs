using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

namespace PreoccupiedGames.SerializeableMethodCall
{
	public class MethodCallEditorAttribute : PropertyAttribute
	{
		public Type TargetType { get; private set; }

		public Type ReturnType { get; private set; }

		public MethodCallEditorAttribute(Type returnType, Type targetType)
		{
			this.ReturnType = returnType;
			this.TargetType = targetType;
		}
	}
}
