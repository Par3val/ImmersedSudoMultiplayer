using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SudoNetworking
{
	public class MultiplayerRoomSimulator : MultiplayerRoom
	{
		public struct PoseData
		{
			Vector3 pos;
			Quaternion rot;

			public PoseData(Vector3 _pos, Quaternion _rot)
			{
				pos = _pos;
				rot = _rot;
			}

			public Vector3 GetPosition() => pos;
			public Quaternion GetRotation() => rot;
			public bool IsNull() => pos.Equals(Vector3.zero) && rot.Equals(Quaternion.identity);
		}

		public XRTools.Rigs.Rig debugRig;
		public Transform spawnOrigin;
		public int numSimulatedStudents = 15;

		PoseData debugHeadPose;
		PoseData debugLeftPose;
		PoseData debugRightPose;

		private void Start()
		{
			players = new List<NetworkedPlayer>();

			for (uint i = 1; i < numSimulatedStudents + 1; i++)
			{
				AddPlayer(i);
			}
		}

		private void Update()
		{
			if (debugRig)
			{
				debugHeadPose = new PoseData(debugRig.Headset.transform.localPosition, debugRig.Headset.transform.localRotation);
				debugLeftPose = new PoseData(debugRig.LeftController.transform.localPosition, debugRig.LeftController.transform.localRotation);
				debugRightPose = new PoseData(debugRig.RightController.transform.localPosition, debugRig.RightController.transform.localRotation);


				foreach (var player in players)
				{
					player.ReceiveRigData(player.transform.position, debugHeadPose, debugLeftPose, debugRightPose);
				}
			}
		}

		public override void AddPlayer(uint id)
		{
			base.AddPlayer(id);

			players[players.Count-1].transform.position = spawnOrigin.position + new Vector3((id % 5) * 1.8f, (id / 5) * 0.2f, (id / 5) * 2.2f);
			players[players.Count-1].transform.eulerAngles += new Vector3(0, 180, 0);

		}
	}

}