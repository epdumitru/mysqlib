using System;
using CommonLib;
using HashCacheServer.Service;

namespace HashCacheClient.Service
{
    public class ServiceCacheLocal : IServiceCacheLocal
    {
        public Cache<string, long> cacheLocal;
        public IServiceCache cacheServer;
        public ServiceCacheLocal()
        {
            cacheLocal = new Cache<string, long>(1000);
            
        }


        public void Insert(object key, object value, TimeSpan time, bool isRelative, ObjectRemoved removeDelegate)
        {
            Type type = value.GetType();
            string valueKey = key.ToString()+ "_" + type.Name;
            cacheLocal.Insert(valueKey, );
        }

        public object Get(object key, Type ValueType)
        {
            throw new System.NotImplementedException();
        }

        public void Remove(object key)
        {
            throw new System.NotImplementedException();
        }
    }
}
