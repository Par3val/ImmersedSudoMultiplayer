using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace XRTools.Utils.Data
{
	/// <summary>
	/// The basis for all ValueListener types.
	/// </summary>
	//public abstract class ValueListener : MonoBehaviour
	//{
	//	/// <summary>
	//	/// Defines the event with the <see cref="bool"/>.
	//	/// </summary>
	//	[System.Serializable]
	//	public class BooleanUnityEvent : UnityEvent<bool> { }

	//	/// <summary>
	//	/// Emitted when <see cref="IsActivated"/> changes.
	//	/// </summary>
	//	public BooleanUnityEvent ActivationStateChanged = new BooleanUnityEvent();

	//	/// <summary>
	//	/// Whether the action is currently activated.
	//	/// </summary>
	//	public bool IsActivated
	//	{
	//		get => isActivated;
	//		protected set
	//		{
	//			if (isActivated == value)
	//			{
	//				return;
	//			}

	//			isActivated = value;
	//			ActivationStateChanged?.Invoke(value);
	//		}
	//	}
	//	private bool isActivated;

	//	/// <summary>
	//	/// Adds a given action to the sources collection.
	//	/// </summary>
	//	/// <param name="action">The action to add.</param>
	//	public abstract void AddSource(ValueListener action);
	//	/// <summary>
	//	/// Removes the given action from the sources collection.
	//	/// </summary>
	//	/// <param name="action">The action to remove.</param>
	//	public abstract void RemoveSource(ValueListener action);
	//	/// <summary>
	//	/// Clears all sources.
	//	/// </summary>
	//	public abstract void ClearSources();
	//	/// <summary>
	//	/// Emits the appropriate event for when the activation state changes from Activated or Deactivated.
	//	/// </summary>
	//	public abstract void EmitActivationState();
	//	/// <summary>
	//	/// Makes the action receive its own default value.
	//	/// </summary>
	//	public abstract void ReceiveDefaultValue();

	//	/// <summary>
	//	/// Whether the event should be emitted.
	//	/// </summary>
	//	/// <returns><see langword="true"/> if the event should be emitted.</returns>
	//	protected virtual bool CanEmit()
	//	{
	//		return isActiveAndEnabled;
	//	}
	//}

	/// <summary>
	/// A generic type that forms as the basis for all ValueListener types.
	/// </summary>
	/// <typeparam name="TSelf">This type itself.</typeparam>
	/// <typeparam name="TValue">The variable type the ValueListener will be utilizing.</typeparam>
	/// <typeparam name="TEvent">The <see cref="UnityEvent"/> type the ValueListener will be utilizing.</typeparam>
	public abstract class ValueListener<TSelf, TValue, TEvent> : MonoBehaviour where TSelf : ValueListener<TSelf, TValue, TEvent> where TEvent : UnityEvent<TValue>, new()
	{
		/// <summary>
		/// The initial value of the action.
		/// </summary>

		public TValue DefaultValue { get; set; }
		/// <summary>
		/// Actions to subscribe to when this action is <see cref="Behaviour.enabled"/>. Allows chaining the source actions to this action.
		/// </summary>
		protected List<TSelf> Sources { get; set; } = new List<TSelf>();

		/// <summary>
		/// Emitted when the action becomes active.
		/// </summary>
		public TEvent Activated = new TEvent();
		/// <summary>
		/// Emitted when the <see cref="Value"/> of the action changes.
		/// </summary>
		public TEvent ValueChanged = new TEvent();
		/// <summary>
		/// Emitted when the <see cref="Value"/> of the action remains unchanged.
		/// </summary>
		public TEvent ValueUnchanged = new TEvent();
		/// <summary>
		/// Emitted when the action becomes deactivated.
		/// </summary>
		public TEvent Deactivated = new TEvent();

		/// <summary>
		/// The value of the action.
		/// </summary>
		public TValue Value { get; protected set; }

		///<summary>
		/// Defines the event with the <see cref="bool"/>.
		/// </summary>
		[System.Serializable]
		public class BooleanUnityEvent : UnityEvent<bool> { }

		/// <summary>
		/// Emitted when <see cref="IsActivated"/> changes.
		/// </summary>
		public BooleanUnityEvent ActivationStateChanged = new BooleanUnityEvent();

		/// <summary>
		/// Whether the action is currently activated.
		/// </summary>
		public bool IsActivated
		{
			get => isActivated;
			protected set
			{
				if (isActivated == value)
				{
					return;
				}

				isActivated = value;
				ActivationStateChanged?.Invoke(value);
			}
		}
		private bool isActivated;

		/// <summary>
		/// Actions subscribed to when this action is <see cref="Behaviour.enabled"/>. Allows chaining the source actions to this action.
		/// </summary>
		public HeapAllocationFreeReadOnlyList<TSelf> ReadOnlySources => Sources;

		/// <inheritdoc />

		public virtual void AddSource(TSelf action)
		{
			if (action == null)
			{
				return;
			}

			Sources.Add((TSelf)action);
			SubscribeToSource((TSelf)action);
		}

		/// <inheritdoc />

		public virtual void RemoveSource(TSelf action)
		{
			if (action == null)
			{
				return;
			}

			UnsubscribeFromSource((TSelf)action);
			Sources.Remove((TSelf)action);
		}

		/// <inheritdoc />

		public virtual void ClearSources()
		{
			UnsubscribeFromSources();
			Sources.Clear();
		}

		/// <inheritdoc />

		public virtual void EmitActivationState()
		{
			if (IsActivated)
			{
				Activated?.Invoke(Value);
				ValueChanged?.Invoke(Value);
			}
			else
			{
				ValueChanged?.Invoke(Value);
				Deactivated?.Invoke(Value);
			}
		}

		/// <inheritdoc />

		public virtual void ReceiveDefaultValue()
		{
			Receive(DefaultValue);
		}

		/// <summary>
		/// Acts on the value.
		/// </summary>
		/// <param name="value">The value to act on.</param>

		public virtual void Receive(TValue value)
		{
			if (IsValueEqual(value))
			{
				ValueUnchanged?.Invoke(Value);
				return;
			}

			ProcessValue(value);
		}

		protected virtual void Awake()
		{
			Value = DefaultValue;
		}

		protected virtual void OnEnable()
		{
			SubscribeToSources();
		}

		protected virtual void OnDisable()
		{
			//ProcessValue(DefaultValue);
			UnsubscribeFromSources();
		}

		/// <summary>
		/// Subscribes the current action as a listener to the given action.
		/// </summary>
		/// <param name="source">The source action to subscribe listeners on.</param>
		protected virtual void SubscribeToSource(TSelf source)
		{
			if (source == null)
			{
				return;
			}

			source.ValueChanged.AddListener(Receive);
			source.ValueUnchanged.AddListener(Receive);
		}

		/// <summary>
		/// Unsubscribes the current action from listening to the given action.
		/// </summary>
		/// <param name="source">The source action to unsubscribe listeners on.</param>
		protected virtual void UnsubscribeFromSource(TSelf source)
		{
			if (source == null)
			{
				return;
			}

			source.ValueChanged.RemoveListener(Receive);
			source.ValueUnchanged.RemoveListener(Receive);
		}

		/// <summary>
		/// Attempts to subscribe listeners to each of the source actions.
		/// </summary>
		protected virtual void SubscribeToSources()
		{
			if (Sources == null)
			{
				return;
			}

			foreach (TSelf source in Sources)
			{
				SubscribeToSource(source);
			}
		}

		/// <summary>
		/// Attempts to unsubscribe existing listeners from each of the source actions.
		/// </summary>
		protected virtual void UnsubscribeFromSources()
		{
			if (Sources == null)
			{
				return;
			}

			foreach (TSelf source in Sources)
			{
				UnsubscribeFromSource(source);
			}
		}

		/// <summary>
		/// Processes the given value and emits the appropriate events.
		/// </summary>
		/// <param name="value">The new value.</param>
		protected virtual void ProcessValue(TValue value)
		{
			Value = value;

			bool shouldActivate = this.ShouldActivate(value);

			if (IsActivated != shouldActivate)
			{
				IsActivated = shouldActivate;
				EmitActivationState();
			}
			else
			{
				ValueChanged?.Invoke(Value);
			}
		}

		/// <summary>
		/// Whether the given <see cref="TValue"/> is equal to the action's cached <see cref="Value"/>.
		/// </summary>
		/// <param name="value">The value to check equality for.</param>
		/// <returns><see langword="true"/> if the given <see cref="TValue"/> is equal to the action's cached <see cref="Value"/>.</returns>
		protected virtual bool IsValueEqual(TValue value)
		{
			return EqualityComparer<TValue>.Default.Equals(Value, value);
		}

		/// <summary>
		/// Whether the action should become active.
		/// </summary>
		/// <param name="value">The current value to check activation state on.</param>
		/// <returns><see langword="true"/> if the action should activate.</returns>
		protected virtual bool ShouldActivate(TValue value)
		{
			return !EqualityComparer<TValue>.Default.Equals(DefaultValue, value);
		}

		/// <summary>
		/// Called after <see cref="DefaultValue"/> has been changed.
		/// </summary>
		//[CalledAfterChangeOf(nameof(DefaultValue))]
		protected virtual void OnAfterDefaultValueChange()
		{
			bool shouldActivate = ShouldActivate(Value);
			if (IsActivated == shouldActivate)
			{
				return;
			}

			IsActivated = shouldActivate;
			EmitActivationState();
		}

		/// <summary>
		/// Called before <see cref="Sources"/> has been changed.
		/// </summary>
		//[CalledBeforeChangeOf(nameof(Sources))]
		protected virtual void OnBeforeSourcesChange()
		{
			UnsubscribeFromSources();
		}

		/// <summary>
		/// Called after <see cref="Sources"/> has been changed.
		/// </summary>
		//[CalledAfterChangeOf(nameof(Sources))]
		protected virtual void OnAfterSourcesChange()
		{
			SubscribeToSources();
		}
	}



	//public abstract class ValueListener : MonoBehaviour
	//{
	//	/// <summary>
	//	/// Defines the event with the <see cref="bool"/>.
	//	/// </summary>
	//	public class BooleanUnityEvent : UnityEvent<bool> { }

	//	/// <summary>
	//	/// Emitted when <see cref="IsActivated"/> changes.
	//	/// </summary>
	//	public BooleanUnityEvent ActivationStateChanged = new BooleanUnityEvent();

	//	/// <summary>
	//	/// Whether the action is currently activated.
	//	/// </summary>
	//	public bool IsActivated
	//	{
	//		get => isActivated;
	//		protected set
	//		{
	//			if (isActivated == value)
	//			{
	//				return;
	//			}

	//			isActivated = value;
	//			ActivationStateChanged?.Invoke(value);
	//		}
	//	}
	//	private bool isActivated;

	//	//public abstract void AddSource(ValueListener action);
	//	//public abstract void RemoveSource(ValueListener action);
	//	//public abstract void ClearSources();
	//	public abstract void ReceiveDefaultValue();

	//	/// <summary>
	//	/// Emits the appropriate event for when the activation state changes from Activated or Deactivated.
	//	/// </summary>
	//	public abstract void EmitActivationState();

	//	/// <summary>
	//	/// Whether the event should be emitted.
	//	/// </summary>
	//	/// <returns><see langword="true"/> if the event should be emitted.</returns>
	//	protected virtual bool CanEmit()
	//	{
	//		return isActiveAndEnabled;
	//	}
	//}

	///// <summary>
	///// A generic type that forms as the basis for all ValueListener types.
	///// </summary>
	///// <typeparam name="TValue">The variable type the action will be utilizing.</typeparam>
	///// <typeparam name="TEvent">The <see cref="UnityEvent"/> type the action will be utilizing.</typeparam>
	//public abstract class ValueListener<TSelf, TValue, TEvent> : ValueListener
	//	where TSelf : ValueListener<TSelf, TValue, TEvent> 
	//	where TEvent : UnityEvent<TValue>, new()
	//{
	//	public TValue DefaultValue { get; set; }
	//	public TValue Value;/*{ get; protected set; }*/

	//	[SerializeReference]
	//	public TEvent Activated = new TEvent();
	//	//[SerializeReference]
	//	public TEvent ValueChanged = new TEvent();
	//	//[SerializeReference]
	//	public TEvent Deactivated = new TEvent();
	//	/// <summary>
	//	/// Emitted when the <see cref="Value"/> of the action remains unchanged.
	//	/// </summary>
	//	//[SerializeReference]
	//	public TEvent ValueUnchanged = new TEvent();

	//	//public class BooleanUnityEvent : UnityEvent<bool> { }

	//	//public BooleanUnityEvent ActivationStateChanged = new BooleanUnityEvent();

	//	///// <summary>
	//	///// Whether the action is currently activated.
	//	///// </summary>
	//	//public bool IsActivated
	//	//{
	//	//	get => isActivated;
	//	//	protected set
	//	//	{
	//	//		if (isActivated == value)
	//	//		{
	//	//			return;
	//	//		}

	//	//		isActivated = value;
	//	//		ActivationStateChanged?.Invoke(value);
	//	//	}
	//	//}
	//	//private bool isActivated;

	//	public override void EmitActivationState()
	//	{
	//		if (IsActivated)
	//		{
	//			//Activated?.Invoke(Value);
	//			//ValueChanged?.Invoke(Value);
	//		}
	//		else
	//		{
	//			//ValueChanged?.Invoke(Value);
	//			//Deactivated?.Invoke(Value);
	//		}
	//	}

	//	public override void ReceiveDefaultValue()
	//	{
	//		Receive(DefaultValue);
	//	}

	//	public virtual void Receive(TValue value)
	//	{
	//		if (IsValueEqual(value))
	//		{
	//			//ValueUnchanged?.Invoke(Value);
	//			return;
	//		}

	//		ProcessValue(value);
	//	}

	//	protected virtual void Awake()
	//	{
	//		Value = DefaultValue;
	//	}

	//	void OnEnable()
	//	{
	//	}

	//	void OnDisable()
	//	{
	//		//ProcessValue(DefaultValue);
	//	}


	//	/// <summary>
	//	/// Processes the given value and emits the appropriate events.
	//	/// </summary>
	//	/// <param name="value">The new value.</param>
	//	public virtual void ProcessValue(TValue value)
	//	{
	//		Value = value;

	//		bool shouldActivate = ShouldActivate(value);
	//		if (IsActivated != shouldActivate)
	//		{
	//			IsActivated = shouldActivate;
	//			EmitActivationState();
	//		}
	//		else
	//		{
	//			//ValueChanged?.Invoke(Value);
	//		}
	//	}

	//	/// <summary>
	//	/// Whether the given <see cref="TValue"/> is equal to the action's cached <see cref="Value"/>.
	//	/// </summary>
	//	/// <param name="value">The value to check equality for.</param>
	//	/// <returns><see langword="true"/> if the given <see cref="TValue"/> is equal to the action's cached <see cref="Value"/>.</returns>
	//	protected virtual bool IsValueEqual(TValue value)
	//	{
	//		return EqualityComparer<TValue>.Default.Equals(Value, value);
	//	}

	//	/// <summary>
	//	/// Whether the action should become active.
	//	/// </summary>
	//	/// <param name="value">The current value to check activation state on.</param>
	//	/// <returns><see langword="true"/> if the action should activate.</returns>
	//	protected virtual bool ShouldActivate(TValue value)
	//	{
	//		return !EqualityComparer<TValue>.Default.Equals(DefaultValue, value);
	//	}

	//}
}