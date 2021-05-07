using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.XR;
using XRTools.Utils;
using XRTools.Interaction;
//using System.Linq;
namespace XRTools.Input
{
	public class InputManagerNew : MonoBehaviour
	{
		public XRNode inputType = XRNode.LeftHand;
		[System.Serializable]
		public enum hands { Right, Left };
		public hands handedness;

		//public AxesToVector3Facade LocomotionAxisCalculator;

		public InputMappingNew[] inputMappings;
		public GameObject rig;//PlayerRigScript

		public InteractorRaw interactor;
		public InputMappingNew grabMap;
		public bool eventsOpen;

		public void OnEnable()
		{
			string parName = transform.name.Substring(0, 1);

			if (parName == "L")
				handedness = hands.Left;
			else if (parName == "R")
				handedness = hands.Right;
			else
				Debug.Log("Parent Hand Unsure");

			if (!grabMap && inputMappings != null && inputMappings.Length > 0)
			{
				foreach (var inputMap in inputMappings)
				{
					if (inputMap.name.Contains("Grab"))
					{
						grabMap = inputMap;
						break;
					}
				}
			}

			if (interactor != null && grabMap != null)
			{
				grabMap.TryGetComponent(out FloatMap floatMap);
				grabMap.TryGetComponent(out ButtonMap buttonMap);

				if (floatMap != null)
				{
					floatMap.Activated.AddListener(interactor.NotifyTryGrabbed);
					floatMap.Deactivated.AddListener(interactor.NotifyTryUnGrabbed);
				}
				else if (buttonMap != null)
				{
					buttonMap.Activated.AddListener(interactor.NotifyTryGrabbed);
					buttonMap.Deactivated.AddListener(interactor.NotifyTryUnGrabbed);
				}
			}
		}
		private void Start()
		{
			RefreshInputMappings();
		}

		public void RefreshInputMappings()
		{
			inputMappings = GetComponentsInChildren<InputMappingNew>();

			if (handedness == hands.Left)
				inputType = XRNode.LeftHand;
			else
				inputType = XRNode.RightHand;

			foreach (var map in inputMappings)
			{
				if (map.hand != inputType)
					map.hand = inputType;
			}
			//if (inputMappings == null)
			//	inputMappings = new InputMappingNew[0];
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(InputManagerNew))]
	public class InputManagerNewEditor : Editor
	{
		InputManagerNew manager;

		static GUILayoutOption miniButtonWidth = GUILayout.Width(25f);
		static GUILayoutOption miniFeildWidth = GUILayout.Width(50f);

		bool hasGrab;

		private void OnEnable()
		{
			manager = (InputManagerNew)target;
			manager.RefreshInputMappings();

			SerializedProperty interactor = serializedObject.FindProperty("interactor");
			SerializedProperty grabMap = serializedObject.FindProperty("grabMap");

			if (interactor.objectReferenceValue == null)
			{
				InteractorRaw _interactor = null;

				_interactor = manager.GetComponent<InteractorRaw>();

				if (_interactor == null)
					_interactor = manager.GetComponentInParent<InteractorRaw>();


				interactor.objectReferenceValue = _interactor;
			}

			if (grabMap.objectReferenceValue == null)
				foreach (var map in manager.inputMappings)
				{
					Debug.Log(map.mapName.ToLower().Contains("grab"));
					if (map.mapName.ToLower().Contains("grab"))
						grabMap.objectReferenceValue = map;
				}

			serializedObject.ApplyModifiedProperties();
		}

		public override void OnInspectorGUI()
		{
			//base.OnInspectorGUI();


			//manager = (InputManager)target;
			SerializedProperty handedness = serializedObject.FindProperty("handedness");
			SerializedProperty grabMap = serializedObject.FindProperty("grabMap");
			SerializedProperty inputMappingsProperty = serializedObject.FindProperty("inputMappings");

			if (grabMap.objectReferenceValue == null)
				foreach (var map in manager.inputMappings)
				{
					Debug.Log(map.mapName.ToLower().Contains("grab map set"));
					if (map.mapName.ToLower().Contains("grab"))
						grabMap.objectReferenceValue = map;
				}

			manager = (InputManagerNew)target;
			manager.RefreshInputMappings();

			GUILayout.Label($"{(InputManagerNew.hands)handedness.intValue} InputManager Maps: " +
				$"{inputMappingsProperty.arraySize}, Interactor: {manager.interactor != null} " +
				$"(grabMap({manager.grabMap != null}))");
			//GUILayout.Label($"Grab Map: {(manager.grabMap ? manager.grabMap.mapName : "NULL")}");
			//System.Collections.IEnumerator inputMappings 
			ShowInputMapping(inputMappingsProperty);

			if (!manager.grabMap)
				if (GUILayout.Button("Add Grab"))
					AddGrab();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(30);

			if (GUILayout.Button("Add Button Map"))
				AddMap(InputMappingNew.InputType.Button, "new Button");
			if (GUILayout.Button("Add Float Map"))
				AddMap(InputMappingNew.InputType.Trigger_Grab, "new Float");
			if (GUILayout.Button("Add Vector2 Map"))
				AddMap(InputMappingNew.InputType.Joystick_Trackpad, "new Vector2");

			GUILayout.Space(30);
			EditorGUILayout.EndHorizontal();


			//if (Selection.activeGameObject == manager.gameObject && manager.rig)
			//	if (GUILayout.Button($"GOTO: {manager.rig.name}"))
			//		EditorUtils.FocusObject(manager.rig.gameObject);

			GUILayout.Space(10);
		}

		public InputMappingNew AddMap(InputMappingNew.InputType mappingType, string newName = "")
		{
			var parent = manager.transform.Find("InputMappings");
			if (!parent)
			{
				new GameObject("InputMappings").transform.parent = manager.transform;
				return AddMap(mappingType, newName);
			}
			InputMappingNew tempMapping = null;
			switch (mappingType)
			{
				case InputMappingNew.InputType.Button:
					tempMapping = (InputMappingNew)Undo.AddComponent(parent.gameObject, typeof(ButtonMap));
					break;
				case InputMappingNew.InputType.Trigger_Grab:
					tempMapping = (InputMappingNew)Undo.AddComponent(parent.gameObject, typeof(FloatMap));
					break;
				case InputMappingNew.InputType.Joystick_Trackpad:
					tempMapping = (InputMappingNew)Undo.AddComponent(parent.gameObject, typeof(Vector2Map));
					break;
			}

			if (!newName.Equals(""))
				tempMapping.mapName = newName;

			manager.RefreshInputMappings();
			return tempMapping;
		}
		public void RemoveMap(InputMappingNew targetMap)
		{

			Undo.SetCurrentGroupName("Removed Input Map");
			Undo.DestroyObjectImmediate(targetMap);
			Undo.RecordObject(manager, "Removed Input Map");
			manager.RefreshInputMappings();
			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
			return;


		}

		public void AddGrab()
		{
			manager.grabMap = AddMap(InputMappingNew.InputType.Trigger_Grab, "Grab");
			Debug.Log("Todo Set Grab type");
			//manager.grabMap.axisType = InputMapping.AxisType.Axis1D;
			//manager.grabMap.type1D = InputMapping.InputTypeAxis1D.Hand;
		}



		void ShowInputMapping(SerializedProperty mappingsList)
		{
			int inputMapSize = mappingsList.arraySize;

			GUIStyle windowSkin = new GUIStyle(GUI.skin.window);
			windowSkin.alignment = TextAnchor.UpperLeft;
			windowSkin.padding = new RectOffset(20, 0, 0, 0);

			for (int i = 0; i < inputMapSize; i++)
			{
				SerializedProperty map = mappingsList.GetArrayElementAtIndex(i);
				if (map == null || map.objectReferenceValue == null)
					continue;
				var mapOBject = (InputMappingNew)map.objectReferenceValue;

				EditorGUILayout.BeginVertical(windowSkin);
				EditorGUILayout.BeginHorizontal();


				mapOBject.editorOpen = EditorGUILayout.BeginFoldoutHeaderGroup(mapOBject.editorOpen, new GUIContent($"{mapOBject.mapName}"));

				if (ShowRemoveButton(mapOBject))
					return;

				GUILayout.Space(5);
				EditorGUILayout.EndHorizontal();

				if (mapOBject.editorOpen)
				{
					mapOBject.editorOpen = true;
					var mapObject = CreateEditor(map.objectReferenceValue);
					mapObject.OnInspectorGUI();
					EditorUtility.SetDirty(mapObject);
				}
				else
					mapOBject.editorOpen = false;
				EditorGUILayout.EndFoldoutHeaderGroup();

				EditorGUILayout.EndVertical();
			}
		}

		bool ShowRemoveButton(InputMappingNew mapping)
		{
			if (GUILayout.Button("-", EditorStyles.miniButtonLeft, miniButtonWidth))
			{
				RemoveMap(mapping);
				return true;
			}

			return false;
		}
	}
#endif

}
//public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source, int count)
//{
//	if (source == null) throw new System.ArgumentNullException("source");
//	if (count < 0) throw new System.ArgumentOutOfRangeException("count");
//	var array = new TSource[count];
//	int i = 0;
//	foreach (var item in source)
//	{
//		array[i++] = item;
//	}
//	return array;
//}