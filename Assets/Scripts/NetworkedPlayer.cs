using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SudoNetworking.MultiplayerRoom;

namespace SudoNetworking
{
	public class NetworkedPlayer : Player
	{
		public override void OnEnable()
		{
			rigData = new RigData(new PoseData(headRep.localPosition, headRep.localRotation));
		}

		private void Update()
		{
			rigData = MultiplayerRoom.instance.GetRigDataFromID(id);

			if (quality >= 1)
			{
				SetTransformToPose(headRep, rigData.Head);
			}
			if (quality >= 2)
			{
				SetTransformToPose(leftHandRep, rigData.Left.Value);
				SetTransformToPose(rightHandRep, rigData.Right.Value);
			}
			if (quality >= 3)
			{
				SetTransformToPose(leftShoulderRep, rigData.LeftShoulder.Value);
				SetTransformToPose(rightShoulderRep, rigData.RightShoulder.Value);
				SetTransformToPose(leftElbowRep, rigData.LeftElbow.Value);
				SetTransformToPose(rightElbowRep, rigData.RightElbow.Value);
			}
		}

		//public void ReceiveRigData(Vector3 playerPos, RigData _rigData)
		//{
		//	transform.position = playerPos;
		//	rigData = _rigData;
		//}

		public void SetRigDataQuality(int quality)
		{
			if (quality < 1)
				quality = 1;
			else if (quality > 3)
				quality = 3;

			if (quality == 1)
				rigData = new RigData(new PoseData(headRep.localPosition, headRep.localRotation));
			else if (quality == 2)
				rigData = new RigData(new PoseData(headRep.localPosition, headRep.localRotation),
							new PoseData(leftHandRep.localPosition, leftHandRep.localRotation),
							new PoseData(rightHandRep.localPosition, rightHandRep.localRotation));
			else if (quality == 3)
				rigData = new RigData(new PoseData(headRep.localPosition, headRep.localRotation),
							new PoseData(leftHandRep.localPosition, leftHandRep.localRotation),
							new PoseData(rightHandRep.localPosition, rightHandRep.localRotation));
			new RigData(new PoseData(headRep.localPosition, headRep.localRotation),
							new PoseData(leftHandRep.localPosition, leftHandRep.localRotation),
							new PoseData(rightHandRep.localPosition, rightHandRep.localRotation),
							new PoseData(leftShoulderRep.localPosition, leftShoulderRep.localRotation),
							new PoseData(rightShoulderRep.localPosition, rightShoulderRep.localRotation),
							new PoseData(leftElbowRep.localPosition, leftElbowRep.localRotation),
							new PoseData(rightElbowRep.localPosition, rightElbowRep.localRotation));


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