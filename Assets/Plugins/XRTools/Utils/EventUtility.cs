using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.Events;

namespace XRTools.Utils
{
	class EventUtillity
	{

		private struct PersistentCall
		{
			private SerializedProperty callProperty;
			private string propertyPathBase;
			private SerializedProperty target;
			private SerializedProperty methodName;
			private SerializedProperty mode;
			private SerializedProperty callState;

			private SerializedProperty args;
			private SerializedProperty objectArg;
			private SerializedProperty objectArgType;
			private SerializedProperty intArg;
			private SerializedProperty floatArg;
			private SerializedProperty stringArg;
			private SerializedProperty boolArg;

			internal PersistentCall(in SerializedProperty callProperty, in string propertyPathBase)
			{
				// Read and cache

				this.callProperty = callProperty;
				this.propertyPathBase = propertyPathBase;

				target = callProperty?.FindPropertyRelative("m_Target");
				methodName = callProperty?.FindPropertyRelative("m_MethodName");
				mode = callProperty?.FindPropertyRelative("m_Mode");
				callState = callProperty?.FindPropertyRelative("m_CallState");

				args = callProperty?.FindPropertyRelative("m_Arguments");
				objectArg = args?.FindPropertyRelative("m_ObjectArgument");
				objectArgType = args?.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName");
				intArg = args?.FindPropertyRelative("m_IntArgument");
				floatArg = args?.FindPropertyRelative("m_FloatArgument");
				stringArg = args?.FindPropertyRelative("m_StringArgument");
				boolArg = args?.FindPropertyRelative("m_BoolArgument");
			}

			internal static void MemberwiseClone(in PersistentCall src, in PersistentCall dst)
			{
				// Write

				dst.target.objectReferenceValue = src.target.objectReferenceValue;
				dst.methodName.stringValue = src.methodName.stringValue;
				dst.mode.enumValueIndex = src.mode.enumValueIndex;
				dst.callState.enumValueIndex = src.callState.enumValueIndex;

				dst.objectArg.objectReferenceValue = src.objectArg.objectReferenceValue;
				dst.objectArgType.stringValue = src.objectArgType.stringValue;
				dst.intArg.intValue = src.intArg.intValue;
				dst.floatArg.floatValue = src.floatArg.floatValue;
				dst.stringArg.stringValue = src.stringArg.stringValue;
				dst.boolArg.boolValue = src.boolArg.boolValue;
			}

			public override string ToString() => $"[{(UnityEventCallState)callState.enumValueIndex}] {target.objectReferenceValue}.{methodName.stringValue}({GetParamSignature(mode.enumValueIndex)})";

			private string GetParamSignature(in int enumIndex)
			{
				switch (enumIndex)
				{
					case 0: // Event Defined
						return $"{GetEventType(this)} (dynamic call)";
					case 1: // void
						return $"{typeof(void)}";
					case 2: // Object
						return $"{objectArg.objectReferenceValue.GetType()} = {objectArg.objectReferenceValue}";
					case 3: // int
						return $"{typeof(int)} = {intArg.intValue}";
					case 4: // float
						return $"{typeof(float)} = {floatArg.floatValue}";
					case 5: // string
						return $"{typeof(string)} = {stringArg.stringValue}";
					case 6: // bool
						return $"{typeof(bool)} = {boolArg.boolValue}";
					default:
						return string.Empty;
				}

				Type GetEventType(in PersistentCall self)
				{
					var @object = self.callProperty.serializedObject.targetObject;
					var names = self.propertyPathBase.Split('.');
					var result = @object.GetType()
										.GetField(names.FirstOrDefault(),
													BindingFlags.NonPublic
													| BindingFlags.Public
													| BindingFlags.Instance)?.GetValue(@object);
					while (!(result is UnityEventBase))
					{
						result = result.GetType()
										.GetField(names.LastOrDefault(),
													BindingFlags.NonPublic
													| BindingFlags.Public
													| BindingFlags.Instance)?.GetValue(result);
					}

					return result?.GetType();
				}
			}
		}

		/// <returns>A log of all transferred calls, empty if there were none.</returns>
		public static string TransferPersistentCalls(
			MonoBehaviour source, MonoBehaviour destination, string srcEventName, string dstEventName, bool removeOldCalls = false, in bool dryRun = true)
		{
			const string CallsPropertyPathFormat = "{0}.m_PersistentCalls.m_Calls";
			var srcPropertyPath = string.Format(CallsPropertyPathFormat, srcEventName);
			var dstPropertyPath = string.Format(CallsPropertyPathFormat, dstEventName);
			var src = new SerializedObject(source);
			var dst = new SerializedObject(destination);
			var srcCalls = src.FindProperty(srcPropertyPath);
			var dstCalls = dst.FindProperty(dstPropertyPath);
			var dstCallsOriginalCount = dstCalls.arraySize;

			var log = string.Empty;
			for (var srcIndex = 0; srcIndex < srcCalls.arraySize; srcIndex++)
			{
				#region Init Source
				var srcCallProperty = srcCalls.GetArrayElementAtIndex(srcIndex);
				var srcCall = new PersistentCall(srcCallProperty, srcEventName);
				var logLine = $"({srcIndex}) {srcCall}\n";
				#endregion

				if (!dryRun)
				{
					SerializedProperty dstCallProperty;

					#region Check if the Call already Exists in the Destination
					if (dstCallsOriginalCount > 0)
					{
						dstCallProperty = dstCalls.GetArrayElementAtIndex(srcIndex);

						// If we are satisfied that the call is exactly the same, skip ahead.
						if (SerializedProperty.DataEquals(srcCallProperty, dstCallProperty))
						{
							log += logLine;
							continue;
						}
					}
					#endregion

					// Only unique properties beyond this point. Append with care.

					#region Copy Properties from Source to Destination
					var dstIndex = dstCallsOriginalCount + srcIndex;
					dstCalls.InsertArrayElementAtIndex(dstIndex);
					dstCallProperty = dstCalls.GetArrayElementAtIndex(dstIndex);

					var dstCall = new PersistentCall(dstCallProperty, dstEventName);
					
					PersistentCall.MemberwiseClone(srcCall, dstCall);
					#endregion
				}
				log += logLine;
			}
			log += $"\n(<b>Bold</b> = not already present in {dstEventName}.)\n";

			if (!dryRun)
			{
				if (removeOldCalls && (dstCalls.arraySize > 0))
				{
					srcCalls.ClearArray();
				}
				src.ApplyModifiedProperties();
				if (source != destination)
				{
					dst.ApplyModifiedProperties();
				}
			}

			return log;
		}
	}
}