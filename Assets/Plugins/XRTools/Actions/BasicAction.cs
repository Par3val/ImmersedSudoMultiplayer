using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XRTools.Actions
{
	[System.Flags, System.Serializable]
	public enum UpdateEvents
	{
		Once = 1 << 0,
		FixedUpdate = 1 << 1,
		Update = 1 << 2,
		LateUpdate = 1 << 3,
		/// <summary>
		/// Executes the processes in the camera PreCull scene rendering part of the Unity game loop.
		/// </summary>
		PreCull = 1 << 4,
		BeforeRender = 1 << 5,
	}

	public delegate BasicAction GetActionEventDelegate();

	public class BasicAction
	{

		UpdateEvents updates;

		public BasicAction(UpdateEvents _updates)
		{
			updates = _updates;
			if (updates.HasFlag(UpdateEvents.BeforeRender) || updates.HasFlag(UpdateEvents.Once))
				Application.onBeforeRender += this.UpdateAction;
			if (updates.HasFlag(UpdateEvents.FixedUpdate))
				EventCaller.AddFixedUpdate(this.UpdateAction);
			//UpdateAction();
		}

		public virtual void UpdateAction()
		{

		}

		public virtual void Enable()
		{
		}

		public virtual void Disable()
		{
			if (updates.HasFlag(UpdateEvents.BeforeRender) || updates.HasFlag(UpdateEvents.Once))
				Application.onBeforeRender -= this.UpdateAction;
			if (updates.HasFlag(UpdateEvents.FixedUpdate))
				EventCaller.RemoveFixedUpdate(this.UpdateAction);
		}
	}
}