using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Logger;
using NTSockv3.Messages;
using ObjectSerializer;
using Mail=NTSockv3.Messages.Mail;
using Request=NTSockv3.Messages.Request;

namespace NTSockv3.Sockets
{
	internal class NetworkListner : INetworkListener
	{
		#region Implementation of INetworkListener

		public void NetworkException(Exception e)
		{
			Log.WriteDebug("Network exception: " + e);
		}

		public void NetworkClose(EndPoint endPoint)
		{
			Log.WriteDebug("Network close: " + endPoint);
		}

		public void NetworkConnected(EndPoint endPoint)
		{
			Log.WriteLog("Network connected: " + endPoint);
		}

		#endregion
	}

	public class ResponseContext
	{
		private readonly object waitingContext;
		private readonly NTSockv3.Messages.Response result;

		public ResponseContext(object waitingContext, Response result)
		{
			this.waitingContext = waitingContext;
			this.result = result;
		}

		public object WaitingContext
		{
			get { return waitingContext; }
		}

		public Response Result
		{
			get { return result; }
		}
	}

	internal class SynchResult
	{
		public ManualResetEvent WaitEvent { get; set; }

		public Response Response { get; set; }
	}

	public class SocketPool : IDisposable
	{
		private readonly object connectLock = new object();
		private readonly ServiceContainer container;
		private readonly EndPoint endpoint;
		private IList<PooledSocket> pooledSockets;
		private readonly ReaderWriterLockSlim pooledSocketsLock;
		private readonly int poolSize;
		private readonly IDictionary<long, WaitCallback> waitingCallback;
		private readonly IDictionary<long, object> waitingContext;
		private readonly IDictionary<long, SynchResult> waitHandles;
		private ReaderWriterLockSlim waitHandlesLock;
		private bool connected;
		private int currentSocket;
		private long mailId;
		private INetworkListener networkListener;
		private ReaderWriterLockSlim waitingCallbackLock;
		private BinaryFormatter formatter;

		public SocketPool(int poolSize, ServiceContainer owner, string host)
		{
			formatter = new BinaryFormatter();
			owner.Pool = this;
			this.poolSize = poolSize;
			pooledSockets = new List<PooledSocket>(poolSize);
			container = owner;
			if (host != null)
			{
				endpoint = GetEndPoint(host);
			}
			else
			{
				throw new ArgumentNullException("Host cannot be null.");
			}
			NetworkListener = new NetworkListner();
			waitingCallback = new Dictionary<long, WaitCallback>();
			waitingContext = new Dictionary<long, object>();
			waitHandles = new Dictionary<long, SynchResult>();
			waitingCallbackLock = new ReaderWriterLockSlim();
			pooledSocketsLock = new ReaderWriterLockSlim();
			waitHandlesLock = new ReaderWriterLockSlim();
			Connect();
		}

		public SocketPool(ServiceContainer owner, string host)
		{
			formatter = new BinaryFormatter();
			owner.Pool = this;
			poolSize = Int32.Parse(ConfigurationManager.AppSettings["NTS_MIN_POOL_SIZE"]);
			pooledSockets = new List<PooledSocket>(poolSize);
			container = owner;
			if (host != null)
			{
				endpoint = GetEndPoint(host);
			}
			else
			{
				throw new ArgumentNullException("Host cannot be null.");
			}
			NetworkListener = new NetworkListner();
			waitingCallback = new Dictionary<long, WaitCallback>();
			waitingContext = new Dictionary<long, object>();
			waitHandles = new Dictionary<long, SynchResult>();
			waitingCallbackLock = new ReaderWriterLockSlim();
			waitHandlesLock = new ReaderWriterLockSlim();
			pooledSocketsLock = new ReaderWriterLockSlim();
			Connect();
		}

		public SocketPool(ServiceContainer container, EndPoint endPoint)
		{
			formatter = new BinaryFormatter();
			container.Pool = this;
			poolSize = Int32.Parse(ConfigurationManager.AppSettings["NTS_MIN_POOL_SIZE"]);
			pooledSockets = new List<PooledSocket>(poolSize);
			this.container = container;
			endpoint = endPoint;
			NetworkListener = new NetworkListner();
			waitingCallback = new Dictionary<long, WaitCallback>();
			waitingContext = new Dictionary<long, object>();
			waitingCallbackLock = new ReaderWriterLockSlim();
			waitHandles = new Dictionary<long, SynchResult>();
			waitHandlesLock = new ReaderWriterLockSlim();
			pooledSocketsLock = new ReaderWriterLockSlim();
			Connect();
		}

		internal SocketPool(ServiceContainer container, Socket socket)
		{
			formatter = new BinaryFormatter();
			container.Pool = this;
			connected = true;
			poolSize = 1;
			this.container = container;
			endpoint = socket.RemoteEndPoint;
			NetworkListener = new NetworkListner();
			waitingCallback = new Dictionary<long, WaitCallback>();
			waitingContext = new Dictionary<long, object>();
			waitingCallbackLock = new ReaderWriterLockSlim();
			waitHandles = new Dictionary<long, SynchResult>();
			waitHandlesLock = new ReaderWriterLockSlim();
			pooledSocketsLock = new ReaderWriterLockSlim();
			pooledSockets = new List<PooledSocket>(poolSize) { PooledSocket.CreateNewSocket(socket, this, formatter) };
		}

		internal void AddNewPooledSocket(Socket socket)
		{
			pooledSocketsLock.EnterWriteLock();
			try
			{
				pooledSockets.Add(PooledSocket.CreateNewSocket(socket, this, formatter));
			}
			finally 
			{
				pooledSocketsLock.ExitWriteLock();
			}
		}

		public INetworkListener NetworkListener
		{
			set { networkListener = value; }
		}

		public bool Connect()
		{
			pooledSocketsLock.EnterWriteLock();
			try
			{
				if (connected)
				{
					return true;
				}
				connected = true;
				IList<PooledSocket> tempPools = new List<PooledSocket>(poolSize);
				for (var i = 0; i < pooledSockets.Count; i++)
				{
					try
					{
						if (pooledSockets[i] != null && pooledSockets[i].Connected)
						{
							tempPools.Add(pooledSockets[i]);
						}
						else
						{
							pooledSockets[i].Close();
						}
					}
					catch
					{
					}
				}
				pooledSockets.Clear();
				pooledSockets = tempPools;
				for (int i = 0; i < poolSize; i++)
				{
					if (tempPools.Count >= poolSize)
					{
						break;
					}
					PooledSocket pooledSocket = PooledSocket.CreateNewSocket(endpoint, this, formatter);
					if (pooledSocket != null)
					{
						pooledSockets.Add(pooledSocket);
					}
					else
					{
						connected = false;
						break;
					}
				}
				return connected;
			}
			finally
			{
				pooledSocketsLock.ExitWriteLock();
			}
		}

		private static IPEndPoint GetEndPoint(string host)
		{
			int port;
			string[] split = host.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries);
			if (!Int32.TryParse(split[1], out port))
			{
				Log.WriteLog("Cannot regconize end point: " + host);
				throw new ArgumentException("Unable to parse host: " + host);
			}
			host = split[0];

			//Parse host string.
			IPAddress address;
			if (IPAddress.TryParse(host, out address))
			{
				//host string successfully resolved as an IP address.
			}
			else
			{
				//See if we can resolve it as a hostname
				try
				{
					address = Dns.GetHostEntry(host).AddressList[0];
				}
				catch (Exception e)
				{
					throw new ArgumentException("Unable to resolve host: " + host, e);
				}
			}

			return new IPEndPoint(address, port);
		}

		public void Request(Request request)
		{
			if (!connected)
			{
				connected = Connect();
				if (!connected)
				{
					throw new ApplicationException("Pool not connected.");	
				}
			}
			PooledSocket socket = Acquire();
			if (socket != null)
			{
				socket.Send(request);	
			}
			else
			{
				Log.WriteLog("NTSock: Acquire null socket");
			}
		}

		private static TimeSpan waitTimeSpan = new TimeSpan(0, 0, 0, 10);

		public T Request<T>(Request request)
		{
			if (!connected)
			{
				connected = Connect();
				if (!connected)
				{
					throw new ApplicationException("Pool not connected.");
				}
			}
			var socket = Acquire();
			if (socket != null)
			{
				request.Id = Interlocked.Increment(ref mailId);
				ManualResetEvent waitEvent;
				waitHandlesLock.EnterWriteLock();
				try
				{
					waitEvent = new ManualResetEvent(false);
					waitHandles.Add(request.Id, new SynchResult() { WaitEvent = waitEvent });
				}
				finally
				{
					waitHandlesLock.ExitWriteLock();
				}
				socket.Send(request);
				waitEvent.WaitOne(waitTimeSpan, false);
				waitHandlesLock.EnterWriteLock();
				try
				{
					SynchResult result;
					waitHandles.TryGetValue(request.Id, out result);
					if (result != null)
					{
						waitHandles.Remove(request.Id);
						var response = result.Response;
						if (response != null)
						{
							if (response is ExceptionResponse)
							{
								throw new ApplicationException(((ExceptionResponse) response).Exception);
							}
							return response.GetResult() is T ? (T)response.GetResult() : default(T);
						}
						else
						{
							return default(T);
						}
					}
					else
					{
						Log.WriteLog("Cannot find result for request: " + request);
						return default(T);
					}
				}
				finally
				{
					waitHandlesLock.ExitWriteLock();
				}
			}
			else
			{
				Log.WriteLog("NTSock: Acquire null socket");
			}
			return default(T);
		}

		public void BeginRequest(Request request, WaitCallback callback, object context)
		{
			if (!connected)
			{
				connected = Connect();
				if (!connected)
				{
					throw new ApplicationException("Pool not connected.");
				}
			}
			var socket = Acquire();
			if (socket != null)
			{
				if (callback != null)
				{
					request.Id = Interlocked.Increment(ref mailId);
					waitingCallbackLock.EnterWriteLock();
					try
					{
						waitingCallback.Add(request.Id, callback);
						if (context != null)
						{
							waitingContext.Add(request.Id, context);
						}
					}
					finally
					{
						waitingCallbackLock.ExitWriteLock();
					}
				}
				socket.Send(request);
			}
			else
			{
				Log.WriteLog("NTSock: Acquire null socket");
			}
		}

		internal void ReceiveMail(Mail mail)
		{
			switch (mail.Type)
			{
				case Mail.RESPONSE_TYPE:
					WaitCallback callback = null;
					object context;
					waitingCallbackLock.EnterUpgradeableReadLock();
					try
					{
						waitingCallback.TryGetValue(mail.Id, out callback);
						waitingContext.TryGetValue(mail.Id, out context);
						if (callback != null)
						{
							waitingCallbackLock.EnterWriteLock();
							try
							{
								waitingCallback.Remove(mail.Id);
								waitingContext.Remove(mail.Id);
							}
							finally
							{
								waitingCallbackLock.ExitWriteLock();
							}
						}
					}
					finally
					{
						waitingCallbackLock.ExitUpgradeableReadLock();
					}
					if (callback != null)
					{
						ThreadPool.QueueUserWorkItem(callback, new ResponseContext(context, (Response)mail));	
					}
					else
					{
						waitHandlesLock.EnterReadLock();
						try
						{
							SynchResult result;
							waitHandles.TryGetValue(mail.Id, out result);
							if (result != null)
							{
								result.Response = (Response) mail;
								result.WaitEvent.Set();
							}
						}
						finally
						{
							waitHandlesLock.ExitReadLock();
						}
					}
					break;
				case Mail.REQUEST_TYPE:
					ThreadPool.QueueUserWorkItem(ProcessRequest, mail);
					break;
			}
		}

		private void ProcessRequest(object state)
		{
			try
			{
				var response = container.Excute((Request) state);
				var socket = Acquire();
				if (socket != null)
				{
					socket.Send(response);
				}	
			}
			catch(Exception e)
			{
				Log.WriteLog("Exception when process request: " + e);
			}
		}

		internal void MessageError(PooledSocket socket, Exception e)
		{
//			Log.WriteDebug("Error when deserialzize message from host:  " + e);
		}

		private PooledSocket Acquire()
		{
			pooledSocketsLock.EnterReadLock();
			try
			{
				if (pooledSockets != null && pooledSockets.Count > 0)
				{
					var index = Interlocked.Increment(ref currentSocket) % pooledSockets.Count;
					return pooledSockets[index];	
				}
				return null;
			}
			finally
			{
				pooledSocketsLock.ExitReadLock();
			}
		}

		public void NetworkException(Exception e)
		{
			connected = false;
			networkListener.NetworkException(e);
		}

		public void NetworkClose()
		{
			connected = false;
			networkListener.NetworkClose(endpoint);
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			try
			{
				for (int i = 0; i < pooledSockets.Count; i++)
				{
					pooledSockets[i].Close();
				}
			}
			catch
			{
			}
			waitingCallback.Clear();
			waitingCallbackLock.Dispose();
			waitingCallbackLock = null;
		}

		#endregion

		internal void RemoveSocket(PooledSocket socket)
		{
			pooledSocketsLock.EnterWriteLock();
			try
			{
				if (pooledSockets != null)
				{
					pooledSockets.Remove(socket);
					socket.Close();
				}
			}
			finally
			{
				pooledSocketsLock.ExitWriteLock();
			}
		}
	}
}