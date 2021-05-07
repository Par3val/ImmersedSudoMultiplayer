using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SudoNetworking;
public class TeacherController : MonoBehaviour
{
	public static TeacherController instance;
	public StudentIdentifier voiceWand;
	List<int> studentsRequesting;

	private void OnEnable()
	{
		if (instance != this)
		{
			if (instance != null)
				enabled = false;
			else
				instance = this;
		}

		var identifiers = FindObjectsOfType<StudentIdentifier>();

		//foreach (var identifier in identifiers)
		//{
		//	if (identifier.name.ToLower().Contains("voice"))
		//		voiceWand = identifier;
		//}
		GameObject wand = (GameObject)NetworkingManager.LoadNetworkedPrefab("StudentSelectWand");
		voiceWand = NetworkingManager.Instantiate(wand).GetComponent<StudentIdentifier>();

		if (voiceWand)
		{
			voiceWand.studentSelected.AddListener(StudentSelectedForVoice);
		}
		studentsRequesting = new List<int>();
	}
	private void OnDisable()
	{
		if (voiceWand)
			voiceWand.studentSelected.RemoveListener(StudentSelectedForVoice);

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

	public void GiveStudentVoice(int studentId)
	{
		MultiplayerRoom.instance.GivePlayerVoice(studentId);
	}

	public void RemoveStudentVoice(int studentId)
	{
		MultiplayerRoom.instance.RemovePlayerVoice(studentId);
	}

	public static void RPCRequestVoice(int id)
	{
		if (!instance.studentsRequesting.Contains(id))
			instance.studentsRequesting.Add(id);

		var player = MultiplayerRoom.GetPlayerByID(id);
	}

	public static void RPCRemoveRequestVoice(int id)
	{
		if (instance.studentsRequesting.Contains(id))
			instance.studentsRequesting.Remove(id);
	}


	IEnumerator ToggleOnOff(GameObject target)
	{
		target.SetActive(false);
		yield return new WaitForSeconds(0.5f);

		target.SetActive(true);
	}
}
