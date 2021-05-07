using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventCaller : MonoBehaviour
{
	private static EventCaller instance;

	static void InstaceCheck(EventCaller _this = null)
	{
		if (instance == null)
		{
			instance = new GameObject("[vrtk0 event caller]").AddComponent<EventCaller>();

			instance.updateCallback += instance.EventDump;
			instance.lateUpdateCallback += instance.EventDump;
			instance.fixedUpdateCallback += instance.EventDump;

		}
		if (instance != _this)
			Destroy(_this);
	}

	void InstaceCheckInternal()
	{
		if (instance != null)
			if (instance != this)
				Destroy(this);
	}

	private void Awake()
	{
		InstaceCheckInternal();
	}

	private void Start()
	{
		InstaceCheckInternal();
	}

	public static void AddUpdate(Action updateMethod)
	{
		InstaceCheck();
		instance.updateCallback += updateMethod;
	}

	public static void AddFixedUpdate(Action updateMethod)
	{
		InstaceCheck();
		instance.updateCallback += updateMethod;
	}

	public static void AddLateUpdate(Action updateMethod)
	{
		InstaceCheck();
		instance.updateCallback += updateMethod;
	}

	public static void RemoveUpdate(Action updateMethod)
	{
		InstaceCheck();
		instance.updateCallback -= updateMethod;
	}

	public static void RemoveFixedUpdate(Action updateMethod)
	{
		InstaceCheck();
		instance.updateCallback -= updateMethod;
	}

	public static void RemoveLateUpdate(Action updateMethod)
	{
		InstaceCheck();
		instance.updateCallback -= updateMethod;
	}

	public Action updateCallback;
	public Action fixedUpdateCallback;
	public Action lateUpdateCallback;

	private void Update()
	{
		updateCallback?.Invoke();
	}

	private void FixedUpdate()
	{
		fixedUpdateCallback?.Invoke();
	}

	public void LateUpdate()
	{
		lateUpdateCallback?.Invoke();
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			updateCallback = null;
			fixedUpdateCallback = null;
			lateUpdateCallback = null;
		}
	}

	void EventDump() { }
}
