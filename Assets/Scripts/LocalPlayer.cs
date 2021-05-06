using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SudoNetworking.MultiplayerRoom;

namespace SudoNetworking
{
	public class LocalPlayer : Player
	{
		public override void OnEnable()
		{
			rigData = new RigData(new PoseData(headRep.localPosition, headRep.localRotation),
							new PoseData(leftHandRep.localPosition, leftHandRep.localRotation),
							new PoseData(rightHandRep.localPosition, rightHandRep.localRotation),
							new PoseData(leftShoulderRep.localPosition, leftShoulderRep.localRotation),
							new PoseData(rightShoulderRep.localPosition, rightShoulderRep.localRotation),
							new PoseData(leftElbowRep.localPosition, leftElbowRep.localRotation),
							new PoseData(rightElbowRep.localPosition, rightElbowRep.localRotation));
		}

		public override RigData GetRigData()
		{
			return rigData;
		}

		private void Update()
		{
			UpdatePoses();
			//TransmitRigData();
		}

		public void UpdatePoses()
		{
			if (quality > 2)
			{
				rigData.LeftShoulder.Value.SetPosition(leftShoulderRep.localPosition);
				rigData.LeftShoulder.Value.SetRotation(leftShoulderRep.localRotation);

				rigData.RightShoulder.Value.SetPosition(rightShoulderRep.localPosition);
				rigData.RightShoulder.Value.SetRotation(rightShoulderRep.localRotation);

				rigData.LeftElbow.Value.SetPosition(leftElbowRep.localPosition);
				rigData.LeftElbow.Value.SetRotation(leftElbowRep.localRotation);

				rigData.RightElbow.Value.SetPosition(rightElbowRep.localPosition);
				rigData.RightElbow.Value.SetRotation(rightElbowRep.localRotation);
			}
			if (quality > 1)
			{
				rigData.Left.Value.SetPosition(leftHandRep.localPosition);
				rigData.Left.Value.SetRotation(leftHandRep.localRotation);

				rigData.Right.Value.SetPosition(rightHandRep.localPosition);
				rigData.Right.Value.SetRotation(rightHandRep.localRotation);
			}

			rigData.Head.SetPosition(headRep.localPosition);
			rigData.Head.SetRotation(headRep.localRotation);
		}

		public void TransmitRigData()
		{
			MultiplayerRoom.instance.SetRigDataByID(id, rigData);
		}


	}

}