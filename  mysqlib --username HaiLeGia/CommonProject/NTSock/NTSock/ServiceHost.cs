using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Logger;
using NTSock.Sockets;

namespace NTSock
{
	public class ServiceHost
	{
		private readonly ServiceContainer container;
		private readonly IList<SocketPool> pools;
		private ReaderWriterLockSlim poolsLock;
		private readonly int servicePort;
		private Socket serviceSocket;

		public ServiceHost(int servicePort)
		{
			pools = new List<SocketPool>();
			poolsLock = new ReaderWriterLockSlim();
			container = new ServiceContainer();
			this.servicePort = servicePort;
		}

		public ServiceContainer Container
		{
			get { return container; }
		}

		public void Open()
		{
			EndPoint endPoint = new IPEndPoint(IPAddress.Any, servicePort);
			serviceSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			serviceSocket.Bind(endPoint);
			serviceSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			serviceSocket.NoDelay = true;
			serviceSocket.Listen(50);
			serviceSocket.BeginAccept(OnServiceRequest, null);
		}

		public void Close()
		{
			try
			{
				serviceSocket.Close();
				foreach (var pool in pools)
				{
					pool.Dispose();
				}
			}
			catch
			{
			}
			Log.Closed();
		}

		private void OnServiceRequest(IAsyncResult ar)
		{
			Socket client = null;
			try
			{
				client = serviceSocket.EndAccept(ar);
			}
			catch (Exception e)
			{
				Log.WriteLog("WCF: Error while accept connection: " + e);
				
			}
			try
			{
				if (serviceSocket != null)
				{
					serviceSocket.BeginAccept(new AsyncCallback(OnServiceRequest), null);
				}
			}
			catch (Exception e)
			{
				Log.WriteLog("WCF: Error while listen more connections: " + e);
				
			}
			if (client != null)
			{
                var pool = new SocketPool(container, client); ; 
				poolsLock.EnterWriteLock();
				try
				{
					pools.Add(pool);
				}
				finally 
				{
					poolsLock.ExitWriteLock();
				}
			}
		}
	}
}