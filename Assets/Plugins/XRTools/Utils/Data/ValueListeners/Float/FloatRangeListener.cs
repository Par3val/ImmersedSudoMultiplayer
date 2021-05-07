using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XRTools.Utils.Data
{
	public class FloatRangeListener : FloatValueListener
	{
		public Vector2 activationRange = new Vector2(0.8f, 1);

		protected override bool ShouldActivate(float value)
		{
			if (value >= activationRange.x && value <= activationRange.y)
				return true;
			return false;
		}
	}
	
#if UNITY_EDITOR
	

	[CustomEditor(typeof(FloatRangeListener), true)]
	public class FloatRangeListenerEditor : FloatValueListenerEditor
	{
		public override void OnInspectorGUI()
		{
			SerializedProperty activationRange = serializedObject.FindProperty("activationRange");

			Vector2 range = activationRange.vector2Value;
			GUILayout.BeginHorizontal(); 
				
			GUILayout.Label(new GUIContent($"Activation Range ({range})", "is based 0 to 1"));
			EditorGUILayout.MinMaxSlider(ref range.x, ref range.y, 0, 1);
			GUILayout.EndHorizontal();

			activationRange.vector2Value = range;

			base.OnInspectorGUI();
			serializedObject.ApplyModifiedProperties();
		}
	}

#endif
}