using System.Collections.Generic;
using UnityEngine;

namespace Util
{
	/// <summary>
	/// A Shuffle Bag that randomizes the order of elements and cycles through them.
	/// When all elements are drawn, the bag reshuffles and starts again.
	/// </summary>
	/// <typeparam name="T">The type of elements contained in the shuffle bag.</typeparam>
	public class ShuffleBag<T>
	{
		private List<T> data = new List<T>(); // Internal list storing elements
		private int cursor = 0; // Tracks the current position in the shuffled sequence

		/// <summary>
		/// Creates an empty Shuffle Bag.
		/// </summary>
		public ShuffleBag() { }

		/// <summary>
		/// Creates a Shuffle Bag with an initial set of values.
		/// </summary>
		/// <param name="initialValues">The initial set of values to populate the shuffle bag.</param>
		public ShuffleBag(IEnumerable<T> initialValues)
		{
			AddRange(initialValues);
		}

		/// <summary>
		/// Adds an item to the shuffle bag and reshuffles the contents.
		/// </summary>
		/// <param name="item">The item to be added.</param>
		public void Add(T item)
		{
			data.Add(item);
			Shuffle();
		}

		/// <summary>
		/// Adds multiple items to the shuffle bag and reshuffles the contents.
		/// </summary>
		/// <param name="collection">A collection of items to add.</param>
		public void AddRange(IEnumerable<T> collection)
		{
			data.AddRange(collection);
			Shuffle();
		}

		/// <summary>
		/// Retrieves the next item in the shuffled sequence. If all items have been drawn, the bag reshuffles and starts again.
		/// </summary>
		/// <returns>The next item in the sequence.</returns>
		/// <exception cref="System.InvalidOperationException">Thrown if the shuffle bag is empty.</exception>
		public T Next()
		{
			if (data.Count == 0)
				throw new System.InvalidOperationException("ShuffleBag is empty.");

			if (cursor >= data.Count)
			{
				Shuffle();
				cursor = 0;
			}

			return data[cursor++];
		}

		/// <summary>
		/// Randomizes the order of elements in the shuffle bag.
		/// </summary>
		private void Shuffle()
		{
			for (int i = 0; i < data.Count; i++)
			{
				int randomIndex = Random.Range(i, data.Count);
				(data[i], data[randomIndex]) = (data[randomIndex], data[i]); // Swap elements
			}
		}

		/// <summary>
		/// Clears all elements from the shuffle bag.
		/// </summary>
		public void Clear()
		{
			data.Clear();
			cursor = 0;
		}

		/// <summary>
		/// Gets the number of items currently in the shuffle bag.
		/// </summary>
		public int Count => data.Count;
	}
}
