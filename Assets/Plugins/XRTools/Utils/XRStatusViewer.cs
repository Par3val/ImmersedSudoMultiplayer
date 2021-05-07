using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

namespace XRTools.Utils
{

	public class XRStatusViewer : MonoBehaviour
	{

		public class InputDeviceData
		{
			public InputDevice device { get; }
			List<InputFeatureUsage> features;
			string ID;
			public InputDeviceData(InputDevice _device)
			{
				device = _device;
				features = new List<InputFeatureUsage>();
				device.TryGetFeatureUsages(features);
				ID = _device.serialNumber;
			}

			public string GetFeaturesNames()
			{
				string result = "";
				for (int i = 0; i < features.Count; i++)
				{
					result += (i == 0 ? "" : ", ") + features[i].name;
				}
				return result;
			}

			public InputFeatureUsage<Vector3> GetDevicePosFeature()
			{
				for (int i = 0; i < features.Count; i++)
				{
					if (features[i].type == typeof(Vector3))
					{
						if (features[i].name.Equals("DevicePosition"))
							return features[i].As<Vector3>();
					}
				}
				return new InputFeatureUsage<Vector3>("NULL");
			}

			public InputFeatureUsage<Quaternion> GetDeviceRotFeature()
			{
				for (int i = 0; i < features.Count; i++)
				{
					if (features[i].type == typeof(Quaternion))
					{
						if (features[i].name.Equals("DeviceRotation"))
							return features[i].As<Quaternion>();
					}
				}
				return new InputFeatureUsage<Quaternion>("NULL");
			}

#if UNITY_EDITOR
			public bool editorOpen;
#endif

		}


		[SerializeField]
		public List<InputDeviceData> devices;

		void OnEnable()
		{
			devices = new List<InputDeviceData>();
			InputDevices.deviceConnected += DeviceConnected;

            InputDevices.deviceDisconnected += InputDevices_deviceDisconnected;
		}

        private void InputDevices_deviceDisconnected(InputDevice obj)
        {
            for (int i = 0; i < devices.Count; i++)
            {
				if(devices[i].device.serialNumber== obj.serialNumber)
                {
					devices.RemoveAt(i);
					return;
                }
            }
        }

        private void DeviceConnected(InputDevice device)
		{
			devices.Add(new InputDeviceData(device));
		}

		private void OnDisable()
		{

		}

		List<InputDevice> deviceList = new List<InputDevice>();

		// Update is called once per frame
		void Update()
		{
			
			InputDevices.GetDevices(deviceList);
			Debug.Log($"{deviceList.Count}");
		}
	}

	

#if UNITY_EDITOR

	[CustomEditor(typeof(XRStatusViewer))]
	public class XRStatusViewerEditor : Editor
	{
		XRStatusViewer status;
		List<InputDevice> tempDevices;

		public override void OnInspectorGUI()
		{
			status = (XRStatusViewer)target;

			tempDevices = new List<InputDevice>();
			InputDevices.GetDevices(tempDevices);
			GUILayout.Label($"XR Status {tempDevices.Count}");
			if (status.devices != null && Application.isPlaying)
			{
				GUILayout.Label($"Devices: {status.devices.Count}");

				foreach (var deviceData in status.devices)
				{
					EditorGUILayout.BeginVertical(EditorUtils.window);

					if (EditorGUILayout.BeginFoldoutHeaderGroup(deviceData.editorOpen, deviceData.device.name))
					{
						deviceData.editorOpen = true;
						ShowInputDevice(deviceData.device);

						string features = deviceData.GetFeaturesNames();

						if (features.Contains("DevicePosition"))
						{
							if (GUILayout.Button("Make Follower"))
							{
								MakeFollower(deviceData);
							}
						}

						EditorStyles.label.wordWrap = true;
						EditorGUILayout.LabelField(features);


					}
					else deviceData.editorOpen = false;

					EditorGUILayout.EndFoldoutHeaderGroup();

					EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
				}
			}
		}

		public void MakeFollower(XRStatusViewer.InputDeviceData deviceData)
		{
			var tempObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
			tempObject.transform.localScale *= .1f;
			tempObject.name = deviceData.device.name + " {" + deviceData.device.serialNumber + "}";
			//tempObject.AddComponent<InputDeviceTracker>().Setup(deviceData);
		}
		public void ShowInputDevice(InputDevice device)
		{
			EditorGUI.indentLevel++;

			EditorGUILayout.LabelField(device.characteristics.ToString());
			EditorGUILayout.LabelField(device.manufacturer + " {" + device.serialNumber + "}");

			EditorGUI.indentLevel--;
			GUILayout.Space(10);

		}
	}


}
#endif