using System;
using CommonLib;
using NTSockv3;

namespace HashCacheServer.Service
{
    public class ServiceCache : IServiceCache
    {
        private Cache<long, object> cache;
        private TimeSpan defaultTime = new TimeSpan(1,0,0);
        public ServiceCache()
        {
            cache = new Cache<long, object>(1000);
        }

        [ServiceMethod]
        public void Insert(long id, object o, TimeSpan time, bool isRelative, ObjectRemoved  removeDelegate)
        {
            cache.Insert(id, 0, time, isRelative, removeDelegate);
        }

        [ServiceMethod]
        public object Get(long id)
        {
            return cache.Get(id);
        }

        [ServiceMethod]
        public void Remote(long id)
        {
            cache.Remove(id);
        }
    }
}
