using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SudoNetworking.MultiplayerRoom;

namespace SudoNetworking
{
	public class NetworkedPlayer : Player
	{
		/// <summary>
		/// describes how much effort is being put into rendering the player
		/// </summary>
		[Range(1, 3), Tooltip("describes how much effort is being put into rendering the player")]
		public int quality = 1;
		

		private void Update()
		{
			rigData = MultiplayerRoom.instance.GetRigDataFromID(id);
			SetTransformToPose(headRep, rigData.Head);
			SetTransformToPose(leftHandRep, rigData.Left);
			SetTransformToPose(rightHandRep, rigData.Right);
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