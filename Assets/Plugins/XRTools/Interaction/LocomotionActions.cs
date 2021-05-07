using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRTools.Interaction;
using XRTools.Utils;

namespace XRTools.Actions
{
	public class LocomotionAction : BasicAction
	{
		public Transform targetBody;
		public Vector3 targetPos;
		protected bool active;

		public LocomotionAction(Transform _targetBody) : base(UpdateEvents.FixedUpdate)
		{
			targetBody = _targetBody;
		}

		public virtual void SetActive(bool state)
		{
			active = state;
		}

		public virtual void Move()
		{

		}

		public virtual void MoveTarget(Vector3 newPos)
		{

		}
	}

	public class DirectTeleportLocomotion : LocomotionAction
	{
		public Pointer pointer;

		public static Vector3? tempTargetPos;
		public DirectTeleportLocomotion(Transform _targetBody, Pointer _pointer) : base(_targetBody)
		{
			pointer = _pointer;
		}

		public void Move(Vector2 worldPos)
		{
			targetPos = worldPos;
			Move();
		}

		public override void SetActive(bool state)
		{
			pointer.gameObject.SetActive(state);
			base.SetActive(state);
		}

		public override void Move()
		{
			if (pointer.gameObject.activeSelf == false)
				pointer.gameObject.SetActive(true);
			tempTargetPos = pointer.GetTargetPosition();
			if (tempTargetPos != null)
				targetBody.position = tempTargetPos.Value;

			SetActive(false);
		}
	}

	public class DirectMoveLocomotion : LocomotionAction
	{
		public float speed;
		public bool moving;

		public DirectMoveLocomotion(Transform _targetBody) : base(_targetBody)
		{ }

		public override void MoveTarget(Vector3 newPos)
		{
			targetPos = newPos;
		}

		public override void UpdateAction()
		{
			if (active)
				Move();
		}
		public override void Move()
		{
			targetBody.position += targetPos.normalized * Time.deltaTime;
		}
	}

	public class RelativeDirectMoveLocomotion : DirectMoveLocomotion
	{
		Transform relativeForward;
		bool removeY;

		public RelativeDirectMoveLocomotion(Transform _targetBody, Transform _relavive, bool _removeY) : base(_targetBody)
		{
			relativeForward = _relavive;
			removeY = _removeY;
		}

		public override void MoveTarget(Vector3 newPos)
		{
			newPos = relativeForward.TransformDirection(newPos);

			if (removeY)
				base.MoveTarget(Vector3Utils.RemoveY(newPos));
			else
				base.MoveTarget(newPos);

		}
	}

	public class PhysicsMoveLocomotion : LocomotionAction
	{
		Rigidbody targetRb;

		public PhysicsMoveLocomotion(Transform _targetBody) : base(_targetBody)
		{
		}
	}


	public class TeleportPointsLocomotion : LocomotionAction
	{
		public Transform[] teleportPoints;

		public TeleportPointsLocomotion(Transform _targetBody) : base(_targetBody)
		{
		}
	}
}