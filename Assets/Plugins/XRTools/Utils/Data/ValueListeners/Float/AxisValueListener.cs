using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XRTools.Utils.Data
{
	public class AxisValueListener : FloatRangeListener
	{
		[System.Serializable]
		public enum SingleAxis { X, Y, Z };
		public SingleAxis axis;
	}

#if UNITY_EDITOR
	
	[CustomEditor(typeof(AxisValueListener), true)]
	public class AxisValueListenerEditor : FloatRangeListenerEditor
	{
		public override void OnInspectorGUI()
		{
			SerializedProperty axis = serializedObject.FindProperty("axis");

			axis.intValue = (int)((AxisValueListener.SingleAxis)EditorGUILayout.EnumPopup("Axis to Listen to ", (AxisValueListener.SingleAxis)axis.intValue));

			base.OnInspectorGUI();
			serializedObject.ApplyModifiedProperties();
		}
	}
	
#endif
}