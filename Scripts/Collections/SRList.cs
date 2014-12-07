﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

/// <summary>
/// IList implementation which does not release the buffer when clearing/removing elements. Based on the NGUI BetterList
/// </summary>
[Serializable]
public class SRList<T> : IList<T>, ISerializationCallbackReceiver
{

	public T[] Buffer
	{
		get { return _buffer; }
		private set { _buffer = value; }
	}

	public int Count
	{
		get { return _count; }
		private set { _count = value; }
	}

	private EqualityComparer<T> _equalityComparer; 

	private EqualityComparer<T> EqualityComparer
	{
		get
		{

			if (_equalityComparer == null)
				_equalityComparer = EqualityComparer<T>.Default;

			return _equalityComparer;

		}
	}

	private ReadOnlyCollection<T> _readOnlyWrapper;

	[SerializeField]
	private T[] _buffer;

	[SerializeField]
	private int _count;

	public SRList() {}
 
	public SRList(int capacity)
	{
		Buffer = new T[capacity];
	}

	/// <summary>
	/// Create a new list with the range of values. Contains a foreach loop, which will allocate garbage when used with most
	/// generic collection types.
	/// </summary>
	public SRList(IEnumerable<T> source)
	{
		AddRange(source);
	} 

	public IEnumerator<T> GetEnumerator()
	{
		if (Buffer != null) {
			for (int i = 0; i < Count; ++i) {
				yield return Buffer[i];
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(T item)
	{

		if (Buffer == null || Count == Buffer.Length)
			Expand();

		Buffer[Count++] = item;

	}

	/// <summary>
	/// Add range of values to the list. Contains a foreach loop, which will allocate garbage when used with most
	/// generic collection types.
	/// </summary>
	/// <param name="range"></param>
	public void AddRange(IEnumerable<T> range)
	{

		foreach (var item in range) {
			Add(item);
		}

	}

	public void Clear()
	{

		Count = 0;

	}

	/// <summary>
	/// Clear the list, optionally setting each element to default(T)
	/// </summary>
	public void Clear(bool clean)
	{
		
		Clear();

		if (!clean)
			return;

		Clean();

	}

	public void Clean()
	{

		if (Buffer == null)
			return;

		for (var i = Count; i < _buffer.Length; i++) {

			_buffer[i] = default(T);

		}

	}

	/// <summary>
	/// Get a read-only wrapper of this list. This is cached, so very little cost after first called.
	/// </summary>
	/// <returns></returns>
	public ReadOnlyCollection<T> AsReadOnly()
	{

		if(_readOnlyWrapper == null)
			_readOnlyWrapper = new ReadOnlyCollection<T>(this);

		return _readOnlyWrapper;

	}

	public bool Contains(T item)
	{

		if (Buffer == null)
			return false;

		for (int i = 0; i < Count; ++i) {

			if (EqualityComparer.Equals(Buffer[i], item))
				return true;

		}

		return false;

	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		Trim();
		Buffer.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		if (Buffer != null) {

			for (int i = 0; i < Count; ++i) {
				if (EqualityComparer.Equals(Buffer[i], item)) {
					--Count;
					Buffer[i] = default(T);
					for (int b = i; b < Count; ++b)
						Buffer[b] = Buffer[b + 1];
					return true;
				}
			}
		}

		return false;
	}

	public bool IsReadOnly
	{
		get { return false; }
	}

	public int IndexOf(T item)
	{

		if (Buffer != null) {

			for (int i = 0; i < Count; ++i) {
				if (EqualityComparer.Equals(Buffer[i], item))
					return i;
			}

		}

		return -1;

	}

	public void Insert(int index, T item)
	{
		if (Buffer == null || Count == Buffer.Length)
			Expand();

		if (index < Count) {
			for (int i = Count; i > index; --i)
				Buffer[i] = Buffer[i - 1];
			Buffer[index] = item;
			++Count;
		} else
			Add(item);
	}

	public void RemoveAt(int index)
	{
		if (Buffer != null && index < Count) {
			--Count;
			Buffer[index] = default(T);
			for (int b = index; b < Count; ++b)
				Buffer[b] = Buffer[b + 1];
		}
	}

	public T this[int index]
	{
		get
		{

			if(Buffer == null)
				throw new IndexOutOfRangeException();

			return Buffer[index];

		}
		set
		{

			if(Buffer == null)
				throw new IndexOutOfRangeException();

			Buffer[index] = value;

		}
	}

	/// <summary>
	///     Helper function that expands the size of the array, maintaining the content.
	/// </summary>
	private void Expand()
	{
		T[] newList = (Buffer != null) ? new T[Mathf.Max(Buffer.Length << 1, 32)] : new T[32];

		if (Buffer != null && Count > 0)
			Buffer.CopyTo(newList, 0);

		Buffer = newList;
	}

	/// <summary>
	///     Trim the unnecessary memory, resizing the buffer to be of 'Length' size.
	///     Call this function only if you are sure that the buffer won't need to resize anytime soon.
	/// </summary>
	public void Trim()
	{
		if (Count > 0) {
			if (Count < Buffer.Length) {
				var newList = new T[Count];
				for (int i = 0; i < Count; ++i)
					newList[i] = Buffer[i];
				Buffer = newList;
			}
		} else
			Buffer = new T[0];
	}


	/// <summary>
	/// List.Sort equivalent.
	/// </summary>
	public void Sort(System.Comparison<T> comparer)
	{
		bool changed = true;

		while (changed) {
			changed = false;

			for (int i = 1; i < Count; ++i) {
				if (comparer.Invoke(Buffer[i - 1], Buffer[i]) > 0) {
					T temp = Buffer[i];
					Buffer[i] = Buffer[i - 1];
					Buffer[i - 1] = temp;
					changed = true;
				}
			}
		}
	}

	public void OnBeforeSerialize()
	{
		Debug.Log("[OnBeforeSerialize] Count: {0}".Fmt(_count));
		// Clean buffer of unused elements before serializing
		Clean();

	}

	public void OnAfterDeserialize()
	{
		Debug.Log("[OnAfterDeserialize] Count: {0}".Fmt(_count));
	}

}
