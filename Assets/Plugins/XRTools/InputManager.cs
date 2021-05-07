using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using XRTools.Utils;
//using System.Linq;
namespace XRTools
{

	[ExecuteInEditMode, System.Serializable]
	public class InputManager : MonoBehaviour
	{

		[System.Serializable]
		public enum hands { Right, Left };
		public hands handedness;

		//public AxesToVector3Facade LocomotionAxisCalculator;

		public InputMapping[] inputMappings;

		public InputMapping grabMap;
		public GameObject rig;//PlayerRigScript

		public bool eventsOpen;

		public void OnEnable()
		{
			string parName = transform.parent.name.Substring(0, 1);

			if (parName == "L")
				handedness = hands.Left;
			else if (parName == "R")
				handedness = hands.Right;
			else
				Debug.Log("Parent Hand Unsure");

			if (!grabMap)
			{
				foreach (var inputMap in inputMappings)
				{
					if (inputMap.mapName.Contains("Grab"))
					{
						grabMap = inputMap;
						break;
					}
				}
			}
		}
		private void Start()
		{
			RefreshInputMappings();

			if (Application.isPlaying)
			{

			}
		}

		public void RefreshInputMappings()
		{
			inputMappings = GetComponentsInChildren<InputMapping>();

			if (inputMappings == null)
				inputMappings = new InputMapping[0];
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(InputManager))]
	public class InputManagerEditor : Editor
	{
		InputManager manager;

		static GUILayoutOption miniButtonWidth = GUILayout.Width(25f);
		static GUILayoutOption miniFeildWidth = GUILayout.Width(50f);

		bool hasGrab;

		public override void OnInspectorGUI()
		{
			manager = (InputManager)target;

			//Input Mappings

			GUILayout.Label($"{manager.handedness} InputManager");
			GUILayout.Label($"Grab Map: {(manager.grabMap ? manager.grabMap.mapName : "NULL")}");
			if (manager.inputMappings == null)
				manager.RefreshInputMappings();

			hasGrab = false;

			if (manager.inputMappings.Length > 0)
			{
				ShowInputMappings();
			}

			if (!hasGrab)
				if (GUILayout.Button("Add Grab"))
					AddGrab();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(30);

			if (GUILayout.Button("Add InputMap"))
				AddMap("New Map");

			GUILayout.Space(30);
			EditorGUILayout.EndHorizontal();

			manager.eventsOpen = EditorGUILayout.Foldout(manager.eventsOpen, "Successfull Grab Events");

			if (manager.eventsOpen)
			{
				Transform grabEvent = manager.transform.GetChild(1).GetChild(1).GetChild(1).GetChild(0);
				//ShowValueEvent(grabEvent.GetComponent<BooleanAction>());

			}
			if (Selection.activeGameObject == manager.gameObject && manager.rig)
				if (GUILayout.Button($"GOTO: {manager.rig.name}"))
					EditorUtils.FocusObject(manager.rig.gameObject);

			GUILayout.Space(10);
		}
		void ShowInputMappings()
		{
			GUIStyle windowSkin = new GUIStyle(GUI.skin.window);
			windowSkin.alignment = TextAnchor.UpperLeft;
			windowSkin.padding = new RectOffset(15, 0, 0, 0);

			for (int i = 0; i < manager.inputMappings.Length; i++)
			{
				if (manager.inputMappings[i] && Event.current.type != EventType.ValidateCommand)
				{
					EditorGUILayout.BeginVertical(windowSkin);
					EditorGUILayout.BeginHorizontal();

					manager.inputMappings[i].editorOpen = EditorGUILayout.Foldout(manager.inputMappings[i].editorOpen, manager.inputMappings[i].mapName, true);


					if (ShowRemoveButton(manager.inputMappings[i]))
						return;

					GUILayout.Space(5);
					EditorGUILayout.EndHorizontal();


					if (manager.inputMappings[i].editorOpen)
					{
						var mapObject = new SerializedObject(manager.inputMappings[i]);
						ShowMap(ref manager.inputMappings[i], mapObject);

						mapObject.ApplyModifiedProperties();
					}


					if (!string.IsNullOrEmpty(manager.inputMappings[i].mapName))
						if (manager.inputMappings[i].mapName.Contains("Grab"))
							hasGrab = true;
					EditorGUILayout.EndVertical();
				}
				else
				{
					manager.RefreshInputMappings();
				}
			}
		}


		public void ShowMap(ref InputMapping map, SerializedObject property)
		{
			var mapName = property.FindProperty("mapName");
			var axisType = property.FindProperty("axisType");

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField("Name: ", GUILayout.Width(37));

			mapName.stringValue = EditorGUILayout.TextField(mapName.stringValue, EditorStyles.label);

			EditorGUILayout.EndHorizontal();



			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Input Type: ", GUILayout.Width(70));
			axisType.enumValueIndex = (int)((InputMapping.AxisType)EditorGUILayout.EnumPopup((InputMapping.AxisType)axisType.enumValueIndex));

			switch ((InputMapping.AxisType)axisType.enumValueIndex)
			{
				case InputMapping.AxisType.Button:
					var typeButton = property.FindProperty("typeButton");
					typeButton.enumValueIndex = (int)(InputMapping.InputTypeButton)EditorGUILayout.EnumPopup(/*"Which Button: ", */(InputMapping.InputTypeButton)typeButton.enumValueIndex);
					EditorGUILayout.EndHorizontal();
					break;

				case InputMapping.AxisType.Axis1D:
					var type1D = property.FindProperty("type1D");
					type1D.enumValueIndex = (int)(InputMapping.InputTypeAxis1D)EditorGUILayout.EnumPopup(/*"Which Axis1D: ", */(InputMapping.InputTypeAxis1D)type1D.enumValueIndex);
					EditorGUILayout.EndHorizontal();

					var range = property.FindProperty("activationRange");
					float tempMin = range.vector2Value.x;
					float tempMax = range.vector2Value.y;

					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Activation Range");
					tempMin = EditorGUILayout.FloatField(tempMin, miniFeildWidth);

					EditorGUILayout.MinMaxSlider(ref tempMin, ref tempMax, 0, 1);

					tempMax = EditorGUILayout.FloatField(tempMax, miniFeildWidth);

					range.vector2Value = new Vector2(tempMin, tempMax);
					EditorGUILayout.EndHorizontal();
					break;

				case InputMapping.AxisType.Axis2D:
					var type2d = property.FindProperty("type2D");
					type2d.enumValueIndex = (int)(InputMapping.InputTypeAxis2D)EditorGUILayout.EnumPopup(/*"Which Axis2D: ", */(InputMapping.InputTypeAxis2D)type2d.enumValueIndex);
					EditorGUILayout.EndHorizontal();
					break;

				default:
					EditorGUILayout.EndHorizontal();
					break;
			}

			EditorGUI.indentLevel++;
			switch ((InputMapping.AxisType)axisType.enumValueIndex)
			{
				case InputMapping.AxisType.Button:
					EditorGUILayout.PropertyField(property.FindProperty("UpdateBool"));
					EditorGUILayout.PropertyField(property.FindProperty("Activated"));
					EditorGUILayout.PropertyField(property.FindProperty("Deactivated"));
					break;
				case InputMapping.AxisType.Axis1D:
					EditorGUILayout.PropertyField(property.FindProperty("Update1D"));
					EditorGUILayout.PropertyField(property.FindProperty("Activated"));
					EditorGUILayout.PropertyField(property.FindProperty("Deactivated"));
					break;
				case InputMapping.AxisType.Axis2D:
					EditorGUILayout.PropertyField(property.FindProperty("Moved2D"));
					EditorGUILayout.PropertyField(property.FindProperty("Moved2DX"));
					EditorGUILayout.PropertyField(property.FindProperty("Moved2DY"));
					break;
			}
			EditorGUI.indentLevel--;
		}
		//private void ShowValueEvent(BooleanAction booleanAction)
		//{
		//	var serializedAction = new SerializedObject(booleanAction);

		//	var activated = serializedAction.FindProperty("Activated");
		//	var valueChanged = serializedAction.FindProperty("ValueChanged");
		//	var deactivated = serializedAction.FindProperty("Deactivated");
		//	EditorGUILayout.PropertyField(activated);
		//	EditorGUILayout.PropertyField(valueChanged);
		//	EditorGUILayout.PropertyField(deactivated);

		//	serializedAction.ApplyModifiedProperties();
		//}



		public InputMapping AddMap(string newName)
		{
			return AddMap(ref manager, newName);
		}
		public static InputMapping AddMap(ref InputManager _manager, string newName)
		{
			var parent = _manager.transform.Find("InputMappings");
			if (!parent)
			{
				new GameObject("InputMappings").transform.parent = _manager.transform;
				return AddMap(ref _manager, newName);
			}
			InputMapping tempMapping = (InputMapping)Undo.AddComponent(parent.gameObject, typeof(InputMapping));
			Undo.SetCurrentGroupName("Added Input Map" + newName);
			tempMapping.mapName = newName;
			Undo.RecordObject(_manager, "Added Input Map" + newName);
			_manager.RefreshInputMappings();
			Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

			return tempMapping;
		}
		public void RemoveMap(InputMapping relMap)
		{

			Undo.DestroyObjectImmediate(relMap);
			Undo.SetCurrentGroupName("Removed Input Map");
			Undo.RecordObject(manager, "Removed Input Map");
			manager.RefreshInputMappings();
			return;

			////var tempList = manager.inputMappings.ToList();
			////tempList.Remove(relMap);
			////manager.inputMappings = tempList.ToArray();
			//InputMapping[] tempMappings = new InputMapping[manager.inputMappings.Length - 1];

			//int removeIndex = 0;

			//for (int i = 0, o = 0; i < manager.inputMappings.Length; i++)
			//{
			//	if (manager.inputMappings[i] == relMap)
			//	{
			//		o = 1;
			//		removeIndex = i;
			//	}
			//	else
			//	{
			//		tempMappings[i - o] = manager.inputMappings[i];
			//	}
			//}
			//if (manager.inputMappings[removeIndex])
			//	DestroyImmediate(manager.inputMappings[removeIndex].gameObject);
			//manager.inputMappings = new InputMapping[tempMappings.Length];
			//manager.inputMappings = tempMappings;
		}
		public void AddGrab()
		{
			manager.grabMap = AddMap("Grab");
			manager.grabMap.axisType = InputMapping.AxisType.Axis1D;
			manager.grabMap.type1D = InputMapping.InputTypeAxis1D.Hand;
		}

		bool ShowRemoveButton(InputMapping mapping)
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

}//public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source, int count)
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