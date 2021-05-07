using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SudoNetworking.NetworkingData;

namespace SudoNetworking
{

	public static class NetworkingManager
	{
		public static Dictionary<int, RigData> rigsData = new Dictionary<int, RigData>();

		public static int nextID = 1;
		/// <summary>
		/// returns whether the rigData was there to change
		/// </summary>
		public static bool SetRigDataByID(int id, RigData data)
		{
			if (!rigsData.ContainsKey(id))
				return false;

			rigsData[id] = data;
			return true;
		}

		public static int GetNewPlayerID()
		{
			nextID++;
			return nextID - 1;

		}

		public static bool CreateRoom(string roomName)
		{
			//here would do netowrking to Create Room

			//insead just opening the simulator scene as teacher
			MultiplayerRoomSimulator.debugAsTeacher = true;
			SceneManager.LoadSceneAsync("NetworkSimulator");

			return true;
		}



		public static bool JoinRoom(string roomName)
		{
			//here would do netowrking to join shared scene and do wahtever 
			//catch up stuff is handled by the networking system

			//insead just opening the simulator scene as student
			MultiplayerRoomSimulator.debugAsTeacher = false;
			SceneManager.LoadSceneAsync("NetworkSimulator");

			return true;
		}

		/// <summary>
		/// returns whether object was succesfuly created localy ;)
		/// </summary>
		public static GameObject Instantiate(GameObject gameObject, Transform parent = null)
		{
			if (parent == null)
				return Object.Instantiate(gameObject);

			return Object.Instantiate(gameObject, parent);
		}



		/// <summary>
		/// returns whether object was succesfuly created localy ;) at specified position and rotation
		/// </summary>
		public static GameObject Instantiate(GameObject gameObject,
			Vector3 position,
			Quaternion rotation,
			Transform parent = null)
		{
			if (parent == null)
				return Object.Instantiate(gameObject, position, rotation);

			return Object.Instantiate(gameObject, position, rotation, parent);
		}

		public static bool IsMe() => true;
		public static void Destroy(Object gameObject)
		{
			GameObject.Destroy(gameObject);
		}

		public static Object LoadNetworkedPrefab(string path)
		{
			if (path.Contains(@"NetworkedPrefabs/"))
				return Resources.Load(path);

			return Resources.Load(@"NetworkedPrefabs/" + path);
		}
	}

	public static class VoiceChatManager
	{
		public static List<int> activeVoices = new List<int>();

		public static void InitializeVoiceConnection(int playerID)
		{
			activeVoices.Add(playerID);
		}

		public static void RemoveVoiceConnection(int playerID)
		{
			activeVoices.Remove(playerID);
		}



	}
}