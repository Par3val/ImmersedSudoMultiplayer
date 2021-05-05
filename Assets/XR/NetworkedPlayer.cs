using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SudoNetworking.MultiplayerRoomSimulator;
namespace SudoNetworking
{
	public class NetworkedPlayer : MonoBehaviour
	{
		public uint id;

		public Transform headRep;
		public Transform leftHandRep;
		public Transform rightHandRep;

		private void OnEnable()
		{

		}

		public void ReceiveRigData(Vector3 playerPos, PoseData head, PoseData handL, PoseData handR)
		{
			transform.position = playerPos;
			SetTransformToPose(headRep, head);
			SetTransformToPose(leftHandRep, handL);
			SetTransformToPose(rightHandRep, handR);
		}

		public void SetTransformToPose(Transform target, PoseData pose)
		{
			if (pose.IsNull())
				return;

			target.localPosition = pose.GetPosition();
			target.localRotation = pose.GetRotation();
		}
	}

}