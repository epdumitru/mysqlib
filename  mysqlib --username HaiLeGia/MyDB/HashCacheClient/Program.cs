using System;
using HashCacheClient.Service;
using Persistents;

namespace HashCacheClient
{
    public class Program
    {
        static void Main(string[] args)
        {
			TimeSpan time = new TimeSpan(0,20, 0, 0);
        	IServiceCacheLocal serviceCache = new ServiceCacheLocal();
			UserData userData = new UserData();
        	userData.Id = 1;
        	userData.Username = "abc";
		
			serviceCache.Insert(userData.Username, userData,time, true, null);
        	Console.ReadLine();
        }
    }
}
