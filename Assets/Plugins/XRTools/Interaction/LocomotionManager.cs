using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRTools.Utils;
using XRTools.Actions;
using XRTools.Rigs;
using XRTools.Interaction;

namespace XRTools.Locomotion
{

	public class LocomotionManager : MonoBehaviour
	{
		[System.Serializable]
		public enum LocomotionTypes
		{
			DirectTelport,
			DirectMovment,
			RelativeDirectMovment,
			PhysicMovment,
			TeleportPoints,
			Gorn,
			Walk_O_Motion
		}
		public LocomotionTypes locomotionType;

		[System.Serializable]
		public enum RotationTypes { Snap, Free, Flip, None }
		public RotationTypes rotationType;

		public Rig PlayerRig;
		public Pointer pointer;
		LocomotionAction action;

		Input.Vector2Map dirMap;
		GetActionEventDelegate locomotionAction;



		public void Start()
		{
			action = GetLocomotionAction();

			if(dirMap)
			{
				dirMap.Touched.AddListener(StartAction);

				dirMap.StateChanged.AddListener(UpdateTarget);

				dirMap.UnTouched.AddListener(EndAction);
			}

			//if (pointer && pointer.gameObject.activeSelf)
			//	pointer.gameObject.SetActive(false);
		}

		public void UpdateTarget(Vector2 targetVector)
		{
			action.MoveTarget(new Vector3(targetVector.x, 0, targetVector.y));
		}

		void UpdateTarget(Vector3 targetVector)
		{

		}

		public void StartAction()
		{
			if(action != null)
			action.SetActive(true);
		}

		public void EndAction()
		{
			if(action != null)
			action.SetActive(false);
		}

		public void ForceMove()
		{
			if (action != null)
				action.Move();
		}

		public LocomotionAction GetLocomotionAction()
		{
			switch (locomotionType)
			{
				case LocomotionTypes.DirectTelport:
					if (!pointer)
						return null;
					return new DirectTeleportLocomotion(transform, pointer);
				case LocomotionTypes.DirectMovment:
					return new DirectMoveLocomotion(transform);
				case LocomotionTypes.RelativeDirectMovment:
					return new RelativeDirectMoveLocomotion(transform, PlayerRig.Headset.transform, true);
				case LocomotionTypes.PhysicMovment:
					break;
				case LocomotionTypes.TeleportPoints:
					break;
				case LocomotionTypes.Gorn:
					break;
				case LocomotionTypes.Walk_O_Motion:
					break;
				default:
					break;
			}
			return null;
		}
	}

}