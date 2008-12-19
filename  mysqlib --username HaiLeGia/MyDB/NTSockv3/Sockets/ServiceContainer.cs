#define DEBUG
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using BLToolkit.Reflection.Emit;
using NTSockv3.Exceptions;
using NTSockv3.Excutors;
using NTSockv3.Messages;

namespace NTSockv3.Sockets
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

		public ServiceContainer(object service, bool isOverride)
			: this()
		{
			RegisterService(service, isOverride);
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

		public void RegisterService(object service, bool isOverride)
		{
			var originalAsmName = new AssemblyName("NTSock" + service.GetType().FullName);
			var assemblyBuilderHelper = new AssemblyBuilderHelper(originalAsmName.Name + ".dll");
			servicesLock.EnterWriteLock();
			try
			{
				var serviceType = service.GetType();
				var methods = serviceType.GetMethods();
				if (methods != null)
				{
					for (var i = 0; i < methods.Length; i++)
					{
						var method = methods[i];
						if (method.GetCustomAttributes(typeof(ServiceMethodAttribute), false).Length == 0)
						{
							continue;
						}
						var executor = ExecutorFactory.CreateExecutor(service, method, i, assemblyBuilderHelper);
						if (methodMaps.ContainsKey(executor.ExecutorKey))
						{
							if (!isOverride)
							{
								throw new ArgumentException("Cannot override an existing service.");
							}
							methodMaps.Remove(executor.ExecutorKey);
						}
						methodMaps.Add(executor.ExecutorKey, executor);
					}
					ExecutorFactory.CreateProxy(service, assemblyBuilderHelper);
				}
#if DEBUG
				assemblyBuilderHelper.Save();
#endif
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
			var requestType = request.requestDescription;
			IExecutor executor = null;
			servicesLock.EnterReadLock();
			try
			{
				methodMaps.TryGetValue(requestType, out executor);
			}
			finally
			{
				servicesLock.ExitReadLock();
			}
			Response response;
			if (executor != null)
			{
				try
				{
					response = executor.Execute(request);
					response.Id = request.Id;	
				}
				catch(Exception e)
				{
					executorLog.WriteLog("Exception when invoke type : " + executor.GetType().FullName + ", with exception: " + e);
					response = new ExceptionResponse(request.Id, e);
				}
			}
			else
			{
				response = new ExceptionResponse(request.Id, new ServiceNotFoundException(request.ToString()));
			}
			return response;
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