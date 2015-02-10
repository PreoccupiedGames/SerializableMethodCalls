using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PreoccupiedGames.SerializeableMethodCall.Editor
{
	[MethodTargetDrawer(typeof(ComponentTarget))]
	public class ComponentTargetEditor : MethodTargetEditor
	{
		private string[] componentNames;
		private Type[] componentTypes;
		private int selectedComponentIndex;

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.objectReferenceValue == null)
			{
				var tgt = ScriptableObject.CreateInstance<ComponentTarget>();
				if (AssetDatabase.GetAssetPath(property.serializedObject.targetObject) != "")
				{
					AssetDatabase.AddObjectToAsset(tgt, property.serializedObject.targetObject);
				}

				property.objectReferenceValue = tgt;
			}

			var first = new Rect(position.x, position.y, position.width / 2, position.height);
			var second = new Rect(position.x + position.width / 2, position.y, position.width / 2, position.height);

			var serializedObj = new SerializedObject(property.objectReferenceValue);
			var asComponentTarget = property.objectReferenceValue as ComponentTarget;
			var gameObjectProp = serializedObj.FindProperty("TargetGameObject");

			EditorGUI.BeginChangeCheck();

			EditorGUI.PropertyField(first, gameObjectProp, GUIContent.none);

			var gameObjectValue = gameObjectProp.objectReferenceValue as GameObject;

			if ((componentTypes == null && gameObjectValue != null) || EditorGUI.EndChangeCheck())
			{
				if (gameObjectValue == null)
				{
					componentNames = null;
					componentTypes = null;
					asComponentTarget.ComponentType = null;
				}
				else
				{
					var types = (from component in gameObjectValue.GetComponents(typeof(MonoBehaviour)) select component.GetType());
					componentTypes = types.ToArray();
					componentNames = (from component in componentTypes select component.FullName).ToArray();

					if (asComponentTarget.ComponentType == null)
					{
						asComponentTarget.ComponentType = componentTypes[0];
						selectedComponentIndex = 0;
					}
					else
					{
						selectedComponentIndex = Array.IndexOf(componentTypes, asComponentTarget.ComponentType);
					}
				}
			}

			if (gameObjectValue != null)
			{
				EditorGUI.BeginChangeCheck();
				selectedComponentIndex = EditorGUI.Popup(second, selectedComponentIndex, componentNames);
				if (EditorGUI.EndChangeCheck())
				{
					asComponentTarget.ComponentType = componentTypes[selectedComponentIndex];
				}
			}

			serializedObj.ApplyModifiedProperties();
		}
	}
}
