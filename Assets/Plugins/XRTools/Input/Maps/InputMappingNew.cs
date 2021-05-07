using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace XRTools.Input
{
	[System.Serializable]
	public class UnityEventFloat : UnityEvent<float> { }
	[System.Serializable]
	public class UnityEventVector2 : UnityEvent<Vector2> { }
	[System.Serializable]
	public class UnityEventButton : UnityEvent<bool> { }

	public class InputMappingNew : MonoBehaviour
	{
		public XRNode hand = XRNode.LeftEye;
		public string mapName = "new Map";
		public InputDevice handDevice;

		public enum InputType { Button, Trigger_Grab, Joystick_Trackpad, /*LowerHand */}

		private void OnEnable()
		{
			InputDevices.deviceConnected += DeviceConected;
		}

		private void FixedUpdate()
		{
			this.UpdateValue();
		}

		public virtual void RefreshFeature()
		{
			var device = InputDevices.GetDeviceAtXRNode(hand);
			List<InputFeatureUsage> features = new List<InputFeatureUsage>();
			device.TryGetFeatureUsages(features);

			string targetFeature = this.GetInputFeature();

			//Debug.Log("Looking For " + targetFeature);

			foreach (var feature in features)
			{
				if (feature.name.Equals(targetFeature))
				{
					this.SetInputFeature(feature);
					//Debug.Log($"Found {feature.name}");
					break;
				}

			}
		}

		void DeviceConected(InputDevice device)
		{
			if (device.characteristics.HasFlag(GetRequiredCharacteristics()))
			{
				handDevice = device;

				InputDevices.deviceConnected -= DeviceConected;
				List<InputFeatureUsage> features = new List<InputFeatureUsage>();
				device.TryGetFeatureUsages(features);

				string targetFeature = this.GetInputFeature();

				//Debug.Log("Looking For " + targetFeature);

				foreach (var feature in features)
				{
					if (feature.name.Equals(targetFeature))
					{
						this.SetInputFeature(feature);
						//Debug.Log($"Found {feature.name}");
						break;
					}

				}
			}
		}

		public virtual InputDeviceCharacteristics GetRequiredCharacteristics()
		{
			InputDeviceCharacteristics result = InputDeviceCharacteristics.HeldInHand
											| InputDeviceCharacteristics.TrackedDevice
											| InputDeviceCharacteristics.Controller;

			if (hand == XRNode.LeftHand)
				return result | InputDeviceCharacteristics.Left;
			else
				return result | InputDeviceCharacteristics.Right;
		}

		public virtual string GetInputFeature()
		{
			return "";
		}

		public virtual void SetInputFeature(InputFeatureUsage inputFeature) { }

		public virtual void UpdateValue() { }

		//public class HandMap : InputMappingNew
		//{
		//	public enum HandFingers { Index, Middle, rig, Pinky }
		//	public HandFingers fingerMap = HandFingers.Index;

		//	InputFeatureUsage<Hand> inputFeature;

		//	public override string GetInputFeature()
		//	{
		//		switch (fingerMap)
		//		{
		//			case HandFingers.Index:
		//				return "IndexFinger";
		//			case HandFingers.Middle:
		//				return "MiddleFinger";
		//			case HandFingers.rig:
		//				return "RingFinger";
		//			case HandFingers.Pinky:
		//				return "PinkyFinger";
		//		}
		//		return base.GetInputFeature();
		//	}
		//}

#if UNITY_EDITOR
		public bool editorOpen = true;

		public static InputMappingNew SwitchType(InputMappingNew originalMap, InputType type)
		{
			int originalIndex = -1;

			var maps = originalMap.gameObject.GetComponents<InputMappingNew>();
			for (int i = 0; i < maps.Length; i++)
			{
				if (maps[i].Equals(originalMap))
				{
					originalIndex = i;
				}
			}

			if (originalIndex == -1)
			{
				Debug.LogError($"Map on {originalMap.gameObject} didnt have {originalMap.mapName}", originalMap);
				return null;
			}

			InputMappingNew newMap = null;
			switch (type)
			{
				case InputType.Button:
					ButtonMap newButtonMap = (ButtonMap)UnityEditor.Undo.AddComponent(originalMap.gameObject, typeof(ButtonMap));

					//originalMap.TryGetComponent(out FloatMap floatMap);
					//if(floatMap != null)
					//{


					//	int listeners = floatMap.Activated.GetPersistentEventCount();
					//	for (int i = 0; i < listeners; i++)
					//	{
					//	string methodName = floatMap.Activated.GetPersistentMethodName(i);
					//	string finalName = floatMap.Activated.GetPersistentMethodName(i);
					//		Utils.EventUtillity.TransferPersistentCalls(floatMap, newButtonMap, "Activated", "Activated");
					//	}

					//}
					newMap = newButtonMap;
					break;
				case InputType.Trigger_Grab:
					FloatMap newFloatMap = (FloatMap)UnityEditor.Undo.AddComponent(originalMap.gameObject, typeof(FloatMap));

					//originalMap.TryGetComponent(out ButtonMap buttonMap);
					//if (buttonMap != null)
					//{


					//	int listeners = buttonMap.Activated.GetPersistentEventCount();
					//	for (int i = 0; i < listeners; i++)
					//	{
					//		string methodName = buttonMap.Activated.GetPersistentMethodName(i);
					//		string finalName = buttonMap.Activated.GetPersistentMethodName(i);
					//		Debug.Log(Utils.EventUtillity.TransferPersistentCalls(buttonMap, newFloatMap, "Activated", "Activated"));
					//	}

					//}

					newMap = newFloatMap;
					break;
				case InputType.Joystick_Trackpad:
					Vector2Map newVec2Map = (Vector2Map)UnityEditor.Undo.AddComponent(originalMap.gameObject, typeof(Vector2Map));
					newMap = newVec2Map;
					break;
			}
			for (int i = 0; i < maps.Length - originalIndex; i++)
			{
				UnityEditorInternal.ComponentUtility.MoveComponentUp(newMap);

			}

			newMap.mapName = originalMap.mapName;

			DestroyImmediate(originalMap);
			return newMap;
		}
#endif

	}
}

/*
 Quest 1 (Through Link?)
 Primary2DAxis(UnityEngine.Vector2) 

 Primary2DAxisClick(System.Boolean) 
 Primary2DAxisTouch(System.Boolean) 
 
Secondary2DAxis(UnityEngine.Vector2) 
			  touch?
			  click?

 Trigger(System.Single) 
 Grip(System.Single) 
 
 IndexFinger(System.Single) 
 MiddleFinger(System.Single) 
 RingFinger(System.Single) 
 PinkyFinger(System.Single) 

 PrimaryButton(System.Boolean) 
 SecondaryButton(System.Boolean) 
 GripButton(System.Boolean) 
 TriggerButton(System.Boolean) 

 DevicePosition(UnityEngine.Vector3) 
 DeviceRotation(UnityEngine.Quaternion) 

 DeviceVelocity(UnityEngine.Vector3) 
 DeviceAngularVelocity(UnityEngine.Vector3) 
 TrackingState(System.int32)
 IsTracked(System.Boolean)
	 */
