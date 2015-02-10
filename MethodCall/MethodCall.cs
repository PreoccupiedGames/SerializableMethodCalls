using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System;
using UnityEditor;

namespace PreoccupiedGames.SerializeableMethodCall
{
	public class MethodCall : ScriptableObject, ISerializationCallbackReceiver, ICloneable
	{
		[HideInInspector, SerializeField]
		protected string methodName;
		[HideInInspector, SerializeField]
		protected List<UnityEngine.Object> unityObjectParameters;
		[HideInInspector, SerializeField]
		protected List<string> stringParameters;
		[HideInInspector, SerializeField]
		protected List<double> numberParameters;
		[HideInInspector, SerializeField]
		protected List<Vector2> vector2Parameters;
		[HideInInspector, SerializeField]
		protected List<Vector3> vector3Parameters;
		[HideInInspector, SerializeField]
		protected string targetTypeName;

		protected ParameterInfo[] parameterInfos;

		protected MethodBase method;

		public object[] Parameters;

		[SerializeField]
		public MethodCallTarget Target;

		public MethodBase Method {
			get {
				return method;
			}
			set {
				if (value != method)
				{
					method = value;

					parameterInfos = method.GetParameters();

					Parameters = new object[parameterInfos.Length];
					
					for (var i = 0; i < parameterInfos.Length; i++)
					{
						if(parameterInfos[i].ParameterType.IsValueType)
						{
							Parameters[i] = Activator.CreateInstance(parameterInfos[i].ParameterType);
						}
						else
						{
							Parameters[i] = null;
						}
					}
				}
			}
		}

		public void OnBeforeSerialize()
		{
			if (Method != null)
			{
				methodName = method.Name;
			}
			else
			{
				methodName = null;
			}

			targetTypeName = Target != null && Target.ValidTarget ? Target.TargetType.FullName : null;

			if (Parameters != null)
			{
				unityObjectParameters = new List<UnityEngine.Object>();
				stringParameters = new List<string>();
				numberParameters = new List<double>();
				vector2Parameters = new List<Vector2>();
				vector3Parameters = new List<Vector3>();

				var i = 0;
				foreach (var parameter in parameterInfos)
				{
					var paramType = parameter.ParameterType;

					if (paramType.IsEnum)
					{
						if (Parameters[i] == null)
						{
							numberParameters.Add(0);
						}
						else
						{
							numberParameters.Add((double)(int)Parameters[i]);
						}
					}
					else if (paramType == typeof(int) || paramType == typeof(float) || paramType == typeof(double))
					{
						numberParameters.Add(Convert.ToDouble(Parameters[i]));
					}
					else if (paramType == typeof(string))
					{
						stringParameters.Add((string)Parameters[i]);
					}
					else if (paramType.IsSubclassOf(typeof(UnityEngine.Object)))
					{
						if (Parameters[i] != null)
						{
							unityObjectParameters.Add((UnityEngine.Object)Parameters[i]);
						}
						else
						{
							unityObjectParameters.Add(null);
						}
					}
					else if (paramType == typeof(Vector3))
					{
						vector3Parameters.Add((Vector3)Parameters[i]);
					}
					else if (paramType == typeof(Vector2))
					{
						vector2Parameters.Add((Vector2)Parameters[i]);
					}
					
					i++;
				}
			}
		}

		public void OnAfterDeserialize()
		{
			//  we can't make comparisons on SerializedObjects
			// off of the main thread, so we need to check the
			// target type ourselves
			if (methodName != null && targetTypeName != null && targetTypeName != "")
			{
				var type = Type.GetType(targetTypeName);
				method = type.GetMethod(methodName);
				if (method != null)
				{
					parameterInfos = method.GetParameters();
				}
				else
				{
					parameterInfos = null;
				}
			}
			else
			{
				method = null;
			}

			if (parameterInfos != null)
			{

				Parameters = new object[parameterInfos.Length];

				var i = 0;
				foreach (var parameter in parameterInfos)
				{
					var paramType = parameter.ParameterType;

					if (paramType.IsEnum)
					{
						Parameters[i] = Enum.ToObject(paramType, (int)numberParameters[0]);
						numberParameters.RemoveAt(0);
					}
					else if (paramType == typeof(int) || paramType == typeof(float) || paramType == typeof(double))
					{
						Parameters[i] = Convert.ChangeType(numberParameters[0], paramType);
						numberParameters.RemoveAt(0);
					}
					else if (paramType == typeof(string))
					{
						Parameters[i] = stringParameters[0];
						stringParameters.RemoveAt(0);
					}
					else if (paramType.IsSubclassOf(typeof(UnityEngine.Object)))
					{
						Parameters[i] = unityObjectParameters[0];
						unityObjectParameters.RemoveAt(0);
					}
					else if (paramType == typeof(Vector3))
					{
						Parameters[i] = vector3Parameters[0];
						vector3Parameters.RemoveAt(0);
					}
					else if (paramType == typeof(Vector2))
					{
						Parameters[i] = vector2Parameters[0];
						vector2Parameters.RemoveAt(0);
					}

					i++;
				}
			}
		}

		public virtual object Clone()
		{
			var clone = ScriptableObject.CreateInstance<MethodCall>();

	#if UNITY_EDITOR
			if (AssetDatabase.GetAssetPath(this) != "")
			{
				AssetDatabase.AddObjectToAsset(clone, this);
			}
	#endif

			clone.Target = this.Target.Clone() as MethodCallTarget;

			clone.Method = this.Method;

			for (int i = 0; i < clone.parameterInfos.Length; i++)
			{
				clone.Parameters[i] = Parameters[i];
			}

			return clone;
		}

		public virtual object Invoke(Dictionary<string, object> Context = null)
		{
			var target = Target.ResolveTarget(Context);

			return Method.Invoke(target, Parameters);
		}
	}
}
