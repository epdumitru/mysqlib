using System;
using System.Configuration;
using System.Threading;
using Chord.Common;
using Chord.FileStorage;
using Logger;

namespace Chord
{
	public class Program
	{
		private static FileNode fileNodeImpl;
		private static bool isStop;
		public static void Start()
		{
			AppDomain.CurrentDomain.UnhandledException += LogException;
			URL url = new URL(ConfigurationManager.AppSettings["URL"]);
			fileNodeImpl = new FileNodeImpl(url);
			fileNodeImpl.Create();
			isStop = false;
			while (!isStop)
			{
				Thread.Sleep(10000);
			}
			Log.WriteDebug("File server started.");
		}

		private static void LogException(object sender, UnhandledExceptionEventArgs e)
		{
			Log.WriteLog("Unhandle Exception in File Server: " + e.ExceptionObject);
		}

		public static void Stop()
		{
			isStop = true;
		}

		private static void Main(string[] args)
		{
			Thread t = new Thread(Start);
			t.Start();
			Log.WriteLog("File server OK");
			Console.ReadLine();
			Stop();
		}

		private static void Write(byte[] result)
		{
			Console.Write("Result: ");
			for (int i = 0; i < result.Length; i++)
			{
				Console.Write(result[i] + "   ");
			}
			Console.WriteLine();
		}
	}
}
