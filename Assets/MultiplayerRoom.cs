using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SudoNetworking
{

	public class MultiplayerRoom : MonoBehaviour
	{
		public List<NetworkedPlayer> players;
		public List<int> speakingPlayerIds;


		public virtual void AddPlayer(uint id)
		{
			var loadedPlayer = Resources.Load<NetworkedPlayer>(@"NetworkedPrefabs/NetworkedPlayerRepresentation");
			NetworkedPlayer instantiatedPlayer = Instantiate(loadedPlayer);
			instantiatedPlayer.id = id;
			players.Add(instantiatedPlayer);
		}
	}

}