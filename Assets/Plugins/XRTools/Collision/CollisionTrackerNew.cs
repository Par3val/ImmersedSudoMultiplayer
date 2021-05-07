﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XRTools.Utils
{
	public class CollisionTrackerNew : CollisionNotifierNew
	{
		/// <summary>
		/// Causes collisions to stop if the <see cref="GameObject"/> on either side of the collision is disabled.
		/// </summary>
		[SerializeField]
		public bool StopCollisionsOnDisable { get; protected set; } = true;

		/// <summary>
		/// A collection of current existing collisions.
		/// </summary>
		protected List<Collider> trackedCollisions = new List<Collider>();

		/// <summary>
		/// Stops the collision between this <see cref="CollisionTrackerNew"/> and the given <see cref="Collider"/>. If there is still a physical intersection, then the collision will start again in the next physics frame.
		/// </summary>
		/// <param name="collider">The collider to stop the collision with.</param>
		public virtual void StopCollision(Collider collider)
		{
			RemoveDisabledObserver(collider);

			if ((StatesToProcess & CollisionStates.Exit) == 0)
			{
				return;
			}
			if(collider)
				OnCollisionStopped(eventData.Set(this, collider.isTrigger, null, collider));
		}

		protected virtual void OnDisable()
		{
			if (!StopCollisionsOnDisable)
			{
				return;
			}

			foreach (Collider collider in trackedCollisions.ToArray())
			{
				StopCollision(collider);
			}
		}

		protected virtual void OnCollisionEnter(Collision collision)
		{
			AddDisabledObserver(collision.collider);

			if ((StatesToProcess & CollisionStates.Enter) == 0)
			{
				return;
			}

			OnCollisionStarted(eventData.Set(this, false, collision, collision.collider));
		}

		protected virtual void OnCollisionStay(Collision collision)
		{
			if ((StatesToProcess & CollisionStates.Stay) == 0)
			{
				return;
			}

			OnCollisionChanged(eventData.Set(this, false, collision, collision.collider));
		}

		protected virtual void OnCollisionExit(Collision collision)
		{
			RemoveDisabledObserver(collision.collider);

			if ((StatesToProcess & CollisionStates.Exit) == 0)
			{
				return;
			}

			OnCollisionStopped(eventData.Set(this, false, collision, collision.collider));
		}

		protected virtual void OnTriggerEnter(Collider collider)
		{
			AddDisabledObserver(collider);

			if ((StatesToProcess & CollisionStates.Enter) == 0)
			{
				return;
			}

			OnCollisionStarted(eventData.Set(this, true, null, collider));
		}

		protected virtual void OnTriggerStay(Collider collider)
		{
			if ((StatesToProcess & CollisionStates.Stay) == 0)
			{
				return;
			}

			OnCollisionChanged(eventData.Set(this, true, null, collider));
		}

		protected virtual void OnTriggerExit(Collider collider)
		{
			RemoveDisabledObserver(collider);

			if ((StatesToProcess & CollisionStates.Exit) == 0)
			{
			Debug.Log("Exit");
				return;
			}

			OnCollisionStopped(eventData.Set(this, true, null, collider));
		}

		/// <summary>
		/// Adds a <see cref="CollisionTrackerNewDisabledObserver"/> to the <see cref="GameObject"/> that contains the <see cref="Collider"/> causing the collision.
		/// </summary>
		/// <param name="target">The target to add the <see cref="CollisionTrackerNewDisabledObserver"/> component to.</param>
		protected virtual void AddDisabledObserver(Collider target)
		{
			if (target == null || !StopCollisionsOnDisable)
			{
				return;
			}

			trackedCollisions.Add(target);
			CollisionTrackerNewDisabledObserver observer = target.gameObject.AddComponent<CollisionTrackerNewDisabledObserver>();
			observer.Source = this;
			observer.Target = target;
		}

		/// <summary>
		/// Removes the <see cref="CollisionTrackerNewDisabledObserver"/> from the <see cref="GameObject"/> that contains the <see cref="Collider"/> ceasing the collision.
		/// </summary>
		/// <param name="target">The target to remove the <see cref="CollisionTrackerNewDisabledObserver"/> component from.</param>
		protected virtual void RemoveDisabledObserver(Collider target)
		{
			if (target == null || !StopCollisionsOnDisable)
			{
				return;
			}

			foreach (CollisionTrackerNewDisabledObserver observer in target.gameObject.GetComponents<CollisionTrackerNewDisabledObserver>())
			{
				if (observer.Source == this && observer.Target == target)
				{
					observer.Destroy();
					trackedCollisions.Remove(target);
				}
			}
		}
	}

	/// <summary>
	/// Observes the disabled state of any <see cref="GameObject"/> that the <see cref="CollisionTrackerNew"/> is currently colliding with.
	/// </summary>
	public class CollisionTrackerNewDisabledObserver : MonoBehaviour
	{
		/// <summary>
		/// The <see cref="CollisionTrackerNew"/> that is causing the collision.
		/// </summary>
		[SerializeField]
		public CollisionTrackerNew Source { get; set; }
		/// <summary>
		/// The <see cref="Collider"/> that is being collided with.
		/// </summary>
		[SerializeField]
		public Collider Target { get; set; }

		/// <summary>
		/// Whether <see cref="this"/> is being destroyed.
		/// </summary>
		protected bool isDestroyed;

		/// <summary>
		/// Destroys <see cref="this"/> from the scene.
		/// </summary>
		public virtual void Destroy()
		{
			isDestroyed = true;
			Destroy(this);
		}

		protected virtual void OnDisable()
		{
			if (!isDestroyed)
			{
				Source.StopCollision(Target);
			}
		}
	} 
}
