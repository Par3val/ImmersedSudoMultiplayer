using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace XRTools.Input
{
	public class Vector2Map : InputMappingNew
	{
		public enum Axis2D { Primary, Secondary }
		public Axis2D vector2Map = Axis2D.Primary;

		Vector2 prevValue;
		Vector2 value;
		bool touched;
		bool clicked;
		InputFeatureUsage<Vector2> inputFeature;

		public InputFeatureUsage<bool> touch;
		public InputFeatureUsage<bool> click;


		public UnityEvent Touched;
		public UnityEvent UnTouched;

		public UnityEvent Clicked;
		public UnityEvent UnClicked;

		public UnityEventFloat MovedX = new UnityEventFloat();
		public UnityEventFloat MovedY = new UnityEventFloat();

		public UnityEventVector2 StateChanged = new UnityEventVector2();

		public override void UpdateValue()
		{
			handDevice.TryGetFeatureValue(inputFeature, out value);

			if (value != prevValue)
			{
				StateChanged.Invoke(value);

				if (value.x != prevValue.x)
					MovedX?.Invoke(value.x);
				else if (value.y != prevValue.y)
					MovedY?.Invoke(value.y);

				prevValue = value;
			}

			handDevice.TryGetFeatureValue(touch, out bool _touched);
			handDevice.TryGetFeatureValue(click, out bool _clicked);

			if (_touched != touched)
			{
				if (_touched)
					Touched.Invoke();
				else
					UnTouched.Invoke();

				touched = _touched;
			}

			if (_clicked != clicked)
			{
				if (_clicked)
					Clicked.Invoke();
				else
					UnClicked.Invoke();

				clicked = _clicked;
			}
		}

		public override void SetInputFeature(InputFeatureUsage usage)
		{
			inputFeature = usage.As<Vector2>();
			touch = new InputFeatureUsage<bool>(usage.name + "Touch");
			click = new InputFeatureUsage<bool>(usage.name + "Click");
		}

		public override string GetInputFeature()
		{
			switch (vector2Map)
			{
				case Axis2D.Primary:
					return "Primary2DAxis";
				case Axis2D.Secondary:
					return "Secondary2DAxis";
			}
			return base.GetInputFeature();
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(Vector2Map))]
	public class Vector2MapEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			SerializedProperty mapName = serializedObject.FindProperty("mapName");
			SerializedProperty vector2Map = serializedObject.FindProperty("vector2Map");
			SerializedProperty activeRange = serializedObject.FindProperty("activeRange");

			SerializedProperty MovedX = serializedObject.FindProperty("MovedX");
			SerializedProperty MovedY = serializedObject.FindProperty("MovedY");
			SerializedProperty StateChanged = serializedObject.FindProperty("StateChanged");

			SerializedProperty Touched = serializedObject.FindProperty("Touched");
			SerializedProperty UnTouched = serializedObject.FindProperty("UnTouched");

			SerializedProperty Clicked = serializedObject.FindProperty("Clicked");
			SerializedProperty UnClicked = serializedObject.FindProperty("UnClicked");

			mapName.stringValue = EditorGUILayout.TextField(mapName.stringValue);

			EditorGUILayout.BeginHorizontal();
			InputMappingNew.InputType typeSwitch = (InputMappingNew.InputType)EditorGUILayout.EnumPopup(InputMappingNew.InputType.Joystick_Trackpad);
			vector2Map.enumValueIndex =
				(int)(FloatMap.FloatInput)EditorGUILayout.EnumPopup((Vector2Map.Axis2D)vector2Map.enumValueIndex);
			EditorGUILayout.EndHorizontal();


			bool splitActivation = Screen.width > 480;

			Utils.EditorUtils.SplitPropertyView(MovedX, MovedY, splitActivation);


			EditorGUILayout.PropertyField(StateChanged);

			EditorGUILayout.Space();

			Utils.EditorUtils.SplitPropertyView(Touched, UnTouched, splitActivation);
			Utils.EditorUtils.SplitPropertyView(Clicked, UnClicked, splitActivation);

			serializedObject.ApplyModifiedProperties();

			if (typeSwitch != InputMappingNew.InputType.Joystick_Trackpad)
			{
				InputMappingNew.SwitchType((Vector2Map)target, typeSwitch);
			}
		}
	}

#endif
}