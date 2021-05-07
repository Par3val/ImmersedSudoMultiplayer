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
	public class GenericDrive : MonoBehaviour
	{
		public Transform Target
		{
			get { return targetObject; }
			set
			{
				if (value.GetComponent<Rigidbody>())
					targetRigidbody = value.GetComponent<Rigidbody>();
				targetObject = value;
			}
		}

		public Transform targetObject;
		Rigidbody targetRigidbody;

		[System.Serializable]
		public enum DriveTypes { Directional, Rotational }

		public DriveTypes driveType;
		public Axis axis;
		public RotAxis rotAxis;


		//Vector3 targetPos;
		Vector3 targetVelocity;
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

		public Vector3 freezePos;
		public Quaternion freezeRot;

		public Vector3 range;
		[Range(-90, 90)]
		public float rotRangeOneMin;
		[Range(-90, 90)]
		public float rotRangeOneMax;

		[Range(-90, 90)]
		public float rotRangeTwoMin;
		[Range(-90, 90)]
		public float rotRangeTwoMax;

		public float speed = 5;
		public bool moveToTarget = true;
		public bool onlyMoveWithVelocity;

		public bool lockPosition;
		public bool lockRotation;
		public bool lockInactiveAxis = true;
		/// <summary>
		/// whether locking pos or rot affects world pos or local
		/// </summary>
		[Tooltip("whether locking pos or rot affects world pos or local")]
		public bool lockLocally;



		//Events
		public Vector3ValueListener.UnityEvent normalizeMoved = new Vector3ValueListener.UnityEvent();
		public FloatValueListener.UnityEvent MovedX;
		public FloatValueListener.UnityEvent MovedY;
		public FloatValueListener.UnityEvent MovedZ;

		public List<AxisValueListener> axisValueEvents = new List<AxisValueListener>();
		public List<Vector3ValueListener> vec3ValueEvents = new List<Vector3ValueListener>();
		Vector3 previousPos;


		public virtual void OnEnable()
		{
			if (targetObject)
				Target = targetObject;
			Application.onBeforeRender += LimitTarget;
		}

		public virtual void OnDisable()
		{
			Application.onBeforeRender -= LimitTarget;
		}

		public virtual void FixedUpdate()
		{
			if (targetObject)
				if (Vector3.Distance(previousPos, targetObject.position) >= float.Epsilon)
				{
					if (moveToTarget)
					{
						MoveTargetToTargetIndex();
					}

					LimitTarget();
					EventCalls();
					previousPos = targetObject.position;
				}
		}

		public virtual void MoveTargetToTargetIndex()
		{
			Vector3 _targetPos = Vector3Utils.Lerp(-range, range, m_targetIndex);
			DriveUtils.LockFreeAxisPosition(ref _targetPos, axis);
			_targetPos = transform.TransformPoint(_targetPos);

			_targetPos = Vector3.SmoothDamp(
						targetObject.position,
						_targetPos,
						ref targetVelocity,
						1 / speed * Time.deltaTime);

			if (!onlyMoveWithVelocity)
			{
				targetObject.position = _targetPos;
			}
			else
			{
				Vector3 positionDelta = _targetPos - targetObject.position;
				Vector3 velocityTarget = positionDelta / Time.deltaTime;
				Vector3 calculatedVelocity = Vector3.MoveTowards(targetRigidbody.velocity, velocityTarget, 10);
				calculatedVelocity *= 1 / speed;
				targetRigidbody.velocity = calculatedVelocity;
			}
		}

		public virtual void EventCalls()
		{
			if (targetObject)
				if (Vector3.Distance(previousPos, targetObject.position) >= float.Epsilon)
				{
					Vector3 localPos = transform.InverseTransformPoint(targetObject.position);
					//localPos += range;
					localPos = Vector3Utils.Divide(localPos, range);
					localPos += Vector3.one;
					localPos /= 2;

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


		/// <summary>
		/// returns true if set
		/// false if target already set
		/// </summary>
		public virtual bool SetTarget(Transform _target)
		{
			if (targetObject)
				return false;

			targetObject = _target;

			freezePos = targetObject.position;
			freezeRot = targetObject.rotation;
			return true;

		}

		/// <summary>
		/// returns an override was reqired
		/// </summary>
		public virtual bool OverrideTarget(Transform _target)
		{
			bool overrideReqired = false;
			if (targetObject)
				overrideReqired = true;

			targetObject = _target;


			freezePos = targetObject.localPosition;
			freezeRot = targetObject.localRotation;

			return overrideReqired;
		}

		public virtual void LimitTarget()
		{
			if (!targetObject)
				return;
			if (lockPosition)
				targetObject.localPosition = freezePos;
			else
			{
				targetObject.position = GetLimitedPositon(targetObject);

				if (targetRigidbody)
					targetRigidbody.velocity = GetLimitedVelocity(targetRigidbody);
			}
			if (lockRotation)
				targetObject.localRotation = freezeRot;
			else
			{
				targetObject.localRotation = GetLimitedRotation(targetObject);

				if (targetRigidbody)
					targetRigidbody.angularVelocity = GetLimitedAngularVelocity(targetRigidbody);
			}
		}

		public virtual Vector3 GetLimitedPositon(Transform _target)
		{
			Vector3 localPos = transform.InverseTransformPoint(_target.transform.position);

			DriveUtils.LimitPos(ref localPos, axis, range);
			if (lockInactiveAxis)
				DriveUtils.LockFreeAxisPosition(ref localPos, axis);

			return transform.TransformPoint(localPos);
		}

		public virtual Quaternion GetLimitedRotation(Transform _target)
		{
			Vector3 localEuler = DriveUtils.LimitRot(targetObject.localEulerAngles,
													rotAxis,
													new Vector2(rotRangeOneMin, rotRangeOneMax),
													new Vector2(rotRangeTwoMin, rotRangeTwoMax));

			if (lockInactiveAxis)
				DriveUtils.LockFreeAxisRotation(ref localEuler, rotAxis);


			return Quaternion.Euler(localEuler);
		}

		public virtual Vector3 GetLimitedVelocity(Rigidbody _targetRb)
		{
			Vector3 velocityProject = _targetRb.velocity * Time.deltaTime + _targetRb.transform.position;
			Vector3 localPos = transform.InverseTransformPoint(velocityProject);

			if (DriveUtils.IsInBounds(localPos, axis, range))
				return _targetRb.velocity;

			DriveUtils.LimitPos(ref localPos, axis, range);
			if (lockInactiveAxis)
				DriveUtils.LockFreeAxisPosition(ref localPos, axis);

			return transform.TransformPoint(localPos) - _targetRb.transform.position;
		}

		public virtual Vector3 GetLimitedAngularVelocity(Rigidbody _targetRb)
		{
			return _targetRb.angularVelocity;
		}

		public void SetMoveToTarget(bool value) => moveToTarget = value;
	}


#if UNITY_EDITOR

	[CustomEditor(typeof(GenericDrive), true), CanEditMultipleObjects]
	public class GenericDriveEditor : Editor
	{
		GenericDrive drive;
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
			drive = (GenericDrive)target;
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
			drive = (GenericDrive)target;

			if (drive.driveType == GenericDrive.DriveTypes.Directional)
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
			else if (drive.driveType == GenericDrive.DriveTypes.Rotational)
			{
				RotationalDriveHandle.QuanternionViewer(drive.transform.position, drive.transform.rotation, drive.targetIndex);
			}

			//RotationalDriveHandle.QuanternionViewer(drive.globalStart, drive.transform.rotation, Vector3.zero);
		}
	}


#endif

}