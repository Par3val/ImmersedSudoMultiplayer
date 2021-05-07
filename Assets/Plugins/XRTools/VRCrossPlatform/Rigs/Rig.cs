using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRTools.Utils;
using UnityEngine.XR;

namespace XRTools.Rigs
{
	public class Rig : MonoBehaviour
	{
		[Header("Play Area")]
		public GameObject PlayArea;

		public PlayAreaRepresentaton playAreaRepresentaton;

		#region Headset Settings

		[Space, Header("Headset")]
		public GameObject Headset;

		//public Camera HeadsetCamera;

		public VelocityTracker HeadsetVelocityTracker;

		#endregion

		#region Left Controller Settings

		[Space, Header("Left Controller")]
		public GameObject LeftController;

		public VelocityTracker LeftControllerVelocityTracker;

		//public HapticProcess LeftControllerHapticProcess;

		#endregion

		#region Right Controller Settings

		[Space, Header("Right Controller")]
		public GameObject RightController;

		public VelocityTracker RightControllerVelocityTracker;

		//public HapticProcess RightControllerHapticProcess;
		#endregion

		public readonly XRNode headNode = XRNode.Head;
		public readonly XRNode leftNode = XRNode.LeftHand;
		public readonly XRNode rightNode = XRNode.RightHand;

		public InputDevice headDevice;
		public List<XRInputSubsystem> subsystems;

		private void Awake()
		{
			//devices = new List<InputDevice>();
			//InputDevices.GetDevices(devices);

			//foreach (var device in devices)
			//{
			//	Debug.Log($"{device.name} {device.subsystem} {device.characteristics}");
			//}
			if (Headset && !HeadsetVelocityTracker)
				HeadsetVelocityTracker = Headset.AddComponent<VelocityTracker>();


			if (LeftController && !LeftControllerVelocityTracker)
				LeftControllerVelocityTracker = LeftController.AddComponent<VelocityTracker>();


			if (RightController && !RightControllerVelocityTracker)
				RightControllerVelocityTracker = RightController.AddComponent<VelocityTracker>();
		}

		bool gotResult = false;
		private void Update()
		{
			if (!gotResult)
			{

				subsystems = new List<XRInputSubsystem>();
				SubsystemManager.GetInstances<XRInputSubsystem>(subsystems);

				if (subsystems.Count > 0)
				{
					gotResult = true;
					Debug.Log($"Subs Systems Operational {{{subsystems[0].SubsystemDescriptor.id}}}");
					if (playAreaRepresentaton)
						playAreaRepresentaton.DrawPlayArea(subsystems[0]);
				}
			}
			//Debug.Log(headDevice.subsystem);
			//InputDevices.GetDevices(devices);

			//foreach (var device in devices)
			//{
			//	Debug.Log($"{device.name} ~~{device.subsystem}~~ {device.characteristics}");
			//}

			//if(!gotResult)
			//if(UnityEngine.Experimental.XR.Boundary.TryGetDimensions(out Vector3 dimensions))
			//{
			//		gotResult = true;
			//		Debug.Log(dimensions);
			//		var debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			//		debugCube.transform.position = transform.position + new Vector3(0, 1,0);
			//		dimensions.y = 2;
			//		debugCube.transform.localScale = dimensions;

			//}
		}
	}

}