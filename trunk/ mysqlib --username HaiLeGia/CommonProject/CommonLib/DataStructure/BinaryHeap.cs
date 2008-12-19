using System;
using System.Collections.Generic;
using System.Threading;
using Logger;

namespace CommonLib.DataStructure
{
	internal class HeapEntry<T>
	{
		internal IComparable key;
		internal T value;

		public HeapEntry()
		{
		}

		public HeapEntry(IComparable key, T value)
		{
			this.key = key;
			this.value = value;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			HeapEntry<T> other = obj as HeapEntry<T>;
			if (other != null)
			{
				return value.Equals(other.value);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}
	}

	public class BinaryHeap<T> : IDisposable
	{
		public const int ASC = 0;
		public const int DESC = 1;

		private IList<HeapEntry<T>> innerList;
		private bool isSynchronized;
		private ReaderWriterLockSlim lck;
		private int orderType;
		private bool disposed;

		public BinaryHeap(bool isSynchronized, int orderType)
		{
			disposed = false;
			innerList = new List<HeapEntry<T>>();
			this.isSynchronized = isSynchronized;
			lck = new ReaderWriterLockSlim();
			this.orderType = orderType;
		}

		public BinaryHeap(int capacity, bool isSynchronized, int orderType)
		{
			disposed = false;
			innerList = new List<HeapEntry<T>>(capacity);
			this.isSynchronized = isSynchronized;
			lck = new ReaderWriterLockSlim();
			this.orderType = orderType;
		}

	    public IList<T> GetAll()
	    {
	        lck.EnterReadLock();
            try
            {
                IList<T> result = new List<T>();
                foreach (HeapEntry<T> entry in innerList)
                {
                    result.Add(entry.value);
                }
                return result;
            }
            finally
            {
                lck.ExitReadLock();
            }
	    }

	    public bool IsSynchronized
		{
			get { return isSynchronized; }
		}

		public T Head
		{
			get
			{
				if (innerList.Count > 0)
				{
					return innerList[0].value;
				}
				return (T) (object) null;
			}
		}

		public void Update(IComparable newKey, T value)
		{
			if (isSynchronized)
			{
				lck.EnterWriteLock();
				try
				{
					_Update(newKey, value);
				}
				finally
				{
					lck.ExitWriteLock();
				}
			}
			else
			{
				_Update(newKey, value);
			}
		}

		public void Add(IComparable key, T value)
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}
			if (isSynchronized)
			{
				lck.EnterWriteLock();
				try
				{
					_Add(key, value);
				}
				finally
				{
					lck.ExitWriteLock();
				}
			}
			else
			{
				_Add(key, value);
			}
		}

		public T Remove()
		{
			if (isSynchronized)
			{
				lck.EnterWriteLock();
				try
				{
					return _Remove();
				}
				finally
				{
					lck.ExitWriteLock();
				}
			}
			else
			{
				return _Remove();
			}
		}

		public bool Contain(T value)
		{
			HeapEntry<T> temp = new HeapEntry<T>();
			temp.value = value;
			if (isSynchronized)
			{
				lck.EnterReadLock();
				try
				{
					return innerList.Contains(temp);
				}
				finally
				{
					lck.ExitReadLock();
				}
			}
			else
			{
				return innerList.Contains(temp);
			}
		}

		private void _Update(IComparable key, T value)
		{
			HeapEntry<T> entry = new HeapEntry<T>(key, value);
			int index = innerList.IndexOf(entry);
			if (index == -1)
			{
				_Add(key, value);
			}
			else
			{
				int curPos = index;
				HeapEntry<T> currentEntry = innerList[index];
				if (currentEntry.key.CompareTo(key) != 0)
				{
					currentEntry.key = entry.key;
					if (innerList.Count <= 1)
					{
						return;
					}
					bool finished = false;
					//check to see if we need swap with its children
					while (true)
					{
						int swapIndex = curPos;
						int leftIndex = curPos*2 + 1;
						if (leftIndex < innerList.Count)
						{
							HeapEntry<T> entry1 = innerList[curPos];
							HeapEntry<T> entry2 = innerList[leftIndex];
							switch (orderType)
							{
								case ASC:
									if (entry1.key.CompareTo(entry2.key) > 0)
									{
										swapIndex = leftIndex;
									}
									break;
								case DESC:
									if (entry1.key.CompareTo(entry2.key) < 0)
									{
										swapIndex = leftIndex;
									}
									break;
							}
						}
						else
						{
							break;
						}
						int rightIndex = curPos*2 + 2;
						if (rightIndex < innerList.Count)
						{
							HeapEntry<T> entry1 = innerList[swapIndex];
							HeapEntry<T> entry2 = innerList[rightIndex];
							switch (orderType)
							{
								case ASC:
									if (entry1.key.CompareTo(entry2.key) > 0)
									{
										swapIndex = rightIndex;
									}
									break;
								case DESC:
									if (entry1.key.CompareTo(entry2.key) < 0)
									{
										swapIndex = rightIndex;
									}
									break;
							}
						}

						if (curPos != swapIndex)
						{
							HeapEntry<T> temp = innerList[curPos];
							innerList[curPos] = innerList[swapIndex];
							innerList[swapIndex] = temp;
							curPos = swapIndex;
							if (!finished)
							{
								finished = true;
							}
						}
						else
						{
							break;
						}
					}

					//check to see if we need swap with parent
					if (!finished)
					{
						bool exit = false;
						while (index > 0 && !exit)
						{
							int parentIndex = (index - 1)/2;
							HeapEntry<T> parentEntry = innerList[parentIndex];
							switch (orderType)
							{
								case ASC:
									if (currentEntry.key.CompareTo(parentEntry.key) < 0)
									{
										innerList[index] = parentEntry;
										index = parentIndex;
									}
									else
									{
										exit = true;
									}
									break;
								case DESC:
									if (currentEntry.key.CompareTo(parentEntry.key) > 0)
									{
										innerList[index] = parentEntry;
										index = parentIndex;
									}
									else
									{
										exit = true;
									}
									break;
							}
						}
						innerList[index] = currentEntry;
					}
				}
			}
		}

		private T _Remove()
		{
			if (innerList.Count <= 0)
			{
				throw new IndexOutOfRangeException("Cannot remove from an emty heap");
			}
			HeapEntry<T> result = innerList[0];
			innerList.RemoveAt(0);
			if (innerList.Count > 1)
			{
				innerList.Insert(0, innerList[innerList.Count - 1]);
				innerList.RemoveAt(innerList.Count - 1);

				int currentPos = 0;
				int swapPos = 0;
				while (true)
				{
					HeapEntry<T> parentEntry = innerList[currentPos];
					int leftChildPos = currentPos*2 + 1;
					if (leftChildPos < innerList.Count)
					{
						HeapEntry<T> leftEntry = innerList[leftChildPos];
						switch (orderType)
						{
							case ASC:
								if (parentEntry.key.CompareTo(leftEntry.key) > 0)
								{
									swapPos = leftChildPos;
								}
								break;
							case DESC:
								if (parentEntry.key.CompareTo(leftEntry.key) < 0)
								{
									swapPos = leftChildPos;
								}
								break;
						}
					}
					else
					{
						break;
					}

					int rightChildPos = currentPos*2 + 2;
					if (rightChildPos < innerList.Count)
					{
						HeapEntry<T> swapEntry = innerList[swapPos];
						HeapEntry<T> rightEntry = innerList[rightChildPos];
						switch (orderType)
						{
							case ASC:
								if (swapEntry.key.CompareTo(rightEntry.key) > 0)
								{
									swapPos = rightChildPos;
								}
								break;
							case DESC:
								if (swapEntry.key.CompareTo(rightEntry.key) < 0)
								{
									swapPos = rightChildPos;
								}
								break;
						}
					}

					if (currentPos != swapPos)
					{
						HeapEntry<T> temp = innerList[currentPos];
						innerList[currentPos] = innerList[swapPos];
						innerList[swapPos] = temp;
						currentPos = swapPos;
					}
					else
					{
						break;
					}
				}
			}
			return result.value;
		}

		private void _Add(IComparable key, T value)
		{
			HeapEntry<T> entry = new HeapEntry<T>(key, value);
			innerList.Add(entry);
			int pos = innerList.Count - 1;
			if (innerList.Count > 1)
			{
				bool exit = false;
				while (pos > 0 && !exit)
				{
					int nextPos = (pos - 1)/2;
					HeapEntry<T> suitableEntry = innerList[nextPos];
					switch (orderType)
					{
						case ASC:
							if (key.CompareTo(suitableEntry.key) < 0)
							{
								innerList[pos] = suitableEntry;
								pos = nextPos;
							}
							else
							{
								exit = true;
							}
							break;
						case DESC:
							if (key.CompareTo(suitableEntry.key) > 0)
							{
								innerList[pos] = suitableEntry;
								pos = nextPos;
							}
							else
							{
								exit = true;
							}
							break;
					}
				}
				innerList[pos] = entry;
			}
		}

        public void Print()
        {
            Logger.Log.WriteLog("--------------------------------------");
            for (int i = 0; i < innerList.Count; i++)
            {
                HeapEntry<T> entry = innerList[i];
                Logger.Log.WriteLog(entry.value.ToString());
            }
        }

		#region IDisposable Members

		///<summary>
		///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		///<filterpriority>2</filterpriority>
		public void Dispose()
		{
			if (!disposed)
			{
				Dispose(true);
				GC.SuppressFinalize(this);
				disposed = true;
			}
		}

		protected virtual void Dispose(bool b)
		{
			if (lck != null)
			{
				lck.Dispose();
				lck = null;
			}
		}

		~BinaryHeap()
		{
			Dispose(false);
		}
		#endregion
	}
}
