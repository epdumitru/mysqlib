using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NTSockv3;

namespace NTSockv3Server
{
	[Serializable]
	public class Param1
	{
		public int Id { get; set; }

		public string UserName { get; set; }
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


	public class MyRemoteService
	{
		private int myvalue;
		private Param1 param;

		public MyRemoteService()
		{
			param = new Param1();
		}

		[ServiceMethod]
		public bool SetValue(int newval, Param1 p)
		{
			for (int i = 0; i < 1000; i++)
			{
				param.Id = p.Id;
				param.UserName = p.UserName;
				myvalue = newval;
			}
			return true;
		}

		[ServiceMethod]
		public int GetValueSimple()
		{
			return myvalue;
		}

		[ServiceMethod]
		public Param1 GetValueComplex()
		{
			return param;
		}

		[ServiceMethod]
		public void DoIt()
		{
			Console.WriteLine("Do it");
		}
	}

	public class Program
	{
		public static void Main(string[] arg)
		{
			var host = new ServiceHost(11885);
			host.Container.RegisterService(new MyRemoteService(), false);
			Console.WriteLine("Registed service");
			host.Open();
			Console.ReadLine();
			Console.WriteLine("Closing");
			host.Close();
		}
	}

}
