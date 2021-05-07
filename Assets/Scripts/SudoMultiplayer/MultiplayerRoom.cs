using System.Collections.Generic;
using UnityEngine;
using static SudoNetworking.NetworkingData;

namespace SudoNetworking
{

	public class MultiplayerRoom : MonoBehaviour
	{

		public static List<Player> players = new List<Player>();
		public static List<int> speakingPlayerIds = new List<int>();

		string roomName;

		public static Player GetPlayerByID(int id)
		{
			foreach (var player in players)
			{
				if (player.id.Equals(id))
					return player;

			}
			Debug.LogError("No player with id " + id);
			return null;
		}

		public static RigData GetRigDataFromID(int id)
		{
			return NetworkingManager.rigsData[id];
		}

		public static MultiplayerRoom instance;
		public int teacherId = 0;

		protected virtual void Start()
		{
			//players = new List<Player>();
			//rigsData = new Dictionary<int, RigData>();
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


		public virtual void PlayerConnect(int Player, bool isTeacher)
		{

			var loadedPlayer = GetPlayer(isTeacher);
			Player instantiatedPlayer = NetworkingManager.Instantiate(loadedPlayer).GetComponent<Player>();
			instantiatedPlayer.id = NetworkingManager.GetNewPlayerID();
			teacherId = instantiatedPlayer.id;

			players.Add(instantiatedPlayer);
			NetworkingManager.rigsData.Add(instantiatedPlayer.id, instantiatedPlayer.GetRigData());

			//if seat system was implemented positon would be updated here 
			//insead it is implemnted in the override in MultiplayerRoomSimulator.cs
			//
			//instantiatedPlayer.transform.position = GetSeatPositon();
			//instantiatedPlayer.transform.localRotation *= GetSeatRotation();

		}


		public virtual void PlayerDisconnect(int id)
		{
			Player player = GetPlayerByID(id);
			if (speakingPlayerIds.Contains(player.id))
				VoiceChatManager.RemoveVoiceConnection(player.id);

			players.Remove(player);
			NetworkingManager.rigsData.Remove(player.id);

			NetworkingManager.Destroy(player.gameObject);
			return;
		}


		public virtual void ChangePlayerQuality(int id, int newQuality)
		{
			Debug.Log("Lets Change Quality");
			if (newQuality < 1)
				newQuality = 1;
			else if (newQuality > 3)
				newQuality = 3;

			var playerCurrent = GetPlayerByID(id);

			GameObject newPrefab = GetStudentAvatarByQuality(newQuality);
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

		public virtual void GivePlayerVoice(int id)
		{
			VoiceChatManager.InitializeVoiceConnection(id);

			speakingPlayerIds.Add(id);
			ChangePlayerQuality(id, speakingPlayerIds.Count > 5 ? 2 : 3);

			GetPlayerByID(id).isSpeaking = true;
		}
		public virtual void RemovePlayerVoice(int id)
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

		public void RequestVoiceToggle(int id)
		{
			if (teacherId != -1)
			{
				var teacherPlayer = GetPlayerByID(teacherId);
			}
		}

		public static GameObject GetPlayer(bool isTeacher, int quality = 1)
		{
			if (isTeacher)
				return (GameObject)NetworkingManager.LoadNetworkedPrefab("TeacherPlayer");

			return GetStudentAvatarByQuality(quality);
		}
		public static GameObject GetStudentAvatarByQuality(int quality)
		{
			if (quality < 1)
				quality = 1;
			else if (quality > 3)
				quality = 3;

			return (GameObject)NetworkingManager.LoadNetworkedPrefab("StudentPlayer_" + quality);
		}
	}

}