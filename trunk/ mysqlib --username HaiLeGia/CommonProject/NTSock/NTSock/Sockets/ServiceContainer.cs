using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using NTSock.Controller;
using NTSock.Exceptions;
using NTSock.Executors;

namespace NTSock.Sockets
{
	public class ServiceContainer : IDisposable
	{
	    private Logger.PurposeLog executorLog = Logger.LoggerManager.CreateLog(typeof (ServiceContainer));
		private readonly IDictionary<string, IExecutor> methodMaps;
		private bool disposed;
		private ReaderWriterLockSlim servicesLock;
		private SocketPool pool;
		/// <summary>
		/// Internal constructor. This method takes the array of hosts and sets up an internal list of socketpools.
		/// </summary>
		public ServiceContainer()
		{
			disposed = false;
            methodMaps = new Dictionary<string, IExecutor>();
			servicesLock = new ReaderWriterLockSlim();
		}

		public ServiceContainer(string serviceName, object service)
			: this()
		{
			RegisterService(serviceName, service, false);
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

		#endregion

		public void RegisterService(string serviceName, object service, bool isOverride)
		{
			servicesLock.EnterWriteLock();
			try
			{
				if (methodMaps.ContainsKey(serviceName))
				{
					if (!isOverride)
					{
						throw new ArgumentException("Cannot add service: " + serviceName);
					}
					methodMaps.Remove(serviceName);
				}
				var serviceType = service.GetType();
				var methods = serviceType.GetMethods();
				if (methods != null)
				{
					foreach (var method in methods)
					{
						var builder = new StringBuilder(serviceName + " " + method.Name + " ");
						var parameters = method.GetParameters();
						if (parameters.Length > 0)
						{
							foreach (ParameterInfo parameter in parameters)
							{
								builder.Append(parameter.ParameterType + " ");
							}
						}
					    var executor = ExecutorFactory.CreateExecutor(service, method);
						methodMaps.Add(builder.ToString().Trim(), executor);
					}
				}
			}
			finally
			{
				servicesLock.ExitWriteLock();
			}
		}

		public void DeregisterService(string name)
		{
			//Do nothing
		}

		internal Response Excute(Request request)
		{
			var response = new Response { Id = request.Id };
			var methodDescriptionBuilder = request.RequestDescription;
			IExecutor executor = null;
			servicesLock.EnterReadLock();
			try
			{
				methodMaps.TryGetValue(methodDescriptionBuilder, out executor);
			}
			finally
			{
				servicesLock.ExitReadLock();
			}
			if (executor == null)
			{
				response.ResultException = new MethodNotFoundException(request.ServiceName, request.ReceiverMethod);
				return response;
			}
			try
			{
				response.Result = executor.Execute(request.Parameters);
			}
			catch (Exception e)
			{
                executorLog.WriteLog("Exception when invoke type : " + executor.GetType().FullName + ", with exception: " + e);
				response.ResultException = new MethodInvokingException(executor.GetType().FullName, request.ReceiverMethod, e);
				return response;
			}
			return response;
		}

		public void Request(Request request)
		{
			pool.Request(request);
		}

		public void BeginRequest(Request request, WaitCallback callback, object context)
		{
			pool.BeginRequest(request, callback, context);
		}

		internal SocketPool Pool
		{
			set
			{
				pool = value;
			}
		}

		~ServiceContainer()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool b)
		{
			if (servicesLock != null)
			{
				servicesLock.Dispose();
				servicesLock = null;
			}
		}
	}
}