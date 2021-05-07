using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XRTools.Utils;
using XRTools.Interaction.Actions;
using XRTools.Actions;
using UnityEditor;

namespace XRTools.Interaction
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CollisionTrackerNew))]
	[SelectionBase]
	public class InteractableRaw : MonoBehaviour
	{
		#region Touch

		[System.Serializable]
		public class TouchConfig
		{
			internal InteractableRaw parent;
			public List<GameObject> touchedObjects;
			//bool sorted;

			internal void NotifyTouched(CollisionEventData col)
			{
				touchedObjects.Add(col.ColliderData.gameObject);
				SortList();
				parent.Touched?.Invoke();
			}

			internal void NotifyTouchChanged(CollisionEventData col)
			{

			}

			internal void NotifyUnTouched(CollisionEventData col)
			{
				touchedObjects.Remove(col.ColliderData.gameObject);
				parent.UnTouched?.Invoke();

			}


			public void TouchedChanged()
			{
				//sort based on dist
				SortList();

				//publish active
				//publish inactive
			}

			public GameObject GetBestTouched(bool requireEnabled = true)
			{
				foreach (var touched in touchedObjects)
				{
					if (requireEnabled)
						if (touched.activeInHierarchy)
							return touched;

					return touched;
				}

				return null;
			}


			static readonly Utils.DistanceCompare compare = new Utils.DistanceCompare();

			public void SortList()
			{
				compare.SourcePosition = parent.transform.position;
				touchedObjects.Sort(compare);
			}
		}

		#endregion

		#region Grab
		[System.Serializable]
		public class GrabData
		{
			public InteractableRaw interactable;
			public InteractorRaw interactor;

			public bool kinematicOnRelease;
			public bool gravityOnRelease;
			public BasicAction action;

			public GrabData(InteractorRaw _hand, InteractableRaw _interactable, BasicAction _action)
			{
				interactor = _hand;
				interactable = _interactable;
				action = _action;
				kinematicOnRelease = interactable.mainRigidbody.isKinematic;
				gravityOnRelease = interactable.mainRigidbody.useGravity;
			}

			public void Destroy()
			{
				action.Disable();
				action = null;
			}
		}

		[System.Serializable]
		public class GrabConfig
		{
			public int handsGrabbing { get { return grabbedObjects.Count; } }
			public InteractorRaw currentGrabber
			{
				get
				{
					if (grabbedObjects != null && grabbedObjects.Count > 0)
						return grabbedObjects[0].interactor;

					return null;
				}
			}

			internal InteractableRaw parent;
			public List<GrabData> grabbedObjects;
			bool toogleGrab;

			public void NotifyGrab(InteractorRaw interactor)
			{
				if (!interactor)
					return;
				
				if (grabbedObjects.Count == 0)
				{

				}
				BasicAction grabTypeAction = null;
				//Utils.CleanInteractions.CleanInteracableTouching(interactor, ref parent.touch.touchedObjects);
				if (grabbedObjects.Count < 1)
					grabTypeAction = parent.PrimaryActionEvent(parent, interactor);
				else//bool canGrab
					if (grabbedObjects.Count == 1)
					grabTypeAction = parent.SecondaryActionEvent(parent, interactor);

				if (grabTypeAction != null)
				{
					grabbedObjects.Add(new GrabData(interactor, parent, grabTypeAction));

					parent.Grabbed?.Invoke(interactor);

					GrabProccess();
				}

			}
			void GrabProccess()
			{
				if (parent.inheritKinematic)
					parent.mainRigidbody.isKinematic = grabbedObjects[0].interactor.mainRigidbody.isKinematic;

				parent.mainRigidbody.useGravity = false;
				parent.mainRigidbody.velocity = Vector3.zero;
				parent.mainRigidbody.angularVelocity = Vector3.zero;

				// any stuff that needs to happen when grabbed
			}

			public void NotifyUnGrab(InteractorRaw interactor = null)
			{
				if (grabbedObjects.Count == 0)
					return;

				int removeIndex = GetUngrabindex(interactor);

				if (removeIndex == -1)
					return;


				if (grabbedObjects.Count == 1) //if last hand is leaving
					UnGrabProcess();

				grabbedObjects[removeIndex].Destroy();


				grabbedObjects.RemoveAt(removeIndex);

				if (grabbedObjects.Count >= 1)
				{
					CascadeActions();
				}

				parent.UnGrabbed?.Invoke(interactor);


				if (handsGrabbing < 0)
					Debug.LogError("Too many ungrabs " + interactor, parent);
			}


			void UnGrabProcess()
			{
				if (parent.inheritKinematic)
					parent.mainRigidbody.isKinematic = grabbedObjects[0].kinematicOnRelease;
				parent.mainRigidbody.useGravity = grabbedObjects[0].gravityOnRelease;


				if (parent.useThrowVelocity)
					parent.mainRigidbody.velocity = grabbedObjects[0].interactor.velocityTracker.GetVelocity();
				if (parent.useThrowAnglularVelocity)
					parent.mainRigidbody.angularVelocity = grabbedObjects[0].interactor.velocityTracker.GetAngularVelocity();

				//Debug.Log(parent.mainRigidbody.velocity + " " + grabbedObjects[0].interactor.velocityTracker.GetVelocity());

			}

			int GetUngrabindex(InteractorRaw interactor = null)
			{
				int index = -1;

				if (interactor != null)
				{
					for (int i = 0; i < grabbedObjects.Count; i++)
					{
						if (interactor == grabbedObjects[i].interactor)
						{
							index = i;
							break;
						}
					}
				}
				else
					index = grabbedObjects.Count - 1;

				return index;
			}

			/// <summary>
			/// if there are still grabbing Objects when ungrabbed then update their actions
			/// </summary>
			void CascadeActions()
			{
				//drop sec down to prime
				grabbedObjects[0].action.Disable();
				grabbedObjects[0].action = parent.PrimaryActionEvent(parent, grabbedObjects[0].interactor);

				if (grabbedObjects.Count > 1)
				{
					grabbedObjects[1].action.Disable();
					grabbedObjects[1].action = parent.SecondaryActionEvent(parent, grabbedObjects[1].interactor);
				}
			}
		}


		#endregion

		[System.Serializable]
		public enum ReleaseType { FirstAvailable, Toggle }
		[System.Serializable]
		public enum FollowType { Transform, Rigidbody, Joint }
		[System.Serializable]
		public enum AttachType { Origin, ConstantOffset, GrabOffset }
		[System.Serializable]
		public enum SecondGrab { None, Switch, Scale, LookAt }

		public ReleaseType release;
		public FollowType followType;
		public bool preciseRotate;
		public AttachType attach;
		public SecondGrab secondary;
		public GetActionInteractableEventDelegate PrimaryActionEvent;
		public GetActionInteractableEventDelegate SecondaryActionEvent;

		[Tooltip("When Grabbed will the Rigidbody use primeary interactor's kinematic value")]
		public bool inheritKinematic = false;
		[Tooltip("When UnGrabbed will the Rigidbody use primeary interactor's velocity")]
		public bool useThrowVelocity = true;
		[Tooltip("When UnGrabbed will the Rigidbody use primeary interactor's angular velocity")]
		public bool useThrowAnglularVelocity = true;


		bool IsKinematicWhenActive = false;
		bool IsKinematicWhenInactive = false;


		public GameObject mainObject;
		public Rigidbody mainRigidbody;



		CollisionTrackerNew collisionTracker;
		List<GameObject> ActiveCollisions;

		public UnityEvent Touched = new UnityEvent();
		public UnityEvent UnTouched = new UnityEvent();


		[System.Serializable]
		public class GrabbedEvent : UnityEvent<InteractorRaw> { }

		public GrabbedEvent Grabbed = new GrabbedEvent();
		public GrabbedEvent UnGrabbed = new GrabbedEvent();


		//TEMP~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		public Vector3 constPosOffset = Vector3.zero;
		public Quaternion constRotOffset = Quaternion.identity;
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		Vector3 secAttachPoint;
		Quaternion secAttachRot;

		public TouchConfig touch;
		public GrabConfig grab;

		private void Start()
		{
			touch.touchedObjects = new List<GameObject>();
			grab.grabbedObjects = new List<GrabData>();
			
			if (!mainObject)
				mainObject = gameObject;
			if (!mainRigidbody)
				mainRigidbody = GetComponent<Rigidbody>();

			touch.parent = this;
			grab.parent = this;
		}

		private void OnEnable()
		{
			collisionTracker = GetComponent<CollisionTrackerNew>();
			collisionTracker.CollisionStarted.AddListener(touch.NotifyTouched);
			collisionTracker.CollisionChanged.AddListener(touch.NotifyTouchChanged);
			collisionTracker.CollisionStopped.AddListener(touch.NotifyUnTouched);

			if (PrimaryActionEvent == null)
				PrimaryActionEvent = this.GetPrimaryAction;


			if (SecondaryActionEvent == null)
				SecondaryActionEvent = this.GetSecondaryAction;
		}
		private void OnDisable()
		{
			if (collisionTracker)
			{
				collisionTracker.CollisionStarted.RemoveListener(touch.NotifyTouched);
				collisionTracker.CollisionChanged.RemoveListener(touch.NotifyTouchChanged);
				collisionTracker.CollisionStopped.RemoveListener(touch.NotifyUnTouched);
			}

			while (grab.grabbedObjects.Count > 0)
			{
				grab.NotifyUnGrab();
			}
		}

		public void Grab(InteractorRaw interactor)
			=> interactor.Grab(this);

		public void UnGrab(InteractorRaw interactor)
			=> interactor.UnGrab(this);


		public Transform CreateOffset(InteractorRaw interactor)
		{
			var grabPoint = new GameObject($"GrabPoint [{attach.ToString()}]").transform;
			grabPoint.parent = transform;

			switch (attach)
			{
				case AttachType.Origin:
					grabPoint.localPosition = Vector3.zero;
					grabPoint.localRotation = Quaternion.identity;
					break;
				case AttachType.ConstantOffset:
					grabPoint.localPosition = constPosOffset;
					grabPoint.localRotation = constRotOffset;
					break;
				case AttachType.GrabOffset:
					grabPoint.position = interactor.transform.position;
					grabPoint.rotation = interactor.transform.rotation;
					break;
			}

			return grabPoint;
		}


		/// <summary>
		/// returns whether any changes were made
		/// </summary>
		/// <returns></returns>
		public virtual bool ResetActions()
		{
			bool changes = false;

			if (PrimaryActionEvent != GetPrimaryAction)
				changes = true;
			if (SecondaryActionEvent != GetSecondaryAction)
				changes = true;

			PrimaryActionEvent = GetPrimaryAction;
			SecondaryActionEvent = GetSecondaryAction;

			return changes;
		}

		public virtual BasicAction GetPrimaryAction(InteractableRaw thisInteractable, InteractorRaw interactor)
		{
			Transform offset = thisInteractable.CreateOffset(interactor);

			switch (thisInteractable.followType)
			{
				case FollowType.Transform:
					if (!thisInteractable.preciseRotate)
						return new GrabFollowTransformAction(thisInteractable, interactor, offset);
					else
						return new GrabFollowTransformPosDifRotAction(thisInteractable, interactor, offset);
				case FollowType.Rigidbody:
					if (!thisInteractable.preciseRotate)
						return new GrabFollowVelocityAction(thisInteractable, interactor, offset);
					else
						return new GrabFollowRigidbodyForceAtPointAction(thisInteractable, interactor, offset);
				case FollowType.Joint:
					return new GrabFollowJointAction(thisInteractable, interactor, offset);
			}
			return null;
		}

		BasicAction GetSecondaryAction(InteractableRaw thisInteractable, InteractorRaw interactor)
		{
			BasicAction action = null;

			switch (secondary)
			{
				case SecondGrab.None:
					action = new NothingAction(thisInteractable, interactor);
					break;
				case SecondGrab.Switch:
					action = new SwitchAction(thisInteractable, interactor);
					break;
				case SecondGrab.Scale:
					break;
				case SecondGrab.LookAt:
					var updateType = followType == FollowType.Transform ? UpdateEvents.BeforeRender : UpdateEvents.FixedUpdate;
					action = new LookAtAction(thisInteractable, interactor, ((FollowAction)grab.grabbedObjects[0].action).offset, true, updateType);
					break;
			}

			return action;
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(InteractableRaw), true)]
	public class InteractableRawEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
		}
	}


#endif
}