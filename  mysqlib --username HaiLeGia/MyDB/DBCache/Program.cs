using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using DBCache.Core;
using DBCache.UserComponents;

namespace DBCache
{
    class BaseTest
    {
        protected int a;
        protected string b;
        protected IList<string> c;

        public int A
        {
            get { return a; }
            set { a = value; }
        }

        public string B
        {
            get { return b; }
            set { b = value; }
        }

        public IList<string> C
        {
            get { return c; }
            set { c = value; }
        }
    }

	class Test : BaseTest
	{
	    private byte d;

	    public byte D
	    {
	        get { return d; }
	        set { d = value; }
	    }
	}

	class Program
	{
		static void Main(string[] args)
		{
		    var testArray = new int[100];
			Console.WriteLine(testArray is Array);
			Console.ReadKey();
//            for (var i = 0; i < testArray.Length; i++)
//            {
//                testArray[i] = i;
//            }
//            var t = new Test();
//            var data = new ClassMetaData(t);
//		    var allInfor = data.AllFields;
//            Console.WriteLine(allInfor.Count);
//                //			var addresses = Dns.GetHostAddresses(Dns.GetHostName());
//                //			Console.WriteLine(ulong.MaxValue);
//                //			for (var i = 0; i < addresses.Length;  i++)
//                //			{
//                //				var address = addresses[i];
//                //				if (address.GetAddressBytes().Length == 4)
//                //				{
//                //					var reference = new Reference(address, 1);
//                //					var localId = reference.LocalId;
//                //					Console.WriteLine("Local Id: " + localId);
//                //					var prevReference = reference.Decrease;
//                //					localId = prevReference.LocalId;
//                //					Console.WriteLine("Local Id: " + localId);
//                //					reference = new Reference(address, ulong.MaxValue);
//                //					localId = reference.LocalId;
//                //					Console.WriteLine("Local Id: " + localId);
//                //					var nextReference = reference.Increase;
//                //					localId = nextReference.LocalId;
//                //					Console.WriteLine("Local Id: " + localId);		
//                //				}
//                //			}
//                Console.ReadLine();
		}
	}
}