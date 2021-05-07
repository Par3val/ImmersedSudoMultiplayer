using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace XRTools.Input
{

	public class ButtonMap : InputMappingNew
	{
		[System.Serializable]
		public enum Buttons { Primary, Secondary, /*Grip, Trigger*/ }
		public Buttons buttonMap = Buttons.Primary;
		bool prevValue;
		bool value;

		InputFeatureUsage<bool> inputFeature;

		public UnityEvent Activated;
		public UnityEvent Deactivated;
		public UnityEventButton StateChanged = new UnityEventButton();


		public override void UpdateValue()
		{
			handDevice.TryGetFeatureValue(inputFeature, out value);

			if (value != prevValue)
			{
				StateChanged.Invoke(value);
				if (value)
					Activated?.Invoke();
				else
					Deactivated?.Invoke();
				prevValue = value;
			}
		}		

		public override void SetInputFeature(InputFeatureUsage usage) => inputFeature = usage.As<bool>();

		public override string GetInputFeature()
		{
			switch (buttonMap)
			{
				case Buttons.Primary:
					return "PrimaryButton";
				case Buttons.Secondary:
					return "SecondaryButton";
			}
			return base.GetInputFeature();
		}

	}
#if UNITY_EDITOR
	[CustomEditor(typeof(ButtonMap))]
	public class ButtonMapEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			SerializedProperty mapName = serializedObject.FindProperty("mapName");
			SerializedProperty buttonMap = serializedObject.FindProperty("buttonMap");
			SerializedProperty value = serializedObject.FindProperty("value"); 
			SerializedProperty activeArea = serializedObject.FindProperty("activeArea");

			SerializedProperty Activated = serializedObject.FindProperty("Activated");
			SerializedProperty DeActivated = serializedObject.FindProperty("Deactivated");
			SerializedProperty StateChanged = serializedObject.FindProperty("StateChanged");


			mapName.stringValue = EditorGUILayout.TextField(mapName.stringValue);

			EditorGUILayout.BeginHorizontal();
			InputMappingNew.InputType typeSwitch = (InputMappingNew.InputType)EditorGUILayout.EnumPopup(InputMappingNew.InputType.Button);
			buttonMap.enumValueIndex =
				(int)(FloatMap.FloatInput)EditorGUILayout.EnumPopup((ButtonMap.Buttons)buttonMap.enumValueIndex);
			EditorGUILayout.EndHorizontal();
			
			
			bool splitActivation = Screen.width > 480;

			Utils.EditorUtils.SplitPropertyView(Activated, DeActivated, splitActivation);
			
			EditorGUILayout.PropertyField(StateChanged);


			serializedObject.ApplyModifiedProperties();

			if (typeSwitch != InputMappingNew.InputType.Button)
			{
				InputMappingNew.SwitchType((ButtonMap)target, typeSwitch);
			}
		}
	}

#endif
}