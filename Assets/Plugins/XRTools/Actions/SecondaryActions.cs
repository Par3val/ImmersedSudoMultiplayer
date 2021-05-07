using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRTools.Utils;
using XRTools.Actions;

namespace XRTools.Interaction.Actions
{
	public class SecondaryAction : BasicAction
	{
		public InteractorRaw grabHand { get; }
		public SecondaryAction(InteractorRaw interactor, UpdateEvents updates) : base(updates)
			=> grabHand = interactor;
	}

	public class NothingAction : SecondaryAction
	{
		InteractableRaw interactable;
		InteractorRaw hand;

		public NothingAction(InteractableRaw target, InteractorRaw interactor) : base(interactor, UpdateEvents.Once)
		{
			interactable = target;
			hand = interactor;
		}

		public override void UpdateAction()
		{

			hand.UnGrab(interactable);
			Disable();
		}
	}

	public class SwitchAction : SecondaryAction
	{
		InteractableRaw interactable;
		InteractorRaw hand;
		public SwitchAction(InteractableRaw target, InteractorRaw interactor) : base(interactor, UpdateEvents.Once)
		{
			interactable = target;
			hand = interactor;
		}

		public override void UpdateAction()
		{
			InteractorRaw primeHand = interactable.grab.currentGrabber;

			if (!primeHand)
			{
				Disable();
				Debug.LogError("uh oh Tried To Switch Hands With an unheld object: count + " + interactable.grab.grabbedObjects.Count);
				return;
			}

			interactable.UnGrab(primeHand);
}
	}

	//Quarentine for now
	public class LookAtAction : SecondaryAction
	{
		InteractableRaw target { get; }
		Transform lookAt { get; }
		Transform pivot { get; }

		public bool PreventLookAtZRotation { get; }
		public float ResetOrientationSpeed { get; set; } = 0.1f;

		public LookAtAction(InteractableRaw interactable, InteractorRaw interactor, Transform primearyOffset, bool preventZ, UpdateEvents updates) : base(interactor, updates)
		{
			target = interactable;
			lookAt = interactor.transform;
			pivot = primearyOffset;
			PreventLookAtZRotation = preventZ;

			SaveOrientation(false);

		}

		public override void UpdateAction()
		{
			if (PreventLookAtZRotation)
			{
				SetTargetRotationWithZLocking();
			}
			else
			{
				SetTargetRotation();
			}
		}

		Quaternion targetInitialRotation;
		Quaternion targetReleaseRotation;
		Quaternion pivotInitialRotation;
		Quaternion pivotReleaseRotation;
		Coroutine resetOrientationRoutine;

		public virtual void ClearPivot()
		{
			pivotReleaseRotation = pivot.transform.rotation;
		}

		public virtual void SaveOrientation(bool cancelResetOrientation = true)
		{
			targetInitialRotation = target != null ? target.transform.rotation : Quaternion.identity;
			pivotInitialRotation = pivot != null ? pivot.transform.rotation : Quaternion.identity;
			if (cancelResetOrientation)
			{
				CancelResetOrientation();
			}
		}

		public virtual void ResetOrientation()
		{
			pivotReleaseRotation = pivot != null ? pivot.transform.rotation : pivotReleaseRotation;

			if (ResetOrientationSpeed < float.MaxValue && ResetOrientationSpeed > 0f)
			{
				resetOrientationRoutine = target.StartCoroutine(ResetOrientationRoutine());
			}
			else if (ResetOrientationSpeed.ApproxEquals(0f))
			{
				SetOrientationToSaved();
			}
		}

		public virtual void CancelResetOrientation()
		{
			if (resetOrientationRoutine != null)
			{
				target.StopCoroutine(resetOrientationRoutine);
			}
			resetOrientationRoutine = null;
			//OrientationResetCancelled?.Invoke();
		}

		public override void Disable()
		{
			CancelResetOrientation();
			base.Disable();
		}

		void SetOrientationToSaved()
		{
			if (target == null)
			{
				return;
			}

			target.transform.rotation = GetActualInitialRotation();
			//OrientationReset?.Invoke();
		}
		void SetTargetRotation()
		{
			if (target == null || lookAt == null || pivot == null)
			{
				return;
			}

			target.transform.rotation = Quaternion.LookRotation(lookAt.transform.position - pivot.transform.position, lookAt.transform.forward);
		}

		void SetTargetRotationWithZLocking()
		{
			Vector3 normalizedForward = (pivot.transform.position - lookAt.transform.position).normalized;
			Quaternion rightLocked = Quaternion.LookRotation(normalizedForward, Vector3.Cross(-pivot.transform.right, normalizedForward).normalized);
			Quaternion targetRotation = target.transform.rotation;
			Quaternion rightLockedDelta = Quaternion.Inverse(targetRotation) * rightLocked;
			Quaternion upLocked = Quaternion.LookRotation(normalizedForward, pivot.transform.forward);
			Quaternion upLockedDelta = Quaternion.Inverse(targetRotation) * upLocked;

			target.transform.rotation = CalculateLockedAngle(upLockedDelta) < CalculateLockedAngle(rightLockedDelta) ? upLocked : rightLocked;
		}

		float CalculateLockedAngle(Quaternion lockedDelta)
		{
			lockedDelta.ToAngleAxis(out float lockedAngle, out Vector3 _);
			if (lockedAngle > 180f)
			{
				lockedAngle -= 360f;
			}
			return Mathf.Abs(lockedAngle);
		}

		/// <summary>
		/// Rotates the target back to the original rotation over a given period of time.
		/// </summary>
		/// <returns>The enumerator.</returns>
		IEnumerator ResetOrientationRoutine()
		{
			if (target == null)
			{
				yield break;
			}

			float elapsedTime = 0f;
			targetReleaseRotation = target.transform.rotation;

			while (elapsedTime < ResetOrientationSpeed)
			{
				target.transform.rotation = Quaternion.Lerp(targetReleaseRotation, GetActualInitialRotation(), elapsedTime / ResetOrientationSpeed);
				elapsedTime += Time.deltaTime;
				yield return null;
			}

			SetOrientationToSaved();
		}

		/// <summary>
		/// Gets the actual initial rotation of the target based on any changes to the pivot rotation.
		/// </summary>
		/// <returns>The actual initial rotation.</returns>
		Quaternion GetActualInitialRotation()
		{
			return targetInitialRotation * (pivotReleaseRotation * Quaternion.Inverse(pivotInitialRotation));
		}
	}

}