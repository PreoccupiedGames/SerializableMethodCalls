using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using System.Collections.Generic;

namespace PreoccupiedGames.SerializeableMethodCall
{
	public class ComponentTarget : MethodCallTarget, ISerializationCallbackReceiver
	{
		[HideInInspector, SerializeField]
		protected string targetComponentTypeName;

		public GameObject TargetGameObject;
		public Type ComponentType;

		public void OnBeforeSerialize()
		{
			if (ComponentType != null)
			{
				targetComponentTypeName = ComponentType.FullName;
			}
			else
			{
				targetComponentTypeName = null;
			}
		}

		public void OnAfterDeserialize()
		{
			if (targetComponentTypeName != null)
			{
				ComponentType = Type.GetType(targetComponentTypeName);
			}
			else
			{
				ComponentType = null;
			}
		}

		public override bool ValidTarget {
			get {
				return TargetGameObject != null && ComponentType != null;
			}
		}

		public override Type TargetType {
			get {
				return ComponentType;
			}
		}

		public override object ResolveTarget(Dictionary<string, object> Context = null)
		{
			return TargetGameObject.GetComponent(ComponentType);
		}

		public override object Clone()
		{
			var clone = ScriptableObject.CreateInstance<ComponentTarget>();

			#if UNITY_EDITOR
			if (AssetDatabase.GetAssetPath(this) != "")
			{
				AssetDatabase.AddObjectToAsset(clone, this);
			}
			#endif

			clone.ComponentType = ComponentType;
			clone.TargetGameObject = TargetGameObject;

			return clone;
		}
	}
}