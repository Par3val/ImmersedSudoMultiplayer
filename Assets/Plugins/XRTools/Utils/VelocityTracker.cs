using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XRTools.Utils
{
	public interface RigidbodyData
	{
		bool IsActive();

		Vector3 GetVelocity();

		Vector3 GetAngularVelocity();
	}

	public class VelocityTracker : MonoBehaviour, RigidbodyData
	{
		public Transform source;
		/// <summary>
		/// An optional object to consider the source relative to when estimating the velocities.
		/// </summary>
		public Transform relativeTo;
		public bool IsEstimating { get; set; } = true;

		/// <summary>
		/// The number of average frames to collect samples for velocity estimation.
		/// </summary>
		public int VelocityAverageFrames = 5;
		/// <summary>
		/// The number of average frames to collect samples for angular velocity estimation.
		/// </summary>
		public int AngularVelocityAverageFrames = 10;

		protected int currentSampleCount;
		protected Vector3[] velocitySamples = System.Array.Empty<Vector3>();
		protected Vector3[] angularVelocitySamples = System.Array.Empty<Vector3>();
		protected Vector3 previousPosition = Vector3.zero;
		protected Quaternion previousRotation = Quaternion.identity;
		protected Vector3 previousRelativePosition = Vector3.zero;
		protected Quaternion previousRelativeRotation = Quaternion.identity;

		public bool IsActive()
		{
			return isActiveAndEnabled && source != null && source.gameObject.activeInHierarchy;
		}

		public Vector3 GetVelocity()
		{
			return IsActive() ? DoGetVelocity() : Vector3.zero;
		}
		public Vector3 GetAngularVelocity()
		{
			return (IsActive() ? DoGetAngularVelocity() : Vector3.zero);
		}
		
		public virtual Vector3 GetAcceleration()
		{
			if (!IsActive())
			{
				return Vector3.zero;
			}

			Vector3 average = Vector3.zero;
			for (int sampleIndex = 2 + currentSampleCount - velocitySamples.Length; sampleIndex < currentSampleCount; sampleIndex++)
			{
				if (sampleIndex >= 2)
				{
					int first = sampleIndex - 2;
					int second = sampleIndex - 1;

					Vector3 v1 = velocitySamples[first % velocitySamples.Length];
					Vector3 v2 = velocitySamples[second % velocitySamples.Length];
					average += v2 - v1;
				}
			}
			average *= 1/ Time.deltaTime;
			return average;
		}

		protected virtual void OnEnable()
		{
			if (!source)
				source = transform;

			previousPosition = source.position;
			previousRotation = source.rotation;
			previousRelativePosition = relativeTo ? relativeTo.position : Vector3.zero;
			previousRelativeRotation = relativeTo ? relativeTo.rotation : Quaternion.identity;

			velocitySamples = new Vector3[VelocityAverageFrames];
			angularVelocitySamples = new Vector3[AngularVelocityAverageFrames];
		}

		protected virtual void LateUpdate()
		{
			ProcessEstimation();
		}
		


		Vector3 DoGetVelocity()
		{
			return GetEstimate(velocitySamples);
		}
		
		Vector3 DoGetAngularVelocity()
		{
			return GetEstimate(angularVelocitySamples);
		}
		
		/// <summary>
		/// Calculates the average estimate for the given sample set.
		/// </summary>
		/// <param name="samples">An array of samples to estimate with.</param>
		/// <returns>The estimated result.</returns>
		Vector3 GetEstimate(Vector3[] samples)
		{
			Vector3 estimate = Vector3.zero;
			int sampleCount = Mathf.Min(currentSampleCount, samples.Length);
			if (sampleCount != 0)
			{
				for (int index = 0; index < sampleCount; index++)
				{
					estimate += samples[index];
				}
				estimate *= 1.0f / sampleCount;
			}
			return estimate;
		}

		/// <summary>
		/// Collects the appropriate samples for velocities and estimates the results.
		/// </summary>
		void ProcessEstimation()
		{
			if (IsEstimating)
			{
				float factor =  1 / Time.deltaTime;
				EstimateVelocity(factor);
				EstimateAngularVelocity(factor);
				currentSampleCount++;
			}
			else
			{
				currentSampleCount = 0;
			}
		}
		void EstimateVelocity(float factor)
		{
			if (velocitySamples.Length == 0)
			{
				return;
			}

			Vector3 currentPosition = source.position;

			Vector3 currentRelativePosition = relativeTo ? relativeTo.position : Vector3.zero; ;
			Vector3 relativeDeltaPosition = currentRelativePosition - previousRelativePosition;

			int sampleIndex = currentSampleCount % velocitySamples.Length;
			velocitySamples[sampleIndex] = factor * (currentPosition - previousPosition - relativeDeltaPosition);
			previousPosition = currentPosition;
			previousRelativePosition = currentRelativePosition;
		}

		void EstimateAngularVelocity(float factor)
		{
			if (angularVelocitySamples.Length == 0)
			{
				return;
			}
			
			Quaternion currentRotation = source.rotation;

			Quaternion currentRelativeRotation = relativeTo ? relativeTo.rotation : Quaternion.identity;
			Quaternion relativeDeltaRotation = currentRelativeRotation * Quaternion.Inverse(previousRelativeRotation);

			Quaternion deltaRotation = Quaternion.Inverse(relativeDeltaRotation) * (currentRotation * Quaternion.Inverse(previousRotation));
			float theta = 2.0f * Mathf.Acos(Mathf.Clamp(deltaRotation.w, -1.0f, 1.0f));
			if (theta > Mathf.PI)
			{
				theta -= 2.0f * Mathf.PI;
			}

			Vector3 angularVelocity = new Vector3(deltaRotation.x, deltaRotation.y, deltaRotation.z);
			if (angularVelocity.sqrMagnitude > 0.0f)
			{
				angularVelocity = theta * factor * angularVelocity.normalized;
			}

			int sampleIndex = currentSampleCount % angularVelocitySamples.Length;
			angularVelocitySamples[sampleIndex] = angularVelocity;
			previousRotation = currentRotation;
			previousRelativeRotation = currentRelativeRotation;
		}
		
	}

}