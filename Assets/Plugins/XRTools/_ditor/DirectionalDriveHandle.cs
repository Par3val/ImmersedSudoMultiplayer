using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using XRTools.Utils;
using XRTools.Interaction.Actions;
using XRTools.Utils.Data;
using UnityEditor.IMGUI.Controls;

namespace XRTools.Handle
{

	public class DirectionalDriveBoundsHandle
	{
		private static readonly int[] s_NextAxis = new[] { 1, 2, 0 };

		internal static GUIContent editModeButton
		{
			get
			{
				if (s_EditModeButton == null)
				{
					s_EditModeButton = new GUIContent(
						EditorGUIUtility.IconContent("EditCollider").image,
						EditorGUIUtility.TrTextContent("Edit bounding volume.\n\n - Hold Alt after clicking control handle to pin center in place.\n - Hold Shift after clicking control handle to scale uniformly.").text
					);
				}
				return s_EditModeButton;
			}
		}
		private static GUIContent s_EditModeButton;

		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~+X,-X,+Y,-Y,+z,-Z
		private int[] controlIDs = new int[6] { 0, 0, 0, 0, 0, 0 };
		int activeID = -1;
		protected enum SideDirs
		{
			PosX,
			NegX,
			PosY,
			NegY,
			PosZ,
			NegZ
		}

		public Bounds startBounds;
		public Bounds liveBounds;
		public Vector3 minPos;
		public Vector3 maxPos;
		public Axis axes { get; set; }
		bool beingChanged;

		static readonly Bounds nullBounds = new Bounds(Vector3.zero, Vector3.zero);
		public Handles.SizeFunction midpointHandleSizeFunction { get; set; }

		public DirectionalDriveBoundsHandle()
		{
			axes = Axis.X | Axis.Y | Axis.Z;

			for (int i = 0, count = controlIDs.Length; i < count; ++i)
				controlIDs[i] = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);

			liveBounds = nullBounds;
		}

		public bool IsIDActive(int id = -1)
		{
			if (id != -1)
				return id == GUIUtility.hotControl;

			foreach (var _id in controlIDs)
				if (_id == GUIUtility.hotControl)
					return true;

			return false;

		}

		public Bounds DrawHandle(Vector3 center, Quaternion rotation, Vector3 range, Axis axis)
		{
			//for (int i = 0, count = controlIDs.Length; i < count; ++i)
			//	controlIDs[i] = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
			if (beingChanged && GUIUtility.hotControl == 0)
			{
				beingChanged = false;
				Bounds finalBounds = liveBounds;
				liveBounds.center = Vector3.zero;
				startBounds = liveBounds;
				return finalBounds;
			}

			if (liveBounds.Equals(nullBounds) || GUIUtility.hotControl == 0)
			{
				liveBounds = new Bounds(Vector3.zero, range);
				startBounds = liveBounds;
				minPos = -liveBounds.size;
				maxPos = liveBounds.size;
			}

			axes = axis;

			// unless holding alt to pin center, exit before drawing control handles when holding alt, since alt-click will rotate scene view
			if (Event.current.alt)
			{
				if (!IsIDActive())
					return new Bounds(center, range);
			}


			int prevHotControl = GUIUtility.hotControl;

			using (new Handles.DrawingScope(Matrix4x4.TRS(center, rotation, Vector3.one)))
			{
				DrawWireframe(liveBounds.center, rotation, liveBounds.size);

				bool isCameraInsideBox = Camera.current != null
					&& liveBounds.Contains(Handles.inverseMatrix.MultiplyPoint(Camera.current.transform.position));

				EditorGUI.BeginChangeCheck();

				MidpointHandles(ref minPos, ref maxPos, rotation, isCameraInsideBox);

			}


			if (prevHotControl != GUIUtility.hotControl && GUIUtility.hotControl != 0)
			{
				startBounds = liveBounds;
				beingChanged = true;
			}

			if (EditorGUI.EndChangeCheck())
			{
				//shift scales from the center
				if (Event.current.shift)
				{
					liveBounds.center = Vector3.zero;
					liveBounds.size = (maxPos - minPos) / 2;
					minPos = -liveBounds.size;
					maxPos = liveBounds.size;
				}
				else
				{
					liveBounds.center = ((minPos - -startBounds.size) + (maxPos - startBounds.size)) / 2;
					liveBounds.size = Vector3Utils.Distance(minPos, maxPos) / 2;
					Handles.CubeHandleCap(-1, liveBounds.center, rotation, .1f, EventType.Repaint);

				}

				if (Event.current.alt)
					UniformScale();

			}

			return new Bounds(center, range);
		}

		public void UniformScale()
		{
			int hotControl = GUIUtility.hotControl;
			Vector3 size = liveBounds.size;
			int scaleAxis = 0;
			if (hotControl == controlIDs[(int)SideDirs.PosY] || hotControl == controlIDs[(int)SideDirs.NegY])
			{
				scaleAxis = 1;
			}
			if (hotControl == controlIDs[(int)SideDirs.PosZ] || hotControl == controlIDs[(int)SideDirs.NegZ])
			{
				scaleAxis = 2;
			}

			if (Mathf.Approximately(startBounds.size[scaleAxis], 0f))
			{
				if (startBounds.size == Vector3.zero)
					size = Vector3.one * size[scaleAxis];
			}
			else
			{
				var scaleFactor = size[scaleAxis] / startBounds.size[scaleAxis];
				var nextAxis = s_NextAxis[scaleAxis];
				size[nextAxis] = scaleFactor * startBounds.size[nextAxis];
				nextAxis = s_NextAxis[nextAxis];
				size[nextAxis] = scaleFactor * startBounds.size[nextAxis];
			}
			liveBounds.size = size;
		}

		public void Alt()
		{
		}


		void DrawWireframe(Vector3 pos, Quaternion rot, Vector3 scale)
		{
			Color startColor = Handles.color;

			scale.x *= axes.HasFlag(Axis.X) ? 1 : 0;
			scale.y *= axes.HasFlag(Axis.Y) ? 1 : 0;
			scale.z *= axes.HasFlag(Axis.Z) ? 1 : 0;

			Vector3[] points = new Vector3[10];

			points[0] = pos + /*rot * */new Vector3(-scale.x, -scale.y, -scale.z);
			points[1] = pos + /*rot * */new Vector3(-scale.x, scale.y, -scale.z);

			points[2] = pos + /*rot * */new Vector3(scale.x, scale.y, -scale.z);
			points[3] = pos + /*rot * */new Vector3(scale.x, -scale.y, -scale.z);

			points[4] = pos + /*rot * */new Vector3(-scale.x, -scale.y, -scale.z);
			points[5] = pos + /*rot * */new Vector3(-scale.x, -scale.y, scale.z);

			points[6] = pos + /*rot * */new Vector3(-scale.x, scale.y, scale.z);
			points[7] = pos + /*rot * */new Vector3(scale.x, scale.y, scale.z);

			points[8] = pos + /*rot * */new Vector3(scale.x, -scale.y, scale.z);
			points[9] = pos + /*rot * */new Vector3(-scale.x, -scale.y, scale.z);

			//Handles.DrawPolyLine(points);

			//Handles.color = Color.red;
			//+X
			Handles.DrawLine(points[1], points[2]);
			Handles.DrawLine(points[6], points[7]);
			//-X
			Handles.DrawLine(points[3], points[4]);
			Handles.DrawLine(points[8], points[9]);


			//Handles.color = Color.green;
			//+Y
			Handles.DrawLine(points[2], points[3]);
			Handles.DrawLine(points[7], points[8]);
			//-Y
			Handles.DrawLine(points[5], points[6]);
			Handles.DrawLine(points[0], points[1]);

			//Handles.color = Color.blue;
			//+Z
			Handles.DrawLine(points[1], points[6]);
			Handles.DrawLine(points[4], points[5]);
			//-Z
			Handles.DrawLine(points[3], points[8]);
			Handles.DrawLine(points[2], points[7]);

			Handles.color = startColor;
		}


		protected virtual Bounds OnHandleChanged(SideDirs handle, Bounds boundsOnClick, Bounds newBounds)
		{
			return newBounds;
		}

		private void MidpointHandles(ref Vector3 minPos, ref Vector3 maxPos, Quaternion rot, bool isCameraInsideBox)
		{
			Color startColor = Handles.color;


			Vector3 xAxis = Vector3.right;
			Vector3 yAxis = Vector3.up;
			Vector3 zAxis = Vector3.forward;
			Vector3 middle = Vector3.zero;

			Vector3 localPos, newPos;
			if (axes.HasFlag(Axis.X))
			{
				Handles.color = Color.red;
				// +X
				localPos = new Vector3(maxPos.x, middle.y, middle.z);
				newPos = MidpointHandle(controlIDs[(int)SideDirs.PosX], localPos, yAxis, zAxis, Mathf.Min(maxPos.y, maxPos.z), isCameraInsideBox);
				maxPos.x = Mathf.Max(newPos.x, minPos.x);
				//Debug.Log($"{localPos} {newPos}");
				// -X
				localPos = new Vector3(minPos.x, middle.y, middle.z);
				newPos = MidpointHandle(controlIDs[(int)SideDirs.NegX], localPos, yAxis, -zAxis, Mathf.Min(maxPos.y, maxPos.z), isCameraInsideBox);
				minPos.x = Mathf.Min(newPos.x, maxPos.x);
			}

			if (axes.HasFlag(Axis.Y))
			{
				Handles.color = Color.green;
				// +Y
				localPos = new Vector3(middle.x, maxPos.y, middle.z);
				newPos = MidpointHandle(controlIDs[(int)SideDirs.PosY], localPos, xAxis, -zAxis, Mathf.Min(maxPos.x, maxPos.z), isCameraInsideBox);
				maxPos.y = Mathf.Max(newPos.y, minPos.y);

				// -Y
				localPos = new Vector3(middle.x, minPos.y, middle.z);
				newPos = MidpointHandle(controlIDs[(int)SideDirs.NegY], localPos, xAxis, zAxis, Mathf.Min(maxPos.x, maxPos.z), isCameraInsideBox);
				minPos.y = Mathf.Min(newPos.y, maxPos.y);
			}

			if (axes.HasFlag(Axis.Z))
			{
				Handles.color = Color.blue;
				// +Z
				localPos = new Vector3(middle.x, middle.y, maxPos.z);
				newPos = MidpointHandle(controlIDs[(int)SideDirs.PosZ], localPos, yAxis, -xAxis, Mathf.Min(maxPos.x, maxPos.y), isCameraInsideBox);
				maxPos.z = Mathf.Max(newPos.z, minPos.z);

				// -Z
				localPos = new Vector3(middle.x, middle.y, minPos.z);
				newPos = MidpointHandle(controlIDs[(int)SideDirs.NegZ], localPos, yAxis, xAxis, Mathf.Min(maxPos.x, maxPos.y), isCameraInsideBox);
				minPos.z = Mathf.Min(newPos.z, maxPos.z);
			}

			//middle = (maxPos + minPos) * 0.5f;
			//m_Bounds.center = middle;
			//m_Bounds.size = middle - maxPos;
			Handles.color = startColor;
		}

		private Vector3 MidpointHandle(int id, Vector3 localPos, Vector3 localTangent, Vector3 localBinormal, float sideAxisSize, bool isCameraInsideBox)
		{
			Color oldColor = Handles.color;

			AdjustMidpointHandleColor(localPos, localTangent, localBinormal, isCameraInsideBox);

			if (Handles.color.a > 0f)
			{
				Vector3 localDir = Vector3.Cross(localTangent, localBinormal).normalized;

				sideAxisSize *= .1f;

				Vector3[] box = new Vector3[4];
				box[0] = localPos + (localTangent + localBinormal) * sideAxisSize;
				box[2] = localPos - (localTangent + localBinormal) * sideAxisSize;

				box[1] = localPos + (localTangent - localBinormal) * sideAxisSize;
				box[3] = localPos - (localTangent - localBinormal) * sideAxisSize;

				Handles.color *= .68f;
				Handles.DrawSolidRectangleWithOutline(box, Handles.color, Color.clear);
				Handles.RectangleHandleCap(id, localPos, Quaternion.LookRotation(localDir), sideAxisSize, Event.current.type);
				Handles.color = oldColor;

				localPos = Handles.Slider(id, localPos, localDir, .1f, Handles.ArrowHandleCap, EditorSnapSettings.scale);
			}

			Handles.color = oldColor;
			return localPos;
		}

		private void AdjustMidpointHandleColor(Vector3 localPos, Vector3 localTangent, Vector3 localBinormal, bool isCameraInsideBox)
		{
			float alphaMultiplier = 1f;

			// if inside the box then ignore back facing alpha multiplier (otherwise all handles will look disabled)
			if (!isCameraInsideBox)
			{
				// use tangent and binormal to calculate normal in case handle matrix is skewed
				Vector3 worldTangent = Handles.matrix.MultiplyVector(localTangent);
				Vector3 worldBinormal = Handles.matrix.MultiplyVector(localBinormal);
				Vector3 worldDir = Vector3.Cross(worldTangent, worldBinormal).normalized;

				// adjust color if handle is back facing
				float cosV;

				if (Camera.current.orthographic)
					cosV = Vector3.Dot(-Camera.current.transform.forward, worldDir);
				else
					cosV = Vector3.Dot((Camera.current.transform.position - Handles.matrix.MultiplyPoint(localPos)).normalized, worldDir);

				if (cosV < -0.0001f)
					alphaMultiplier *= .4f;

				//Handles.Label(localPos, cosV.ToString());
			}

			Handles.color *= new Color(1f, 1f, 1f, alphaMultiplier);
		}
	}

}
//public class DirectionalDriveHandle
//{
//	//	public Vector3 center = Vector3.zero;
//	//	public Quaternion rotation = Quaternion.identity;
//	//	public Vector3 range = Vector3.one;
//	public Axis axis;

//	public Vector2 startMousePos;
//	public Vector2 currentMousePos;
//	// +X,-X,+Y,-Y,+z,-Z
//	private int[] controlIDs = new int[6] { 0, 0, 0, 0, 0, 0 };

//	public DirectionalDriveHandle()
//	{
//		for (int i = 0, count = controlIDs.Length; i < count; ++i)
//			controlIDs[i] = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);
//	}

//	public Vector3 Draw(Vector3 center, Quaternion rotation, Vector3 range)
//	{
//		Color oldColor = Handles.color;

//		Handles.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
//		Handles.Label(center, $"{controlIDs[0]} {controlIDs[5]}");
//		DrawWireframe(center, rotation, range);


//		Event e = Event.current;
//		foreach (var id in controlIDs)
//		{
//			switch (e.GetTypeForControl(id))
//			{
//				case EventType.Layout:
//					break;
//				case EventType.Repaint:



//					break;
//			}
//		}

//		Handles.color = oldColor;
//		return center;
//	}

//	protected static void DrawWireframe(Vector3 pos, Quaternion rotation, Vector3 scale)
//	{
//		Vector3[] points = new Vector3[10];
//		scale *= .5f;

//		points[0] = pos + (rotation * new Vector3(-scale.x, -scale.y, -scale.z));
//		points[1] = pos + (rotation * new Vector3(-scale.x, scale.y, -scale.z));

//		points[2] = pos + (rotation * new Vector3(scale.x, scale.y, -scale.z));
//		points[3] = pos + (rotation * new Vector3(scale.x, -scale.y, -scale.z));

//		points[4] = pos + (rotation * new Vector3(-scale.x, -scale.y, -scale.z));
//		points[5] = pos + (rotation * new Vector3(-scale.x, -scale.y, scale.z));

//		points[6] = pos + (rotation * new Vector3(-scale.x, scale.y, scale.z));
//		points[7] = pos + (rotation * new Vector3(scale.x, scale.y, scale.z));

//		points[8] = pos + (rotation * new Vector3(scale.x, -scale.y, scale.z));
//		points[9] = pos + (rotation * new Vector3(-scale.x, -scale.y, scale.z));

//		Handles.DrawPolyLine(points);
//		Handles.DrawLine(points[1], points[6]);
//		Handles.DrawLine(points[2], points[7]);
//		Handles.DrawLine(points[3], points[8]);
//	}

//	public void DrawHandles(Vector3 center, Quaternion direction, Vector3 range, Axis axis)
//	{
//		Vector3 xAxis = direction * Vector3.right;
//		Vector3 yAxis = direction * Vector3.up;
//		Vector3 zAxis = direction * Vector3.forward;
//		bool isCameraInsideBox = false;
//		//Vector3 localPos, newPos;
//		//if (axis.HasFlag(Axis.X))
//		//{
//		//	Handles.color = Color.red;
//		//	// +X
//		//	localPos = new Vector3(range.x, center.y, center.z);

//		//	newPos = MidpointHandle(controlIDs[0], localPos, yAxis, zAxis, isCameraInsideBox);
//		//	maxPos.x = Mathf.Max(newPos.x, -range.x);

//		//	// -X
//		//	localPos = new Vector3(-range.x, center.y, center.z);
//		//	newPos = MidpointHandle(controlIDs[1], localPos, yAxis, -zAxis, isCameraInsideBox);
//		//	minPos.x = Mathf.Min(newPos.x, range.x);
//		//}

//		//if (axis.HasFlag(Axis.Y))
//		//{
//		//	Handles.color = Color.green;

//		//	// +Y
//		//	localPos = new Vector3(center.x, range.y, center.z);
//		//	newPos = MidpointHandle(controlIDs[2], localPos, xAxis, -zAxis, isCameraInsideBox);
//		//	maxPos.y = Mathf.Max(newPos.y, -range.y);

//		//	// -Y
//		//	localPos = new Vector3(center.x, -range.y, center.z);
//		//	newPos = MidpointHandle(controlIDs[3], localPos, xAxis, zAxis, isCameraInsideBox);
//		//	minPos.y = Mathf.Min(newPos.y, range.y);
//		//}

//		//if (axis.HasFlag(Axis.Z))
//		//{
//		//	Handles.color = Color.blue;
//		//	// +Z
//		//	localPos = new Vector3(center.x, center.y, range.z);
//		//	newPos = MidpointHandle(controlIDs[4], localPos, yAxis, -xAxis, isCameraInsideBox);
//		//	maxPos.z = Mathf.Max(newPos.z, -range.z);

//		//	// -Z
//		//	localPos = new Vector3(center.x, center.y, -range.z);
//		//	newPos = MidpointHandle(controlIDs[5], localPos, yAxis, xAxis, isCameraInsideBox);
//		//	minPos.z = Mathf.Min(newPos.z, range.z);
//		//}
//	}

//	private Vector3 MidpointHandle(int id, Vector3 localPos, Vector3 localTangent, Vector3 localBinormal, bool isCameraInsideBox)
//	{
//		Color oldColor = Handles.color;

//		//AdjustMidpointHandleColor(localPos, localTangent, localBinormal, isCameraInsideBox);

//		if (Handles.color.a > 0f)
//		{
//			Vector3 localDir = Vector3.Cross(localTangent, localBinormal).normalized;

//			//var size = midpointHandleSizeFunction == null ? 0f : midpointHandleSizeFunction(localPos);

//			//localPos = UnityEditorInternal.Slider1D.Do(id, localPos, localDir, .01f, Handles.DotCap, EditorSnapSettings.scale);
//		}

//		Handles.color = oldColor;
//		return localPos;
//	}
//}