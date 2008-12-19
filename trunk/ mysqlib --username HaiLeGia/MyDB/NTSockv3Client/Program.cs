using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using NTSockv3.Messages;
using NTSockv3.Sockets;
using NTSockv3Server;

namespace NTSockv3Client
{
	class Job
	{
		public ManualResetEvent waitEventToStart;
		public ManualResetEvent resetEvent;
		public MyRemoteServiceProxy proxy;
		public long totalTime;
		public void Run()
		{
			waitEventToStart.WaitOne();
			Stopwatch watch = new Stopwatch();
			watch.Start();
			for (int i = 0; i < 10000; i++)
			{
				proxy.DoIt();
			}
			watch.Stop();
			totalTime = watch.ElapsedMilliseconds;
			resetEvent.Set();
		}
	}

	class Job2
	{
		public ManualResetEvent resetEvent;
		public long totalTime;
		public void Run()
		{
			var now = DateTime.Now;
			Console.WriteLine("Start run: " + now.Ticks);
			double l = 0;
			for (int i = 0; i < 100000; i++)
			{
				l = l * 2 + 12 + Math.Abs(10) + Math.Cos(1.2);
			}
			Console.WriteLine("Before call set: " + DateTime.Now.Ticks);
			resetEvent.Set();
			now = DateTime.Now;
			Console.WriteLine("After call set: " + now.Ticks);
		}
	}
	class Program
	{
		
		static void Main(string[] args)
		{
//			var waitEvent = new ManualResetEvent(false);
//			var job = new Job2() {resetEvent = waitEvent};
//			var t = new Thread(job.Run);
//			t.Start();
//			waitEvent.WaitOne();
//			var now = DateTime.Now;
//			Console.WriteLine("Time to start again: " + now.Ticks);
			int threadCount = 1;
			var serviceContainer = new ServiceContainer();
			var proxy = new MyRemoteServiceProxy(serviceContainer, new SocketPool(threadCount, serviceContainer, "127.0.0.1:11885"));
			var jobs = new Job[threadCount];
			var threadWaitEvent = new ManualResetEvent(false);
			var mainWaitEvents = new ManualResetEvent[threadCount];
			for (var i = 0; i < threadCount; i++)
			{
				mainWaitEvents[i] = new ManualResetEvent(false);
				jobs[i] = new Job() {proxy = proxy, resetEvent = mainWaitEvents[i], waitEventToStart = threadWaitEvent};
				var t = new Thread(jobs[i].Run);
				t.Start();
			}
			//All thread run now
			threadWaitEvent.Set();
			//Wait all thread to finish
			WaitHandle.WaitAll(mainWaitEvents);
			long totalTime = 0;
			for (int i = 0; i < threadCount; i++)
			{
				totalTime += jobs[i].totalTime;
			}
			Console.WriteLine("Total time: " + totalTime);
			Console.ReadKey();
		}
	}
}
