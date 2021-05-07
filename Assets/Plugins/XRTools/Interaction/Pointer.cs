using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XRTools.Actions;
using XRTools.Utils;
using static XRTools.Actions.PointerActions;

namespace XRTools.Interaction
{

	public class Pointer : MonoBehaviour
	{
		[System.Serializable]
		public enum LineType { Straight, Curved }
		public LineType pointerType;

		public PrimitiveType shapeType;

		[Tooltip("Default Unilt/Color \nReqiures _MainColor")]
		public Shader shaderOverride;

		public Color ValidColor = Color.green * new Color(1, .8f, 1, 1);
		public Color InvalidColor = Color.red * new Color(.8f, 1, 1, 1);
		public Color SelectingColor = Color.blue * new Color(1, 1, .8f, 1);

		public Vector3 relScale = new Vector3(.1f, .1f, 1f);
		public int numSegments = 9;
		public float width = .1f;

		public Vector3 MaxLength = new Vector3(10f, Mathf.Infinity, 0);
		public float heightLimitAngle = 10;

		[Range(0.001f, 1)]
		public float strength = 0.1f;
		public float curveOffset = 1;
		public Transform targetIdentifier;
		List<GameObject> segments;

		Material segmentMaterial;
		PrimitiveType prevType;
		LineType prevPointer;
		PointerAction action;

		public GetPointerEventDelegate getLineAction;

		private void OnEnable()
		{
			Application.onBeforeRender += BeforeRender;
			if (targetIdentifier)
				targetIdentifier.gameObject.SetActive(true);

			if (segments == null)
				segments = new List<GameObject>();

			if (prevType != shapeType || numSegments != segments.Count)
			{
				ResetSegments();
			}

			getLineAction = GetLineActionType;

			if (action != null)
			{
				action.Disable();
				action = null;
			}
			action = getLineAction(this);
			prevPointer = pointerType;


		}

		private void OnDisable()
		{
			Application.onBeforeRender -= BeforeRender;

			if (targetIdentifier)
				targetIdentifier.gameObject.SetActive(false);
		}

		private void Update()
		{
			if (numSegments < 1)
				numSegments = 1;
		}

		void BeforeRender()
		{
			if (action == null)
				return;
			Vector3 targetPoint = action.GetTargetPos();

			if (targetIdentifier)
				if (!targetPoint.Equals(Vector3.one * -1))
					targetIdentifier.position = targetPoint;
		}

		public Vector3? GetTargetPosition()
		{
			if (action == null)
				return null;
			Vector3 targetPos = action.GetTargetPos();

			return action.GetTargetPos();
		}

		/// <summary>
		/// global forward direction
		/// </summary>
		/// <returns></returns>
		public Vector3? GetTargetForward()
		{
			if (action == null)
				return null;

			return Vector3.forward;
		}


		public void FixedUpdate()
		{
			if (prevType != shapeType || numSegments != segments.Count)
			{
				ResetSegments();
				prevType = shapeType;
			}

			if (prevPointer != pointerType)
			{
				action.Disable();
				action = getLineAction(this);
				prevPointer = pointerType;
				return;
			}

			action.DrawSegments(ref segments);
			//targetIdentifier.position = action.GetTargetPos();
			//if (segments != null && segments.Count > 0)
			//{
			//Vector3 forward = PhysicsProject(transform.position, transform.forward);
			//GeneratePoints(forward, PhysicsProject(forward, Vector3.down));
			//}

			//if (segments != null && segments.Count > 0)
			//for (int i = 0; i < segments.Count; i++)
			//UpdateElement(linePoints, i, true, segments[i]);

		}

		void ResetSegments()
		{
			if (segments != null && segments.Count > 0)
				for (int i = 0; i < segments.Count; i += 0)
				{
					GameObject tempSegment = segments[0];
					segments.RemoveAt(0);
					Destroy(tempSegment);
				}
			if (shaderOverride == null)
				segmentMaterial = new Material(Shader.Find("Unlit/Color"));
			else
				segmentMaterial = new Material(shaderOverride);

			segmentMaterial.color = ValidColor;

			for (int i = 0; i < numSegments; i++)
			{
				var primitive = PrimitiveHelper.CreatePrimitive(shapeType, false);

				Transform segParent = new GameObject($"Segment {i}").transform;
				segParent.SetParent(transform);
				segParent.localPosition = Vector3.zero;

				primitive.transform.SetParent(segParent);

				primitive.GetComponent<MeshRenderer>().material = segmentMaterial;

				primitive.transform.forward =
					segParent.rotation * PointerUtils.GetPrimitiveForward(shapeType);

				primitive.transform.localScale = PointerUtils.GetPrimitiveScale(shapeType);

				segments.Add(segParent.gameObject);
			}
		}

		public virtual PointerAction GetLineActionType(Pointer pointer)
		{
			switch (pointer.pointerType)
			{
				case LineType.Straight:
					return new StraightLine(pointer.transform, pointer);
				case LineType.Curved:
					return new CurvedLine(pointer.transform, pointer);
			}
			return null;
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(Pointer), true)]
	public class PointerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
		}
	}


#endif
}