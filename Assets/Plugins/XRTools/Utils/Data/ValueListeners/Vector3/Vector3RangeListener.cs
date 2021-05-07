using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XRTools.Utils.Data
{
	public class Vector3RangeListener : Vector3ValueListener
	{
		public Vector2 activationRangeX = new Vector2(0.8f, 1);
		public Vector2 activationRangeY = new Vector2(0.8f, 1);
		public Vector2 activationRangeZ = new Vector2(0.8f, 1);
	}


#if UNITY_EDITOR

	[CustomEditor(typeof(Vector3RangeListener))]
	public class Vector3RangeListenerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			SerializedProperty activationRangeX = serializedObject.FindProperty("activationRangeX");
			SerializedProperty activationRangeY = serializedObject.FindProperty("activationRangeY");
			SerializedProperty activationRangeZ = serializedObject.FindProperty("activationRangeZ");

			Vector2 rangex = activationRangeX.vector2Value;
			Vector2 rangey = activationRangeY.vector2Value;
			Vector2 rangez = activationRangeZ.vector2Value;

			EditorGUILayout.MinMaxSlider(ref rangex.x, ref rangex.y, 0, 1);
			EditorGUILayout.MinMaxSlider(ref rangey.x, ref rangey.y, 0, 1);
			EditorGUILayout.MinMaxSlider(ref rangez.x, ref rangez.y, 0, 1);

			activationRangeX.vector2Value = rangex;
			activationRangeY.vector2Value = rangey;
			activationRangeZ.vector2Value = rangez;

			serializedObject.ApplyModifiedProperties();
			base.OnInspectorGUI();
		}
	}
	
#endif
}