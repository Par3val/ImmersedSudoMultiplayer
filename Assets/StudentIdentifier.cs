using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SudoNetworking;
using XRTools.Interaction;
using XRTools.Input;

[RequireComponent(typeof(InteractableRaw))]
public class StudentIdentifier : MonoBehaviour
{
	[System.Serializable]
	public class StudentSelectEvent : UnityEvent<StudentController> { }

	public StudentController targetStudent;
	public LineRenderer visual;
	public float maxDistance = 100;
	public FloatMap selectMap;
	public StudentSelectEvent studentSelected = new StudentSelectEvent();
	InteractableRaw thisInteractable;
	RaycastHit hitData;

	private void OnEnable()
	{
		thisInteractable = GetComponent<InteractableRaw>();
		selectMap.Activated.AddListener(Select);

		thisInteractable.Grabbed.AddListener(Grabbed);
		thisInteractable.UnGrabbed.AddListener(UnGrabbed);

		if (!visual)
			visual = GetComponentInChildren<LineRenderer>();


		if (!selectMap)
			selectMap = GetComponentInChildren<FloatMap>();
	}

	private void OnDisable()
	{
		selectMap.Activated.RemoveListener(Select);

		thisInteractable.Grabbed.RemoveListener(Grabbed);
		thisInteractable.UnGrabbed.RemoveListener(UnGrabbed);
	}

	void Update()
	{
		if (visual)
			visual.SetPosition(1, new Vector3(0, 0, maxDistance));

		int layerMask = 0 << 2;
		layerMask = ~layerMask;
		if (Physics.Raycast(transform.position, transform.forward, out hitData, maxDistance, layerMask, QueryTriggerInteraction.Collide))
		{
			if (visual)
				visual.SetPosition(1, new Vector3(0, 0, hitData.distance));

			var possiblePlayer = hitData.collider.gameObject.GetComponentInParent<Player>();

			if (possiblePlayer)
			{
				if (possiblePlayer.GetComponent<StudentController>())
				{
					SetTarget(possiblePlayer.GetComponent<StudentController>());
				}

			}
			else
			{
				RemoveTarget();
			}
		}
		else if (targetStudent)
			RemoveTarget();
	}

	public void SetTarget(StudentController target)
	{
		if (target.Equals(targetStudent))
			return;

		if (targetStudent)
			RemoveTarget();

		targetStudent = target;
		targetStudent.gameObject.AddComponent<QuickOutline>();
	}

	public void RemoveTarget()
	{
		if (targetStudent)
			if (targetStudent.GetComponent<QuickOutline>())
				Destroy(targetStudent.GetComponent<QuickOutline>());
		targetStudent = null;
	}

	public void Grabbed(InteractorRaw interactor)
	{
		selectMap.hand = interactor.inputManager.inputType;
		selectMap.RefreshFeature();
	}

	public void UnGrabbed(InteractorRaw interactor)
	{
		selectMap.hand = UnityEngine.XR.XRNode.Head;
		selectMap.RefreshFeature();
	}

	public void Select()
	{
		Debug.Log("Selected");
		if (!targetStudent)
			return;
		GameObject selectedStudent = targetStudent.gameObject;
		//targetStudent = null;
		studentSelected.Invoke(targetStudent);

		var outline = selectedStudent.GetComponent<QuickOutline>();
		outline.OutlineColor = Color.red;
		outline.OutlineWidth *= 1.3f;

	}
}
