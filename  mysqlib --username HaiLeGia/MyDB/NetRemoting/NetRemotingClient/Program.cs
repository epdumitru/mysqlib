using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using NetRemoting;
using NetRemotingServer;

namespace NetRemotingClient
{
    class Program
    {
        delegate void SetValueDelegate(int myValue, Param1 param);

        private delegate int GetValueDelegate();
        

        public static void CallbackMethod(object state)
        {
            Console.WriteLine("Callback method is called ");
        }

        static void Main(string[] args)
        {
            int n = 10000;
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel);
            MyRemoteService obj = (MyRemoteService)Activator.GetObject(typeof(MyRemoteService), "tcp://127.0.0.1:1234/MyRemoteObject");
            DateTime start, end;
            TimeSpan duration;
            Param1 param = new Param1();
            param.UserName = "hello";
            param.Id = 10;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < n; i++)
            {
                var sucess = obj.SetValue(i, param);
                Console.WriteLine(i + sucess.ToString());
            }
            watch.Stop();
            Console.WriteLine(string.Format("client seted value for remote service. Execution took {0} milliseconds", (float) watch.ElapsedMilliseconds /n));


            /*Console.WriteLine("Client set value");
            start = System.DateTime.Now;
            obj.setValue(45);
            end = System.DateTime.Now;
            duration = end.Subtract(start);
            Console.WriteLine(string.Format("client seted value for remote service. Execution took {0} milliseconds", duration.TotalMilliseconds));
            Console.WriteLine("Client get value");
            start = System.DateTime.Now;
            int result = obj.getValue();
            end = System.DateTime.Now;
            duration = end.Subtract(start);
            Console.WriteLine(string.Format("Client geted value from remote service. value = {0}. Execution took {1} milliseconds ", result, duration.TotalMilliseconds));
            Console.WriteLine("client call DoIt method of the remoteService");
            Param1 param = new Param1();
            param.E = "Say hello";
            param.I = 10;
            start = DateTime.Now;
            obj.DoIt(param);
            end = DateTime.Now;
            duration = end - start;
            Console.WriteLine(string.Format("Execution took {0} miliseconds", duration.TotalMilliseconds));
            Console.ReadLine();
            

            string a = "context";
            Param1 param = new Param1();
            param.E = "Say hello";
            param.I = 10;
            SetValueDelegate setdel = new SetValueDelegate(obj.SetValue);
            GetValueDelegate getdel = new GetValueDelegate(obj.GetValueSimple);
            IAsyncResult ar = setdel.BeginInvoke(42, param, CallbackMethod, a);
            IAsyncResult ar2 = getdel.BeginInvoke(null, null);
            int result2 = getdel.EndInvoke(ar2);
            // ... do something different here
            //setdel.EndInvoke(ar);

            Console.WriteLine("Got result: '{0}'", result2);

            //wait for return to close
            Console.ReadLine();*/
            Console.ReadLine();
        }

    }
}
