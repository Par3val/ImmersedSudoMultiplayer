using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SudoNetworking
{

	public static class NetworkingData
	{
		[System.Serializable]
		public struct PoseData
		{
			public static readonly PoseData empty = new PoseData(Vector3.zero, Quaternion.identity);
			Vector3 pos;
			Quaternion rot;

			public PoseData(Vector3 _pos, Quaternion _rot)
			{
				pos = _pos;
				rot = _rot;
			}

			public Vector3 GetPosition() => pos;
			public Quaternion GetRotation() => rot;

			public void SetPosition(Vector3 _pos) => pos = _pos;
			public void SetRotation(Quaternion _rot) => rot = _rot;
			public bool IsNull() => pos.Equals(Vector3.zero) && rot.Equals(Quaternion.identity);
		}

		[System.Serializable]
		public class RigData
		{
			/// <summary>
			/// describes how much of the data is actually available\n
			/// 1 - just Head\n
			/// 2 - Head + Hands\n
			/// 3 - Full Avatar Rig\n
			/// </summary>
			[Range(1, 3)]
			public int quality = 1;
			public PoseData Head { get; set; }
			public PoseData Left { get; set; }
			public PoseData Right { get; set; }

			public PoseData LeftShoulder { get; set; }
			public PoseData RightShoulder { get; set; }

			public PoseData LeftElbow { get; set; }
			public PoseData RightElbow { get; set; }

			public RigData(PoseData _head)
			{
				quality = 1;
				Head = _head;
				Left = PoseData.empty;
				Right = PoseData.empty;

				LeftShoulder = PoseData.empty;
				RightShoulder = PoseData.empty;

				LeftElbow = PoseData.empty;
				RightElbow = PoseData.empty;

			}

			public RigData(PoseData _head, PoseData _left, PoseData _right)
			{
				quality = 2;
				Head = _head;
				Left = _left;
				Right = _right;

				LeftShoulder = PoseData.empty;
				RightShoulder = PoseData.empty;

				LeftElbow = PoseData.empty;
				RightElbow = PoseData.empty;

			}

			public RigData(PoseData _head, PoseData _left, PoseData _right,
				PoseData _leftShoulder, PoseData _rightShoulder,
				PoseData _leftElbow, PoseData _rightElbow)
			{
				quality = 3;
				Head = _head;
				Left = _left;
				Right = _right;

				LeftShoulder = _leftShoulder;
				RightShoulder = _rightShoulder;

				LeftElbow = _leftElbow;
				RightElbow = _rightElbow;
			}
		}
	}

}