using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using XRTools.Actions;
using XRTools.Handle;
using XRTools.Interaction.Actions;
using XRTools.Utils;
using XRTools.Utils.Data;

namespace XRTools.Interaction
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CollisionTrackerNew))]
	[SelectionBase]
	public class DriveInteractable : InteractableRaw
	{
		[System.Serializable]
		public enum DriveTypes { Directional, Rotational }

		public DriveTypes driveType;
		public Axis axis;
		public bool global = true;

		Vector3 targetPos;
		public Vector3 m_targetIndex;
		public Vector3 targetIndex
		{
			get { return m_targetIndex; }

			set
			{
				if (value.x > 1)
					value.x = 1;
				if (value.x < 0)
					value.x = 0;

				if (value.y > 1)
					value.y = 1;
				if (value.y < 0)
					value.y = 0;

				if (value.z > 1)
					value.z = 1;
				if (value.z < 0)
					value.z = 0;
				//targetPos = Vector3Utils.Lerp(minPoses, maxPoses, value);

				m_targetIndex = value;
			}
		}


		//public Vector3 minPoses;
		//public Vector3 maxPoses;
		public Vector3 range;
		public float speed = 5;
		public bool moveToTarget = true;
		public bool lockInactiveAxis = true;
		/// <summary>
		/// if Directional Lock Rotation, 
		/// if Rotational lock Positional
		/// </summary>
		[Tooltip("if Directional Lock Rotation \nif Rotational lock Position")]
		public bool lockInverse = true;
		public Matrix4x4 startMatrix;
		public Matrix4x4 inStartMatrix;

		public float index;

		public Vector3 globalStart;

		public Vector3 middlePos;

		public Quaternion middleRot;

		Vector3 scratchPos;
		Quaternion scratchRot;
		Vector3 moveVel;

		//public override BasicAction GetPrimaryAction(InteractorRaw interactor, InteractableRaw thisInteractable)
		//{
		//	//return base.GetPrimaryAction(interactor);
		//	Transform offset = thisInteractable.CreateOffset(interactor);
		//	Debug.Log($"{offset.gameObject.name}");
		//	if (driveType == DriveTypes.Directional)
		//		switch (thisInteractable.followType)
		//		{
		//			case FollowType.Transform:
		//				return new DirectionalTransformDriveAction(this, thisInteractable, interactor, offset);
		//			case FollowType.Rigidbody:
		//				return new DirectionalRigidbodyDriveAction(this, thisInteractable, interactor, offset);
		//			case FollowType.Joint:
		//				return new DirectionalDriveJointAction(this, thisInteractable.mainRigidbody, interactor, offset);
		//		}
		//	else if (driveType == DriveTypes.Rotational)
		//		switch (thisInteractable.followType)
		//		{
		//			case FollowType.Transform:
		//				return new RotationalTransformDriveAction(this, thisInteractable, interactor, offset);
		//			case FollowType.Rigidbody:
		//				break;
		//			case FollowType.Joint:
		//				break;
		//		}

		//	return null;
		//}

		private void Start()
		{
			middlePos = transform.localPosition;
			middleRot = transform.localRotation;

			globalStart = transform.position;
			startMatrix = transform.localToWorldMatrix;
			inStartMatrix = transform.worldToLocalMatrix;

			//CalculateLocalPosRange();

			Application.onBeforeRender += LimitPos;
			//Application.onBeforeRender += LimitRot;

			SetupDir();
			SetupRot();
		}

		void SetupDir()
		{
			switch (followType)
			{
				case FollowType.Transform:
					mainRigidbody.isKinematic = true;
					break;
				case FollowType.Rigidbody:
					mainRigidbody.constraints = RigidbodyConstraints.None;
					if (lockInactiveAxis)
					{
						mainRigidbody.constraints = AxisUtil.GetRigidbodyConstraints(axis, Axis.None);
					}

					if (lockInverse)
						mainRigidbody.constraints |= RigidbodyConstraints.FreezeRotation;
					break;
				case FollowType.Joint:
					mainRigidbody.constraints = RigidbodyConstraints.None;
					if (lockInactiveAxis)
					{
						mainRigidbody.constraints = AxisUtil.GetRigidbodyConstraints(axis, Axis.None);
					}

					if (lockInverse)
						mainRigidbody.constraints |= RigidbodyConstraints.FreezeRotation;
					break;
			}
		}


		void SetupRot()
		{
			switch (followType)
			{
				case FollowType.Transform:

					break;
				case FollowType.Rigidbody:
					break;
				case FollowType.Joint:
					break;
			}

		}

		private void FixedUpdate()
		{
			if (grab.currentGrabber == null)
			{
				if (moveToTarget)
				{
					if (driveType == DriveTypes.Directional)
					{
						scratchPos = Vector3Utils.Lerp(-range, range, m_targetIndex);
						scratchPos = startMatrix.MultiplyPoint3x4(scratchPos);
						scratchPos = Vector3.SmoothDamp(transform.position, scratchPos, ref moveVel, 1 / speed);
						transform.position = scratchPos;
					}
					else if (lockInverse)
					{

					}
				}
			}
			else
			{

			}
			LimitPos();
			LimitRot();
		}

		public void LimitPos()
		{
			if (driveType == DriveTypes.Directional)
			{
				scratchPos = -transform.InverseTransformPoint(globalStart);

				DriveUtils.LimitPos(ref scratchPos, axis, range);

				if(lockInactiveAxis)
					DriveUtils.LockFreeAxisPosition(ref scratchPos, axis);

				transform.position = startMatrix.MultiplyPoint3x4(scratchPos);
			}
			else if (lockInverse)
				transform.localPosition = middlePos;
		}

		public void LimitRot()
		{
			if (driveType == DriveTypes.Rotational)
			{
				scratchRot = transform.localRotation;
				//scratchPos = DriveUtils.LimitPos(scratchPos, axis, activeRange);

				//DriveUtils.LockFreeAxisPosition(ref scratchPos, axis, middlePos);
				//transform.localRotation = middleRot;

				transform.localRotation = scratchRot;
			}
			else if (lockInverse)
				transform.localRotation = middleRot;
		}

		public void SetIndex(float _index)
		{

		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.DrawSphere(targetIndex, .1f);

			Vector3 temp = transform.InverseTransformPoint(targetIndex);
			Vector3 temp2 = inStartMatrix.MultiplyPoint3x4(targetIndex);

			Gizmos.color = Color.red;
			Gizmos.DrawSphere(startMatrix.MultiplyPoint(temp), .1f);


			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(startMatrix.MultiplyPoint(temp2), .1f);
		}

		//	static Vector3 TransformPoint()
		//{
		//	transform.rotation* Vector3.Scale(myVector, transform.localScale) + transform.position;
		//}

		//private void OnDrawGizmos()
		//{
		//	Vector3 startRel = -transform.InverseTransformPoint(globalStart);

		//	Gizmos.DrawSphere(globalStart, .05f);
		//	Gizmos.DrawSphere(Vector3.zero, .05f);
		//	Gizmos.color = Color.green;
		//	Gizmos.DrawSphere(globalStart - (globalStart - transform.localPosition), .1f);

		//	Gizmos.color = Color.red;
		//	Gizmos.DrawSphere((transform.TransformPoint(-startRel)), .1f);

		//	Gizmos.color = Color.blue;
		//	scratchPos = Vector3Utils.Lerp(-range / 2, range / 2, m_targetIndex);
		//	Gizmos.DrawSphere(startMatrix.MultiplyPoint3x4(scratchPos), .1f);
		//	scratchPos = Vector3.SmoothDamp(startRel, scratchPos, ref moveVel, 1 / speed);
		//}

	}


#if UNITY_EDITOR

	[CustomEditor(typeof(DriveInteractable)), CanEditMultipleObjects]
	public class DriveInteractableEditor : InteractableRawEditor
	{
		DriveInteractable drive;
		RotationalDriveHandle rotHandle;
		DirectionalDriveBoundsHandle dirHandle;
		BoxBoundsHandle box;
		private void OnEnable()
		{
			//rotHandle = new RotationalDriveHandle();
			dirHandle = new DirectionalDriveBoundsHandle();
			box = new BoxBoundsHandle();
		}
		//public override void OnInspectorGUI()
		//{
		//	base.OnInspectorGUI();
		//	drive = (DriveInteractable)target;


		//}

		private void OnSceneGUI()
		{
			drive = (DriveInteractable)target;
			//SerializedProperty size = serializedObject.FindProperty("range");
			//rotHandle.Draw(20, drive.transform.position, drive.transform.rotation, drive.transform.up, .5f);
			if (drive.driveType == DriveInteractable.DriveTypes.Directional)
			{
				Vector3 center = Application.isPlaying ? drive.middlePos : drive.transform.position;
				Quaternion rot = Application.isPlaying ? drive.middleRot : drive.transform.localRotation;
				//DirectionalDriveHandle.DrawRange(center, rot, drive.range);
				Vector3 glboalLocal = drive.transform.InverseTransformPoint(drive.globalStart);
				Vector3 localPos = drive.transform.localPosition - glboalLocal;

				Handles.Label(center, $"({-glboalLocal.x},{-glboalLocal.y},{-glboalLocal.z})");

				dirHandle.axes = drive.axis;

				EditorGUI.BeginChangeCheck();

				dirHandle.DrawHandle(drive.globalStart, rot, drive.range, drive.axis);
				drive.range = dirHandle.liveBounds.size;
				//using (new Handles.DrawingScope(Matrix4x4.TRS(center, rot, Vector3.one)))
				//{
				//	box.size = drive.range / 2;
				//	box.DrawHandle();
				//	drive.range = box.size * 2;

				//}



//				drive.startMatrix = drive.transform.localToWorldMatrix;
//				drive.inStartMatrix = drive.transform.worldToLocalMatrix;

				//dirHandle.range = size.vector3Value * 2;
				//size.vector3Value = dirHandle.range / 2;
				if (EditorGUI.EndChangeCheck())
				{

				}
			}
			else if (drive.driveType == DriveInteractable.DriveTypes.Rotational)
			{
				RotationalDriveHandle.QuanternionViewer(drive.transform.position, drive.transform.rotation, drive.targetIndex);
			}

			//RotationalDriveHandle.QuanternionViewer(drive.globalStart, drive.transform.rotation, Vector3.zero);
		}
	}


#endif

}
/* 
				dirHandle.axis |= drive.axis.HasFlag(Axis.X) ? PrimitiveBoundsHandle.Axes.X : PrimitiveBoundsHandle.Axes.None;
				dirHandle.axis |= drive.axis.HasFlag(Axis.Y) ? PrimitiveBoundsHandle.Axes.Y : PrimitiveBoundsHandle.Axes.None;
				dirHandle.axis |= drive.axis.HasFlag(Axis.Z) ? PrimitiveBoundsHandle.Axes.Z : PrimitiveBoundsHandle.Axes.None;
*/
