using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XRTools.Interaction.Actions
{
	public delegate void FixedUpdateEvent();




	

	//public class OrentationHandle
	//{
	//	public Vector3 localPosition { get; }
	//	public Quaternion localRotation { get; }
	//	public Vector3 scale { get; }

	//	public Transform parent { get; }

	//	/// <summary>
	//	/// If no parent same as == <see cref"localPosition">
	//	/// </summary>
	//	public Vector3 globalPosition { get => parent ? localPosition + parent.position : localPosition; }

	//	/// <summary>
	//	/// If no parent same as == <see cref"localRotation">
	//	/// </summary>
	//	public Quaternion rotation { get => parent ? parent.rotation * localRotation : localRotation; }

	//	/*
	//	 * TODO FIX

	//	/// <summary>
	//	/// If no parent same as == <see cref"localScale">
	//	/// </summary>
	//	public Vector3 globalScale { get => parent ? localScale + parent. : localScale; }
	//	*/


	//	public OrentationHandle()
	//	{
	//		localPosition = Vector3.zero;
	//		localRotation = Quaternion.identity;
	//		scale = Vector3.one;
	//	}
	//	public OrentationHandle(Transform transform, Transform parent = null)
	//	{
	//		localPosition = transform.localPosition;
	//		localRotation = transform.localRotation;
	//		scale = transform.localScale;

	//		if (parent != null)
	//			this.parent = parent;
	//	}

	//	public OrentationHandle(Vector3 pos, Quaternion rot)
	//	{
	//		localPosition = pos;
	//		localRotation = rot;
	//	}
	//	//add
	//	//rotation = source * differenceAngle

	//	//sub
	//	//rotation = Quaternion.Inverse(transform.rotation) * qTargetRot;
	//}

	//public interface GrabAction
	//{
	//	InteractorRaw grabHand { get; set; }

	//	void Grabed(InteractorRaw interactor);
	//}
	//public interface FollowMethod
	//{
	//	void Position(Vector3 inPos);

	//	void Rotation(Quaternion inRot);

	//	void Scale(Vector3 inScale);
	//}
	//public class GrabAction : BasicAction
	//{
	//	public GrabAction(InteractorRaw interactor)
	//	{
	//		grabHand = interactor;
	//	}

	//	InteractorRaw grabHand;
	//}
}