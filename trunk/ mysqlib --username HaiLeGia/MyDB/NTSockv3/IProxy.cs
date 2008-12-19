using System;
using System.Threading;
using NTSockv3.Messages;
using NTSockv3.Sockets;

namespace NTSockv3
{
	public abstract class IProxy
	{
		protected SocketPool pool;
		protected ServiceContainer container;

		public IProxy() {}
		protected void BeginRequest(Request request, WaitCallback callback, object context)
		{
			if (callback == null)
			{
				DoRequest(request);
				return;
			}
			try
			{
				pool.BeginRequest(request, callback, context);
			}
			catch (Exception e)
			{
				Logger.Log.WriteLog(e);
			}
		}

		protected void DoRequest(Request request)
		{
			try
			{
				pool.Request(request);
			}
			catch(Exception e)
			{
				Logger.Log.WriteLog(e);
			}
		}

		protected T RequestSync<T>(Request request)
		{
			try
			{
				return pool.Request<T>(request);
			}
			catch(Exception e)
			{
				Logger.Log.WriteLog(e);
			}
			return default(T);
		}

	}
}