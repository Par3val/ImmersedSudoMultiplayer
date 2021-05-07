using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SudoNetworking
{

	public class LobbyManager : MonoBehaviour
	{

		private void OnEnable()
		{
			DontDestroyOnLoad(this);
		}
		public virtual void CreateRoom(string roomName = null)
		{
			NetworkingManager.CreateRoom(roomName);
		}

		public virtual void JoinRoom(string roomName)
		{
			NetworkingManager.JoinRoom(roomName);
		}
	}

}