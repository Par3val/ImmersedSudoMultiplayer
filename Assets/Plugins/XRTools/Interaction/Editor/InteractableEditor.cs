using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XRTools.Interaction
{

	[CustomEditor(typeof(InteractableEditor))]
	public class InteractableEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
		}
	}
}