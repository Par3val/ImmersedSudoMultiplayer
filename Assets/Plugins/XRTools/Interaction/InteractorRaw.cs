using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XRTools.Utils;
using XRTools.Input;

namespace XRTools.Interaction
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CollisionTrackerNew))]
	public class InteractorRaw : MonoBehaviour
	{
		/*
		//refs
			grab action
			velocity tracker
			grab precog (float, ?)

		//actions
			touched
			untouched
			grabbed
			ungrabbed

		//tk
		*/

		#region Touch

		[System.Serializable]
		public class TouchConfig
		{
			internal InteractorRaw parent;
			public List<GameObject> touchedObjects;
			//bool sorted;

			public void TouchedChanged()
			{
				CleanInteractions.DefragTouchingGameObjects(ref touchedObjects);
				//sort based on dist
				SortList();

				//publish active
				//publish inactive
			}

			public GameObject GetBestTouched(bool requireEnabled = true)
			{
				TouchedChanged();
				foreach (var touched in touchedObjects)
				{
					if (touched == null)
						continue;

					if (requireEnabled && !touched.activeInHierarchy)
						continue;

					return touched;
				}

				return null;
			}

			public InteractableRaw GetBestInteractable(bool requireEnabled = true)
			{
				TouchedChanged();
				InteractableRaw interactableTemp;
				foreach (var touched in touchedObjects)
				{
					if (touched == null)
						continue;

					if (requireEnabled && !touched.activeInHierarchy)
						continue;

					interactableTemp = touched.GetComponentInParent<InteractableRaw>();

					if (!interactableTemp)
						continue;
					if (interactableTemp.grab.currentGrabber)
						continue;

					return interactableTemp;
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

		//Touch Config
		//{
		//	active collison container
		//	current active collison (slicer)
		//	start touching publisher (events for when list changes)
		//	stop  touching publisher (events for when list changes)

		//	GameObject[] GetTouchedObjects
		//	{
		//		return new list from active collison container.elements
		//	}

		//	GameObject GetActiveTouchedObject
		//	{
		//		if(current active collison && current active collison.activeCols.count > 0)
		//		return current active collison[0]
		//	}

		//	void PublishersSetup
		//	{
		//		start touching publisher.sourceContainer = facade
		//		stop  touching publisher.sourceContainer = facade
		//	}

		//	void OnDisable
		//	{
		//		stop  touching publisher.GetEverytingfrom(start touching publisher)
		//		stop  touching publisher.publish
		//	}
		//}

		#endregion

		#region Grab

		[System.Serializable]

		public class GrabConfig
		{
			internal InteractorRaw parent;
			public List<InteractableRaw> grabbedObjects;
			bool toogleGrab;

			public void Grab(InteractableRaw interactable, Collider col = null, Collision collision = null)
			{
				if (!interactable)
					return;

				if (grabbedObjects.Contains(interactable))
					return;

				grabbedObjects.Add(interactable);
				//Debug.Log($"{parent.name} Grabbed {interactable.name} : {grabbedObjects.Count}");//publish

				Debug.Log("Still grabbing _" + parent);
				interactable.grab.NotifyGrab(parent);
				//Utils.CleanInteractions.CleanInteractorTouching(interactable, ref parent.touch.touchedObjects);

				//create data package for grab
				//publish grab data
				//GrabProccess(true);

				//if (interactable.release == InteractableRaw.ReleaseType.Toggle)
				//{
				//	GrabProccess(false);
				//}
			}
			void GrabProccess(bool grabState)
			{
				//if (parent.grabAction.Value != grabState)
				//	parent.grabAction.Receive(grabState);

				//either start precog timer or auto grab

			}

			public InteractableRaw UnGrab(InteractableRaw tryRemove = null)
			{
				if (grabbedObjects.Count == 0)
					return null;

				InteractableRaw removeObject = null;

				if (tryRemove)
				{
					if (grabbedObjects.Contains(tryRemove))
						removeObject = grabbedObjects[0];
					else
						return null;
				}
				else
					removeObject = grabbedObjects[0];

				removeObject.grab.NotifyUnGrab(parent);
				grabbedObjects.Remove(removeObject);

				return removeObject;

			}


			//public void UnGrabProcess()
			//{
			//	if (numInHand == 1)
			//	{
			//		grabbedObjects.RemoveAt(0);
			//		parent.mainAction.Disable();
			//		parent.mainAction = null;
			//		numInHand--;
			//	}
			//	else if (numInHand == 2)
			//	{
			//		grabbedObjects.RemoveAt(1);
			//		parent.secAction.Disable();
			//		parent.secAction = null;
			//		numInHand--;
			//	}

			//	if (numInHand < 0)
			//		numInHand = 0;

			//}

		}

		//Grab Config
		//{
		//	attachpoint
		//	velocityTrackerProccessor (reads from rig to find one on source)

		//	grab action (bool)

		//	start grabbing publisher
		//	stop  grabbing publisher

		//	Instant grab proccessor

		//	precog grab proccessor
		//	precog timer
		//	min precog time

		//	Grabbed Objects[]

		//	(List<CollisionNotifier.EventData>)
		//	readonly   active Collisions data


		//	void Grab(target, collison, collider)
		//	{
		//		if(!target)
		//			return	

		//		ungrab()

		//		start grabbing publisher.setActiveCollions(make event data based on args)
		//		ProccessGrab(start grabbing publisher, true)
		//		if(target.toggleGrab)  (based on action needing to go down to go up again?)
		//			ProccessGrab(start grabbing publisher, false)
		//			
		//	}

		//	void ProccessGrab(publisher, bool state /grabbing or not grabbing\ )
		//	{
		//		Instant grab proccessor.active = false	//gets undone at bottom??
		//		precog grab proccessor.active = false	//<--

		//		if(grab action.value != state)
		//			grab action.Receive(state)

		//		if(grab action.value)
		//			publisher.publish()

		//		ChooseGrabProcessor()
		//	}

		//	void Ungrab
		//	{
		//		if(no grabbed objects)
		//			return

		//		interactible = Grabbed Objects[0]

		//		if(interactible.toggleGrab)
		//			if(start grabbing publisher.activeCollisions == 0)
		//				start grabbing publisher.setActiveCollions(intemake event data based on interacible and nulls)
		//			ProccessGrab(start grabbing publisher, true)

		//		ProccessGrab(stop  grabbing publisher, false)
		//
		//	}



		//	void SetupGrabAction
		//	{
		//		if(grab action)
		//			grab action.clearSources
		//			grab action.addSource(facade.grab action)
		//	}

		//	void SetUpVelocityTrackers
		//	{
		//		if(velocity tracker)	
		//			velocityTracker.trackers.clear
		//			velocityTracker.trackers.add(facade.velocityTracker)
		//	}

		//	void SetUpPublishers	
		//	{
		//		start grabbing publisher?.source = attachPoint
		//		stop  grabbing publisher?.source = attachPoint
		//	}

		//	void SetupPrecog
		//	{
		//		if(facade.grab precog < min precog time && facade.grab precog !~equalTo(0))
		//			facade.grab precog = min precog time
		//		precog timer.startTime = facade.grab precog
		//		ChooseGrabProcessor()	
		//	}

		//	void ChooseGrabProcessor
		//	{
		//		dissablePrecog = precogTime.startTime.~equals(0)

		//		Instant grab proccessor.active = dissablePrecog
		//		precog grab proccessor.active = !dissablePrecog
		//	}

		//}

		#endregion


		public InputManagerNew inputManager;
		//[HideInInspector]
		public VelocityTracker velocityTracker;
		CollisionTrackerNew collisionTracker;

		public GameObject mainObject;
		public Rigidbody mainRigidbody;

		public TouchConfig touch = new TouchConfig();
		public GrabConfig grab = new GrabConfig();

		[Tooltip("Time After Grab Input that will still activate a Grab")]
		public float precogTime = 0.1f;
		Vector3 attachPoint;
		Quaternion attachRot;

		[System.Serializable]
		public class InteractorEvent : UnityEvent<Collider> { }
		[System.Serializable]
		public class InteractableEvent : UnityEvent<InteractableRaw> { }

		public InteractorEvent Touched = new InteractorEvent();
		public InteractorEvent UnTouched = new InteractorEvent();
		[Space()]
		public InteractableEvent Grabbed = new InteractableEvent();
		public InteractableEvent UnGrabbed = new InteractableEvent();

		public void Grab(InteractableRaw interactable)
		{
			grab.Grab(interactable);
		}

		public void UnGrab(InteractableRaw interactable = null)
		{
			if (grab.grabbedObjects.Count > 0)
				grab.UnGrab(interactable);

		}


		private void OnEnable()
		{
			touch.touchedObjects = new List<GameObject>();
			grab.grabbedObjects = new List<InteractableRaw>();

			collisionTracker = GetComponent<CollisionTrackerNew>();


			if (!inputManager)
				inputManager = GetComponent<InputManagerNew>();
			if (!inputManager)
				inputManager = GetComponentInChildren<InputManagerNew>();

			if (!mainObject)
				mainObject = gameObject;
			if (!mainRigidbody)
				mainRigidbody = GetComponent<Rigidbody>();
			if (!velocityTracker)
			{
				velocityTracker = gameObject.GetComponent<VelocityTracker>();

				if (!velocityTracker)
					velocityTracker = gameObject.AddComponent<VelocityTracker>();
			}

			touch.parent = this;
			grab.parent = this;
		}



		private void Start()
		{
			collisionTracker.CollisionStarted.AddListener(NotifyTouched);
			collisionTracker.CollisionChanged.AddListener(NotifyTouchChanged);
			collisionTracker.CollisionStopped.AddListener(NotifyUnTouched);

			if (inputManager)
			{
				if (inputManager.grabMap.GetType().Equals(typeof(ButtonMap)))
				{
					var buttonMap = (ButtonMap)inputManager.grabMap;
					buttonMap.Activated.AddListener(NotifyTryGrabbed);
					buttonMap.Deactivated.AddListener(NotifyTryUnGrabbed);
				}
				else if (inputManager.grabMap.GetType().Equals(typeof(FloatMap)))
				{
					var floatMap = (FloatMap)inputManager.grabMap;
					floatMap.Activated.AddListener(NotifyTryGrabbed);
					floatMap.Deactivated.AddListener(NotifyTryUnGrabbed);
				}
			}
		}

		private void OnDisable()
		{
			if (collisionTracker)
			{
				collisionTracker.CollisionStarted.RemoveListener(NotifyTouched);
				collisionTracker.CollisionChanged.RemoveListener(NotifyTouchChanged);
				collisionTracker.CollisionStopped.RemoveListener(NotifyUnTouched);
			}
			if (inputManager)
			{
				if (inputManager.grabMap.GetType().Equals(typeof(ButtonMap)))
				{
					var buttonMap = (ButtonMap)inputManager.grabMap;
					buttonMap.Activated.RemoveListener(NotifyTryGrabbed);
					buttonMap.Deactivated.RemoveListener(NotifyTryUnGrabbed);
				}
				else if (inputManager.grabMap.GetType().Equals(typeof(FloatMap)))
				{
					var floatMap = (FloatMap)inputManager.grabMap;
					floatMap.Activated.RemoveListener(NotifyTryGrabbed);
					floatMap.Deactivated.RemoveListener(NotifyTryUnGrabbed);
				}
			}
		}

		protected void NotifyTouched(CollisionEventData col)
		{
			if (col.ColliderData.transform.IsChildOf(transform))
				return;

			touch.touchedObjects.Add(col.ColliderData.gameObject);
			Touched?.Invoke(col.ColliderData);
		}

		protected void NotifyTouchChanged(CollisionEventData col)
		{

		}

		protected void NotifyUnTouched(CollisionEventData col)
		{
			//if (touch.touchedObjects.Contains(col.ColliderData.gameObject))
			touch.touchedObjects.Remove(col.ColliderData.gameObject);
			UnTouched?.Invoke(col.ColliderData);
		}

		public void NotifyTryGrabbed()
		{
			if (!precogTime.ApproxEquals(0))
				if (touch.touchedObjects.Count == 0)
					StartCoroutine(PrecogTimer.GrabPrecogCoroutine(precogTime, this, NotifyTryGrabbed));

			var bestInteractableRaw = touch.GetBestInteractable();
			if (bestInteractableRaw)
			{
				grab.Grab(bestInteractableRaw);
				Grabbed?.Invoke(bestInteractableRaw);
				//bestInteractableRaw.grab.NotifyGrab(this);
			}
		}

		public void NotifyTryUnGrabbed()
		{
			if (grab.grabbedObjects.Count > 0)
			{
				var removed = grab.UnGrab();
				if (removed)
					UnGrabbed?.Invoke(removed);
			}
		}


	}
}