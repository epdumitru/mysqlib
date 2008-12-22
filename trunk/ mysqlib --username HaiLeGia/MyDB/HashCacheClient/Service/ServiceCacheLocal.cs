using System;
using System.Collections.Generic;
using System.Threading;
using CommonLib;
using HashCacheServer.Service;
using NTSockv3.Sockets;
using ObjectMapping;
using NTSockv3;


namespace HashCacheClient.Service
{
    public class ServiceCacheLocal : IServiceCacheLocal
    {
        public Cache<string, string> cacheLocal;
		public IDictionary<string, ServiceCacheProxy> cacheProxies;
    	private ReaderWriterLockSlim proxyLock;
        public ServiceCacheLocal()
        {
            cacheLocal = new Cache<string, string>(1000);
			cacheProxies = new Dictionary<string, ServiceCacheProxy>();
			proxyLock = new ReaderWriterLockSlim();
//			ServiceContainer serviceContainer = new ServiceContainer();
//			cacheServer = new ServiceCacheProxy(serviceContainer, new SocketPool(10, serviceContainer, "ken-2a8c766c617:11885"));
        }

		public void AddCacheProxies(string host, ServiceCacheProxy cacheProxy)
		{
			proxyLock.EnterWriteLock();
			try
			{
				cacheProxies.Remove(host);
				cacheProxies.Add(host, cacheProxy);
			}
			finally
			{
				proxyLock.ExitWriteLock();
			}
		}

		public void RemoveCacheProxies(string host)
		{
			proxyLock.EnterWriteLock();
			try
			{
				cacheProxies.Remove(host);
			}
			finally
			{
				proxyLock.ExitWriteLock();
			}
		}

        public void Insert(object key, IDbObject value, TimeSpan time, bool isRelative, ObjectRemoved removeDelegate)
        {
			string valueKey = key.ToString() + "_" + value.GetType().Name;
			string id = cacheLocal.Get(valueKey);
			if (id != null)
			{
				string[] str = id.Split(new char[] { '_' }, 1);
				string host = str[0];
				ServiceCacheProxy cacheProxy = cacheProxies[host];
				if (cacheProxy != null)
				{
					cacheProxy.BeginInsert(FinishInsert, new object[] { valueKey, time, isRelative, removeDelegate },value, time, isRelative, null);
				}
			}
			//TODO insert
			
        }

		public void FinishInsert(object state)
		{
			var response = (ResponseContext)state;
			string id = (string) response.Result.GetResult();
			object[] context = (object[]) response.WaitingContext;
			cacheLocal.Insert((string) context[0], id, (TimeSpan) context[1], (bool) context[2], (ObjectRemoved) context[3]);
		}

		public object Get(object key, Type ValueType)
        {
        	string valueKey = key.ToString() + "_" + ValueType.Name;
			string id =  cacheLocal.Get(valueKey);
			if(id != null)
			{
				string[] str = id.Split(new char[] { '_' }, 1);
				string host = str[0];
				ServiceCacheProxy cacheProxy = cacheProxies[host];
				if (cacheProxy != null)
				{
					return cacheProxy.Get(valueKey);
				}
			}
			//queryDatabase va insert
			return null;
        }

		public void Remove(object key, Type ValueType)
        {
			string valueKey = key.ToString() + "_" + ValueType.Name;
			string id = cacheLocal.Get(valueKey);
			cacheLocal.Remove(valueKey);
			string[] str = id.Split(new char[] { '_' }, 1);
			string host = str[0];
			ServiceCacheProxy cacheProxy = cacheProxies[host];
			if(cacheProxy != null)
			{
				cacheProxy.Remote(valueKey);
			}
			
        }
    }
}
