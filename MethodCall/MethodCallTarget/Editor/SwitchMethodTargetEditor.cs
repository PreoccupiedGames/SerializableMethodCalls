using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using UnityEditor;

namespace PreoccupiedGames.SerializeableMethodCall.Editor
{
	[MethodTargetDrawer(typeof(MethodCallTarget))]
	public class SwitchMethodTargetEditor : MethodTargetEditor
	{
		private string[] methodTargetTypeNames;
		private Type[] methodTargetTypes;
		private int selectedTypeIndex;
		private MethodTargetEditor targetPropertyDrawer;

		public override void OnGUI (Rect position, UnityEditor.SerializedProperty property, GUIContent label)
		{
			var first = new Rect(position.x, position.y, position.width / 6, position.height);
			var second = new Rect(position.x + position.width / 6, position.y, position.width / 6 * 5, position.height);

			var serializedObj = property.serializedObject;
			var methodTargetType = property.objectReferenceValue == null ? null : property.objectReferenceValue.GetType();

			if (methodTargetTypes == null)
			{
				var type = typeof(MethodCallTarget);
				var listMethodTargetTypes = (AppDomain.CurrentDomain.GetAssemblies()
				                         .SelectMany(s => s.GetTypes())
				                         .Where(p => p.IsSubclassOf(type))).ToList();
				listMethodTargetTypes.Insert(0, null);
				methodTargetTypes = listMethodTargetTypes.ToArray();
				methodTargetTypeNames = (from methodType in methodTargetTypes select methodType == null ? "None" : methodType.Name).ToArray();
			}

			if (methodTargetType != null)
			{
				selectedTypeIndex = Array.IndexOf(methodTargetTypes, methodTargetType);

				if (targetPropertyDrawer == null)
				{
					createTargetDrawer(methodTargetType, property);
				}
			}
			else
			{
				selectedTypeIndex = 0;
			}

			EditorGUI.BeginChangeCheck();

			selectedTypeIndex = EditorGUI.Popup(first, selectedTypeIndex, methodTargetTypeNames);

			if (EditorGUI.EndChangeCheck())
			{
				if (selectedTypeIndex == 0)
				{
					property.objectReferenceValue = null;
					targetPropertyDrawer = null;
				}
				else
				{
					var tgtType = methodTargetTypes[selectedTypeIndex];
					var tgt = ScriptableObject.CreateInstance(tgtType);

					if (AssetDatabase.GetAssetPath(property.serializedObject.targetObject) != "")
					{
						AssetDatabase.AddObjectToAsset(tgt, property.serializedObject.targetObject);
					}

					property.objectReferenceValue = tgt;
					createTargetDrawer(tgtType, property);
				}
			}

			if (property.objectReferenceValue != null && targetPropertyDrawer != null)
			{
				EditorGUI.BeginProperty(position, GUIContent.none, property);
				
				targetPropertyDrawer.OnGUI(second, property, GUIContent.none);
				
				EditorGUI.EndProperty();
			}

			serializedObj.ApplyModifiedProperties();
		}

		private void createTargetDrawer(Type targetType, SerializedProperty property)
		{	
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var type in assembly.GetTypes())
				{
					var attrs = type.GetCustomAttributes(typeof(MethodTargetDrawerAttribute), false);
					var attr = attrs.FirstOrDefault();
					if (attr != null && (attr as MethodTargetDrawerAttribute).TargetType == targetType)
					{
						targetPropertyDrawer = Activator.CreateInstance(type) as MethodTargetEditor;
						targetPropertyDrawer.MethodCallEditorAttribute = this.MethodCallEditorAttribute as MethodCallEditorAttribute;
						targetPropertyDrawer.MethodCallOwningObject = this.MethodCallOwningObject;
						targetPropertyDrawer.MethodCallProperty = this.MethodCallProperty;
					}
				}
			}
		}
	}
}