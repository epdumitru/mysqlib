using System;
using HashCacheServer.Service;
using NTSockv3;

namespace HashCacheServer
{
    class Program
    {
        static void Main(string[] args)
        {
			ServiceHost host = new ServiceHost(11885);
			host.Container.RegisterService(new ServiceCache(), false);
			Console.WriteLine("Register....");
			host.Open();
			Console.WriteLine("Open");
        	Console.ReadKey();
			Console.WriteLine("close");
			host.Close();
        }
    }
}
