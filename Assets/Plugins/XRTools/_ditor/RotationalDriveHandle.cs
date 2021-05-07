using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XRTools.Utils;

namespace XRTools.Handle
{

	public class RotationalDriveHandle
	{
		int id;
		public RotationalDriveHandle(Transform drive)
		{
			id = GUIUtility.GetControlID(drive.GetHashCode(), FocusType.Passive);
		}

		public Vector2 startMousePos, currentMousePos;
		public void Draw(Vector3 center, Quaternion localRot, Vector3 up, float radius)
		{
			Event evt = Event.current;
			switch (evt.GetTypeForControl(id))
			{
				case EventType.Layout:
					Handles.CylinderHandleCap(id, center, localRot, radius * .1f, EventType.Layout);
					break;
				case EventType.MouseDown:
					if (evt.button == 0 && GUIUtility.hotControl == 0 && HandleUtility.nearestControl == id)
					{
						GUIUtility.hotControl = id;
						currentMousePos = startMousePos = evt.mousePosition;
						startMousePos = currentMousePos;

						evt.Use();
						EditorGUIUtility.SetWantsMouseJumping(1);
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == id)
					{
						//currentMousePos += evt.delta;
						//Vector3 tempPos = HandleUtility.ClosestPointToArc(center, normal, AngleDir, 360, radius);
						//angle += Vector3.SignedAngle(AngleDir, tempPos - center, normal);
						evt.Use();
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == id && (evt.button == 0 || evt.button == 2))
					{
						GUIUtility.hotControl = 0;
						evt.Use();
						EditorGUIUtility.SetWantsMouseJumping(0);
					}
					break;
				case EventType.Repaint:
					Color tempColor = Handles.color;

					if (id == GUIUtility.hotControl)
						Handles.color = Handles.selectedColor;

					Handles.Label(center + up * .5f, id.ToString());
					Handles.CylinderHandleCap(id, center, localRot, radius * .1f, EventType.Repaint);
					Handles.color = tempColor;
					break;
			}
		}

		public static void QuanternionViewer(Vector3 pos, Quaternion quaternion, Vector3 eulerOffset)
		{
			Event e = Event.current;
			Quaternion tempRot = quaternion * Quaternion.Euler(eulerOffset);
			float screenSizeSize = HandleUtility.GetHandleSize(pos);
			switch (e.type)
			{
				case EventType.Repaint:
					Handles.DrawLine(pos, pos + tempRot * Vector3.forward * screenSizeSize);
					Handles.color = Color.blue;
					Handles.DrawLine(pos, pos + tempRot * Vector3.up * screenSizeSize);

					Handles.color = Color.cyan * .9f;
					//Handles.DrawLine(pos, pos + quaternion * Vector3.forward * screenSizeSize);
					//Handles.color = Color.magenta * .9f;
					//Handles.DrawLine(pos, pos + quaternion * Vector3.up * .45f * screenSizeSize);
					//Handles.ConeHandleCap(-1, pos + tempRot * Vector3.forward, Quaternion.AngleAxis(0, tempRot * Vector3.forward), screenSizeSize * .4f, EventType.Repaint);
					//Handles.ConeHandleCap(-1, pos + tempRot * Vector3.up * .45f, Quaternion.FromToRotation(tempRot * Vector3.forward, tempRot * Vector3.up), screenSizeSize * .18f, EventType.Repaint);

					//x = right
					//y = up
					//z = forward
					float angle = 90 - ((Vector3.Dot(quaternion * Vector3.forward, tempRot * Vector3.forward)) * 90);

					Vector3 forward =  quaternion * Vector3.forward;
					Vector3 up =  quaternion * Vector3.up;
					Vector3 right = quaternion * Vector3.right;

					forward.x = 0;
					forward.y = 0;

					up.x = 0;
					up.z = 0;

					right.y = 0;
					right.z = 0;

					//float forwardAngle = Quaternion.Angle(quaternion, tempRot);
					float forwardAngle = CalcAngle(pos, forward, tempRot * Vector3.forward, quaternion * Vector3.right);
					float upAngle =		 CalcAngle(pos, up,	  tempRot * Vector3.up,		 quaternion * Vector3.forward);
					float rightAngle =   CalcAngle(pos, right,   tempRot * Vector3.right,   quaternion * Vector3.up);


					Handles.color = Color.red;
					Handles.DrawSolidArc(pos, quaternion * Vector3.right, quaternion * Vector3.forward, forwardAngle, screenSizeSize);
					Handles.Label(pos + quaternion * Vector3.forward * screenSizeSize * 1.3f, forwardAngle.ToString());

					Handles.color = Color.green;
					Handles.DrawSolidArc(pos, quaternion * Vector3.forward, quaternion * Vector3.up, upAngle, screenSizeSize);
					Handles.Label(pos + quaternion * Vector3.up * screenSizeSize * 1.3f, upAngle.ToString());

					//Handles.color = Color.blue;
					//Handles.DrawSolidArc(pos, quaternion * Vector3.up, quaternion * Vector3.forward, rightAngle, screenSizeSize);
					//Handles.Label(pos + quaternion * Vector3.right * screenSizeSize * 1.3f, rightAngle.ToString());

					//Handles.color = Color.red;
					//Handles.DrawSolidArc(pos, quaternion * Vector3.right, quaternion * Vector3.forward, rightAngle, screenSizeSize);
					//Handles.Label(pos + quaternion * Vector3.right * screenSizeSize * 1.3f, rightAngle.ToString());


					//Handles.DrawWireArc(pos, )
					break;
			}
		}

		public static float CalcAngle(Vector3 pos, Vector3 startDir, Vector3 targetDir, Vector3 originDirection)
		{
			if (startDir.magnitude.ApproxEquals(0f))
			{
				return 0f;
			}

			Vector3 startDirAjust = startDir * (1f / startDir.magnitude);

			Vector3 cross1 = Vector3.Cross(targetDir, startDirAjust);
			float axisAngleDot =	   Vector3.Dot(targetDir, startDirAjust);

			float dot1 = Vector3.Dot(originDirection, cross1);



			//Handles.color = Color.blue;
			//Handles.DrawLine(pos, pos + targetDir);
			//Handles.color = Color.red;
			//Handles.DrawLine(pos, pos + startDirAjust);
			//Handles.color = Color.magenta;
			//Handles.DrawLine(pos, pos + cross1);

			//float angle = Vector3.Dot(startDir, targetDir) * 180;

			//Handles.Label(pos + cross1 * .125f, 
			//	$"Dot 1:{(dot1.ApproxEquals(0) ? 0f : dot1)}\n" +
			//	$"axisAngleDot:{(axisAngleDot.ApproxEquals(0) ? 0f : axisAngleDot)}\n" +
			//	$"{Mathf.Atan2(dot1, axisAngleDot) * Mathf.Rad2Deg}");
			


			return -Mathf.Atan2(dot1, axisAngleDot) * Mathf.Rad2Deg;
		}

		public float DrawAngleHandle(int id, Vector3 center, Vector3 normal, Vector3 tangent, float angle, float radius)
		{
			Vector3 direction = Vector3.Dot(Vector3.forward, normal) == 0 ? Vector3.forward : Vector3.up;
			Vector3 AngleDir = Quaternion.AngleAxis(angle, normal) * direction * radius;
			Vector3 handlePos = AngleDir + center;
			Quaternion handleRot = Quaternion.AngleAxis(-angle + 90, tangent);

			float handleSize = HandleUtility.GetHandleSize(handlePos) * .125f;

			Event evt = Event.current;
			switch (evt.GetTypeForControl(id))
			{
				case EventType.Layout:
					Handles.CylinderHandleCap(id, handlePos, handleRot, handleSize, EventType.Layout);
					break;
				case EventType.MouseDown:
					if (evt.button == 0 && GUIUtility.hotControl == 0 && HandleUtility.nearestControl == id)
					{
						GUIUtility.hotControl = id;
						currentMousePos = startMousePos = evt.mousePosition;
						startMousePos = currentMousePos;

						evt.Use();
						EditorGUIUtility.SetWantsMouseJumping(1);
					}
					break;
				case EventType.MouseDrag:
					if (GUIUtility.hotControl == id)
					{
						currentMousePos += evt.delta;
						Vector3 tempPos = HandleUtility.ClosestPointToArc(center, normal, AngleDir, 360, radius);
						angle += Vector3.SignedAngle(AngleDir, tempPos - center, normal);
						evt.Use();
					}
					break;
				case EventType.MouseUp:
					if (GUIUtility.hotControl == id && (evt.button == 0 || evt.button == 2))
					{
						GUIUtility.hotControl = 0;
						evt.Use();
						EditorGUIUtility.SetWantsMouseJumping(0);
					}
					break;
				case EventType.Repaint:
					Color tempColor = Handles.color;

					if (id == GUIUtility.hotControl)
						Handles.color = Handles.selectedColor;

					Handles.Label(handlePos, id.ToString());
					Handles.CylinderHandleCap(id, handlePos, handleRot, handleSize, EventType.Repaint);
					Handles.color = tempColor;
					break;
			}

			return angle;
		}
	}

}



			//Handles.color = Color.green;
			//Handles.DrawLine(pos, pos + originDirection);
			//Handles.color *= .6f;
			//Handles.DrawLine(pos, pos + cross1);


