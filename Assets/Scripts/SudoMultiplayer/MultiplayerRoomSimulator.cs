using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SudoNetworking.NetworkingData;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace SudoNetworking
{
	public class MultiplayerRoomSimulator : MultiplayerRoom
	{
		public static int debugPlayerIndex = 0;
		public static bool debugAsTeacher = true;
		public Player DebugPlayer;
		public Transform spawnOrigin;
		public int numSimulatedStudents = 15;

		PoseData debugHeadPose;
		PoseData debugLeftPose;
		PoseData debugRightPose;


		protected override void Start()
		{
			if (debugAsTeacher)
				base.PlayerConnect(NetworkingManager.GetNewPlayerID(), true);
			else
				PlayerConnect(NetworkingManager.GetNewPlayerID(), true);

			DebugPlayer = players[players.Count - 1];

			int playerIndex = Random.Range(0, numSimulatedStudents);
			if (debugAsTeacher)
				playerIndex = -1;

			base.Start();
			for (int i = 1; i < numSimulatedStudents + 1; i++)
			{
				if (playerIndex.Equals(i))
				{
					base.PlayerConnect(NetworkingManager.GetNewPlayerID(), false);
					players[players.Count - 1].transform.position = GetSeatPositon();
					players[players.Count - 1].transform.rotation *= GetSeatRotation();
					DebugPlayer = players[players.Count - 1];
				}
				else
					PlayerConnect(NetworkingManager.GetNewPlayerID(), false);
			}
		}

		private void Awake()
		{
			//Resources.Load("SimulatorVRPlayer")
		}

		private void Update()
		{
			if (DebugPlayer)
			{
				RigData debugRigData = DebugPlayer.GetRigData();

				RigData tempData = new RigData(debugRigData.Head);
				foreach (Player player in players)
				{
					if (player.quality == 1)
					{
						tempData = new RigData(debugRigData.Head);
					}
					if (player.quality == 2)
					{
						tempData = new RigData(debugRigData.Head,
												debugRigData.Left,
												debugRigData.Right);
					}
					else if (player.quality == 3)
					{

						tempData = new RigData(debugRigData.Head,
												debugRigData.Left,
												debugRigData.Right,
												debugRigData.LeftShoulder,
												debugRigData.RightShoulder,
												debugRigData.LeftElbow,
												debugRigData.RightElbow);
					}
					NetworkingManager.SetRigDataByID(player.id, tempData);
				}
			}
		}

		public override void PlayerConnect(int id, bool isTeacher)
		{
			var loadedPlayer = GetSimulatedPlayer(isTeacher);
			Player instantiatedPlayer = NetworkingManager.Instantiate(loadedPlayer).GetComponent<Player>();
			instantiatedPlayer.id = NetworkingManager.GetNewPlayerID();
			teacherId = instantiatedPlayer.id;

			players.Add(instantiatedPlayer);
			NetworkingManager.rigsData.Add(instantiatedPlayer.id, instantiatedPlayer.GetRigData());

			//to be replaced by a seat system where the enviornement puts forth a list of available seats in the enviorment
			//and moves the player at an available seat or one they choose before/while connected

			if (!isTeacher)
			{
				players[players.Count - 1].transform.position = GetSeatPositon();
				players[players.Count - 1].transform.localRotation *= GetSeatRotation();
			}

		}


		public override void ChangePlayerQuality(int id, int newQuality)
		{
			if (newQuality < 1)
				newQuality = 1;
			else if (newQuality > 3)
				newQuality = 3;

			var playerCurrent = GetPlayerByID(id);

			GameObject newPrefab = GetSimulatedStudentAvatarByQuality(newQuality);
			NetworkedPlayerSimulator newPlayer = NetworkingManager.Instantiate(newPrefab,
				playerCurrent.transform.position,
				playerCurrent.transform.rotation,
				playerCurrent.transform.parent).GetComponent<NetworkedPlayerSimulator>();

			newPlayer.id = playerCurrent.id;
			newPlayer.quality = newQuality;
			newPlayer.isSpeaking = playerCurrent.isSpeaking;
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			players[players.IndexOf(playerCurrent)] = newPlayer;

			NetworkingManager.Destroy(playerCurrent.gameObject);
		}

		public override Vector3 GetSeatPositon(int seatIndex = -1)
		{
			if (seatIndex.Equals(-1))
			{
				//temp layout logic 
				int newPlayerID = players[players.Count - 1].id - 1;
				return spawnOrigin.position + new Vector3((newPlayerID % 15) * 1.8f, (newPlayerID / 15) * 0.2f, (newPlayerID / 15) * 2.2f);
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

		public static GameObject GetSimulatedPlayer(bool isTeacher, int quality = 1)
		{
			if (isTeacher)
				return (GameObject)Resources.Load("SimulatedTeacherPlayer");

			return GetSimulatedStudentAvatarByQuality(quality);
		}
		public static GameObject GetSimulatedStudentAvatarByQuality(int quality)
		{
			if (quality < 1)
				quality = 1;
			else if (quality > 3)
				quality = 3;

			return (GameObject)Resources.Load("SimulatedStudentPlayer_" + quality);
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
					simulator.PlayerConnect((int)MultiplayerRoom.players.Count + 1, false);
					previousNumStudents++;
				}

				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button(new GUIContent(" ConnectPlayer", "REALITY IS A LIE")))
				{
					simulator.PlayerConnect((int)MultiplayerRoom.players.Count + 1, false);
					simulator.numSimulatedStudents++;
				}

				if (GUILayout.Button("DisconnectPlayer"))
				{
					simulator.PlayerDisconnect((int)MultiplayerRoom.players.Count);

					simulator.numSimulatedStudents--;
				}
				EditorGUILayout.EndHorizontal();
			}
		}
	}

#endif
}
