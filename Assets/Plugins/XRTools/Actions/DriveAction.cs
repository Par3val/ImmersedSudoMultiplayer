using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRTools.Actions;
using XRTools.Utils;
using XRTools.Utils.Data;
using XRTools.Interaction.Actions;

namespace XRTools.Interaction.Actions
{
	[System.Flags]
	public enum Axis
	{
		None = 0 << 0,
		X = 1 << 1,
		Y = 1 << 2,
		Z = 1 << 3,
		//XY = X | Y,
		//XZ = X | Z,
		//YZ = Y | Z,
		//XYZ = X | Y | Z
	}

	public enum RotAxis
	{
		None = 0 << 0,
		X = 1 << 1,
		Y = 1 << 2,
		Z = 1 << 3,
		XY = X | Y,
		XZ = X | Z,
		YZ = Y | Z,
		//XYZ = X | Y | Z
	}

	public static class AxisUtil
	{
		public static RigidbodyConstraints GetRigidbodyConstraints(Axis posAxis, Axis rotAxis)
		{
			RigidbodyConstraints constraints = RigidbodyConstraints.None;
			if (!posAxis.HasFlag(Axis.X))
				constraints |= RigidbodyConstraints.FreezePositionX;
			if (!posAxis.HasFlag(Axis.Y))
				constraints |= RigidbodyConstraints.FreezePositionY;
			if (!posAxis.HasFlag(Axis.Z))
				constraints |= RigidbodyConstraints.FreezePositionZ;

			if (!rotAxis.HasFlag(Axis.X))
				constraints |= RigidbodyConstraints.FreezeRotationX;
			if (!rotAxis.HasFlag(Axis.Y))
				constraints |= RigidbodyConstraints.FreezeRotationY;
			if (!rotAxis.HasFlag(Axis.Z))
				constraints |= RigidbodyConstraints.FreezeRotationZ;

			return constraints;
		}

	}

	public static class DriveUtils
	{

		public static void LimitPos(ref Vector3 targetPos, Axis axis, Vector3 range)
		{
			if (axis.HasFlag(Axis.X))
			{
				targetPos.x = Mathf.Max(targetPos.x, -range.x);
				targetPos.x = Mathf.Min(targetPos.x, range.x);
			}

			if (axis.HasFlag(Axis.Y))
			{
				targetPos.y = Mathf.Max(targetPos.y, -range.y);
				targetPos.y = Mathf.Min(targetPos.y, range.y);
			}

			if (axis.HasFlag(Axis.Z))
			{
				targetPos.z = Mathf.Max(targetPos.z, -range.z);
				targetPos.z = Mathf.Min(targetPos.z, range.z);
			}
		}
		public static Vector3 LimitPos(Vector3 targetPos, Axis axis, Vector3 range)
		{
			if (axis.HasFlag(Axis.X))
			{
				targetPos.x = Mathf.Max(targetPos.x, -range.x);
				targetPos.x = Mathf.Min(targetPos.x, range.x);
			}

			if (axis.HasFlag(Axis.Y))
			{
				targetPos.y = Mathf.Max(targetPos.y, -range.y);
				targetPos.y = Mathf.Min(targetPos.y, range.y);
			}

			if (axis.HasFlag(Axis.Z))
			{
				targetPos.z = Mathf.Max(targetPos.z, -range.z);
				targetPos.z = Mathf.Min(targetPos.z, range.z);
			}

			return targetPos;
		}

		public static Vector3 LimitRot(Vector3 targetEuler, RotAxis rotAxis, Vector2 rangeOne, Vector2 rangeTwo)
		{
			switch (rotAxis)
			{
				case RotAxis.X:
					targetEuler.x = Mathf.Max(targetEuler.x, rangeOne.x);
					targetEuler.x = Mathf.Min(targetEuler.x, rangeOne.y);
					break;
				case RotAxis.Y:
					targetEuler.y = Mathf.Max(targetEuler.y, rangeOne.x);
					targetEuler.y = Mathf.Min(targetEuler.y, rangeOne.y);
					break;
				case RotAxis.Z:
					targetEuler.z = Mathf.Max(targetEuler.z, rangeOne.x);
					targetEuler.z = Mathf.Min(targetEuler.z, rangeOne.y);
					break;
				case RotAxis.XY:
					targetEuler.x = Mathf.Max(targetEuler.x, rangeOne.x);
					targetEuler.x = Mathf.Min(targetEuler.x, rangeOne.y);

					targetEuler.y = Mathf.Max(targetEuler.y, rangeTwo.x);
					targetEuler.y = Mathf.Min(targetEuler.y, rangeTwo.y);
					break;
				case RotAxis.XZ:

					targetEuler.x = Mathf.Max(targetEuler.x, rangeOne.x);
					targetEuler.x = Mathf.Min(targetEuler.x, rangeOne.y);

					targetEuler.z = Mathf.Max(targetEuler.z, rangeTwo.x);
					targetEuler.z = Mathf.Min(targetEuler.z, rangeTwo.y);
					break;
				case RotAxis.YZ:

					targetEuler.y = Mathf.Max(targetEuler.y, rangeOne.x);
					targetEuler.y = Mathf.Min(targetEuler.y, rangeOne.y);

					targetEuler.z = Mathf.Max(targetEuler.z, rangeTwo.x);
					targetEuler.z = Mathf.Min(targetEuler.z, rangeTwo.y);
					break;
			}
			return targetEuler;
		}

		public static bool IsInBounds(Vector3 targetPos, Axis axis, Vector3 range)
		{
			if (axis.HasFlag(Axis.X))
			{
				if (targetPos.x < -range.x || targetPos.x > range.x)
					return false;
			}
			else if (targetPos.x != 0)
				return false;

			if (axis.HasFlag(Axis.Y))
			{

				if (targetPos.y < -range.y || targetPos.y > range.y)
					return false;
			}
			else if (targetPos.y != 0)
				return false;

			if (axis.HasFlag(Axis.Z))
			{
				if (targetPos.z < -range.z || targetPos.z > range.z)
					return false;
			}
			else if (targetPos.z != 0)
				return false;

			return true;
		}

		public static void LockFreeAxisPosition(ref Vector3 currentPos, Axis activeAxis)
		{
			if (!activeAxis.HasFlag(Axis.X))
				currentPos.x = 0;
			if (!activeAxis.HasFlag(Axis.Y))
				currentPos.y = 0;
			if (!activeAxis.HasFlag(Axis.Z))
				currentPos.z = 0;
		}

		public static void LockFreeAxisRotation(ref Vector3 cuurentEuler, RotAxis activeRotAxis)
		{
			switch (activeRotAxis)
			{
				case RotAxis.X:
					cuurentEuler.y = 0;
					cuurentEuler.z = 0;
					break;
				case RotAxis.Y:

					cuurentEuler.x = 0;
					cuurentEuler.z = 0;
					break;
				case RotAxis.Z:

					cuurentEuler.x = 0;
					cuurentEuler.y = 0;
					break;
				case RotAxis.XY:
					cuurentEuler.z = 0;
					break;
				case RotAxis.XZ:
					cuurentEuler.y = 0;
					break;
				case RotAxis.YZ:
					cuurentEuler.x = 0;
					break;
			}
		}

		/// <summary>
		/// Calculates the rotational angle for an axis based on the difference between two points around the origin.
		/// 
		/// 
		/// 
		/// </summary>
		public static float CalcAngle(Vector3 originDirection, Vector3 currentPoint, Vector3 prevPoint, Vector3 grabOffset)
		{
			Vector3 relDir = currentPoint - grabOffset;
			float relMag = relDir.magnitude;
			Vector3 sideA = prevPoint - grabOffset;

			if (relMag.ApproxEquals(0f))
			{
				return 0f;
			}

			Vector3 sideB = relDir * (1f / relMag);
			return Mathf.Atan2(Vector3.Dot(originDirection, Vector3.Cross(sideA, sideB)), Vector3.Dot(sideA, sideB)) * Mathf.Rad2Deg;
		}
	}

	public interface DriveAction
	{
		GenericDrive Drive { get; }
	}

	public class DirectionalTransformDriveAction : GrabFollowTransformAction, DriveAction
	{
		protected GenericDrive drive;
		public GenericDrive Drive { get => drive; }
		Vector3 offsetDirection;

		public DirectionalTransformDriveAction(GenericDrive _drive, InteractableRaw _target, InteractorRaw _source, Transform _offset = null)
			: base(_target, _source, _offset)
		{ drive = _drive; }

		public override void Position(Vector3 inPos)
		{
			if (drive.lockPosition)
				return;

			base.Position(inPos);

			if (!DriveUtils.IsInBounds(drive.transform.InverseTransformPoint(inPos), drive.axis, drive.range))
				Drive.LimitTarget();
		}

		public override void Rotation(Quaternion inRot)
		{
			if (drive.lockRotation)
				return;

			base.Rotation(inRot);
		}
	}

	public class DirectionalRigidbodyDriveAction : GrabFollowVelocityAction, DriveAction
	{
		protected GenericDrive drive;
		public GenericDrive Drive { get => drive; }
		Vector3 offsetDirection;

		public DirectionalRigidbodyDriveAction(GenericDrive _drive, InteractableRaw _target, InteractorRaw _source, Transform _offset = null)
			: base(_target, _source, _offset)
		{ drive = _drive; }

		public override void Position(Vector3 inPos)
		{
			if (drive.lockPosition)
				return;

			base.Position(inPos);

			if (!DriveUtils.IsInBounds(drive.transform.InverseTransformPoint(inPos), drive.axis, drive.range))
				Drive.LimitTarget();
		}

		public override void Rotation(Quaternion inRot)
		{
			if (drive.lockRotation)
				return;

			base.Rotation(inRot);
		}
	}

	public class RotationalTransformDriveAction : GrabFollowTransformPosDifRotAction, DriveAction
	{
		protected GenericDrive drive;
		public GenericDrive Drive { get => drive; }

		public Vector3State FollowOnAxis { get; set; } = Vector3State.True;
		public float AngularDrag = 1f;
		Vector3 AngularVelocity;
		Vector3 previousSourcePosition;

		public RotationalTransformDriveAction(GenericDrive _drive, InteractableRaw _target, InteractorRaw _source, Transform _offset = null)
			: base(_target, _source, _offset)
		{
			drive = _drive;
			previousSourcePosition = _source.transform.position;
			//FollowOnAxis = new Vector3State(drive.axis.HasFlag(Axis.X), drive.axis.HasFlag(Axis.Y), drive.axis.HasFlag(Axis.Z));
		}

		public override void Position(Vector3 inPos)
		{
			if (drive.lockPosition)
				return;

			base.Position(inPos);

		}

		public override void Rotation(Quaternion inRot)
		{
			if (drive.lockRotation)
				return;

			base.Rotation(inRot);
		}
	}

	public class RotationalRigidBodyDriveAction : GrabFollowJointAction, DriveAction
	{
		protected GenericDrive drive;
		public GenericDrive Drive { get => drive; }

		public RotationalRigidBodyDriveAction(GenericDrive _drive, InteractableRaw _target, InteractorRaw _source, Transform _offset = null)
			: base(_target, _source, _offset)
		{
			drive = _drive;
		}

		public override void Position(Vector3 inPos)
		{
			if (drive.lockPosition)
				return;

			base.Position(inPos);

		}

		public override void Rotation(Quaternion inRot)
		{
			if (drive.lockRotation)
				return;

			base.Rotation(inRot);
		}
	}

}