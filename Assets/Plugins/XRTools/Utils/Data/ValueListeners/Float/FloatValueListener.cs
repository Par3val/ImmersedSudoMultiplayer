using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XRTools.Utils.Data
{
	public class FloatValueListener : ValueListener<FloatRangeListener, float, FloatValueListener.UnityEvent>
	{
		[System.Serializable]
		public class UnityEvent : UnityEvent<float>
		{
		}


	}

#if UNITY_EDITOR

	[CustomEditor(typeof(FloatValueListener), true)]
	public class FloatValueListenerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			//base.OnInspectorGUI();
			EditorUtils.DrawValueListnerEditor(serializedObject);

			serializedObject.ApplyModifiedProperties();
		}
	}
#endif
}