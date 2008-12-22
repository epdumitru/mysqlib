using System;
using CommonLib;
using ObjectMapping;

namespace HashCacheClient.Service
{
    public interface IServiceCacheLocal
    {
		void Insert(object key, IDbObject value, TimeSpan time, bool isRelative, ObjectRemoved removeDelegate);
		object Get(object key, Type ValueType);
		void Remove(object key, Type ValueType);
    }
}
