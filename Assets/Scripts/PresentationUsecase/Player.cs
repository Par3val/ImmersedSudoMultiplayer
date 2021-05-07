using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SudoNetworking.NetworkingData;

public class Player : MonoBehaviour
{
	public Transform headRep;
	public Transform leftHandRep;
	public Transform rightHandRep;

	public Transform leftShoulderRep;
	public Transform rightShoulderRep;

	public Transform leftElbowRep;
	public Transform rightElbowRep;

	//protected RigData rigData;


	/// <summary>
	/// describes how much effort is being put into rendering the player
	/// </summary>
	[Range(1, 3), Tooltip("describes how much effort is being put into rendering the player")]
	public int quality = 1;

	public bool isSpeaking = false;

	public int id;

	//public virtual void OnEnable()
	//{
	//	SetRigDataQuality(quality);
	//}

	//private void Update()
	//{
	//	if (rigData.quality != quality)
	//		SetRigDataQuality(quality);

	//	UpdateRigData();
	//}


	//public virtual void UpdateRigData()
	//{
	//	if (quality > 2)
	//	{
	//		rigData.LeftShoulder.SetPosition(leftShoulderRep.localPosition);
	//		rigData.LeftShoulder.SetRotation(leftShoulderRep.localRotation);

	//		rigData.RightShoulder.SetPosition(rightShoulderRep.localPosition);
	//		rigData.RightShoulder.SetRotation(rightShoulderRep.localRotation);

	//		rigData.LeftElbow.SetPosition(leftElbowRep.localPosition);
	//		rigData.LeftElbow.SetRotation(leftElbowRep.localRotation);

	//		rigData.RightElbow.SetPosition(rightElbowRep.localPosition);
	//		rigData.RightElbow.SetRotation(rightElbowRep.localRotation);
	//	}
	//	if (quality > 1)
	//	{
	//		rigData.Left.SetPosition(leftHandRep.localPosition);
	//		rigData.Left.SetRotation(leftHandRep.localRotation);

	//		rigData.Right.SetPosition(rightHandRep.localPosition);
	//		rigData.Right.SetRotation(rightHandRep.localRotation);
	//	}

	//	rigData.Head.SetPosition(headRep.localPosition);
	//	rigData.Head.SetRotation(headRep.localRotation);
	//}

	//public void SetRigDataQuality(int quality)
	//{
	//	Debug.Log("Set Quality");
	//	if (quality < 1)
	//		quality = 1;
	//	else if (quality > 3)
	//		quality = 3;
	//	qua
	//	if (quality == 1)
	//		rigData = new RigData(PoseData.empty);
	//	else if (quality == 2)
	//		rigData = new RigData(PoseData.empty,
	//					PoseData.empty,
	//					PoseData.empty);
	//	else if (quality == 3)
	//		rigData = new RigData(PoseData.empty,
	//					PoseData.empty,
	//					PoseData.empty,
	//					PoseData.empty,
	//					PoseData.empty,
	//					PoseData.empty,
	//					PoseData.empty);

	//	UpdateRigData();
	//}

	public virtual RigData GetRigData()
	{
		if (quality == 1)
			return new RigData(new PoseData(headRep.localPosition, headRep.localRotation));
		else if (quality == 2)
			return new RigData(new PoseData(headRep.localPosition, headRep.localRotation),
						new PoseData(leftHandRep.localPosition, leftHandRep.localRotation),
						new PoseData(rightHandRep.localPosition, rightHandRep.localRotation));
		else
			return new RigData(new PoseData(headRep.localPosition, headRep.localRotation),
						new PoseData(leftHandRep.localPosition, leftHandRep.localRotation),
						new PoseData(rightHandRep.localPosition, rightHandRep.localRotation),
						new PoseData(leftShoulderRep.localPosition, leftShoulderRep.localRotation),
						new PoseData(rightShoulderRep.localPosition, rightShoulderRep.localRotation),
						new PoseData(leftElbowRep.localPosition, leftElbowRep.localRotation),
						new PoseData(rightElbowRep.localPosition, rightElbowRep.localRotation));
	}
}
