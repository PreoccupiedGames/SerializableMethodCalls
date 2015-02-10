using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

namespace PreoccupiedGames.SerializeableMethodCall.Editor
{
	[CustomPropertyDrawer(typeof(MethodCallEditorAttribute), true)]
	public class MethodCallEditor : PropertyDrawer
	{
		protected SerializedProperty editing;
		protected int selectedMethodIndex = -1;
		protected MethodInfo[] methods;
		protected string[] methodNames;
		protected ParameterInfo[] parameterInfos;

		protected int currentSegment;
		protected int segmentCount;
		protected List<Rect> segments;
		protected MethodCallTarget target;
		protected SerializedObject targetSerializedObject;
		protected MethodCall asMethodCallBase;
		protected MethodTargetEditor targetPropertyDrawer;

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{
			if (editing == null || property.propertyPath != editing.propertyPath)
			{
				editing = property;
				selectedMethodIndex = -1;
				methods = null;
				methodNames = null;
				parameterInfos = null;
			}

			if (targetPropertyDrawer == null)
			{
				var tgtType = (attribute as MethodCallEditorAttribute).TargetType;

				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (var type in assembly.GetTypes())
					{
						var attrs = type.GetCustomAttributes(typeof(MethodTargetDrawerAttribute), false);
						var attr = attrs.FirstOrDefault();
						if (attr != null && (attr as MethodTargetDrawerAttribute).TargetType == tgtType)
						{
							targetPropertyDrawer = Activator.CreateInstance(type) as MethodTargetEditor;
							targetPropertyDrawer.MethodCallEditorAttribute = this.attribute as MethodCallEditorAttribute;
							targetPropertyDrawer.MethodCallOwningObject = property.serializedObject.targetObject;
							targetPropertyDrawer.MethodCallProperty = property;
						}
					}
				}
			}

			if (property.objectReferenceValue == null)
			{
				var value = ScriptableObject.CreateInstance<MethodCall>();

				if (AssetDatabase.GetAssetPath(property.serializedObject.targetObject) != "")
				{
					AssetDatabase.AddObjectToAsset(value, property.serializedObject.targetObject);
				}

				property.objectReferenceValue = value;
				property.serializedObject.ApplyModifiedProperties();
			}

			targetSerializedObject = new SerializedObject(property.objectReferenceValue);
			asMethodCallBase = targetSerializedObject.targetObject as MethodCall;

			segmentCount = parameterInfos != null ? parameterInfos.Count() + 2 : 2;
			var segmentWidth = position.width / segmentCount;
			segments = new List<Rect>();

			for (int i = 0; i < segmentCount; i++)
			{
				var rect = new Rect(position.x + i * segmentWidth, position.y, segmentWidth, position.height);
				segments.Add(rect);
			}

			currentSegment = 0;

			DoTargetEditor();

			if (target != null && target.ValidTarget)
			{
				DoMethodEditor();

				if (parameterInfos != null && currentSegment < segmentCount)
				{
					DoParameterEditor();
				}
			}

			targetSerializedObject.ApplyModifiedProperties();
		}

		protected virtual void DoTargetEditor()
		{
			var targetProperty = targetSerializedObject.FindProperty("Target");
		
			EditorGUI.BeginChangeCheck();

			EditorGUI.BeginProperty(segments[currentSegment], GUIContent.none, targetProperty);

			targetPropertyDrawer.OnGUI(segments[currentSegment++], targetProperty, GUIContent.none);

			EditorGUI.EndProperty();

			target = targetProperty.objectReferenceValue as MethodCallTarget;
			
			if ((methods == null && target != null && target.ValidTarget) || EditorGUI.EndChangeCheck() )
			{
				if (target != null && target.ValidTarget)
				{
					methods = (from method in target.TargetType.GetMethods()
					           where method.ReturnType == (attribute as MethodCallEditorAttribute).ReturnType
					           select method).ToArray();

					methodNames = (from method in methods
					               select method.Name).ToArray();
				}
				else
				{
					methodNames = null;
					methods = null;
				}
			}
		}

		protected virtual void DoMethodEditor()
		{
			if (selectedMethodIndex == -1 && asMethodCallBase.Method != null)
			{
				selectedMethodIndex = Array.FindIndex(methods, m => m.MethodHandle == asMethodCallBase.Method.MethodHandle);
				parameterInfos = asMethodCallBase.Method.GetParameters();
				
			}
			
			var methodIndexBefore = selectedMethodIndex;
			
			selectedMethodIndex = EditorGUI.Popup(segments[currentSegment++], selectedMethodIndex, methodNames);
			
			if (selectedMethodIndex != methodIndexBefore)
			{
				asMethodCallBase.Method = methods[selectedMethodIndex];
				
				parameterInfos = asMethodCallBase.Method.GetParameters();
			}
		}

		protected virtual void DoParameterEditor()
		{
			var i = 0;
			foreach (var parameter in parameterInfos)
			{
				if (currentSegment >= segmentCount)
				{
					break;
				}
				
				if (parameter.ParameterType == typeof(int))
				{
					asMethodCallBase.Parameters[i] = EditorGUI.IntField(segments[currentSegment++], (int)asMethodCallBase.Parameters[i]);
				}
				else if (parameter.ParameterType.IsEnum)
				{
					Enum e = asMethodCallBase.Parameters[i] as Enum;
					if (e != null)
					{
						asMethodCallBase.Parameters[i] = EditorGUI.EnumPopup(segments[currentSegment++], e);
					}
					else
					{
						EditorGUI.LabelField(segments[currentSegment++], "Enum is angry. Please delete and retry.");
					}
				}
				else if (parameter.ParameterType.IsSubclassOf(typeof(UnityEngine.Object)))
				{
					if (!(parameter.ParameterType.IsSubclassOf(typeof(GameObject)) || parameter.ParameterType == typeof(GameObject) || parameter.ParameterType.IsSubclassOf(typeof(Component))))
					{
						var all = GameObject.FindObjectsOfType(parameter.ParameterType);
						var names = (from reaction in all select reaction.name).ToArray();
						var index = Array.IndexOf(all, asMethodCallBase.Parameters[i]);

						if (index == -1)
						{
							index = 0;
						}

						index = EditorGUI.Popup(segments[currentSegment++], index, names);

						asMethodCallBase.Parameters[i] = all[index];
					}
					else
					{
						asMethodCallBase.Parameters[i] = EditorGUI.ObjectField(segments[currentSegment++], (UnityEngine.Object)asMethodCallBase.Parameters[i], parameter.ParameterType, true);
					}
				}
				else if (parameter.ParameterType == typeof(string))
				{
					asMethodCallBase.Parameters[i] = EditorGUI.TextField(segments[currentSegment++], (string)asMethodCallBase.Parameters[i]);
				}
				else if (parameter.ParameterType == typeof(Vector3))
				{
					asMethodCallBase.Parameters[i] = EditorGUI.Vector3Field(segments[currentSegment++], GUIContent.none, (Vector3)asMethodCallBase.Parameters[i]);
				}
				else if (parameter.ParameterType == typeof(Vector2))
				{
					asMethodCallBase.Parameters[i] = EditorGUI.Vector2Field(segments[currentSegment++], GUIContent.none, (Vector2)asMethodCallBase.Parameters[i]);
				}
				else
				{
					EditorGUI.LabelField(segments[currentSegment++], "Unsupported Param");
				}
				i++;
			}
		}
	}
}

