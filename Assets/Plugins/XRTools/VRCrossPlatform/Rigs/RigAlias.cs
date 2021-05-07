using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using XRTools.Actions;

namespace XRTools.Rigs
{
	public class RigAlias : MonoBehaviour
	{
		[SerializeField]
		Rig[] rigs;

		#region Alias Refrences

		[Space, Header("Aliases")]
		public GameObject playAreaAlias;
		public GameObject headsetAlias;
		public GameObject leftAlias;
		public GameObject rightAlias;

		#endregion

		List<FollowAction> followActions;

		[Space, Space]
		public UnityEvent HeadsetTrackingBegun = new UnityEvent();
		public UnityEvent LeftControllerTrackingBegun = new UnityEvent();
		public UnityEvent RightControllerTrackingBegun = new UnityEvent();

		void OnEnable()
		{
			followActions = new List<FollowAction>();

			if (rigs[0].PlayArea)
			{
				followActions.Add(new FollowAction(playAreaAlias, rigs[0].PlayArea, UpdateEvents.BeforeRender));

			}

			if (rigs[0].Headset)
			{
				followActions.Add(new FollowAction(headsetAlias, rigs[0].Headset, UpdateEvents.BeforeRender));
			}

			if (rigs[0].LeftController)
			{
				followActions.Add(new FollowAction(leftAlias, rigs[0].LeftController, UpdateEvents.BeforeRender));
				leftAlias.GetComponentInChildren<Interaction.InteractorRaw>().velocityTracker = rigs[0].LeftControllerVelocityTracker;
			}

			if (rigs[0].LeftController)
			{
				followActions.Add(new FollowAction(rightAlias, rigs[0].RightController, UpdateEvents.BeforeRender));
				rightAlias.GetComponentInChildren<Interaction.InteractorRaw>().velocityTracker = rigs[0].RightControllerVelocityTracker;
			}

			//new Input.InputMappingNew(true);
		}

		private void Update()
		{

			var hand = true ? XRNode.LeftHand : XRNode.RightHand;

			//var handDevice = InputDevices.GetDeviceAtXRNode(hand);
			//List<InputFeatureUsage> usages = new List<InputFeatureUsage>();
			//handDevice.TryGetFeatureUsages(usages);

			//foreach (var usage in usages)
			//{
			//	Debug.Log(usage.name + " " + usage.type);
			//}

		}
		void OnDisable()
		{
			foreach (var action in followActions)
			{
				action.Disable();
			}
		}
	}

}