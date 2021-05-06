using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SudoNetworking;
public class TeacherController : MonoBehaviour
{
	public StudentIdentifier voiceWand;
	public StudentIdentifier hotSeatWand;

	private void OnEnable()
	{
		voiceWand.studentSelected.AddListener(StudentSelectedForVoice);
	}
	private void OnDisable()
	{

	}

	public void StudentSelectedForVoice(StudentController student)
	{
		if (!student)
			return;

		if (!student.GetComponent<Player>().isSpeaking)
			GiveStudentVoice(student.GetComponent<Player>().id);
		else
			RemoveStudentVoice(student.GetComponent<Player>().id);
	}

	public void StudentSelectedForHotSeat(StudentController student)
	{
		if (!student)
			return;

		if (!student.GetComponent<Player>().isSpeaking)
			GiveStudentVoice(student.GetComponent<Player>().id);
		else
			RemoveStudentVoice(student.GetComponent<Player>().id);
	}


	public void GiveStudentVoice(uint studentId)
	{
		MultiplayerRoom.instance.GivePlayerVoice(studentId);
	}

	public void RemoveStudentVoice(uint studentId)
	{
		MultiplayerRoom.instance.RemovePlayerVoice(studentId);
	}
}
