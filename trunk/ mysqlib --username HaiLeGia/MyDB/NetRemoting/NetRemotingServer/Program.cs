using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using NetRemoting;

namespace NetRemotingServer
{
    
    public class MyRemoteService : MarshalByRefObject
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
            Console.WriteLine("MyRemoteService.setValue(): old {0} new {1}",
                                  myvalue, newval);
            param.Id = p.Id;
            param.UserName = p.UserName;
            myvalue = newval;
            Console.WriteLine(" .setValue() -> value is now set");
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

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ServerStartup.Main(): Server started");

            TcpChannel chnl = new TcpChannel(1234);
            ChannelServices.RegisterChannel(chnl);

            RemotingConfiguration.RegisterWellKnownServiceType(
                  typeof(MyRemoteService),
                  "MyRemoteObject",
                  WellKnownObjectMode.Singleton);

            // the server will keep running until keypress.
            Console.ReadLine();
        }
    }
}


