using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Util
{
	/// <summary>
	/// Shuffle Bag Algorithm Class. Functions like drawing names out of a hat. It randomizes the sequence of T objects, and then takes T out of the hat. When all T's are removed, then 
	/// the shuffle bag reshuffles and starts from the beginning again.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ShuffleBag<T> : ICollection<T>, IList<T>
	{
		private List<T> data = new List<T>();
		private int cursor = 0;

		/// <summary>
		/// Gets the next value in the list of values.
		/// </summary>
		/// <returns>T: The next value in the randomized sequence.</returns>
		public T Next()
		{

			if (cursor >= data.Count)
			{
				// End of list reached, reshuffle and reset cursor back to start.
				Shuffle();
				cursor = 0;
			}

			return data[cursor++];
		}

		/// <summary>
		/// Helper method that randomizes the sequence of the values in the data list.
		/// </summary>
		private void Shuffle()
		{
			for (int i = 0; i < data.Count; i++)
			{
				int randomIndex = Random.Range(i, data.Count);
				T temp = data[i];
				data[i] = data[randomIndex];
				data[randomIndex] = temp;
			}
		}

		/// <summary>
		/// Constructor that initializes a shuffle bag with a given array of values.
		/// </summary>
		/// <param name="initialValues"> An array of values to initialize the shuffle bag with.</param>
		public ShuffleBag(T[] initialValues)
		{
			for (int i = 0; i < initialValues.Length; i++) Add(initialValues[i]);
		}

		/// <summary>
		/// Constructor that initializes a shuffle bag with a given list of values.
		/// </summary>
		/// <param name="initialValues"> An list of values to initialize the shuffle bag with.</param>
		public ShuffleBag(List<T> initialValues)
		{
			for (int i = 0; i < initialValues.Count; i++) Add(initialValues[i]);
		}

		/// <summary>
		/// Default Shuffle bag with empty list.
		/// </summary>
		public ShuffleBag() { }

		/// <summary>
		/// Getter/Setter that returns or sets the value of the shuffle bag at the given index.
		/// </summary>
		/// <param name="index">The index to set or get from in the shuffle bag.</param>
		/// <returns>T: The object at the given index.</returns>
		public T this[int index] { get => data[index]; set => data[index] = value; }


		public bool IsReadOnly => false;

		public void Add(T item)
		{
			data.Add(item);
			cursor = data.Count - 1;
			Shuffle();
		}

		public void AddRange(IEnumerable<T> collection)
		{
			IEnumerator<T> enumerator = collection.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Add(enumerator.Current);
			}
		}

		public int Count => data.Count;


		public void Clear() { data.Clear(); }

		public bool Contains(T item) { return data.Contains(item); }

		public void CopyTo(T[] array, int arrayIndex)
		{
			foreach (T item in data)
			{
				array.SetValue(item, arrayIndex);
				arrayIndex = arrayIndex + 1;
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			throw new System.NotImplementedException();
		}

		public int IndexOf(T item) { return data.IndexOf(item); }

		public void Insert(int index, T item)
		{
			cursor = data.Count;
			data.Insert(index, item);
		}

		public bool Remove(T item)
		{
			cursor = data.Count - 2;
			return data.Remove(item);
		}

		public void RemoveAt(int index)
		{
			cursor = data.Count - 2;
			data.RemoveAt(index);
		}

		IEnumerator IEnumerable.GetEnumerator(){ return data.GetEnumerator(); }
	}

}
