using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

[assembly : CompilationRelaxations(CompilationRelaxations.NoStringInterning)]

namespace CommonLib
{
	public delegate void ObjectRemoved(object value);

	internal class CacheEntry<V>
	{
		private readonly ObjectRemoved removeDelegate;
		private DateTime deadTime;
		private bool isTimeRelative;
		private TimeSpan liveTime;
		private V value;

		public CacheEntry(V value)
			: this(value, new TimeSpan(1, 0, 0, 0), true, null)
		{
		}

		public CacheEntry(V value, TimeSpan liveTime, bool isTimeRelative, ObjectRemoved removeDelegate)
		{
			this.value = value;
			deadTime = DateTime.Now.Add(liveTime);
			this.liveTime = this.liveTime;
			this.isTimeRelative = isTimeRelative;
			this.removeDelegate = removeDelegate;
		}

		public V Value
		{
			get { return value; }
			set { this.value = value; }
		}

		public ObjectRemoved RemoveDelegate
		{
			get { return removeDelegate; }
		}

		public TimeSpan LiveTime
		{
			get { return liveTime; }
			set { liveTime = value; }
		}

		public DateTime DeadTime
		{
			get { return deadTime; }
			set { deadTime = value; }
		}

		public bool IsTimeRelative
		{
			get { return isTimeRelative; }
			set { isTimeRelative = value; }
		}
	}

	public class Cache<K, V> : IDisposable
	{
		private IDictionary<K, CacheEntry<V>> localCache;
		private ReaderWriterLockSlim localCacheLock;
		private readonly Timer cleanTimer;
		private bool disposed;

		public Cache(long primTime)
		{
			localCache = new Dictionary<K, CacheEntry<V>>();
			localCacheLock = new ReaderWriterLockSlim();
			cleanTimer = new Timer(RemoveObject, null, 0, primTime);
			disposed = false;
		}

		public IList<V> GetAllValues()
		{
			IList<V> result = new List<V>();
			localCacheLock.EnterReadLock();
			try
			{
				ICollection<CacheEntry<V>> values = localCache.Values;
				foreach (var value in values)
				{
					result.Add(value.Value);
				}
			}
			catch {}
			finally
			{
				localCacheLock.ExitReadLock();
			}
			return result;
		}

		public V this[K key]
		{
			get { return Get(key); }
			set { Insert(key, value); }
		}

		public void Remove(K key)
		{
			localCacheLock.EnterWriteLock();
			try
			{
				if (localCache.ContainsKey(key))
				{
					CacheEntry<V> entry = localCache[key];
					localCache.Remove(key);
					NotifyRemoveObject(entry);
				}
			}
			finally
			{
				localCacheLock.ExitWriteLock();
			}
		}

		public bool ContainKey(K key)
		{
			localCacheLock.EnterReadLock();
			try
			{
				return localCache.ContainsKey(key);
			}
			finally
			{
				localCacheLock.ExitReadLock();
			}	
		}

		private void Insert(K key, V value)
		{
			localCacheLock.EnterUpgradeableReadLock();
			try
			{
				if (localCache.ContainsKey(key))
				{
					CacheEntry<V> entry = localCache[key];
					entry.Value = value;
					if (entry.IsTimeRelative)
					{
						entry.DeadTime.Add(entry.LiveTime);
					}
				}
				else
				{
					localCacheLock.EnterWriteLock();
					try
					{
						localCache.Add(key, new CacheEntry<V>(value));
					}
					finally
					{
						localCacheLock.ExitWriteLock();
					}
				}
			}
			finally
			{
				localCacheLock.ExitUpgradeableReadLock();
			}
		}

		public V Get(K key)
		{
			localCacheLock.EnterReadLock();
			try
			{
				CacheEntry<V> entry;
				localCache.TryGetValue(key, out entry);
				if (entry != null)
				{
					if (entry.IsTimeRelative)
					{
						entry.DeadTime = DateTime.Now.Add(entry.LiveTime);
					}
					return entry.Value;
				}
				return default(V);
			}
			finally
			{
				localCacheLock.ExitReadLock();
			}
		}

		public void Insert(K key, V value, TimeSpan liveTime, bool isRelative, ObjectRemoved removeDelegate)
		{
			localCacheLock.EnterUpgradeableReadLock();
			try
			{
				if (localCache.ContainsKey(key))
				{
					CacheEntry<V> entry = localCache[key];
					entry.Value = value;
					entry.LiveTime = liveTime;
					entry.IsTimeRelative = isRelative;
					if (isRelative)
					{
						entry.DeadTime = DateTime.Now.Add(entry.LiveTime);
					}
				}
				else
				{
					localCacheLock.EnterWriteLock();
					try
					{
						localCache.Add(key, new CacheEntry<V>(value, liveTime, isRelative, removeDelegate));
					}
					finally
					{
						localCacheLock.ExitWriteLock();
					}
				}
			}
			finally
			{
				localCacheLock.ExitUpgradeableReadLock();
			}
		}

		private void RemoveObject(object state)
		{
			KeyValuePair<K, CacheEntry<V>>[] objects;
			IList<K> removeObjects = new List<K>();

			localCacheLock.EnterReadLock();
			try
			{
				objects = new KeyValuePair<K, CacheEntry<V>>[localCache.Count];
				localCache.CopyTo(objects, 0);
			}
			finally
			{
				localCacheLock.ExitReadLock();
			}
			DateTime now = DateTime.Now;
			for (int i = 0; i < objects.Length; i++)
			{
				CacheEntry<V> entryI = objects[i].Value;
				DateTime deadTime = entryI.DeadTime;
				if (deadTime <= now)
				{
					removeObjects.Add(objects[i].Key);
				}
			}

			if (removeObjects.Count > 0)
			{
				localCacheLock.EnterWriteLock();
				try
				{
					now = DateTime.Now;
					foreach (K key in removeObjects)
					{
						if (localCache.ContainsKey(key))
						{
							CacheEntry<V> entryI = localCache[key];
							if (!entryI.IsTimeRelative || entryI.DeadTime <= now)
							{
								localCache.Remove(key);
								ThreadPool.QueueUserWorkItem(NotifyRemoveObject, entryI);
							}
						}
					}
				}
				finally
				{
					localCacheLock.ExitWriteLock();
				}
			}
		}

		private static void NotifyRemoveObject(object state)
		{
			CacheEntry<V> entry = (CacheEntry<V>)state;
			if (entry.RemoveDelegate != null)
			{
				entry.RemoveDelegate(entry.Value);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~Cache()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				localCacheLock.Dispose();
				localCache.Clear();
				cleanTimer.Dispose();
				disposed = true;
			}
		}
	}	
}