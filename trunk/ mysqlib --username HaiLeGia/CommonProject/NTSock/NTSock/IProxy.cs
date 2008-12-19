using System;
using System.Threading;
using NTSock.Controller;
using NTSock.Sockets;

namespace NTSock
{
	public abstract class IProxy
	{
		protected SocketPool pool;
		protected ServiceContainer container;

		protected void DoRequest(Request request, WaitCallback callback, object context)
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
				
			}
		}
	}
}
