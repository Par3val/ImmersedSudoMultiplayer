using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SudoNetworking.MultiplayerRoom;

public class Player : MonoBehaviour
{
	public Transform headRep;
	public Transform leftHandRep;
	public Transform rightHandRep;

	public Transform leftShoulderRep;
	public Transform rightShoulderRep;

	public Transform leftElbowRep;
	public Transform rightElbowRep;

	protected RigData rigData;


	/// <summary>
	/// describes how much effort is being put into rendering the player
	/// </summary>
	[Range(1, 3), Tooltip("describes how much effort is being put into rendering the player")]
	public int quality = 1;

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
