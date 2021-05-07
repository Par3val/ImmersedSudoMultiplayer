using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SudoNetworking;

[RequireComponent(typeof(Player))]
public class StudentController : MonoBehaviour
{

	public Player player;

	private void OnEnable()
	{
		player = GetComponent<Player>();
	}

	private void FixedUpdate()
	{
		if (!player.leftHandRep || !player.rightHandRep)
			return;

		bool leftAboveHead = player.headRep.position.y < player.leftHandRep.position.y;
		bool rightAboveHead = player.headRep.position.y < player.rightHandRep.position.y;
		if (leftAboveHead || rightAboveHead)
		{
			if (wasRaised && handRaised)
			{
				handRaised = false;
				wasRaised = false;
				TeacherController.RPCRemoveRequestVoice(player.id);
			}
			else if (handRaised)
			{
				return;
			}
			else
			{
				handRaised = true;
				TeacherController.RPCRequestVoice(player.id);
			}
		}
		else if (handRaised)
			wasRaised = true;
	}

	bool handRaised = false;
	bool wasRaised = false;
	public void RequestVoice()
	{

	}
}
