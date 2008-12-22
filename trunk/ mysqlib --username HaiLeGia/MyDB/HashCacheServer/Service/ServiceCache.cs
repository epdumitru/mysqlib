using System;
using System.Net;
using CommonLib;
using NTSockv3;
using ObjectMapping;

namespace HashCacheServer.Service
{
    public class ServiceCache : IServiceCache
    {
        private Cache<string, object> cache;
        private TimeSpan defaultTime = new TimeSpan(1,0,0);
    	private string ownIp;
        public ServiceCache()
        {
            cache = new Cache<string, object>(1000);
        	ownIp = Dns.GetHostName();
        }

        [ServiceMethod]
		public string Insert(IDbObject o, TimeSpan time, bool isRelative, ObjectRemoved removeDelegate)
        {
			string key = o.Id + "_" + o.GetType().Name;
            cache.Insert(key, o, time, isRelative, removeDelegate);
        	return ownIp + "_" + key;
        }

        [ServiceMethod]
        public object Get(string id)
        {
            object result =  cache.Get(id);
        	if (result != null)
        	{
        		return result;
        	}
			//Query database
        	return null;
        }

        [ServiceMethod]
        public void Remote(string id)
        {
            cache.Remove(id);
        }
    }
}
