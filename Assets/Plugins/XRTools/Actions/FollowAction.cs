using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRTools.Utils;
using XRTools.Actions;

namespace XRTools.Actions
{
	public class FollowAction : BasicAction
	{
		public Transform target { get; }
		public Transform source { get; }
		public Transform offset { get; }
		bool scale = false;
		public FollowAction(GameObject _target, GameObject _source, UpdateEvents whenToUpdate, Transform _offset = null, bool useScale = false)
			: base(whenToUpdate)
		{

			target = _target.transform;
			source = _source.transform;
			if (_offset)
				offset = _offset.transform;   /* ? new OrentationHandle(_offset, _target) : new OrentationHandle();*/
			else
			{
				offset = new GameObject("Auto Follow Offset").transform;

				offset.parent = target;
				offset.localPosition = Vector3.zero;
				offset.localRotation = Quaternion.identity;
				offset.localScale = Vector3.one;
			}

			scale = useScale;
			Enable();
		}


		// Debug.LogWarning("No Follow Method Set, the \"FollowAction\" class is a parent class for a Follow Method");
		public override void UpdateAction()
		{
			Position(source.position);
			Rotation(source.rotation);
			if (scale)
				Scale(source.localScale);

		}
		public override void Disable()
		{
			Object.Destroy(offset.gameObject);
			base.Disable();
		}

		public virtual void Position(Vector3 inPos) { target.position = source.position; }

		public virtual void Rotation(Quaternion inRot) { target.rotation = source.rotation; }

		public virtual void Scale(Vector3 inScale) { target.localScale = source.localScale; }

	}

	public class FollowTransformAction : FollowAction
	{
		public FollowTransformAction(GameObject _target, GameObject _source, Transform _offset = null)
			: base(_target, _source, UpdateEvents.BeforeRender, _offset, false) { }


		public override void Position(Vector3 inPos)
		=> target.position = inPos - (offset.position - target.position);

		public override void Rotation(Quaternion inRot)
			=> target.rotation = inRot * Quaternion.Inverse(offset.localRotation);

	}

	public class FollowVelocityAction : FollowAction
	{
		public Rigidbody targetRb { get; }

		public float MaxDistanceDelta = 10;
		public float VelocityLimit = Mathf.Infinity;
		public float AngularVelocityLimit = Mathf.Infinity;

		public FollowVelocityAction(Rigidbody _target, GameObject _source, Transform _offset = null) :
			base(_target.gameObject, _source, UpdateEvents.FixedUpdate, _offset)
		{
			targetRb = _target;
		}

		public override void Position(Vector3 inPos)
		{
			Vector3 positionDelta = inPos - offset.position;
			Vector3 velocityTarget = positionDelta / Time.deltaTime;
			Vector3 calculatedVelocity = Vector3.MoveTowards(targetRb.velocity, velocityTarget, MaxDistanceDelta);


			if (calculatedVelocity.sqrMagnitude < VelocityLimit)
				targetRb.velocity = calculatedVelocity;
		}

		public override void Rotation(Quaternion inRot)
		{
			Quaternion rotationDelta = inRot * Quaternion.Inverse(offset.rotation);
			rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);
			angle = (angle > 180f) ? angle - 360f : angle;

			if (!angle.ApproxEquals(0))
			{
				Vector3 angularTarget = angle * axis;
				Vector3 calculatedAngularVelocity = Vector3.MoveTowards(targetRb.angularVelocity, angularTarget, MaxDistanceDelta);
				if (float.IsPositiveInfinity(AngularVelocityLimit) || calculatedAngularVelocity.sqrMagnitude < AngularVelocityLimit)
				{
					targetRb.angularVelocity = calculatedAngularVelocity;
				}
			}
		}
	}


	public class FollowTransformPositionDifferenceRotationAction : FollowTransformAction
	{
		public Vector3State FollowOnAxis { get; set; } = Vector3State.True;

		public float AngularDrag = 1f;
		Vector3 AngularVelocity;
		Vector3 previousSourcePosition;


		public FollowTransformPositionDifferenceRotationAction(GameObject _target, GameObject _source, Transform _offset = null) :
			base(_target, _source, _offset)
		{
			previousSourcePosition = _source.transform.position;
		}

		public override void Rotation(Quaternion inRot)
		{
			float xDegree = FollowOnAxis.useX ? CalculateAngle(target.transform.right) : 0f;
			float yDegree = FollowOnAxis.useY ? CalculateAngle(target.transform.up) : 0f;
			float zDegree = FollowOnAxis.useZ ? CalculateAngle(target.transform.forward) : 0f;

			previousSourcePosition = source.position;

			AngularVelocity = new Vector3(xDegree, yDegree, zDegree) * (1f / AngularDrag);
			target.transform.localRotation *= Quaternion.Euler(AngularVelocity);
		}

		/// <summary>
		/// Calculates the rotational angle for an axis based on the difference between two points around the origin.
		/// </summary>
		float CalculateAngle(Vector3 originDirection)
		{
			Vector3 heading = source.position - target.position;
			float headingMagnitude = heading.magnitude;
			Vector3 sideA = previousSourcePosition - target.position;

			if (headingMagnitude.ApproxEquals(0f))
			{
				return 0f;
			}

			Vector3 sideB = heading * (1f / headingMagnitude);
			return Mathf.Atan2(Vector3.Dot(originDirection, Vector3.Cross(sideA, sideB)), Vector3.Dot(sideA, sideB)) * Mathf.Rad2Deg;
		}
	}

	public class FollowRigidbodyForceAtPointAction : FollowVelocityAction
	{
		public FollowRigidbodyForceAtPointAction(Rigidbody _target, GameObject _source, Transform _offset = null) :
			base(_target, _source, _offset)
		{ }

		public override void Position(Vector3 inPos)
		{

		}

		public override void Rotation(Quaternion inRot)
		{
			Vector3 attachmentPointPosition = source.transform.position - offset.localPosition;
			Vector3 rotationForce = source.transform.position - attachmentPointPosition;

			targetRb.AddForceAtPosition(rotationForce, attachmentPointPosition, ForceMode.VelocityChange);
		}
	}
}

namespace XRTools.Interaction.Actions
{
	public delegate BasicAction GetActionInteractableEventDelegate(InteractableRaw target, InteractorRaw hand);

	public interface InteractableFollow
	{
		InteractableRaw Interactable { get; }
		InteractorRaw Interactor { get; }
	}

	public class GrabFollowTransformAction : FollowTransformAction, InteractableFollow
	{
		protected InteractableRaw _interactable;
		protected InteractorRaw _interactor;
		public InteractableRaw Interactable { get => _interactable; }
		public InteractorRaw Interactor { get => _interactor; }

		public GrabFollowTransformAction(InteractableRaw _target, InteractorRaw _source, Transform _offset = null) 
			: base(_target.mainObject, _source.mainObject, _offset)
		{
			_interactable = _target;
			_interactor = _source;
		}
	}

	public class GrabFollowVelocityAction : FollowVelocityAction, InteractableFollow
	{
		protected InteractableRaw _interactable;
		protected InteractorRaw _interactor;
		public InteractableRaw Interactable { get => _interactable; }
		public InteractorRaw Interactor { get => _interactor; }

		public GrabFollowVelocityAction(InteractableRaw _target, InteractorRaw _source, Transform _offset = null) 
			: base(_target.mainRigidbody, _source.mainObject, _offset)
		{
			_interactable = _target;
			_interactor = _source;
		}
	}

	public class GrabFollowTransformPosDifRotAction : FollowTransformPositionDifferenceRotationAction, InteractableFollow
	{
		protected InteractableRaw _interactable;
		protected InteractorRaw _interactor;
		public InteractableRaw Interactable { get => _interactable; }
		public InteractorRaw Interactor { get => _interactor; }

		public GrabFollowTransformPosDifRotAction(InteractableRaw _target, InteractorRaw _source, Transform _offset = null)
			: base(_target.mainObject, _source.mainObject, _offset)
		{
			_interactable = _target;
			_interactor = _source;
		}
	}

	public class GrabFollowRigidbodyForceAtPointAction : FollowRigidbodyForceAtPointAction, InteractableFollow
	{
		protected InteractableRaw _interactable;
		protected InteractorRaw _interactor;
		public InteractableRaw Interactable { get => _interactable; }
		public InteractorRaw Interactor { get => _interactor; }

		public GrabFollowRigidbodyForceAtPointAction(InteractableRaw _target, InteractorRaw _source, Transform _offset = null)
			: base(_target.mainRigidbody, _source.mainObject, _offset)
		{
			_interactable = _target;
			_interactor = _source;
		}
	}


	//public class FollowGrabAction : FollowAction
	//{
	//	public InteractorRaw grabHand { get; }
	//	public FollowGrabAction(GameObject _target, InteractorRaw _source, UpdateEvents whenToUpdate, Transform _offset = null) :
	//		base(_target, _source.gameObject, whenToUpdate, _offset)
	//	{
	//		grabHand = _source;
	//	}
	//}
	////public class GrabFollowAction : FollowAction
	////{
	////	public GrabFollowAction(InteractableRaw interactable, InteractorRaw interactor, Transform grabPoint) : 
	////		base(interactable.transform, interactor.transform, grabPoint)
	////	{
	////		type = interactable.follow;
	////		attach = interactable.attach;
	////	}

	////	InteractableRaw.FollowType type;
	////	InteractableRaw.AttachType attach;
	////	//secondary OrentationHandle array
	////}

	//public class FollowTransformAction : FollowGrabAction
	//{
	//	public FollowTransformAction(GameObject _target, InteractorRaw _source, Transform _offset = null) :
	//		base(_target.gameObject, _source, UpdateEvents.BeforeRender, _offset)
	//	{ }

	//	public override void Position(Vector3 inPos)
	//		=> target.position = inPos - (offset.position - target.position);

	//	public override void Rotation(Quaternion inRot)
	//		=> target.rotation = inRot * Quaternion.Inverse(offset.localRotation);
	//}

	//public class FollowRigidbodyAction : FollowGrabAction
	//{
	//	public Rigidbody targetRb { get; }

	//	public float MaxDistanceDelta = 10;
	//	public float VelocityLimit = Mathf.Infinity;
	//	public float AngularVelocityLimit = Mathf.Infinity;

	//	public FollowRigidbodyAction(Rigidbody _taret, InteractorRaw _source, Transform _offset = null) :
	//		base(_taret.gameObject, _source, UpdateEvents.FixedUpdate, _offset)
	//	{
	//		targetRb = _taret;
	//	}

	//	public override void Position(Vector3 inPos)
	//	{
	//		Vector3 positionDelta = inPos - offset.position;
	//		Vector3 velocityTarget = positionDelta / Time.deltaTime;
	//		Vector3 calculatedVelocity = Vector3.MoveTowards(targetRb.velocity, velocityTarget, MaxDistanceDelta);


	//		if (calculatedVelocity.sqrMagnitude < VelocityLimit)
	//			targetRb.velocity = calculatedVelocity;
	//	}

	//	public override void Rotation(Quaternion inRot)
	//	{
	//		Quaternion rotationDelta = inRot * Quaternion.Inverse(offset.rotation);
	//		rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);
	//		angle = (angle > 180f) ? angle - 360f : angle;

	//		if (!angle.ApproxEquals(0))
	//		{
	//			Vector3 angularTarget = angle * axis;
	//			Vector3 calculatedAngularVelocity = Vector3.MoveTowards(targetRb.angularVelocity, angularTarget, MaxDistanceDelta);
	//			if (float.IsPositiveInfinity(AngularVelocityLimit) || calculatedAngularVelocity.sqrMagnitude < AngularVelocityLimit)
	//			{
	//				targetRb.angularVelocity = calculatedAngularVelocity;
	//			}
	//		}
	//	}
	//}




	//public class FollowTransformPositionDifferenceRotation : FollowTransformAction
	//{
	//	public Vector3State FollowOnAxis { get; set; } = Vector3State.True;

	//	public float AngularDrag = 1f;
	//	Vector3 AngularVelocity;
	//	Vector3 previousSourcePosition;


	//	public FollowTransformPositionDifferenceRotation(GameObject _target, InteractorRaw _source, Transform _offset = null) :
	//		base(_target, _source, _offset)
	//	{
	//		previousSourcePosition = _source.transform.position;
	//	}

	//	public override void Rotation(Quaternion inRot)
	//	{
	//		float xDegree = FollowOnAxis.useX ? CalculateAngle(target.transform.right) : 0f;
	//		float yDegree = FollowOnAxis.useY ? CalculateAngle(target.transform.up) : 0f;
	//		float zDegree = FollowOnAxis.useZ ? CalculateAngle(target.transform.forward) : 0f;

	//		previousSourcePosition = source.position;

	//		AngularVelocity = new Vector3(xDegree, yDegree, zDegree) * (1f / AngularDrag);
	//		target.transform.localRotation *= Quaternion.Euler(AngularVelocity);
	//	}

	//	/// <summary>
	//	/// Calculates the rotational angle for an axis based on the difference between two points around the origin.
	//	/// </summary>
	//	float CalculateAngle(Vector3 originDirection)
	//	{
	//		Vector3 heading = source.position - target.position;
	//		float headingMagnitude = heading.magnitude;
	//		Vector3 sideA = previousSourcePosition - target.position;

	//		if (headingMagnitude.ApproxEquals(0f))
	//		{
	//			return 0f;
	//		}

	//		Vector3 sideB = heading * (1f / headingMagnitude);
	//		return Mathf.Atan2(Vector3.Dot(originDirection, Vector3.Cross(sideA, sideB)), Vector3.Dot(sideA, sideB)) * Mathf.Rad2Deg;
	//	}
	//}

	//public class FollowRigidbodyForceAtPointAction : FollowRigidbodyAction
	//{
	//	public FollowRigidbodyForceAtPointAction(Rigidbody _target, InteractorRaw _source, Transform _offset = null) :
	//		base(_target, _source, _offset)
	//	{ }

	//	public override void Position(Vector3 inPos)
	//	{

	//	}

	//	public override void Rotation(Quaternion inRot)
	//	{
	//		Vector3 attachmentPointPosition = source.transform.position - offset.localPosition;
	//		Vector3 rotationForce = source.transform.position - attachmentPointPosition;

	//		targetRb.AddForceAtPosition(rotationForce, attachmentPointPosition, ForceMode.VelocityChange);
	//	}
	//}
}