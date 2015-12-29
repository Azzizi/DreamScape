using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIL.Utilities
{
	public class CircularArray<T>
	{
		private T[] data;
		private int head = -1, tail = -1;

		/// <summary>
		/// If true, when adding an item to a full array, the array will overwrite and move the head(tail in a reversed array).
		/// </summary>
		public bool CapacityOverwrite { get; set; }

		/// <summary>
		/// If true, the array adds to the head instead of the tail.
		/// </summary>
		public bool AddToHead { get; set; }

		public T Head 
		{
			get { if (head >= 0 && head <= data.Length) return data[head]; else return default(T); }
			set { if (head >= 0 && head <= data.Length) data[head]= value; } 
}
		public T Tail 
		{
			get { if (tail >= 0 && tail <= data.Length) return data[tail]; else return default(T); }
			set { if (tail >= 0 && tail <= data.Length) data[tail] = value; }
		}

		public int Length 
		{ 
			get 
			{
				if (head == -1 && tail == -1)
					return 0;
				else if (head == -1 || tail == -1)
					return 1;
				else
					return (head > tail) ? (data.Length - head + tail + 1) : (tail - head + 1); 
			} 
		}

		public CircularArray(int capacity)
		{
			data = new T[capacity];
		}

		public T this[int index] { get { return data[index]; } set { data[index] = value; } }

		/// <summary>
		/// Add an item to the array.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <returns>Returns the index of the added item or -1 if full and CapacityOverwrite is false.</returns>
		public int Add(T item)
		{
			if (AddToHead)
			{
				if (head == -1)
					head = 0;

				else if (tail == -1)
					tail = 0;

				else
				{
					int temp = PrevItem(head);
					if (!CapacityOverwrite && temp == tail)
						return -1;
					
					else if (temp == tail)
						tail = PrevItem(temp);

					head = temp;
				}

				data[head] = item;
				return head;
			}
			else
			{
				if (tail == -1)
					tail = 0;

				else if (head == -1)
					head = 0;

				else
				{
					int temp = NextItem(tail);
					if ( !CapacityOverwrite && temp == head )
						return -1;
					
					else if (temp == head)
						head = NextItem(temp);
					
					tail = temp;
				}
				data[tail] = item;
				return tail;
			}
		}

		/// <summary>
		/// Removes one item off the tail of the array.
		/// </summary>
		public void RemoveTail()
		{
			if (tail == head)
				return;
			tail = PrevItem(tail);
		}

		/// <summary>
		/// Removes one item off the head of the array.
		/// </summary>
		public void RemoveHead()
		{
			if (tail == head)
				return;
			head = NextItem(head);
		}

		/// <summary>
		/// Clear the array and free item data.
		/// </summary>
		public void Clear()
		{
			for (int c = 0; c < data.Length; c++)
				data[c] = default(T);

			head = tail = -1;
		}

		int NextItem(int index)
		{
			return (index + 1) % data.Length;
		}

		int PrevItem(int index)
		{
			return (index - 1 + data.Length) % data.Length;
		}

		/// <summary>
		/// Traverse the array from head to tail and perform an action on each item.
		/// </summary>
		/// <param name="processor">The action to perform on each item.</param>
		public void Traverse(ArrayItemProcessor processor)
		{
			if (head == -1 && tail == -1)
				return;
			else if (head == -1)
			{
				processor(data[tail], tail);
				return;
			}
			else if (tail == -1)
			{
				processor(data[head], head);
				return;
			}

			int index = head;
			while (index != tail)
			{
				processor(data[index], index);
				index = NextItem(index);
			}

			processor(data[tail], tail);
		}

		/// <summary>
		/// Traverse the array from tail to head and perform an action on each item.
		/// </summary>
		/// <param name="processor">The action to perform on each item.</param>
		public void TraverseReversed(ArrayItemProcessor processor)
		{
			if (head == -1 && tail == -1)
				return;
			else if (head == -1)
			{
				processor(data[tail], tail);
				return;
			}
			else if (tail == -1)
			{
				processor(data[head], head);
				return;
			}

			int index = tail;
			while (index != head)
			{
				processor(data[index], index);
				index = PrevItem(index);
			}
			processor(data[head], head);
		}
		public delegate void ArrayItemProcessor(T item, int index);
	}
}
