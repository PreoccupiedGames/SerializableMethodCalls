using UnityEngine;
using System.Collections;
using UnityEditor;

namespace PreoccupiedGames.SerializeableMethodCall.Example.Editor
{
	[CustomPropertyDrawer(typeof(Sequence))]
	public class SequenceEditor : PropertyDrawer
	{
		private bool foldoutOpen;
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.objectReferenceValue == null)
			{
				property.objectReferenceValue = ScriptableObject.CreateInstance<Sequence>();
				return;
			}

			var lineHeight = base.GetPropertyHeight(property, label);
			var currentPos = new Rect(position.xMin, position.yMin, position.width, lineHeight);
			var foldoutPos = new Rect(position.xMin, position.yMin, position.width / 2, lineHeight);
			var addPos = new Rect(position.xMin + position.width / 2, position.yMin, position.width / 2, lineHeight);

			var serializedObj = new SerializedObject(property.objectReferenceValue);
		
			foldoutOpen = EditorGUI.Foldout(foldoutPos, foldoutOpen, label);
			currentPos.y += lineHeight;

			var itemsProp = serializedObj.FindProperty("sequence");

			if (GUI.Button(addPos, "Add"))
			{
				var newIndex = itemsProp.arraySize;
				itemsProp.InsertArrayElementAtIndex(newIndex);
				itemsProp.GetArrayElementAtIndex(newIndex).objectReferenceValue = null;
			}

			if (foldoutOpen)
			{
				var toDelete = -1;

				for (int i = 0; i < itemsProp.arraySize; i++)
				{
					var fieldRect = new Rect(currentPos.xMin, currentPos.yMin, currentPos.width / 10 * 9, currentPos.height);
					var deleteRect = new Rect(fieldRect.xMax, currentPos.yMin, currentPos.width / 10, currentPos.height);
					var el = itemsProp.GetArrayElementAtIndex(i);
					EditorGUI.PropertyField(fieldRect, el, GUIContent.none);
					if (GUI.Button(deleteRect, "X"))
					{
						toDelete = i;
					}
					currentPos.y += lineHeight;
				}

				if (toDelete != -1)
				{
					itemsProp.DeleteArrayElementAtIndex(toDelete);
					itemsProp.DeleteArrayElementAtIndex(toDelete);
				}
			}

			serializedObj.ApplyModifiedProperties();
		}

		public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
		{
			if (property.objectReferenceValue == null)
				return base.GetPropertyHeight(property, label);

			var serializedObj = new SerializedObject(property.objectReferenceValue);
			var elementCount = serializedObj.FindProperty("sequence").arraySize;
			return foldoutOpen ? base.GetPropertyHeight(property, label) * (1 + elementCount) : base.GetPropertyHeight(property, label);
		}
	}
}
