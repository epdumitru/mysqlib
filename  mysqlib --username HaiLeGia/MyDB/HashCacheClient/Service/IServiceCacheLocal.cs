using System;
using CommonLib;

namespace HashCacheClient.Service
{
    public interface IServiceCacheLocal
    {
        void Insert(object key, object value, TimeSpan time, bool isRelative, ObjectRemoved removeDelegate);
        object Get(object key, Type ValueType);
        void Remove(object key);
    }
}
