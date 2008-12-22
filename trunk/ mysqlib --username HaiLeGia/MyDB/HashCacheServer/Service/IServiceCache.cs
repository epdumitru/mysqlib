using System;
using CommonLib;

namespace HashCacheServer.Service
{
    public interface IServiceCache
    {
        void Insert(long id, object o, TimeSpan time, bool isRelative, ObjectRemoved removeDelegate);
        object Get(long id);
        void Remote(long id);
    }
}
