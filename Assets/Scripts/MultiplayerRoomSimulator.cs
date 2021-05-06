using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace SudoNetworking
{
	public class MultiplayerRoomSimulator : MultiplayerRoom
	{
		public static uint debugPlayerIndex = 0;

		public XRTools.Rigs.Rig debugRig;
		public Transform spawnOrigin;
		public int numSimulatedStudents = 15;

		PoseData debugHeadPose;
		PoseData debugLeftPose;
		PoseData debugRightPose;

		protected override void Start()
		{
			base.Start();
			for (uint i = 1; i < numSimulatedStudents + 1; i++)
			{
				PlayerConnected(i);
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
					SetRigDataByID(player.id, new RigData(debugHeadPose, debugLeftPose, debugRightPose));
				}
			}
		}

		public override void PlayerConnected(uint id)
		{
			base.PlayerConnected(id);

			//to be replaced by a seat system where the enviornement puts forth a list of available seats in the enviorment
			//and moves the player at an available seat or one they choose before connecting


			players[players.Count - 1].transform.position = GetSeatPositon();
			players[players.Count - 1].transform.localRotation *= GetSeatRotation();

		}

		public override Vector3 GetSeatPositon(int seatIndex = -1)
		{
			if (seatIndex.Equals(-1))
			{
				//temp layout logic 
				uint newPlayerID = players[players.Count - 1].id;
				return spawnOrigin.position + new Vector3((newPlayerID % 5) * 1.8f, (newPlayerID / 5) * 0.2f, (newPlayerID / 5) * 2.2f);
			}


			Debug.LogError($"Seat Index System Not Implemented (expected -1 received {seatIndex})", gameObject);
			return Vector3.zero;
		}
		public override Quaternion GetSeatRotation(int seatIndex = -1)
		{
			if (seatIndex.Equals(-1))
			{
				//temp locgic to spin the simulations around
				return Quaternion.Euler(new Vector3(0, 180, 0));
			}


			Debug.LogError($"Seat Index System Not Implemented (expected -1 received {seatIndex})", gameObject);
			return Quaternion.identity;
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(MultiplayerRoomSimulator))]
	public class MultiplayerRoomSimulatorEditor : Editor
	{
		int previousNumStudents = 0;

		private void OnEnable()
		{
			previousNumStudents = ((MultiplayerRoomSimulator)target).numSimulatedStudents;
		}
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			MultiplayerRoomSimulator simulator = (MultiplayerRoomSimulator)target;

			if (Application.isPlaying)
			{
				while (!previousNumStudents.Equals(simulator.numSimulatedStudents))
				{
					simulator.PlayerConnected((uint)simulator.players.Count + 1);
					previousNumStudents++;
				}

				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button(new GUIContent(" ConnectPlayer", "REALITY IS A LIE")))
				{
					simulator.PlayerConnected((uint)simulator.players.Count + 1);
					simulator.numSimulatedStudents++;
				}

				if (GUILayout.Button("DisconnectPlayer"))
				{
					simulator.PlayerDisconnected((uint)simulator.players.Count);

					simulator.numSimulatedStudents--;
				}
				EditorGUILayout.EndHorizontal();
			}
		}
	}

#endif
}
