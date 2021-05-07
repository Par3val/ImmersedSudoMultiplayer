using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRTools.Actions;
using XRTools.Interaction;
using XRTools.Interaction.Actions;
using XRTools.Utils;

namespace XRTools.Actions
{
	public class FollowJointAction : FollowAction
	{
		public Rigidbody targetRb { get; }
		public FixedJoint joint { get; }

		public FollowJointAction(Rigidbody _taret, Rigidbody _source, Transform _offset = null) :
			base(_taret.gameObject, _source.gameObject, UpdateEvents.Once, _offset)
		{
			targetRb = _taret;
			joint = target.gameObject.AddComponent<FixedJoint>();
			joint.connectedBody = _source;
			joint.anchor = offset.localPosition;
		}

		public override void Position(Vector3 inPos)
		{
		}

		public override void Rotation(Quaternion inRot)
		{
		}

		public static IEnumerator WaitFrameToThrow(Vector3 throwVel, Vector3 throwAngVel, Rigidbody target)
		{
			yield return new WaitForFixedUpdate();
			target.velocity = throwVel;
			target.angularVelocity = throwAngVel;
		}

		
	}

	public class GrabFollowJointAction : FollowJointAction, InteractableFollow
	{
		protected InteractableRaw _interactable;
		protected InteractorRaw _interactor;
		public InteractableRaw Interactable { get => _interactable; }
		public InteractorRaw Interactor { get => _interactor; }

		public GrabFollowJointAction(InteractableRaw _target, InteractorRaw _source, Transform _offset = null)
			: base(_target.mainRigidbody, _source.mainRigidbody, _offset)
		{
			_interactable = _target;
			_interactor = _source;
		}

		public override void Disable()
		{
			joint.breakForce = 0;
			joint.breakTorque = 0;

			_interactable.StartCoroutine(WaitFrameToThrow(targetRb.velocity, targetRb.angularVelocity, targetRb));

			base.Disable();
		}
	}

	//[System.Obsolete]
	//public class DirectionalDriveJointAction : FollowGrabAction
	//{
	//	public Rigidbody targetRb { get; }
	//	public SpringJoint joint { get; }
	//	DrivePort drive;

	//	public DirectionalDriveJointAction(DrivePort _drive, Rigidbody _taret, InteractorRaw _source, Transform _offset = null) :
	//		base(_taret.gameObject, _source, UpdateEvents.Once, _offset)
	//	{
	//		targetRb = _taret;
	//		joint = target.gameObject.AddComponent<SpringJoint>();
	//		joint.connectedBody = _source.mainRigidbody;
	//		joint.anchor = offset.localPosition;
	//		joint.enablePreprocessing = false;
	//		joint.spring *= 30;
	//		drive = _drive;
	//	}

	//	public override void Position(Vector3 inPos)
	//	{
	//		targetRb.velocity = Vector3.zero;
	//	}

	//	public override void Rotation(Quaternion inRot)
	//	{
	//		targetRb.angularVelocity = Vector3.zero;
	//	}

	//	public static IEnumerator WaitFrameToThrow(Vector3 throwVel, Vector3 throwAngVel, Rigidbody target)
	//	{
	//		yield return new WaitForFixedUpdate();
	//		target.velocity = throwVel;
	//		target.angularVelocity = throwAngVel;
	//	}

	//	public override void Disable()
	//	{
	//		joint.breakForce = 0;
	//		joint.breakTorque = 0;

	//		grabHand.StartCoroutine(WaitFrameToThrow(targetRb.velocity, targetRb.angularVelocity, targetRb));

	//		base.Disable();
	//	}
	//}

	//[System.Obsolete]
	//public class RotationalDriveJointAction : FollowGrabAction
	//{
	//	public Rigidbody targetRb { get; }
	//	public ConfigurableJoint joint { get; }
	//	DrivePort drive;

	//	public RotationalDriveJointAction(DrivePort _drive, Rigidbody _taret, InteractorRaw _source, Transform _offset = null) :
	//		base(_taret.gameObject, _source, UpdateEvents.Once, _offset)
	//	{
	//		targetRb = _taret;
	//		joint = target.gameObject.AddComponent<ConfigurableJoint>();
	//		joint.connectedBody = _source.mainRigidbody;
	//		joint.anchor = offset.localPosition;
	//		drive = _drive;
	//	}

	//	public override void Position(Vector3 inPos)
	//	{
	//		inPos = drive.transform.InverseTransformPoint(inPos);

	//		DriveUtils.LimitPos(ref inPos, drive.axis, drive.range);
	//		if (drive.lockInactiveAxis)
	//			DriveUtils.LockFreeAxisPosition(ref inPos, drive.axis);

	//		base.Position(drive.transform.TransformPoint(inPos));
	//	}

	//	public override void Rotation(Quaternion inRot)
	//	{
	//		if (drive.lockInverse)
	//			target.localRotation = drive.transform.localRotation * offset.localRotation;
	//		else
	//			base.Rotation(inRot);
	//	}

	//	public static IEnumerator WaitFrameToThrow(Vector3 throwVel, Vector3 throwAngVel, Rigidbody target)
	//	{
	//		yield return new WaitForFixedUpdate();
	//		target.velocity = throwVel;
	//		target.angularVelocity = throwAngVel;
	//	}

	//	public override void Disable()
	//	{
	//		joint.breakForce = 0;
	//		joint.breakTorque = 0;

	//		grabHand.StartCoroutine(WaitFrameToThrow(targetRb.velocity, targetRb.angularVelocity, targetRb));

	//		base.Disable();
	//	}
	//}
}