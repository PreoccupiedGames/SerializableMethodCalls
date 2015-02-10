using UnityEngine;
using System.Collections;
using UnityEditor;

namespace PreoccupiedGames.SerializeableMethodCall.Editor
{
	public abstract class MethodTargetEditor : PropertyDrawer
	{
		public UnityEngine.Object MethodCallOwningObject { get; set; }
		public MethodCallEditorAttribute MethodCallEditorAttribute { get; set; }
		public SerializedProperty MethodCallProperty { get; set; }
	}
}
