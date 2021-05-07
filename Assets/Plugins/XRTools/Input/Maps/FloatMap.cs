using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using XRTools.Utils.Data;

namespace XRTools.Input
{

	public class FloatMap : InputMappingNew
	{
		[System.Serializable]
		public enum FloatInput { Trigger, Grip }
		public FloatInput floatMap = FloatInput.Grip;
		public Vector2 activeRange = new Vector2(0.8f, 1);

		float prevValue;
		float value;
		bool activeState;
		InputFeatureUsage<float> inputFeature;


		public UnityEvent Activated;
		public UnityEvent Deactivated;
		public UnityEventFloat StateChanged = new UnityEventFloat();
		
		public override void UpdateValue()
		{
			if(inputFeature != null)
			handDevice.TryGetFeatureValue(inputFeature, out value);

			if (!value.Equals(prevValue))
			{
				StateChanged.Invoke(value);
				prevValue = value;

				if (activeState != CheckActive(value))
				{
					if (CheckActive(value))
						Activated?.Invoke();
					else
						Deactivated?.Invoke();
					activeState = CheckActive(value);
				}
			}
		}

		public override void RefreshFeature()
		{
			inputFeature = new InputFeatureUsage<float>();
			base.RefreshFeature();
		}

		bool CheckActive(float _value)
			 => (_value >= activeRange.x && _value <= activeRange.y);

		public override void SetInputFeature(InputFeatureUsage usage) => inputFeature = usage.As<float>();


		public override string GetInputFeature()
		{
			switch (floatMap)
			{
				case FloatInput.Trigger:
					return "Trigger";
				case FloatInput.Grip:
					return "Grip";
			}
			return base.GetInputFeature();
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(FloatMap))]
	public class FloatMapEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			SerializedProperty mapName = serializedObject.FindProperty("mapName");
			SerializedProperty floatMap = serializedObject.FindProperty("floatMap");
			SerializedProperty activeRange = serializedObject.FindProperty("activeRange");

			SerializedProperty Activated = serializedObject.FindProperty("Activated");
			SerializedProperty Deactivated = serializedObject.FindProperty("Deactivated");
			SerializedProperty StateChanged = serializedObject.FindProperty("StateChanged");


			mapName.stringValue = EditorGUILayout.TextField(mapName.stringValue);

			EditorGUILayout.BeginHorizontal();
			InputMappingNew.InputType typeSwitch = (InputMappingNew.InputType)EditorGUILayout.EnumPopup(InputMappingNew.InputType.Trigger_Grab);

				floatMap.enumValueIndex = 
					(int)(FloatMap.FloatInput)EditorGUILayout.EnumPopup((FloatMap.FloatInput)floatMap.enumValueIndex);
			EditorGUILayout.EndHorizontal();

			var tempRange = new FloatRange(activeRange.vector2Value);
			Utils.EditorUtils.ShowRangeArea(ref tempRange);
			activeRange.vector2Value = tempRange.ToVector2();
			bool splitActivation = Screen.width > 480;
			
			Utils.EditorUtils.SplitPropertyView(Activated, Deactivated, splitActivation);
			
			EditorGUILayout.PropertyField(StateChanged);


			serializedObject.ApplyModifiedProperties();

			if(typeSwitch != InputMappingNew.InputType.Trigger_Grab)
			{
				InputMappingNew.SwitchType((FloatMap)target, typeSwitch);
			}
		}


	}

#endif
}