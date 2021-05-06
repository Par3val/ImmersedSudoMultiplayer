using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SudoNetworking.MultiplayerRoom;

public class Player : MonoBehaviour
{
	public Transform headRep;
	public Transform leftHandRep;
	public Transform rightHandRep;

	protected RigData rigData;

	public bool isSpeaking = false;

	public uint id;

	public virtual void OnEnable()
	{
		rigData = new RigData(new PoseData(headRep.localPosition, headRep.localRotation),
							new PoseData(leftHandRep.localPosition, leftHandRep.localRotation),
							new PoseData(rightHandRep.localPosition, rightHandRep.localRotation));
	}

	public virtual RigData GetRigData()
	{
		return rigData;
	}
}
