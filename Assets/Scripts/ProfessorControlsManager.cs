using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SudoNetworking;
public class ProfessorControlsManager : MonoBehaviour
{
    public void GiveStudentVoice(uint studentId)
	{
		MultiplayerRoom.instance.GivePlayerVoice(studentId);
	}

	public void RemoveStudentVoice(uint studentId)
	{
		MultiplayerRoom.instance.RemovePlayerVoice(studentId);
	}
}
