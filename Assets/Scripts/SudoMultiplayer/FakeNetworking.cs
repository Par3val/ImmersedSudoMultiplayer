using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class NetworkingManager
{
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
	public static List<uint> activeVoices = new List<uint>();

	public static void InitializeVoiceConnection(uint playerID)
	{
		activeVoices.Add(playerID);
	}

	public static void RemoveVoiceConnection(uint playerID)
	{
		activeVoices.Remove(playerID);
	}



}