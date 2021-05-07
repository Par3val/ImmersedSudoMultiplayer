using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XRTools.Utils.Data
{
	public class Vector3ValueListener : ValueListener<Vector3ValueListener, Vector3, Vector3ValueListener.UnityEvent>
	{
		[System.Serializable]
		public class UnityEvent : UnityEvent<Vector3>
		{
		}
	}


#if UNITY_EDITOR
	
	[CustomEditor(typeof(Vector3ValueListener))]
	public class Vector3ValueListenerEditor : Editor
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