using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRTools.Interaction;
using XRTools.Utils;

namespace XRTools.Actions
{

	public class PointerUtils
	{

		public const float adjustmentOffset = 0.0001f;

		public static Vector3 PhysicsProject(Vector3 origin, Vector3 dir, float maxLength = Mathf.Infinity, float heightLimitAngle = 10)
		{
			float rotation = Vector3.Dot(Vector3.up, dir.normalized);
			float length = maxLength;

			if ((rotation * 100f) > heightLimitAngle)
			{
				float controllerRotationOffset = 1f - (rotation - heightLimitAngle / 100f);
				length = maxLength * controllerRotationOffset * controllerRotationOffset;
			}

			Ray ray = new Ray(origin, dir);
			bool hasCollided = Physics.Raycast(ray, out RaycastHit hit, Physics.IgnoreRaycastLayer);

			// Adjust the cast length if something is blocking it.
			if (hasCollided && hit.distance < length)
			{
				length = hit.distance;
			}

			// Use an offset to move the point back and up a bit to prevent the cast clipping at the collision point.
			return ray.GetPoint(length - adjustmentOffset) + (Vector3.up * adjustmentOffset);

		}


		public static Vector3 GetPrimitiveForward(PrimitiveType shapeType)
		{
			switch (shapeType)
			{
				case PrimitiveType.Sphere:
					return Vector3.forward;

				case PrimitiveType.Capsule:
					return Vector3.up;

				case PrimitiveType.Cylinder:
					return Vector3.up;

				case PrimitiveType.Cube:
					return Vector3.forward;

				case PrimitiveType.Plane:
					return Vector3.forward;

				case PrimitiveType.Quad:
					return Vector3.forward;

			}
			return Vector3.forward;
		}

		public static Vector3 GetPrimitiveScale(PrimitiveType shapeType)
		{
			switch (shapeType)
			{
				case PrimitiveType.Sphere:
					return Vector3.one;

				case PrimitiveType.Capsule:
					return Vector3.one * .5f;

				case PrimitiveType.Cylinder:
					return new Vector3(1, .5f, 1);

				case PrimitiveType.Cube:
					return Vector3.one;

				case PrimitiveType.Plane:
					return Vector3.one * .1f;

				case PrimitiveType.Quad:
					return Vector3.one;

			}
			return Vector3.forward;
		}


		public static Vector3 GetAjacentPoint(Vector3[] points, int i)
		{
			if (i >= points.Length - 1)
				return points[i - 1];
			else
				return points[i + 1];
		}
		public static Vector3 GetScale(Vector3 startScale, Vector3 relScale, float scaleTarget, float width = 1)
		{
			for (int axis = 0; axis < 3; axis++)
			{
				if (Mathf.Abs(relScale[axis]) == 0)
					startScale[axis] = width;
				else
					startScale[axis] = relScale[axis] * scaleTarget;
			}

			return startScale;
		}


		public static Vector3 ProjectForward(Transform origin, float maxDistance = 10, float maxHeightAngle = 45)
		{
			float rotation = Vector3.Dot(Vector3.up, origin.transform.forward.normalized);
			float length = maxDistance;

			if ((rotation * 100f) > maxHeightAngle)
			{
				float controllerRotationOffset = 1f - (rotation - maxHeightAngle / 100f);
				length = maxDistance * controllerRotationOffset * controllerRotationOffset;
			}

			Ray ray = new Ray(origin.transform.position, origin.transform.forward);
			bool hasCollided = Physics.Raycast(ray, out RaycastHit hitData, length);

			// Adjust the cast length if something is blocking it.
			if (hasCollided && hitData.distance < length)
			{
				length = hitData.distance;
			}

			// Use an offset to move the point back and up a bit to prevent the cast clipping at the collision point.
			return ray.GetPoint(length - adjustmentOffset) + (Vector3.up * adjustmentOffset);
		}

		public static Vector3 ProjectDown(Vector3 downwardOrigin, float maxDistance = 10)
		{
			Vector3 point = Vector3.zero;
			Ray ray = new Ray(downwardOrigin, Vector3.down);

			if (Physics.Raycast(ray, out RaycastHit hitData, maxDistance))//|| (TargetHit?.collider != null && TargetHit.Value.collider != hitData.collider)
			{
				//TargetHit = null;
				point = ray.GetPoint(hitData.distance);
			}
			else
			{
				point = ray.GetPoint(0f);
				//TargetHit = hitData;
			}

			return point;
		}

	}

	public class PointerActions
	{
		public delegate PointerAction GetPointerEventDelegate(Pointer pointer);

		public class PointerAction : BasicAction
		{
			public Transform origin;
			public Pointer pointer;
			protected Vector3[] points;

			public PointerAction(Transform _origin, Pointer _pointer) : base(UpdateEvents.BeforeRender)
			{
				origin = _origin;
				pointer = _pointer;
				points = new Vector3[pointer.numSegments];
			}

			public override void Enable()
			{
				base.Enable();
			}

			public virtual Vector3 GetTargetPos()
			{ return Vector3.one * -1; }


			public virtual void DrawSegments(ref List<GameObject> segments)
			{
				if (points.Length != pointer.numSegments)
					points = new Vector3[pointer.numSegments];

				this.GetPoints(ref points);

				for (int i = 0; i < pointer.numSegments; i++)
				{
					Vector3 targetPoint = points[i];
					Vector3 ajacentPoint = PointerUtils.GetAjacentPoint(points, i);

					//rotation
					Vector3 relativeForward = ajacentPoint - targetPoint;
					//position
					Vector3 position = targetPoint + 0.5f * relativeForward;

					//scale
					float scaleTarget = Vector3.Distance(targetPoint, ajacentPoint);
					Vector3 scale = segments[i].transform.lossyScale;

					scale = PointerUtils.GetScale(scale, pointer.relScale, scaleTarget, pointer.width);



					segments[i].transform.position = position;
					segments[i].transform.forward = relativeForward;
					segments[i].transform.SetGlobalScale(scale);
				}
			}

			protected virtual void GetPoints(ref Vector3[] _points)
			{ }
		}


		public class StraightLine : PointerAction
		{
			public StraightLine(Transform _origin, Pointer _pointer) : base(_origin, _pointer)
			{

			}

			public override Vector3 GetTargetPos()
			{
				return PointerUtils.PhysicsProject(origin.position, origin.forward, pointer.MaxLength.x);
			}

			protected override void GetPoints(ref Vector3[] _points)
			{
				Debug.Log("Staight");
				var forward = PointerUtils.PhysicsProject(origin.position, origin.forward, pointer.MaxLength.x);
				float index = 0;
				for (int i = 0; i < _points.Length; i++)
				{
					index = i + (1 / (float)_points.Length) * .5f;
					_points[i] = Vector3.Lerp(pointer.transform.position, forward, index);

				}
			}
		}


		public class CurvedLine : PointerAction
		{
			int checkFrequency = 0;
			public CurvedLine(Transform _origin, Pointer _pointer) : base(_origin, _pointer)
			{

			}

			public override Vector3 GetTargetPos()
			{
				Vector3 farthestForward = PointerUtils.PhysicsProject(origin.position, origin.forward, pointer.MaxLength.x);
				Vector3 forward = PointerUtils.ProjectForward(pointer.transform, maxDistance: 10);
				return PointerUtils.ProjectDown(forward);
				//Vector3 farthestForward = PointerUtils.PhysicsProject(origin.position, origin.forward, pointer.MaxLength.x);
				//return PointerUtils.PhysicsProject(farthestForward, Vector3.down, pointer.MaxLength.y, 360);
			}

			protected override void GetPoints(ref Vector3[] _points)
			{
				Vector3 forward = PointerUtils.ProjectForward(pointer.transform, pointer.MaxLength.x, pointer.heightLimitAngle);
				Vector3 down = PointerUtils.ProjectDown(forward);
				GetPointsExpensive(ref _points, forward, down);
			}

			public Vector3[] GenerateBezier(Vector3 forwardPos, Vector3 downPos, float numSegments)
			{
				Vector3[] curvePoints = new Vector3[4];
				curvePoints[0] = pointer.transform.position;
				curvePoints[1] = forwardPos + (Vector3.up * pointer.curveOffset);
				curvePoints[2] = downPos;
				curvePoints[3] = downPos;

				return BezierCurveGenerator.GeneratePoints((int)numSegments, curvePoints);
			}

			protected virtual void GetPointsExpensive(ref Vector3[] _points, Vector3 forward, Vector3 down)
			{
				var bezierPoints = GenerateBezier(forward, down, pointer.numSegments);

				int step = (int)((float)pointer.numSegments / (checkFrequency > 0f ? checkFrequency : 1f));

				for (int i = 0; i < pointer.numSegments - step; i += step)
				{
					Vector3 currentPoint = points[i];
					Vector3 nextPoint = i + step < points.Length ? points[i + step] : points[points.Length - 1];
					Vector3 nextPointDirection = (nextPoint - currentPoint).normalized;
					float nextPointDistance = Vector3.Distance(currentPoint, nextPoint);

					Ray pointsRay = new Ray(currentPoint, nextPointDirection);

					if (!Physics.Raycast(pointsRay, out RaycastHit pointsHitData, nextPointDistance, Physics.IgnoreRaycastLayer))
					{
						continue;
					}

					Vector3 collisionPoint = pointsRay.GetPoint(pointsHitData.distance);
					Ray downwardRay = new Ray(collisionPoint + Vector3.up * 0.01f, Vector3.down);
					RaycastHit? targetHit;

					if (!Physics.Raycast(downwardRay, out RaycastHit downwardHitData, float.PositiveInfinity, Physics.IgnoreRaycastLayer))
					{
						targetHit = null;
						continue;
					}

					targetHit = downwardHitData;

					Vector3 newDownPosition = downwardRay.GetPoint(downwardHitData.distance);
					Vector3 newJointPosition = newDownPosition.y < forward.y ? new Vector3(newDownPosition.x, forward.y, newDownPosition.z) : forward;

					bezierPoints = GenerateBezier(newJointPosition, newDownPosition, pointer.numSegments);

					break;
				}

				for (int i = 0; i < bezierPoints.Length; i++)
				{
					_points[i] = bezierPoints[i];
				}

				//ResultsChanged?.Invoke(eventData.Set(TargetHit, IsTargetHitValid, Points));
			}

		}

	}

}