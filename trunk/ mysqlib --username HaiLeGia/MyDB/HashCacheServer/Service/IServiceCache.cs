using System;
using CommonLib;
using ObjectMapping;

namespace HashCacheServer.Service
{
    public interface IServiceCache
    {
		string Insert(IDbObject o, TimeSpan time, bool isRelative, ObjectRemoved removeDelegate);
        object Get(string id);
        void Remote(string id);
    }
}
