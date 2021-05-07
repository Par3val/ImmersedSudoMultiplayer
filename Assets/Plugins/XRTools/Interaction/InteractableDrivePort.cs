using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRTools.Actions;
using XRTools.Handle;
using XRTools.Interaction.Actions;
using XRTools.Utils;
using XRTools.Utils.Data;
using static XRTools.Interaction.InteractableRaw;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XRTools.Interaction
{
	[SelectionBase]
	public class InteractableDrivePort : DrivePort
	{
		public InteractableRaw interactable;
		int interactableChildIndex = -1;
		Vector3 previousPos;

		public Vector3ValueListener.UnityEvent normalizeMoved = new Vector3ValueListener.UnityEvent();
		public FloatValueListener.UnityEvent MovedX;
		public FloatValueListener.UnityEvent MovedY;
		public FloatValueListener.UnityEvent MovedZ;


		public List<AxisValueListener> axisValueEvents = new List<AxisValueListener>();
		public List<Vector3ValueListener> vec3ValueEvents = new List<Vector3ValueListener>();

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if (!interactable)
				return;

			if (interactableChildIndex == -1)
			{
				InteractableRaw tempInteractable = interactable;
				interactable = null;
				SetInteractable(tempInteractable);
				Debug.Log("WHAT");
				return;
			}

			if (!(interactable.PrimaryActionEvent == GetDrivePrimaryAction))
				interactable.PrimaryActionEvent = GetDrivePrimaryAction;

			EventCalls();
		}

		public bool SetInteractable(InteractableRaw _interactable)
		{
			if (interactable)
				return false;

			//adds to children list and if cant be added escapes
			if (!AddChild(_interactable.gameObject))
				return false;

			interactable = _interactable;
			interactable.PrimaryActionEvent = GetDrivePrimaryAction;
			interactableChildIndex = children.Length;//index when added to array

			previousPos = interactable.transform.position;

			return true;
		}

		public override bool AddChild(GameObject newChild)
		{
			if(!interactable)
			if (newChild.GetComponent<InteractableRaw>())
				{
					interactable = newChild.GetComponent<InteractableRaw>();
					interactable.PrimaryActionEvent = GetDrivePrimaryAction;
					interactableChildIndex = children.Length;//index when added to array

					previousPos = interactable.transform.position;
				}
				SetInteractable(newChild.GetComponent<InteractableRaw>());
			return base.AddChild(newChild);
		}

		public override bool FreeChild(int index)
		{

			if (interactableChildIndex == index)
			{
				interactable.ResetActions();
				interactable = null;
				interactableChildIndex = -1;
			}


			return base.FreeChild(index);

		}

		public override void OnEnable()
		{
			base.OnEnable();

			if (interactable && interactableChildIndex == -1)
			{
				InteractableRaw tempInteractable = interactable;
				interactable = null;
				SetInteractable(tempInteractable);
			}
		}

		public void EventCalls()
		{
			if (interactable)
				if (Vector3.Distance(previousPos, interactable.transform.position) >= float.Epsilon)
				{
					Vector3 localPos = transform.InverseTransformPoint(interactable.transform.position);
					localPos += range;
					localPos = Vector3Utils.Clamp01(localPos);

					normalizeMoved?.Invoke(localPos);
					MovedX?.Invoke(localPos.x);
					MovedY?.Invoke(localPos.y);
					MovedZ?.Invoke(localPos.z);

					if (axisValueEvents != null)
						foreach (AxisValueListener axisListener in axisValueEvents)
						{
							switch (axisListener.axis)
							{
								case AxisValueListener.SingleAxis.X:
									axisListener.Receive(localPos.x);
									break;
								case AxisValueListener.SingleAxis.Y:
									axisListener.Receive(localPos.y);
									break;
								case AxisValueListener.SingleAxis.Z:
									axisListener.Receive(localPos.z);
									break;
							}
						}

					if (vec3ValueEvents != null)
						foreach (Vector3ValueListener _event in vec3ValueEvents)
						{
							_event.Receive(localPos);
						}
				}
		}

	}

	[SelectionBase]
	public class DrivePort : MonoBehaviour
	{
		[SerializeField]
		public GameObject[] children;

		[System.Serializable]
		public enum DriveTypes { Directional, Rotational }

		public DriveTypes driveType;
		public Axis axis;
		//public bool global = true;
		public bool activeLimitChildren;

		Vector3 targetPos;
		public Vector3 m_targetIndex;
		public Vector3 targetIndex
		{
			get { return m_targetIndex; }

			set
			{
				value = Vector3Utils.Limit(value, 0, 1);
				//targetPos = Vector3Utils.Lerp(minPoses, maxPoses, value);

				m_targetIndex = value;
			}
		}

		public Vector3[] targetVelocities;

		//
		//
		//NEED ACCEPTABLE WAY TO NOT HAVE ALL OR NONE OF THE CHILDREN TO GO TO TARGETPOS
		//
		//
		//public bool[] targetMoveChildren;

		public Vector3 range;
		public float speed = 5;
		public bool moveToTarget = true;
		/// <summary>
		/// if Directional Lock Rotation, 
		/// if Rotational lock Positional
		/// </summary>
		[Tooltip("if Directional Lock Rotation \nif Rotational lock Position")]
		public bool lockInverse = true;
		public bool lockInactiveAxis = true;

		public virtual bool AddChild(GameObject newChild)
		{
			GameObject[] tempArray = (GameObject[])children.Clone();
			children = new GameObject[tempArray.Length + 1];
			targetVelocities = new Vector3[tempArray.Length + 1];

			for (int i = 0; i < children.Length; i++)
			{
				if (i == children.Length - 1)
				{
					children[i] = newChild;
					return true;
				}
				children[i] = tempArray[i];
			}

			return false;
		}

		public virtual bool FreeChild(int index)
		{
			if (index > children.Length - 1)
				return false;


			GameObject[] tempArray = (GameObject[])children.Clone();
			children = new GameObject[tempArray.Length - 1];
			targetVelocities = new Vector3[tempArray.Length - 1];

			for (int i = 0; i < children.Length; i++)
			{
				if (i == index)
				{
					continue;
				}

				children[i] = tempArray[i + (i > index ? 1 : 0)];
			}

			return false;
		}

		public bool FreeChild(GameObject targetChild)
		{
			for (int i = 0; i < children.Length; i++)
			{
				if (children[i].Equals(targetChild))
					return FreeChild(i);
			}

			return false;
		}

		public virtual void OnEnable()
		{
			if (targetVelocities.Length != children.Length)
				targetVelocities = new Vector3[children.Length];
		}

		public virtual void Start()
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				AddChild(transform.GetChild(i).gameObject);
			}

			Application.onBeforeRender += LimitChildrenPositon;
		}

		public virtual void FixedUpdate()
		{
			LimitChildrenVeloctiy();

			if (moveToTarget)
			{
				Vector3 targetPos = Vector3Utils.Lerp(-range, range, m_targetIndex);
				targetPos = transform.TransformPoint(targetPos);

				for (int i = 0; i < children.Length; i++)
				{
					children[i].transform.position = Vector3.SmoothDamp(children[i].transform.position, targetPos, ref targetVelocities[i], 1 / speed);

				}
			}
		}

		public virtual void LimitChildrenPositon()
		{
			foreach (var child in children)
			{
				Vector3 localPos = transform.InverseTransformPoint(child.transform.position);

				DriveUtils.LimitPos(ref localPos, axis, range);
				if (lockInactiveAxis)
					DriveUtils.LockFreeAxisPosition(ref localPos, axis);

				child.transform.position = transform.TransformPoint(localPos);
			}
		}


		public virtual void LimitChildrenVeloctiy()
		{
			foreach (var child in children)
			{
				Rigidbody childRb = child.GetComponent<Rigidbody>();
				if (childRb == null)
					continue;

				Vector3 velocityProject = childRb.velocity * Time.deltaTime + child.transform.position;
				Vector3 localPos = transform.InverseTransformPoint(velocityProject);

				if (DriveUtils.IsInBounds(localPos, axis, range))
					continue;

				DriveUtils.LimitPos(ref localPos, axis, range);
				if (lockInactiveAxis)
					DriveUtils.LockFreeAxisPosition(ref localPos, axis);

				childRb.velocity = transform.TransformPoint(localPos) - child.transform.position;
			}
		}

		public virtual BasicAction GetDrivePrimaryAction(InteractableRaw thisInteractable, InteractorRaw interactor)
		{
			//return base.GetPrimaryAction(interactor);
			Transform offset = thisInteractable.CreateOffset(interactor);

			//if (driveType == DriveTypes.Directional)
			//	switch (thisInteractable.followType)
			//	{
			//		case FollowType.Transform:
			//			return new DirectionalTransformDriveAction(this, thisInteractable, interactor, offset);
			//		case FollowType.Rigidbody:
			//			return new DirectionalRigidbodyDriveAction(this, thisInteractable, interactor, offset);
			//		case FollowType.Joint:
			//			return new DirectionalDriveJointAction(this, thisInteractable.mainRigidbody, interactor, offset);
			//	}
			//else if (driveType == DriveTypes.Rotational)
			//	switch (thisInteractable.followType)
			//	{
			//		case FollowType.Transform:
			//			return new RotationalTransformDriveAction(this, thisInteractable, interactor, offset);
			//		case FollowType.Rigidbody:
			//			break;
			//		case FollowType.Joint:
			//			break;
			//	}

			return null;
		}

	}


#if UNITY_EDITOR

	[CustomEditor(typeof(DrivePort), true), CanEditMultipleObjects]
	public class DrivePortEditor : Editor
	{
		DrivePort drive;
		RotationalDriveHandle rotHandle;
		DirectionalDriveBoundsHandle dirHandle;

		Bounds driveBounds;

		static GUILayoutOption miniButtonWidth = GUILayout.Width(25f);
		static GUILayoutOption miniFeildWidth = GUILayout.Width(50f);

		static GUIStyle windowSkin;

		private void OnEnable()
		{
			//rotHandle = new RotationalDriveHandle();
			dirHandle = new DirectionalDriveBoundsHandle();
			
		}
		public override void OnInspectorGUI()
		{
			windowSkin = new GUIStyle(GUI.skin.window);
			windowSkin.alignment = TextAnchor.UpperLeft;
			windowSkin.padding = new RectOffset(20, 0, 0, 0);

			base.OnInspectorGUI();
			drive = (DrivePort)target;
			SerializedProperty size = serializedObject.FindProperty("range");

			SerializedProperty axisValueEvents = serializedObject.FindProperty("axisValueEvents");
			SerializedProperty vec3ValueEvents = serializedObject.FindProperty("vec3ValueEvents");


			for (int i = 0; i < axisValueEvents.arraySize; i++)
			{
				SerializedProperty valueListener = axisValueEvents.GetArrayElementAtIndex(i);
				ShowValueListener(valueListener);
			}

			for (int i = 0; i < vec3ValueEvents.arraySize; i++)
			{
				SerializedProperty valueListener = vec3ValueEvents.GetArrayElementAtIndex(i);
				ShowValueListener(valueListener);
			}

			GUILayout.Label($"Axis Listeners: {axisValueEvents.arraySize} \n" +
				$"regular float listners {vec3ValueEvents.arraySize}");
			//GUILayout.Label($"{(InputManagerNew.hands)handedness.intValue} InputManager Maps: " +
			//	$"{inputMappingsProperty.arraySize}, Interactor: {manager.interactor != null} " +
			//	$"(grabMap({manager.grabMap != null}))");

			if (GUILayout.Button("Add FloatRange"))
				drive.gameObject.AddComponent<AxisValueListener>();
			if (GUILayout.Button("Add Vector3 Range"))
				drive.gameObject.AddComponent<Vector3RangeListener>();

		}

		void ShowValueListener(SerializedProperty serializedlistener)
		{
			EditorGUILayout.BeginVertical(windowSkin);
			GUILayout.Space(10);
			if (serializedlistener == null || serializedlistener.objectReferenceValue == null)
			{
				GUILayout.Label("Null");
			}
			else
			{
				GUILayout.Label($"{serializedlistener.objectReferenceValue.GetType()}");
				var listenerEditor = CreateEditor(serializedlistener.objectReferenceValue);
				listenerEditor.OnInspectorGUI();
				EditorUtility.SetDirty(listenerEditor);
			}

			GUILayout.Space(10);
			EditorGUILayout.EndVertical();
		}
		//void ShowValueEvent(ValueListener Valuelistener, int i)
		//{

		//	EditorGUILayout.BeginHorizontal();

		//	drive.ValueEventsBools[i] = EditorGUILayout.BeginFoldoutHeaderGroup(drive.ValueEventsBools[i], "Positive Bounds");
		//	EditorGUILayout.EndFoldoutHeaderGroup();

		//	ShowRangeArea(ref valueEvent);

		//	if (ShowRemoveButton(valueEvent))
		//		return null;

		//	EditorGUILayout.EndHorizontal();

		//	if (drive.ValueEventsBools[i])
		//		ShowValueEvents(valueEvent.booleanAction);

		//	return valueEvent;
		//}

		//private void ShowValueEvents(BooleanAction booleanAction)
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

		//void ShowRangeArea(ref FloatRangeListener rangeListener)
		//{
		//	MyEditorTools.BeginHorizontal();

		//	FloatRange tempRange = rangeListener;

		//	tempRange.minimum = EditorGUILayout.FloatField(tempRange.minimum, EditorStyles.numberField, miniFeildWidth);

		//	EditorGUILayout.MinMaxSlider(ref tempRange.minimum, ref tempRange.maximum, 0, 1);

		//	tempRange.maximum = EditorGUILayout.FloatField(tempRange.maximum, EditorStyles.numberField, miniFeildWidth);

		//	if (tempRange.maximum > 1)
		//		tempRange.maximum = 1;

		//	valueEvent.floatToBoolean.SetActivationRange(tempRange);

		//	MyEditorTools.EndHorizontal();
		//}

		//bool ShowRemoveButton(ValueListener listener)
		//{
		//	if (GUILayout.Button("-", EditorStyles.miniButtonLeft, miniButtonWidth))
		//	{
		//		//RemoveMap(mapping);
		//		return true;
		//	}

		//	return false;
		//}
		private void OnSceneGUI()
		{
			drive = (DrivePort)target;

			if (drive.driveType == DrivePort.DriveTypes.Directional)
			{
				Vector3 center = drive.transform.position;
				Quaternion rot = drive.transform.localRotation;

				dirHandle.axes = drive.axis;

				EditorGUI.BeginChangeCheck();

				driveBounds = new Bounds(drive.transform.position, drive.range);

				var newBounds = dirHandle.DrawHandle(drive.transform.position, rot, drive.range, drive.axis);

				if (!newBounds.Equals(driveBounds))
				{
					Undo.RecordObject(drive, "Update Center and Range");
					Undo.RecordObject(drive.transform, "Update Center and Range");
					drive.transform.position =
						drive.transform.localToWorldMatrix.MultiplyPoint3x4(newBounds.center);
					drive.range = newBounds.size;

				}
			}
			else if (drive.driveType == DrivePort.DriveTypes.Rotational)
			{
				RotationalDriveHandle.QuanternionViewer(drive.transform.position, drive.transform.rotation, drive.targetIndex);
			}

			//RotationalDriveHandle.QuanternionViewer(drive.globalStart, drive.transform.rotation, Vector3.zero);
		}
	}


#endif

}