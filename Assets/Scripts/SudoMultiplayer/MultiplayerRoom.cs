using System.Collections.Generic;
using UnityEngine;

namespace SudoNetworking
{

	public class MultiplayerRoom : MonoBehaviour
	{
		public interface VRPLayer
		{
			PoseData Head { get; set; }
			PoseData Left { get; set; }
			PoseData Right { get; set; }
		}
		[System.Serializable]
		public struct PoseData
		{
			public static readonly PoseData empty = new PoseData(Vector3.zero, Quaternion.identity);
			Vector3 pos;
			Quaternion rot;

			public PoseData(Vector3 _pos, Quaternion _rot)
			{
				pos = _pos;
				rot = _rot;
			}

			public Vector3 GetPosition() => pos;
			public Quaternion GetRotation() => rot;

			public void SetPosition(Vector3 _pos) => pos = _pos;
			public void SetRotation(Quaternion _rot) => rot = _rot;
			public bool IsNull() => pos.Equals(Vector3.zero) && rot.Equals(Quaternion.identity);
		}

		[System.Serializable]
		public class RigData
		{
			public PoseData Head { get; set; }
			public PoseData? Left { get; set; }
			public PoseData? Right { get; set; }

			public PoseData? LeftShoulder { get; set; }
			public PoseData? RightShoulder { get; set; }

			public PoseData? LeftElbow { get; set; }
			public PoseData? RightElbow { get; set; }

			public RigData(PoseData _head)
			{
				Head = _head;
				Left = null;
				Right = null;

				LeftShoulder = null;
				RightShoulder = null;

				LeftElbow = null;
				RightElbow = null;

			}

			public RigData(PoseData _head, PoseData _left, PoseData _right)
			{
				Head = _head;
				Left = _left;
				Right = _right;

				LeftShoulder = null;
				RightShoulder = null;

				LeftElbow = null;
				RightElbow = null;

			}

			public RigData(PoseData _head, PoseData _left, PoseData _right,
				PoseData _leftShoulder, PoseData _rightShoulder,
				PoseData _leftElbow, PoseData _rightElbow)
			{
				Head = _head;
				Left = _left;
				Right = _right;

				LeftShoulder = _leftShoulder;
				RightShoulder = _rightShoulder;

				LeftElbow = _leftElbow;
				RightElbow = _rightElbow;
			}
		}

		public static MultiplayerRoom instance;
		public Dictionary<uint, RigData> rigsData;
		public List<Player> players;
		public List<uint> speakingPlayerIds;
		public uint teacherId = 0;

		protected virtual void Start()
		{
			players = new List<Player>();
			rigsData = new Dictionary<uint, RigData>();
		}

		private void OnEnable()
		{
			if (instance != this)
			{
				if (instance != null)
					enabled = false;
				else
					instance = this;
			}
		}

		private void Update()
		{
			if (NetworkingManager.IsMe())
			{

			}
		}


		public virtual void PlayerConnected(uint id)
		{
			var loadedPlayer = GetAvatarByQuality(1);
			Player instantiatedPlayer = NetworkingManager.Instantiate(loadedPlayer).GetComponent<Player>();
			instantiatedPlayer.id = id;

			players.Add(instantiatedPlayer);
			rigsData.Add(instantiatedPlayer.id, instantiatedPlayer.GetRigData());
			instantiatedPlayer.gameObject.AddComponent<StudentController>();


			//if seat system was implemented positon would be updated here 
			//insead it is implemnted in the override in MultiplayerRoomSimulator.cs
			//
			//instantiatedPlayer.transform.position = GetSeatPositon();
			//instantiatedPlayer.transform.localRotation *= GetSeatRotation();

		}
		public virtual void PlayerDisconnected(uint id)
		{
			foreach (NetworkedPlayer player in players)
			{
				if (player.id.Equals(id))
				{
					if (speakingPlayerIds.Contains(player.id))
						VoiceChatManager.RemoveVoiceConnection(player.id);

					players.Remove(player);
					rigsData.Remove(player.id);

					NetworkingManager.Destroy(player.gameObject);
					return;
				}

			}
		}


		public virtual void ChangePlayerQuality(uint id, int newQuality)
		{
			if (newQuality < 1)
				newQuality = 1;
			else if (newQuality > 3)
				newQuality = 3;

			var playerCurrent = GetPlayerByID(id);

			GameObject newPrefab = GetAvatarByQuality(newQuality);
			NetworkedPlayer newPlayer = NetworkingManager.Instantiate(newPrefab,
				playerCurrent.transform.position,
				playerCurrent.transform.rotation,
				playerCurrent.transform.parent).GetComponent<NetworkedPlayer>();

			newPlayer.gameObject.AddComponent<StudentController>();

			newPlayer.id = playerCurrent.id;
			newPlayer.quality = newQuality;
			newPlayer.isSpeaking = playerCurrent.isSpeaking;
			newPlayer.SetRigDataQuality(newQuality);
			players[players.IndexOf(playerCurrent)] = newPlayer;

			NetworkingManager.Destroy(playerCurrent.gameObject);
		}

		public virtual void GivePlayerVoice(uint id)
		{
			VoiceChatManager.InitializeVoiceConnection(id);

			speakingPlayerIds.Add(id);
			ChangePlayerQuality(id, 3);

			GetPlayerByID(id).isSpeaking = true;
		}
		public virtual void RemovePlayerVoice(uint id)
		{
			VoiceChatManager.RemoveVoiceConnection(id);

			speakingPlayerIds.Remove(id);
			ChangePlayerQuality(id, 1);

			GetPlayerByID(id).isSpeaking = false;
		}

		public virtual Vector3 GetSeatPositon(int seatIndex = -1)
		{
			throw new System.NotImplementedException();
		}
		public virtual Quaternion GetSeatRotation(int seatIndex = -1)
		{
			throw new System.NotImplementedException();
		}

		public static Player GetPlayerByID(uint id)
		{
			foreach (var player in instance.players)
			{
				if (player.id.Equals(id))
					return player;

			}
			Debug.LogError("No player with id " + id);
			return null;
		}

		public RigData GetRigDataFromID(uint id)
		{
			return rigsData[id];
		}

		/// <summary>
		/// returns whether the rigData was there to change
		/// </summary>
		public bool SetRigDataByID(uint id, RigData data)
		{
			if (!rigsData.ContainsKey(id))
				return false;

			rigsData[id] = data;
			return true;
		}

		public GameObject GetAvatarByQuality(int quality)
		{
			if (quality < 1)
				quality = 1;
			else if (quality > 3)
				quality = 3;

			return (GameObject)NetworkingManager.LoadNetworkedPrefab("NetworkedPlayerRepresentation_" + quality);
		}
	}

}