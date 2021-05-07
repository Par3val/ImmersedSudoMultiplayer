using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRTools.Interaction;

namespace XRTools.Utils
{
	public class Slicer<TElement>
	{
		public static List<TElement> Slice(List<TElement> list, int startIndex, int count)
		{
			var tempList = new List<TElement>();
			SliceNoAlloc(list, out tempList, startIndex, count);
			return tempList;
		}

		public static void SliceNoAlloc(List<TElement> list, out List<TElement> result, int startIndex, int count)
		{
			startIndex = GetStartIndex(startIndex, list.Count);
			count = GetRangeLength(startIndex, count, list.Count);

			result = new List<TElement>();

			for (int i = startIndex; i < startIndex + count; i++)
			{
				result.Add(list[i]);
			}
		}

		public static TElement[] Slice(TElement[] array, int startIndex, int length)
		{
			var tempArray = new TElement[0];
			SliceNoAlloc(array, out tempArray, startIndex, length);
			return tempArray;
		}

		public static void SliceNoAlloc(TElement[] array, out TElement[] result, int startIndex, int length)
		{
			startIndex = GetStartIndex(startIndex, array.Length);
			length = GetRangeLength(startIndex, length, array.Length);

			result = new TElement[length];

			for (int i = startIndex; i < startIndex + length; i++)
			{
				result[i] = array[i];
			}
		}

		/// <summary>
		/// Gets the actual start index even if the index is a negative value.
		/// </summary>
		/// <param name="checkIndex">The index to start from.</param>
		/// <param name="count">The total length of the entire collection</param>
		/// <returns>The actual start index to start from.</returns>
		static int GetStartIndex(int checkIndex, int count)
		{
			return Mathf.Clamp(checkIndex < 0 ? count + checkIndex : checkIndex, 0, count);
		}

		/// <summary>
		/// Gets the actual valid length for the proposed range.
		/// </summary>
		/// <param name="checkIndex">The index to start from.</param>
		/// <param name="checkLength">The length of elements to return.</param>
		/// <param name="count">The total length of the entire collection</param>
		/// <returns>The actual valid length for the given range.</returns>
		static int GetRangeLength(int checkIndex, int checkLength, int count)
		{
			int returnLength = checkLength;
			int actualLength = checkIndex + checkLength;
			if (actualLength >= count)
			{
				int offset = actualLength - count;
				returnLength = checkLength - offset;
			}

			return returnLength;
		}


		//TODO remainded
	}

	/// <summary>
	/// Extended methods for the <see cref="Collider"/> Type.
	/// </summary>
	public static class ColliderExtensions
	{
		/// <summary>
		/// Gets the <see cref="Transform"/> of the container of the collider.
		/// </summary>
		/// <param name="collider">The <see cref="Collider"/> to check against.</param>
		/// <returns>The container.</returns>
		public static Transform GetContainingTransform(this Collider collider)
		{
			if (collider == null)
			{
				return null;
			}

			Rigidbody attachedRigidbody = collider.GetAttachedRigidbody();
			return attachedRigidbody == null ? collider.transform : attachedRigidbody.transform;
		}

		/// <summary>
		/// Gets the parent <see cref="Rigidbody"/> for the given <see cref="Collider"/> even if the <see cref="GameObject"/> is disabled.
		/// </summary>
		/// <param name="collider">The <see cref="Collider"/> to check against.</param>
		/// <returns>The parent <see cref="Rigidbody"/>.</returns>
		public static Rigidbody GetAttachedRigidbody(this Collider collider)
		{
			Rigidbody attachedRigidbody = collider.attachedRigidbody;
			if (!collider.gameObject.activeInHierarchy)
			{
				collider.GetComponentsInParent(true, foundRigidbodies);
				foreach (Rigidbody foundRigidbody in foundRigidbodies)
				{
					attachedRigidbody = foundRigidbody;
					break;
				}
			}
			return attachedRigidbody;
		}

		/// <summary>
		/// A <see cref="Rigidbody"/> collection to store found <see cref="Rigidbody"/>s in.
		/// </summary>
		private static List<Rigidbody> foundRigidbodies = new List<Rigidbody>();
	}

	/// <summary>
	/// Updates the rigidbody angular velocity by rotating towards a given source.
	/// </summary>
	public static class FloatExtensions
	{
		/// <summary>
		/// Determines if two <see cref="float"/> values are equal based on a given tolerance.
		/// </summary>
		/// <param name="a">The <see cref="float"/> to compare against.</param>
		/// <param name="b">The <see cref="float"/> to compare with.</param>
		/// <param name="tolerance">The tolerance in which the two <see cref="float"/> values can be within to be considered equal.</param>
		/// <returns><see langword="true"/> if the two <see cref="float"/> values are considered equal.</returns>
		public static bool ApproxEquals(this float a, float b, float tolerance = float.Epsilon)
		{
			float difference = Mathf.Abs(tolerance);
			return (Mathf.Abs(a - b) <= difference);
		}
	}


	public class DistanceCompare : IComparer<GameObject>
	{
		[SerializeField]
		public Vector3 SourcePosition { get; set; }

		public virtual int Compare(GameObject x, GameObject y)
		{
			float dist1 = Vector3.Distance(x.transform.position, SourcePosition);
			float dist2 = Vector3.Distance(y.transform.position, SourcePosition);
			return dist1.CompareTo(dist2);
		}
	}

	public static class CleanInteractions
	{
		public static void CleanInteractorTouching(InteractableRaw grabbed, ref List<GameObject> touched)
		{
			Transform parent = grabbed.transform;
			for (int i = 0; i < touched.Count; i++)
			{
				if (touched[i].transform.IsChildOf(parent))
				{
					touched.RemoveAt(i);
					i--;
				}
			}
		}

		public static void CleanInteracableTouching(InteractorRaw grabber, ref List<GameObject> touched)
		{
			Transform parent = grabber.transform;
			for (int i = 0; i < touched.Count; i++)
			{
				if (touched[i].transform.IsChildOf(parent))
				{
					touched.RemoveAt(i);
					i--;
				}
			}
		}

		public static readonly List<GameObject> currentGameObjects = new List<GameObject>();

		public static bool DefragTouchingGameObjects(ref List<GameObject> touched)
		{
			if (currentGameObjects.Count != 0)
				return false;

			for (int i = 0; i < touched.Count; i++)
			{
				if (currentGameObjects.Contains(touched[i]) || touched[i] == null)
				{
					touched.RemoveAt(i);
					i--;
				}
				else if (i != touched.Count - 1)
					currentGameObjects.Add(touched[i]);
			}

			currentGameObjects.Clear();

			return true;
		}
	}

	public struct Vector3State
	{
		public bool useX;
		public bool useY;
		public bool useZ;

		/// <summary>
		/// Shorthand for writing <c>Vector3State(false, false, false)</c>.
		/// </summary>
		public static readonly Vector3State False = new Vector3State(false, false, false);

		/// <summary>
		/// Shorthand for writing <c>Vector3State(true, true, true)</c>.
		/// </summary>
		public static readonly Vector3State True = new Vector3State(true, true, true);

		/// <summary>
		/// Shorthand for writing <c>Vector3State(true, false, false)</c>.
		/// </summary>
		public static readonly Vector3State XOnly = new Vector3State(true, false, false);

		/// <summary>
		/// Shorthand for writing <c>Vector3State(false, true, false)</c>.
		/// </summary>
		public static readonly Vector3State YOnly = new Vector3State(false, true, false);

		/// <summary>
		/// Shorthand for writing <c>Vector3State(false, false, true)</c>.
		/// </summary>
		public static readonly Vector3State ZOnly = new Vector3State(false, false, true);

		/// <summary>
		/// The Constructor that allows setting the individual states at instantiation.
		/// </summary>
		/// <param name="x">The X State.</param>
		/// <param name="y">The Y State.</param>
		/// <param name="z">The Z State.</param>
		public Vector3State(bool x, bool y, bool z)
		{
			useX = x;
			useY = y;
			useZ = z;
		}

		/// <summary>
		/// Returns the current state as a <see cref="Vector3"/> representation.
		/// </summary>
		/// <returns>The representation of the current state.</returns>
		public Vector3 ToVector3()
		{
			return new Vector3(useX ? 1f : 0f, useY ? 1f : 0f, useZ ? 1f : 0f);
		}
	}


	public static class GetFirstActive
	{
		static GameObject GameObject(IEnumerable<GameObject> collection)
		{
			foreach (GameObject element in collection)
			{
				if (element.gameObject.activeInHierarchy)
				{
					return element;
				}
			}
			return null;
		}


		static Camera Camera(IEnumerable<Camera> collection)
		{
			foreach (Camera element in collection)
			{
				if (element.gameObject.activeInHierarchy)
				{
					return element;
				}
			}
			return null;
		}
	}

	public class OnBeforeRenderCall
	{
		void temp()
		{
		}
		void AddCameraEvent()
		{
			if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
			{
#if UNITY_2019_1_OR_NEWER
				UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += OnSrpCameraPreRender;
#else
                    Debug.LogWarning("SRP is only supported on Unity 2019.1 or above");
#endif
			}
			else
			{
				Camera.onPreRender += OnCameraPreRender;
			}
		}
		void RemoveCameraEvent()
		{
			if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
			{
#if UNITY_2019_1_OR_NEWER
				UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= OnSrpCameraPreRender;
#else
                    Debug.LogWarning("SRP is only supported on Unity 2019.1 or above");
#endif
			}
			else
			{
				Camera.onPreRender -= OnCameraPreRender;
			}
		}

#if UNITY_2019_1_OR_NEWER
		void OnSrpCameraPreRender(UnityEngine.Rendering.ScriptableRenderContext context, Camera givenCamera)
		{
			//UpdateAction();
		}
#endif

		void OnCameraPreRender(Camera givenCamera)
		{
			//UpdateAction();
		}
	}

	public static class PrecogTimer
	{

		public static IEnumerator GrabPrecogCoroutine(float waitTime, InteractorRaw interactor, System.Action grabEvent)
		{
			float endTime = Time.time + waitTime;

			while (interactor.touch.touchedObjects.Count <= 0 && Time.time <= endTime)
				yield return new WaitForEndOfFrame();

			if (interactor.touch.touchedObjects.Count > 0)
				grabEvent.Invoke();
		}
	}

	public static class Vector3Utils
	{
		public static Vector3 Lerp(Vector3 start, Vector3 end, Vector3 indexes)
		{
			return new Vector3(Mathf.Lerp(start.x, end.x, indexes.x),
								Mathf.Lerp(start.y, end.y, indexes.y),
								 Mathf.Lerp(start.z, end.z, indexes.z));
		}

		public static Vector3 Distance(Vector3 start, Vector3 end)
		{
			return new Vector3(end.x - start.x, end.y - start.y, end.z - start.z);
		}

		public static Vector3 Limit(Vector3 value, float min, float max)
		{
			if (value.x > max)
				value.x = max;
			if (value.x < min)
				value.x = min;

			if (value.y > max)
				value.y = max;
			if (value.y < min)
				value.y = min;

			if (value.z > max)
				value.z = max;
			if (value.z < min)
				value.z = min;

			return value;
		}

		public static Vector3 RemoveY(Vector3 value) => new Vector3(value.x, 0, value.z);

		public static void RemoveY(ref Vector3 value) => value.y = 0;
		/// <summary>
		/// remove y  value
		/// </summary>
		/// <param name="value"></param>
		public static void Flatten(this Vector3 value) => value.y = 0;

		public static Vector3 Multiply(Vector3 val1, Vector3 val2)
		 => new Vector3(
				val1.x * val2.x,
				val1.y * val2.y,
				val1.z * val2.z);

		public static bool ApproxEquals(this Vector3 a, Vector3 b, float tolerance = float.Epsilon)
		{
			float difference = Mathf.Abs(tolerance);
			return LessThanEqual(Abs(a - b), difference);
		}

		public static void Absolutize(this Vector3 vector)
		=> new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));

		public static Vector3 Abs(Vector3 vector)
		=> new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));

		public static Vector3 Divide(Vector3 top, Vector3 bottom)
		 => new Vector3(
				top.x / bottom.x,
				top.y / bottom.y,
				top.z / bottom.z);

		public static Vector3 Clamp(Vector3 value, Vector2 range)
		{
			value.x = Mathf.Max(value.x, range.x);
			value.x = Mathf.Min(value.x, range.y);

			value.y = Mathf.Max(value.y, range.x);
			value.y = Mathf.Min(value.y, range.y);

			value.z = Mathf.Max(value.z, range.x);
			value.z = Mathf.Min(value.z, range.y);
			return value;
		}

		public static Vector3 Clamp01(Vector3 value)
		{
			value.x = Mathf.Max(value.x, 0);
			value.x = Mathf.Min(value.x, 1);

			value.y = Mathf.Max(value.y, 0);
			value.y = Mathf.Min(value.y, 1);

			value.z = Mathf.Max(value.z, 0);
			value.z = Mathf.Min(value.z, 1);
			return value;
		}


		public static bool LessThan(Vector3 less, Vector3 greater)
		{
			return less.x < greater.x 
				&& less.y < greater.y 
				&& less.z < greater.z;
		}
		public static bool GreaterThan(Vector3 less, Vector3 greater)
		{
			return less.x > greater.x 
				&& less.y > greater.y 
				&& less.z > greater.z;
		}
		public static bool LessThanEqual(Vector3 less, Vector3 greater)
		{
			return less.x <= greater.x
				&& less.y <= greater.y
				&& less.z <= greater.z;
		}
		public static bool GreaterThanEqual(Vector3 less, Vector3 greater)
		{
			return less.x >= greater.x
				&& less.y >= greater.y
				&& less.z >= greater.z;
		}
		public static bool LessThan(Vector3 less, float greater)
		{
			return less.x < greater
				&& less.y < greater
				&& less.z < greater;
		}
		public static bool GreaterThan(Vector3 less, float greater)
		{
			return less.x > greater
				&& less.y > greater
				&& less.z > greater;
		}
		public static bool LessThanEqual(Vector3 less, float greater)
		{
			return less.x <= greater
				&& less.y <= greater
				&& less.z <= greater;
		}
		public static bool GreaterThanEqual(Vector3 less, float greater)
		{
			return less.x >= greater
				&& less.y >= greater
				&& less.z >= greater;
		}

	}
	public static class Vector3Extensions
	{
		/// <summary>
		/// Determines if two <see cref="Vector3"/> values are equal based on a given tolerance.
		/// </summary>
		/// <param name="a">The <see cref="Vector3"/> to compare against.</param>
		/// <param name="b">The <see cref="Vector3"/> to compare with.</param>
		/// <param name="tolerance">The tolerance in which the two <see cref="Vector3"/> values can be within to be considered equal.</param>
		/// <returns><see langword="true"/> if the two <see cref="Vector3"/> values are considered equal.</returns>
		public static bool ApproxEquals(this Vector3 a, Vector3 b, float tolerance = float.Epsilon)
		{
			return (Vector3.Distance(a, b) <= tolerance);
		}

		/// <summary>
		/// Divides each component of the given <see cref="Vector3"/> against the given <see cref="float"/>.
		/// </summary>
		/// <param name="dividend">The value to divide by each component.</param>
		/// <param name="divisor">The components to divide with.</param>
		/// <returns>The quotient.</returns>
		public static Vector3 Divide(float dividend, Vector3 divisor)
		{
			return new Vector3(dividend / divisor.x, dividend / divisor.y, dividend / divisor.z);
		}
		/// <summary>
		/// Divides two <see cref="Vector3"/>s component-wise.
		/// </summary>
		/// <param name="dividend">The value to divide by each component.</param>
		/// <param name="divisor">The components to divide with.</param>
		/// <returns>The quotient.</returns>
		public static Vector3 Divide(this Vector3 dividend, Vector3 divisor)
		{
			return Vector3.Scale(dividend, Divide(1, divisor));
		}
	}

	public static class BezierCurveGenerator
	{
		private static Vector3[] calculatedPoints = new Vector3[0];

		public static Vector3[] GeneratePoints(int pointsCount, Vector3[] controlPoints)
		{
			float stepSize = pointsCount != 1 ? 1f / (pointsCount - 1) : pointsCount;

			calculatedPoints = new Vector3[pointsCount];
			for (int index = 0; index < pointsCount; index++)
			{
				calculatedPoints[index] = GeneratePoint(controlPoints, index * stepSize);
			}

			return calculatedPoints;
		}

		/// <summary>
		/// Generates a point at a specific location along the control points.
		/// </summary>
		/// <param name="controlPoints">The collection of points where the point can be generated.</param>
		/// <param name="pointLocation">The specific location along the collection where to generate the point.</param>
		/// <returns></returns>
		private static Vector3 GeneratePoint(IReadOnlyList<Vector3> controlPoints, float pointLocation)
		{
			int index;
			if (pointLocation >= 1f)
			{
				pointLocation = 1f;
				index = controlPoints.Count - 4;
			}
			else
			{
				pointLocation = Mathf.Clamp01(pointLocation) * ((controlPoints.Count - 1) / 3f);
				index = (int)pointLocation;
				pointLocation -= index;
				index *= 3;
			}

			float normalizedPointLocation = Mathf.Clamp01(pointLocation);
			float oneMinusT = 1f - normalizedPointLocation;

			return oneMinusT * oneMinusT * oneMinusT * controlPoints[index] + 3f * oneMinusT * oneMinusT * normalizedPointLocation * controlPoints[index + 1] + 3f * oneMinusT * normalizedPointLocation * normalizedPointLocation * controlPoints[index + 2] + normalizedPointLocation * normalizedPointLocation * normalizedPointLocation * controlPoints[index + 3];
		}
	}
	public static class TransformExtensions
	{
		/// <summary>
		/// The SetGlobalScale method is used to set a <see cref="Transform"/> scale based on a global scale instead of a local scale.
		/// </summary>
		/// <param name="transform">The reference to the <see cref="Transform"/> to scale.</param>
		/// <param name="globalScale">The global scale to apply to the given <see cref="Transform"/>.</param>
		public static void SetGlobalScale(this Transform transform, Vector3 globalScale)
		{
			if (transform == null)
			{
				return;
			}

			transform.localScale = Vector3.one;
			transform.localScale = globalScale.Divide(transform.lossyScale);
		}
	}

	/// <summary>
	/// credit to: Octopoid 
	/// https://answers.unity.com/questions/514293/changing-a-gameobjects-primitive-mesh.html
	/// </summary>
	public static class PrimitiveHelper
	{
		private static Dictionary<PrimitiveType, Mesh> primitiveMeshes = new Dictionary<PrimitiveType, Mesh>();

		public static GameObject CreatePrimitive(PrimitiveType type, bool withCollider)
		{
			if (withCollider) { return GameObject.CreatePrimitive(type); }

			GameObject gameObject = new GameObject(type.ToString());
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = GetPrimitiveMesh(type);
			gameObject.AddComponent<MeshRenderer>();

			return gameObject;
		}

		public static Mesh GetPrimitiveMesh(PrimitiveType type)
		{
			if (!primitiveMeshes.ContainsKey(type))
			{
				CreatePrimitiveMesh(type);
			}

			return primitiveMeshes[type];
		}

		private static Mesh CreatePrimitiveMesh(PrimitiveType type)
		{
			GameObject gameObject = GameObject.CreatePrimitive(type);
			Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
			GameObject.Destroy(gameObject);

			primitiveMeshes[type] = mesh;
			return mesh;
		}
	}
}
