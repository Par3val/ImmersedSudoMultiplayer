using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.XR;
using UnityEngine.XR;

namespace XRTools
{

	public class PlayAreaRepresentaton : MonoBehaviour
	{
		public LineRenderer representation;

		List<Vector3> boundaryPoints;

		private void OnEnable()
		{
			boundaryPoints = new List<Vector3>();
		}
		
		public bool DrawPlayArea(XRInputSubsystem subsystem)
		{
			if (subsystem != null)
				if (subsystem.TryGetBoundaryPoints(boundaryPoints))
				{
					Debug.Log($"Got {boundaryPoints.Count} boundary points");
					if (boundaryPoints.Count > 0)
					{

						ReadBoundPoints();
						return true;
					}
				}
			return false;
		}

		void ReadBoundPoints()
		{
			representation.positionCount = boundaryPoints.Count;

			Vector3[] levelPoints = new Vector3[boundaryPoints.Count];

			for (int i = 0; i < boundaryPoints.Count; i++)
			{
				levelPoints[i] = boundaryPoints[i];
				levelPoints[i].y = 0;
			}

			representation.SetPositions(levelPoints);
		}

	}

}
//#if UNITY_2020_2_OR_NEWER

//#else

//		public Vector3 currentPlayAreaDimetion
//		{
//			get
//			{
//				if (Boundary.configured)
//					if (Boundary.TryGetDimensions(out Vector3 dimensions))
//						return dimensions;
//				return Vector3.zero;
//			}
//		}
//		public static Vector3 GetPlayAreaDimention(Boundary.Type type = Boundary.Type.PlayArea)
//		{
//			if (Boundary.TryGetDimensions(out Vector3 dimensions, type))
//				return dimensions;
//			return Vector3.zero;
//		}

//#endif