using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using Chord.Common;
using CommonLib;
using Logger;
using NTSock;
using NTSock.Controller;
using NTSock.Sockets;

namespace Chord.FileStorage
{
	public class ProxyCache
	{
		private IDictionary<URL, FileNodeSocketProxy> cache;
		private ReaderWriterLockSlim cacheLock;

		public ProxyCache()
		{
			cacheLock = new ReaderWriterLockSlim();
			cache = new Dictionary<URL, FileNodeSocketProxy>();
		}

		public FileNodeSocketProxy GetProxy(URL url)
		{
			cacheLock.EnterUpgradeableReadLock();
			try
			{
				FileNodeSocketProxy node;
				cache.TryGetValue(url, out node);
				if (node == null)
				{
					cacheLock.EnterWriteLock();
					try
					{
						node = new FileNodeSocketProxy(url);
						cache.Add(url, node);
					}
					finally
					{
						cacheLock.ExitWriteLock();
					}
					return node;
				}
				return node;
			}
			finally
			{
				cacheLock.ExitUpgradeableReadLock();
			}
		}
	}

	public class FileNodeSocketProxy : IProxy, FileNodeProxy
	{
		private const string SERVICENAME = "FileNodeService";
		private ProxyCache cache;
		private Cache<string, byte[]> chunkCache;

		internal FileNodeSocketProxy(URL url)
		{
			container = new ServiceContainer();
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(url.Host), url.Port);
			pool = new SocketPool(container, endPoint);
			chunkCache = new Cache<string, byte[]>(60000); //one minute
		}

		private static readonly TimeSpan chunkLiveTime = new TimeSpan(0, 0, 3, 0);

		#region Implementation of FileNodeProxy

		public void CreateDirectory(string ownerName, string directoryName, WaitCallback callback, object excuteContext)
		{
			Request request = new Request(SERVICENAME, "CreateDirectory", new object[] { ownerName, directoryName });
			DoRequest(request, callback, excuteContext);
		}

		public void CreateDirectory(string ownerName, string pathName, string directoryName, WaitCallback callback, object excuteContext)
		{
			var request = new Request(SERVICENAME, "CreateDirectory", new object[] { ownerName, pathName, directoryName });
			DoRequest(request, callback, excuteContext);
		}

		public void GetDirectoryInformation(string pathName, string directoryName, WaitCallback callback, object excuteContext)
		{
			var request = new Request(SERVICENAME, "GetDirectoryInformation", new object[] { pathName, directoryName });
			DoRequest(request, callback, excuteContext);
		}

		public void GetFileInformation(string pathName, string directoryName, WaitCallback callback, object excuteContext)
		{
			var request = new Request(SERVICENAME, "GetFileInformation", new object[] { pathName, directoryName });
			DoRequest(request, callback, excuteContext);
		}

		public void CreateFile(string fileName, string path, bool isOverride, WaitCallback callback, object excuteContext)
		{
			var request = new Request(SERVICENAME, "CreateFile", new object[] { fileName, path, isOverride });
			DoRequest(request, callback, excuteContext);
		}

		public void WriteAll(string filePath, byte[] chunkData, WaitCallback callback, object excuteContext)
		{
			var keyBuilder = new StringBuilder(filePath);
			var keyCache = keyBuilder.ToString();
			chunkCache.Insert(keyCache, chunkData, chunkLiveTime, true, null);
			keyBuilder.Append(" " + 0);
			keyBuilder.Append(" " + chunkData.Length);
			keyCache = keyBuilder.ToString();
			chunkCache.Insert(keyCache, chunkData, chunkLiveTime, true, null);
			var request = new Request(SERVICENAME, "WriteAll", new object[] { filePath, chunkData });
			DoRequest(request, callback, excuteContext);
		}

		public void WriteChunk(string filePath, byte[] chunkData, int offset, int count, WaitCallback callback, object excuteContext)
		{
			var keyBuilder = new StringBuilder(filePath);
			keyBuilder.Append(" " + offset);
			keyBuilder.Append(" " + count);
			var keyCache = keyBuilder.ToString();
			chunkCache.Insert(keyCache, chunkData, chunkLiveTime, true, null);
			var request = new Request(SERVICENAME, "WriteChunk", new object[] {filePath, chunkData, offset, count});
			DoRequest(request, callback, excuteContext);
		}

		public void WriteChunk(string filePath, byte[] chunkData, WaitCallback callback, object excuteContext)
		{
			var keyBuilder = new StringBuilder(filePath);
			keyBuilder.Append(" " + 0);
			keyBuilder.Append(" " + chunkData.Length);
			var keyCache = keyBuilder.ToString();
			chunkCache.Insert(keyCache, chunkData, chunkLiveTime, true, null);
			var request = new Request(SERVICENAME, "WriteChunk", new object[] { filePath, chunkData });
			DoRequest(request, callback, excuteContext);
		}

		public void ReadChunk(string filePath, int offset, int length, WaitCallback callback, object excuteContext)
		{
			var keyBuilder = new StringBuilder(filePath);
			keyBuilder.Append(" " + offset);
			keyBuilder.Append(" " + length);
			string keyCache = keyBuilder.ToString();
			byte[] result = chunkCache.Get(keyCache);
			if (result != null)
			{
				callback(new ResponseContext(excuteContext, new Response() {Result = result}));
			}
			else
			{
				var request = new Request(SERVICENAME, "ReadChunk", new object[] {filePath, offset, length});
				DoRequest(request, EndReadChunk, new object[] {keyCache, callback, excuteContext});
			}
		}

		private void EndReadChunk(object state)
		{
			try
			{
				var responseContext = (ResponseContext)state;
				var callContext = responseContext.WaitingContext as object[];
				var response = responseContext.Result;
				if (response.ResultException == null)
				{
					var readData = response.Result as byte[];
					if (readData != null)
					{
						chunkCache.Insert((string)callContext[0], readData, chunkLiveTime, true, null);
					}
				}
				var outSideCallback = callContext[1] as WaitCallback;
				outSideCallback(new ResponseContext(callContext[2], response));
			}
			catch (Exception e)
			{
				Log.WriteLog("Exception: " + e);
			}
		}

		public void ReadAll(string filePath, WaitCallback callback, object excuteContext)
		{
			var keyBuilder = new StringBuilder(filePath);
			var keyCache = keyBuilder.ToString();
			var result = chunkCache.Get(keyCache);
			if (result != null)
			{
				callback(new ResponseContext(excuteContext, new Response() {Result = result}));
                return;
			}
			var request = new Request(SERVICENAME, "ReadAll", new object[] {filePath});
			DoRequest(request, EndReadAll, new object[] { keyCache, callback, excuteContext });
		}

		private void EndReadAll(object state)
		{
			try
			{
				var responseContext = (ResponseContext) state;
				var callContext = responseContext.WaitingContext as object[];
				var response = responseContext.Result;
				if (response.ResultException == null)
				{
					var readData = response.Result as byte[];
					if (readData != null)
					{
						var keyCache = (string) callContext[0];
						chunkCache.Insert(keyCache, readData, chunkLiveTime, true, null);
						var keyBuilder = new StringBuilder(keyCache);
						keyBuilder.Append(" " + 0);
						keyBuilder.Append(" " + readData.Length);
						keyCache = keyBuilder.ToString();
						chunkCache.Insert(keyCache, readData, chunkLiveTime, true, null);
					}
				}
				var outSideCallback = callContext[1] as WaitCallback;
				outSideCallback(new ResponseContext(callContext[2], response));
			}
			catch (Exception e)
			{
				Log.WriteLog("Exception: " + e);
			}
		}

		public void DeleteFile(string filePath)
		{
			var request = new Request(SERVICENAME, "DeleteFile", new object[] { filePath });
			DoRequest(request);
		}

		public void MoveFile(string oldPath, string newPath)
		{
			byte[] oldMemData = chunkCache.Get(oldPath);
			if (oldMemData != null)
			{
				chunkCache.Insert(newPath, oldMemData, chunkLiveTime, true, null);
			}
			var request = new Request(SERVICENAME, "MoveFile", new object[] { oldPath, newPath });
			DoRequest(request);
		}

		#endregion
	}
}
