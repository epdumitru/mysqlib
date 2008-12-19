using System;
using System.Collections.Generic;
using System.Diagnostics;
using Logger;
using NTSock;
using NTSock.Controller;
using NTSock.Executors;
using NTSock.Sockets;
using NTSockClient;

namespace NTSockServer
{
	/*public class MyService
	{
		private ServiceContainer container;

		public ServiceContainer Container
		{
			get { return container; }
			set { container = value; }
		}

		public void DoIt2()
		{
			Console.WriteLine("Helle everybody");
		}

		public IList<Param1> DoIt(Param1 param1)
		{
			Console.WriteLine("Param1: " + param1.E + ", " + param1.I);
			var list = new List<Param1>();
			var result = new Param2 {Param1 = list};
			list.Add(param1);
			container.Request(new Request("ClientCallback", "TestCallback", new object[] { true }));
			return list;
		}

		private void ClientCallMe(object state)
		{
			ResponseContext context = (ResponseContext) state;
            Console.WriteLine("Client receive result: " + context.Result.Result);
		}
	}

	internal class Program
	{
		private static void Main(string[] args)
		{
			var host = new ServiceHost(11885);
			var service = new MyService {Container = host.Container};
			host.Container.RegisterService("My Service", service, false);
			host.Open();
			Log.WriteLog("Host Opened");
			Console.ReadLine();
			Log.WriteLog("Closing");
			host.Close();
		}
	}*/

    public class MyRemoteService
    {
        int myvalue;
        private Param1 param;

        public MyRemoteService()
        {
            Console.WriteLine("MyRemoteObject.Constructor: New Object created");
            param = new Param1();
        }

        public bool SetValue(int newval, Param1 p)
        {
//            Console.WriteLine("MyRemoteService.setValue(): old {0} new {1}",
//                                  myvalue, newval);
            for (int i = 0; i < 1000; i++)
            {
            	param.Id = p.Id;
				param.UserName = p.UserName;
				myvalue = newval;
			}
//            Console.WriteLine(" .setValue() -> value is now set");
            return true;
        }

        public int GetValueSimple()
        {
            Console.WriteLine("MyRemoteObject.GetValueSimple(): current {0}", myvalue);
            return myvalue;
        }

        public Param1 GetValueComplex()
        {
            Console.WriteLine("MyRemoteObject.GetValueComplex(): current {0}", myvalue);
            return param;
        }
    }

    public class Program
    {
        public static void Main(string[] arg)
        {
        	int n = 10000;
        	var proxy = new MyRemoteService();
			var param = new Param1();
			Console.WriteLine("Client set value for remote service");
			Stopwatch watch = new Stopwatch();
			watch.Start();
			for (int i = 0; i < n; i++)
			{
				var sucess = proxy.SetValue(i, param);
//				Console.WriteLine(i + sucess.ToString());
			}
			watch.Stop();
			Console.WriteLine(string.Format("Client seted value for remote service. Execution took {0} miliseconds", (float)watch.ElapsedMilliseconds / n));
        	Console.ReadLine();
//            ServiceHost host = new ServiceHost(11885);
//            Console.WriteLine("host open ");
//            host.Container.RegisterService("My Service",
//                                new MyRemoteService(), false);
//            Console.WriteLine("registed service");
//            host.Open();
//            Console.ReadLine();
//            Console.WriteLine("Closing");
//            host.Close();
        }
    }
}