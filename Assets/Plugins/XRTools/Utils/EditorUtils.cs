using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System;

#if UNITY_EDITOR

namespace XRTools.Utils
{

	public class EditorUtils
	{
		static EditorUtils()
		{
			window = new GUIStyle(GUI.skin.window);
			window.alignment = TextAnchor.UpperLeft;
			window.padding = new RectOffset(20, 0, 0, 0);
		}

		public static void ShowRefrenceButton(GameObject _refrence)
		{
			if (GUILayout.Button("ref", EditorStyles.miniButtonLeft, GUILayout.Width(25f)))
			{
				Selection.activeObject = _refrence;
				SceneView.FrameLastActiveSceneView();
			}
		}

		public static void FocusObject(GameObject _refrence, bool updateSceneAngle = false)
		{
			Selection.activeObject = _refrence;
			if (updateSceneAngle)
				SceneView.FrameLastActiveSceneView();
		}

		public static void BeginHorizontal()
		{
			EditorGUILayout.BeginHorizontal();
		}

		public static void EndHorizontal()
		{
			EditorGUILayout.EndHorizontal();
		}

		public static void ShowRangeArea(ref Vector2 range, bool ajustableRange = false)
		{
			EditorGUILayout.BeginHorizontal();

			if (ajustableRange)
				range.x = EditorGUILayout.FloatField(range.x, EditorStyles.numberField, GUILayout.Width(50f));
			else
				GUILayout.Label(range.x.ToString(), GUILayout.Width(30f));

			EditorGUILayout.MinMaxSlider(ref range.x, ref range.y, 0, 1);

			if (ajustableRange)
				range.y = EditorGUILayout.FloatField(range.y, EditorStyles.numberField, GUILayout.Width(50f));
			else
				GUILayout.Label(range.y.ToString(), GUILayout.Width(30f));


			if (range.y > 1)
				range.y = 1;

			EditorGUILayout.EndHorizontal();
		}

		public static void ShowRangeArea(ref Data.FloatRange range, bool ajustableRange = false)
		{
			EditorGUILayout.BeginHorizontal();

			if (ajustableRange)
				range.min = EditorGUILayout.FloatField(range.min, EditorStyles.numberField, GUILayout.Width(50f));
			else
				GUILayout.Label(range.min.ToString(), GUILayout.Width(30f));

			EditorGUILayout.MinMaxSlider(ref range.min, ref range.max, 0, 1);

			if (ajustableRange)
				range.max = EditorGUILayout.FloatField(range.max, EditorStyles.numberField, GUILayout.Width(50f));
			else
				GUILayout.Label(range.max.ToString(), GUILayout.Width(30f));


			if (range.max > 1)
				range.max = 1;

			EditorGUILayout.EndHorizontal();
		}

		public static void SplitPropertyView(SerializedProperty prop1, SerializedProperty prop2, bool split)
		{
			Rect _cur = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 0);
			if (split)
				EditorGUILayout.BeginHorizontal();

			float width = split ? (EditorGUIUtility.currentViewWidth - 12 - _cur.x - 62) / 2 : (EditorGUIUtility.currentViewWidth - _cur.x - 59);

			EditorGUILayout.PropertyField(prop1, GUILayout.Width(width));

			GUILayout.Space(split ? 12 : 0);

			EditorGUILayout.PropertyField(prop2, GUILayout.Width(width));

			if (split)
				GUILayout.Space(12);

			if (split)
				EditorGUILayout.EndHorizontal();
		}

		public static void SplitPropertyView(SerializedProperty prop1, SerializedProperty prop2, float minSplitWidth)
		{
			Rect _cur = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 0);
			bool split = Screen.width > minSplitWidth;
			if (split)
				EditorGUILayout.BeginHorizontal();

			float width = split ? (EditorGUIUtility.currentViewWidth - 12 - _cur.x -(minSplitWidth * .05f)) / 2 : (EditorGUIUtility.currentViewWidth - _cur.x - (minSplitWidth * .05f));

			EditorGUILayout.PropertyField(prop1, GUILayout.Width(width));

			GUILayout.Space(split ? 12 : 0);

			EditorGUILayout.PropertyField(prop2, GUILayout.Width(width));

			if (split)
				GUILayout.Space(12);

			if (split)
				EditorGUILayout.EndHorizontal();
		}

		public static GUIStyle window
		{
			get;
			protected set;
		}

		public static void CopyEventData(UnityEvent source, UnityEvent target)
		{
			int listeners = source.GetPersistentEventCount();

			for (int i = 0; i < listeners; i++)
			{
				System.Reflection.MethodInfo info = UnityEventBase.GetValidMethodInfo(source.GetPersistentTarget(i), source.GetPersistentMethodName(i), new Type[] { typeof(float) });
				UnityAction execute = () => info.Invoke(source.GetPersistentTarget(i), new object[] { 180f });
				target.AddListener(execute);
			}
		}


		public static Vector3 GetSlderConstraint(Vector2 mousePos, Vector3 constraintAxis, Vector3 startPos)
		{
			Vector3 mouseCamForward = HandleUtility.GUIPointToWorldRay(mousePos).direction;
			Vector3 OnVectorMousePos = GetIntersectPoint(
				mouseCamForward,
				Camera.current.transform.position,//HandleUtility.GUIPointToWorldRay(Vector2.zero).origin
				constraintAxis,
				startPos);

			return OnVectorMousePos;
		}

		static Vector3 GetIntersectPoint(Vector3 camToMouseDir, Vector3 camPos, Vector3 destDir, Vector3 destPos)
		{
			Vector3 normal = Vector3.Cross(camToMouseDir, destDir);
			Vector3 uish = Vector3.Cross(normal, camPos - destPos) / Vector3.Dot(normal, normal);

			return destPos - destDir * Vector3.Dot(camToMouseDir, uish);
		}

		public static void HandlesLabels(Vector3[] poses, string[] values)
		{
			for (int i = 0; i < poses.Length; i++)
			{
				if (i < values.Length)
					Handles.Label(poses[i], values[i]);
				else
					Handles.Label(poses[i], "NO\nVALUE");

			}
		}

		public static void DrawValueListnerEditor(SerializedObject listener)
		{
			SerializedProperty Activated = listener.FindProperty("Activated");
			SerializedProperty Deactivated = listener.FindProperty("Deactivated");

			SerializedProperty ValueChanged = listener.FindProperty("ValueChanged");
			SerializedProperty ValueUnchanged = listener.FindProperty("ValueUnchanged");

			bool splitActivation = Screen.width > 480;

			SplitPropertyView(Activated, Deactivated, splitActivation);
			SplitPropertyView(ValueChanged, ValueUnchanged, splitActivation);

			listener.ApplyModifiedProperties();
		}
	}

}
#endif