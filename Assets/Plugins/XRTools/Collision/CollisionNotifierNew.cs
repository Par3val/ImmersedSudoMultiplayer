using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XRTools.Utils;

namespace XRTools
{
	/// <summary>
	/// Defines the event with the <see cref="CollisionEventData"/>.
	/// </summary>
	[System.Serializable]
	public class CollisionDataEvent : UnityEvent<CollisionEventData> { }

	/// <summary>
	/// The types of collisions that events will be emitted for.
	/// </summary>
	[System.Flags, System.Serializable]
	public enum CollisionTypes
	{
		/// <summary>
		/// A regular, non-trigger collision.
		/// </summary>
		Collision = 1 << 0,
		/// <summary>
		/// A trigger collision
		/// </summary>
		Trigger = 1 << 1
	}

	/// <summary>
	/// The states of a tracked collision.
	/// </summary>
	[System.Flags, System.Serializable]
	public enum CollisionStates
	{
		/// <summary>
		/// When a new collision occurs.
		/// </summary>
		Enter = 1 << 0,
		/// <summary>
		/// When an existing collision continues to exist.
		/// </summary>
		Stay = 1 << 1,
		/// <summary>
		/// When an existing collision ends.
		/// </summary>
		Exit = 1 << 2
	}

	public class CollisionNotifierNew : MonoBehaviour
	{
		/// <summary>
		/// The types of collisions that events will be emitted for.
		/// </summary>
		//[SerializeField]
		public CollisionTypes EmittedTypes = (CollisionTypes)(-1);// { get; set; } = (CollisionTypes)(-1);
																  /// <summary>
																  /// The <see cref="CollisionStates"/> to process.
																  /// </summary>
		[SerializeField]
		public CollisionStates StatesToProcess = (CollisionStates)(-1);// { get; set; } = (CollisionStates)(-1);

		public bool ignoreChildren = true;

		/// <summary>
		/// Allows to optionally determine which forwarded collisions to react to based on the set rules for the forwarding sender.
		/// </summary>
		//[SerializeField]
		//public RuleContainer ForwardingSourceValidity { get; set; }

		/// <summary>
		/// Emitted when a collision starts.
		/// </summary>
		public CollisionDataEvent CollisionStarted = new CollisionDataEvent();
		/// <summary>
		/// Emitted when the current collision changes.
		/// </summary>
		public CollisionDataEvent CollisionChanged = new CollisionDataEvent();
		/// <summary>
		/// Emitted when the current collision stops.
		/// </summary>
		public CollisionDataEvent CollisionStopped = new CollisionDataEvent();

		/// <summary>
		/// A reused instance to use when raising any of the events.
		/// </summary>
		protected readonly CollisionEventData eventData = new CollisionEventData();
		/// <summary>
		/// A reused instance to use when looking up <see cref="CollisionNotifier"/> components on start.
		/// </summary>
		protected readonly List<CollisionNotifierNew> startCollisionNotifiers = new List<CollisionNotifierNew>();
		/// <summary>
		/// A reused instance to use when looking up <see cref="CollisionNotifier"/> components on change.
		/// </summary>
		protected readonly List<CollisionNotifierNew> changeCollisionNotifiers = new List<CollisionNotifierNew>();
		/// <summary>
		/// A reused instance to use when looking up <see cref="CollisionNotifier"/> components on stop.
		/// </summary>
		protected readonly List<CollisionNotifierNew> stopCollisionNotifiers = new List<CollisionNotifierNew>();

		/// <summary>
		/// Whether the <see cref="collisionNotifiers"/> collection is being processed on start.
		/// </summary>
		protected bool isProcessingStartNotifierCollection;
		/// <summary>
		/// Whether the <see cref="collisionNotifiers"/> collection is being processed on change.
		/// </summary>
		protected bool isProcessingChangeNotifierCollection;
		/// <summary>
		/// Whether the <see cref="collisionNotifiers"/> collection is being processed on stop.
		/// </summary>
		protected bool isProcessingStopNotifierCollection;

		/// <summary>
		/// Determines whether events should be emitted.
		/// </summary>
		/// <param name="data">The data to check.</param>
		/// <returns><see langword="true"/> if events should be emitted.</returns>
		protected virtual bool CanEmit(CollisionEventData data)
		{
			return (data.IsTrigger && (EmittedTypes & CollisionTypes.Trigger) != 0
					|| !data.IsTrigger && (EmittedTypes & CollisionTypes.Collision) != 0)
				&& (data.ForwardSource == null || true /*ForwardingSourceValidity.Accepts(data.ForwardSource.gameObject)*/);//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~Validity
		}

		/// <summary>
		/// Returns a <see cref="CollisionNotifier"/> collection for the given <see cref="CollisionEventData"/> containing <see cref="Transform"/>.
		/// </summary>
		/// <param name="data">The <see cref="CollisionEventData"/> that holds the containing <see cref="Transform"/></param>
		/// <returns>A <see cref="CollisionNotifier"/> collection for items found on the containing <see cref="Transform"/> component.</returns>
		protected virtual List<CollisionNotifierNew> GetNotifiers(CollisionEventData data, List<CollisionNotifierNew> collisionNotifiers)
		{
			Transform reference = data.ColliderData.GetContainingTransform();

			if (transform.IsChildOf(reference))
			{
				collisionNotifiers.Clear();
			}
			else
			{
				reference.GetComponentsInChildren(collisionNotifiers);
			}

			return collisionNotifiers;
		}

		/// <summary>
		/// Processes any collision start events on the given data and propagates it to any linked <see cref="CollisionNotifier"/>.
		/// </summary>
		/// <param name="data">The collision data.</param>
		protected virtual void OnCollisionStarted(CollisionEventData data)
		{
			if ((StatesToProcess & CollisionStates.Enter) == 0 || !CanEmit(data))
			{
				return;
			}

			if (ignoreChildren)
				if (data.ColliderData.transform.IsChildOf(transform))
					return;

			CollisionStarted?.Invoke(data);

			if (isProcessingStartNotifierCollection)
			{
				return;
			}

			isProcessingStartNotifierCollection = true;
			foreach (CollisionNotifierNew notifier in GetNotifiers(data, startCollisionNotifiers))
			{
				notifier.OnCollisionStarted(data);
			}
			isProcessingStartNotifierCollection = false;
		}

		/// <summary>
		/// Processes any collision change events on the given data and propagates it to any linked <see cref="CollisionNotifier"/>.
		/// </summary>
		/// <param name="data">The collision data.</param>
		protected virtual void OnCollisionChanged(CollisionEventData data)
		{
			if ((StatesToProcess & CollisionStates.Stay) == 0 || !CanEmit(data))
			{
				return;
			}

			CollisionChanged?.Invoke(data);

			if (isProcessingChangeNotifierCollection)
			{
				return;
			}

			isProcessingChangeNotifierCollection = true;
			foreach (CollisionNotifierNew notifier in GetNotifiers(data, changeCollisionNotifiers))
			{
				notifier.OnCollisionChanged(data);
			}
			isProcessingChangeNotifierCollection = false;
		}

		/// <summary>
		/// Processes any collision stop events on the given data and propagates it to any linked <see cref="CollisionNotifier"/>.
		/// </summary>
		/// <param name="data">The collision data.</param>
		protected virtual void OnCollisionStopped(CollisionEventData data)
		{
			if ((StatesToProcess & CollisionStates.Exit) == 0 || !CanEmit(data))
			{
				return;
			}
			CollisionStopped?.Invoke(data);

			if (isProcessingStopNotifierCollection)
			{
				return;
			}

			isProcessingStopNotifierCollection = true;
			foreach (CollisionNotifierNew notifier in GetNotifiers(data, stopCollisionNotifiers))
			{
				notifier.OnCollisionStopped(data);
			}
			isProcessingStopNotifierCollection = false;
		}
	}

	/// <summary>
	/// Holds data about a <see cref="CollisionTracker"/> event.
	/// </summary>
	[System.Serializable]
	public class CollisionEventData : System.IEquatable<CollisionEventData>
	{
		/// <summary>
		/// The source of this event in case it was forwarded.
		/// </summary>
		/// <remarks><see langword="null"/> if this event wasn't forwarded from anything.</remarks>
		//[System.Serialized, Cleared]Malimbe
		[SerializeField]
		public Component ForwardSource { get; set; }
		/// <summary>
		/// Whether the collision was observed through a <see cref="Collider"/> with <see cref="Collider.isTrigger"/> set.
		/// </summary>
		//[Serialized]Malimbe
		[SerializeField]
		public bool IsTrigger { get; set; }
		/// <summary>
		/// The observed <see cref="Collision"/>. <see langword="null"/> if <see cref="IsTrigger"/> is <see langword="true"/>.
		/// </summary>
		//[Serialized]Malimbe
		[SerializeField]
		public Collision CollisionData { get; set; }
		/// <summary>
		/// The observed <see cref="Collider"/>.
		/// </summary>
		//[Serialized]Malimbe
		[SerializeField]
		public Collider ColliderData { get; set; }

		public CollisionEventData Set(CollisionEventData source)
		{
			return Set(source.ForwardSource, source.IsTrigger, source.CollisionData, source.ColliderData);
		}

		public CollisionEventData Set(Component forwardSource, bool isTrigger, Collision collision, Collider collider)
		{
			ForwardSource = forwardSource;
			IsTrigger = isTrigger;
			CollisionData = collision;
			ColliderData = collider;
			return this;
		}

		public void Clear()
		{
			Set(default, default, default, default);
		}

		public bool Equals(CollisionEventData other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Equals(ForwardSource, other.ForwardSource) && Equals(ColliderData, other.ColliderData);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			return obj is CollisionEventData other && Equals(other);
		}

		public override int GetHashCode()
		{
			return ((ForwardSource != null ? ForwardSource.GetHashCode() : 0) * 397) ^ (ColliderData != null ? ColliderData.GetHashCode() : 0);
		}

		public static bool operator ==(CollisionEventData left, CollisionEventData right) => Equals(left, right);
		public static bool operator !=(CollisionEventData left, CollisionEventData right) => !Equals(left, right);
	}
}