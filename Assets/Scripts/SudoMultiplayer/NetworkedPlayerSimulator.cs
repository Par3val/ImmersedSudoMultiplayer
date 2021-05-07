using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SudoNetworking.NetworkingData;

namespace SudoNetworking
{
	public class NetworkedPlayerSimulator : Player
	{

		private void Update()
		{
			var rigData = MultiplayerRoom.GetRigDataFromID(id);


			if (rigData.quality >= 1)
			{
				SetTransformToPose(headRep, rigData.Head);
			}
			if (rigData.quality >= 2)
			{
				SetTransformToPose(leftHandRep, rigData.Left);
				SetTransformToPose(rightHandRep, rigData.Right);
			}
			if (rigData.quality >= 3)
			{
				SetTransformToPose(leftShoulderRep, rigData.LeftShoulder);
				SetTransformToPose(rightShoulderRep, rigData.RightShoulder);
				SetTransformToPose(leftElbowRep, rigData.LeftElbow);
				SetTransformToPose(rightElbowRep, rigData.RightElbow);
			}
		}

		//public void ReceiveRigData(Vector3 playerPos, RigData _rigData)
		//{
		//	transform.position = playerPos;
		//	rigData = _rigData;
		//}
		
		public void SetTransformToPose(Transform target, PoseData pose)
		{
			if (pose.IsNull())
				return;

			target.localPosition = pose.GetPosition();
			target.localRotation = pose.GetRotation();
		}

	}

}