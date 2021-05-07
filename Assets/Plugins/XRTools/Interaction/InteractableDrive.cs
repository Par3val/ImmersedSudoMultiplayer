using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRTools.Actions;
using XRTools.Interaction.Actions;
using static XRTools.Interaction.InteractableRaw;

namespace XRTools.Interaction
{
	public class InteractableDrive : GenericDrive
	{
		public InteractableRaw interactable;

		public override void FixedUpdate()
		{
			if (interactable.PrimaryActionEvent != GetDrivePrimaryAction)
			{
				SetInteractable(interactable, true);
			}

			base.FixedUpdate();
		}

		public virtual void SetInteractable(InteractableRaw _interactable, bool replace)
		{
			if (!replace && interactable)
				return;

			if (replace)
				OverrideTarget(_interactable.transform);
			else
				SetTarget(_interactable.transform);

			if (interactable)
			{
				interactable.PrimaryActionEvent = interactable.GetPrimaryAction;
			}

			interactable = _interactable;
			interactable.PrimaryActionEvent = GetDrivePrimaryAction;
		}

		public virtual void Attach()
		{
			//if in hand replace with correct action
		}

		public virtual void Detach()
		{
			//if in hand replace with correct action
		}

		public override void MoveTargetToTargetIndex()
		{
			if (!interactable.grab.currentGrabber)
				base.MoveTargetToTargetIndex();
		}

		public virtual BasicAction GetDrivePrimaryAction(InteractableRaw thisInteractable, InteractorRaw interactor)
		{
			//return base.GetPrimaryAction(interactor);
			Transform offset = thisInteractable.CreateOffset(interactor);

			if (driveType == DriveTypes.Directional)
				switch (thisInteractable.followType)
				{
					case FollowType.Transform:
						return new DirectionalTransformDriveAction(this, thisInteractable, interactor, offset);
					case FollowType.Rigidbody:
						return new DirectionalRigidbodyDriveAction(this, thisInteractable, interactor, offset);
					case FollowType.Joint:
						return null;// new DirectionalDriveJointAction(this, thisInteractable.mainRigidbody, interactor, offset);
				}
			else if (driveType == DriveTypes.Rotational)
				switch (thisInteractable.followType)
				{
					case FollowType.Transform:
						return new RotationalTransformDriveAction(this, thisInteractable, interactor, offset);
					case FollowType.Rigidbody:
						return new RotationalTransformDriveAction(this, thisInteractable, interactor, offset);
					case FollowType.Joint:
						break;
				}

			return null;
		}

	}
}