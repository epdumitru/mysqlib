using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NTSock;
using NTSock.Controller;
using NTSock.Sockets;

namespace NTSockClient
{
	/*[Serializable]
	public class Param1
	{
		public int I { get; set; }

		public string E { get; set; }
	}

	[Serializable]
	public class Param2
	{
		public IList<Param1> Param1 { get; set; }
	}

	public class ClientCallback
	{
		public void Callback(bool result)
		{
			Console.WriteLine("Result: " + result);
		}

		public void TestCallback(bool result)
		{
			Console.WriteLine("Result: " + result);
		}
	}
	
	public class MyServiceProxy
	{
		private readonly SocketPool pool;
		private ServiceContainer serviceContainer;
//        private static Request doIt1Request = new Request("My Service", D);
		public MyServiceProxy(ServiceContainer container, string host)
		{
			serviceContainer = container;
			pool = new SocketPool(10, container, host);
		}

		public void DoIt2()
		{
			var request = new Request {ServiceName = "My Service", ReceiverMethod = "DoIt2", Parameters = null};
			pool.Request(request);
		}

		public void DoIt(Param1 param1, WaitCallback callback)
		{
			var request = new Request
			              	{
			              		ServiceName = "My Service",
			              		ReceiverMethod = "DoIt",
			              		Parameters = new object[] {param1}
			              	};
			pool.BeginRequest(request, callback, null);
		}
	}

	
	internal class Program
	{
		private static MyServiceProxy proxy;
		private static Dictionary<Type[], string> testMap;
		private static ManualResetEvent waitEvent;

		private static void DoItFinished(object state)
		{
			ResponseContext context = (ResponseContext) state;
			var result = (IList<Param1>)context.Result.Result;
			for (int i = 0; i < result.Count; i++)
			{
				Console.WriteLine(result[i].E);
			}
		}

		private static void Main(string[] args)
		{
			var serviceContainer = new ServiceContainer();
			serviceContainer.RegisterService("ClientCallback", new ClientCallback(), false);
			proxy = new MyServiceProxy(serviceContainer, "localhost:11885");
			Test();
            Test();
//			waitEvent = new ManualResetEvent(false);
//			for (int i = 0; i < 2; i++)
//			{
//				Thread t = new Thread(Test);
//				t.Start();
//			}
//			waitEvent.Set();
			Console.ReadLine();
		}

		private static void Test()
		{
//			WaitHandle.WaitAll(new WaitHandle[] { waitEvent });
			var p1 = new Param1();
			p1.E = "hello world";
			p1.I = 100;
			proxy.DoIt(p1, DoItFinished);
		}
	}*/
    [Serializable]
    public class Param1
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string PassWord { get; set; }

        public string FullName { get; set; }

        public int sex { get; set; }

        public DateTime Birthday { get; set; }

        public string Address { get; set; }
    }
    public class MyProxy
    {
        private SocketPool pool;
        public MyProxy(SocketPool pool)
        {
            this.pool = pool;
        }

        private static Request setValueRequestPattern = new Request("My Service", "SetValue", new Type[] { typeof(int), typeof(Param1) });

        public bool SetValue(int newValue, Param1 param)
        {
            Request request = new Request("My Service", "SetValue", new object[] {newValue, param });
            return pool.Request<bool>(request);
        }

        public void SetValue(int newValue, Param1 param, WaitCallback CallbackMethod, object context)
        {
            Request request = new Request("My Service", "SetValue", new object[] { newValue, param });
            pool.BeginRequest(request, CallbackMethod, context);
        }

        public int GetValueSimple()
        {
            Request request = new Request("My Service", "GetValueSimple", null);
            return pool.Request<int>(request);
        }

        public Param1 GetValueComplex()
        {
            Request request = new Request("My Service", "GetValueSimple", null);
            return pool.Request<Param1>(request);
        }
    }
    public class Program
    {
        private static MyProxy proxy;
        public static void CallbackMethod(object state)
        {
            Console.WriteLine("Callback method is called ");
            ResponseContext response = (ResponseContext)state;
            if (response.Result.Result != null)
            {
                bool result = (bool)response.Result.Result;
                Console.WriteLine(result);
            }

        }

        public static void Main(string[] arg)
        {
            int n = 10000;
            ServiceContainer container = new ServiceContainer();
            proxy = new MyProxy(new SocketPool(1, container,
                "127.0.0.1:11885"));
            DateTime start, end;
            TimeSpan duration;
            Param1 param = new Param1();
            param.Id = 10;
            param.UserName = "hello";
            Console.WriteLine("Client set value for remote service");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < n; i++)
            {
                var sucess = proxy.SetValue(i, param);
//                Console.WriteLine(i + sucess.ToString());
            }
            watch.Stop();
            Console.WriteLine(string.Format("Client seted value for remote service. Execution took {0} miliseconds", (float) watch.ElapsedMilliseconds / n));
            /*Console.WriteLine("Client get value from remote service");
            start = DateTime.Now;
            int result = proxy.GetValueSimple();
            end = DateTime.Now;
            duration = end.Subtract(start);
            Console.WriteLine("Client geted from remote service. value = {0}. Execution took {1} miliseconds", result, duration.TotalMilliseconds);
            */
            Console.WriteLine("press anyone key to exit");
            Console.ReadLine();
        }
    }
}