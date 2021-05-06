using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SudoNetworking.MultiplayerRoom;

namespace SudoNetworking
{
	public class LocalPlayer : Player
	{	
		public PoseData LeftShoulder { get; set; }
		public PoseData RightShoulder { get; set; }

		public PoseData LeftElbow { get; set; }
		public PoseData RightElbow { get; set; }
		

		private void Update()
		{
			UpdatePoses();
		}

		public void UpdatePoses()
		{

			rigData.Head.SetPosition(headRep.localPosition);
			rigData.Head.SetRotation(headRep.localRotation);

			rigData.Left.SetPosition(leftHandRep.localPosition);
			rigData.Left.SetRotation(leftHandRep.localRotation);

			rigData.Right.SetPosition(rightHandRep.localPosition);
			rigData.Right.SetRotation(rightHandRep.localRotation);
		}

		public void TransmitRigData(Vector3 playerPos, PoseData head, PoseData handL, PoseData handR)
		{
			MultiplayerRoom.instance.SetRigDataByID(id, rigData);
		}


	}

}