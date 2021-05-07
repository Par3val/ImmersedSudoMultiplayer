using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Events;

namespace XRTools.Utils.Data
{
	#region FloatRange
	/// <summary>
	/// Specifies a valid range between a lower and upper float value limit.
	/// </summary>
	[System.Serializable]
	public struct FloatRange
	{
		public float min;
		public float max;

		public static readonly FloatRange MinMax = new FloatRange(float.MinValue, float.MaxValue);

		public FloatRange(float minimum, float maximum)
		{
			this.min = minimum;
			this.max = maximum;
		}

		public FloatRange(Vector2 range)
		{
			min = range.x;
			max = range.y;
		}

		public bool Contains(float value)
		{
			return value >= min && value <= max;
		}

		public Vector2 ToVector2()
		{
			return new Vector2(min, max);
		}

		public override string ToString()
		{
			return $"({min}, {max})";
		}

		public static FloatRange operator +(FloatRange range, float div) => new FloatRange(range.max + div, range.max + div);
		public static FloatRange operator -(FloatRange range, float div) => new FloatRange(range.max - div, range.max - div);

		public static FloatRange operator *(FloatRange range, float div) => new FloatRange(range.max * div, range.max * div);
		public static FloatRange operator /(FloatRange range, float div) => new FloatRange(range.max / div, range.max / div);
	}

#if UNITY_EDITOR

	[CustomPropertyDrawer(typeof(FloatRange))]
	public class Limits2DDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			//label.tooltip = EditorHelper.GetTooltipAttribute(fieldInfo)?.tooltip ?? string.Empty;
			SerializedProperty minProperty = property.FindPropertyRelative("min");
			SerializedProperty maxProperty = property.FindPropertyRelative("max");

			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			float updatePositionX = position.x;
			const float labelWidth = 30f;
			float fieldWidth = (position.width / 3f) - labelWidth;

			using (new EditorGUI.PropertyScope(position, GUIContent.none, minProperty))
			{
				EditorGUI.LabelField(new Rect(updatePositionX, position.y, labelWidth, position.height), "Min");
				updatePositionX += labelWidth;
				minProperty.floatValue = EditorGUI.FloatField(
					new Rect(updatePositionX, position.y, fieldWidth, position.height),
					minProperty.floatValue);
				updatePositionX += fieldWidth;
			}

			using (new EditorGUI.PropertyScope(position, GUIContent.none, maxProperty))
			{
				EditorGUI.LabelField(new Rect(updatePositionX, position.y, labelWidth, position.height), "Max");
				updatePositionX += labelWidth;
				maxProperty.floatValue = EditorGUI.FloatField(
					new Rect(updatePositionX, position.y, fieldWidth, position.height),
					maxProperty.floatValue);
				updatePositionX += fieldWidth;
			}

			EditorGUI.indentLevel = indent;
		}
	}




#endif
	#endregion

	#region HeapAllocationFreeReadOnlyList
	/// <summary>
	/// Represents a read-only collection of elements that can be accessed by index. Accessing it will not create any heap allocations.
	/// </summary>
	/// <typeparam name="T">The type of the elements.</typeparam>
	public struct HeapAllocationFreeReadOnlyList<T> : IReadOnlyList<T>
	{
		/// <summary>
		/// Enumerates a <see cref="IList{T}"/>.
		/// </summary>
		public struct Enumerator : IEnumerator<T>
		{
			private readonly IList<T> list;
			private readonly int start;
			private readonly int count;
			private int index;

			/// <inheritdoc />
			public T Current { get; private set; }

			/// <inheritdoc />
			object IEnumerator.Current
			{
				get
				{
					if (index == start || index == count + 1)
					{
						throw new InvalidOperationException();
					}

					return Current;
				}
			}

			/// <summary>
			/// Creates a new <see cref="Enumerator"/> that can enumerate the given <see cref="IList{T}"/>.
			/// </summary>
			/// <param name="list">The list to enumerate.</param>
			/// <param name="start">The index to start enumerating at.</param>
			/// <param name="count">How many items to enumerate over.</param>
			public Enumerator(IList<T> list, int start, int count)
			{
				this.list = list ?? Array.Empty<T>();
				this.start = start;
				this.count = count;
				index = start;
				Current = default;
			}

			/// <inheritdoc />
			public void Dispose()
			{
			}

			/// <inheritdoc />
			public bool MoveNext()
			{
				if (index < count)
				{
					Current = list[index];
					index++;
					return true;
				}

				index = count + 1;
				Current = default;
				return false;
			}

			/// <inheritdoc />
			public void Reset()
			{
				index = start;
				Current = default;
			}
		}

		private readonly IList<T> list;
		private readonly int start;
		private readonly int count;

		/// <inheritdoc/>
		public int Count => list?.Count ?? 0;
		/// <inheritdoc/>
		public T this[int index] => (list ?? Array.Empty<T>())[index];

		/// <summary>
		/// Creates a new instance of <see cref="HeapAllocationFreeReadOnlyList{T}"/>.
		/// </summary>
		/// <param name="list">The list to enumerate.</param>
		/// <param name="start">The index to start enumerating at.</param>
		/// <param name="count">How many items to enumerate over.</param>
		public HeapAllocationFreeReadOnlyList(IList<T> list, int start, int count)
		{
			this.list = list;
			this.start = start;
			this.count = count;
		}

		/// <summary>
		/// Implicitly converts an instance of <see cref="List{T}"/> to a <see cref="HeapAllocationFreeReadOnlyList{T}"/>.
		/// </summary>
		/// <param name="list">The <see cref="List{T}"/> to convert.</param>
		public static implicit operator HeapAllocationFreeReadOnlyList<T>(List<T> list)
		{
			return new HeapAllocationFreeReadOnlyList<T>(list, 0, list?.Count ?? 0);
		}

		/// <summary>
		/// Implicitly converts an instance of <see cref="T:T[]"/> to a <see cref="HeapAllocationFreeReadOnlyList{T}"/>.
		/// </summary>
		/// <param name="array">The <see cref="T:T[]"/> to convert.</param>
		public static implicit operator HeapAllocationFreeReadOnlyList<T>(T[] array)
		{
			return new HeapAllocationFreeReadOnlyList<T>(array, 0, array?.Length ?? 0);
		}

		/// <summary>
		/// Implicitly converts an instance of <see cref="ArraySegment{T}"/> to a <see cref="HeapAllocationFreeReadOnlyList{T}"/>.
		/// </summary>
		/// <param name="arraySegment">The <see cref="ArraySegment{T}"/> to convert.</param>
		public static implicit operator HeapAllocationFreeReadOnlyList<T>(ArraySegment<T> arraySegment)
		{
			return new HeapAllocationFreeReadOnlyList<T>(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the elements.
		/// </summary>
		/// <returns>An enumerator to iterate through the elements.</returns>
		public Enumerator GetEnumerator()
		{
			return new Enumerator(list, start, count);
		}

		/// <inheritdoc/>
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
	#endregion
}