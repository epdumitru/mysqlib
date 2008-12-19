#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;


namespace Logger
{
	public class LoggerManager
	{
		private static IDictionary<Type, PurposeLog> allLog;
		private static object lockObject;
		
		static LoggerManager()
		{
			allLog = new Dictionary<Type, PurposeLog>();
			lockObject = new object();
		}

		public static PurposeLog CreateLog(Type type)
		{
			lock (lockObject)
			{
				if (allLog.ContainsKey(type))
				{
					return allLog[type];
				}
				else
				{
					var log = new PurposeLog(type);
					allLog.Add(type, log);
					return log;
				}
			}
		}

		public static PurposeLog CreateLog(Type type, string logfile)
		{
			lock (lockObject)
			{
				if (allLog.ContainsKey(type))
				{
					return allLog[type];
				}
				else
				{
					var log = new PurposeLog(logfile);
					allLog.Add(type, log);
					return log;
				}
			}
		}
	}

	public class PurposeLog
	{
		private StreamWriter logWriter;
		private string logfile;
		private object lockObject;

		internal PurposeLog(Type type)
		{
			logfile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + type.FullName;
			lockObject = new object();
		}

		internal PurposeLog(string logfile)
		{
			this.logfile = logfile;
			lockObject = new object();
		}

		public void WriteLog(string message)
		{
			var now = DateTime.Now;
			message = now.ToShortDateString() + " " + now.ToLongTimeString() + ": " + message;
#if DEBUG
			Console.WriteLine(message);
#endif
			var datetimeString = DateTime.Now.ToShortDateString();
			datetimeString = datetimeString.Replace('/', '-');
			var sb = new StringBuilder(logfile);
			sb.Append("_");
			sb.Append(datetimeString);
			sb.Append(".log");
			lock (lockObject)
			{
				logWriter = new StreamWriter(sb.ToString(), true);
				logWriter.WriteLine(message);
				logWriter.Flush();
				logWriter.Close();	
			}
		}

        public void WriteLog(Exception e)
        {
            WriteLog(e.ToString());
        }

	}

	public class Log
	{
		private static string debugFile = "NTDebugger";
		private static IList debugMessages;
		private static StreamWriter dgWriter;
		private static bool isClosed;
		private static string logfile = "NTLogger";
		private static IList messages;
		private static StreamWriter sw;

		static Log()
		{
			isClosed = false;
			messages = ArrayList.Synchronized(new ArrayList());
			debugMessages = ArrayList.Synchronized(new ArrayList());
			var t = new Thread(LogWriter);
			t.Start();
			var t2 = new Thread(DebugWriter);
			t2.Start();
		}

		public static string Logfile
		{
			get { return logfile; }
			set { logfile = value; }
		}

		public static string DebugFile
		{
			set { debugFile = value; }
		}

		private static void DebugWriter()
		{
			try
			{
				while (debugMessages != null && !isClosed)
				{
					string datetimeString = DateTime.Now.ToShortDateString();
					datetimeString = datetimeString.Replace('/', '-');
					StringBuilder sb = new StringBuilder(debugFile);
					sb.Append("_");
					sb.Append(datetimeString);
					sb.Append(".log");
					dgWriter = new StreamWriter(sb.ToString(), true);
					for (int i = 0; i < debugMessages.Count; i++)
					{
						string message = (string) debugMessages[i];
						dgWriter.WriteLine(message);
					}
					debugMessages.Clear();
					dgWriter.Flush();
					dgWriter.Close();
					Thread.Sleep(1000);
				}
			}
			catch
			{
			}
		}

		private static void LogWriter()
		{
			try
			{
				while (messages != null && !isClosed)
				{
					string datetimeString = DateTime.Now.ToShortDateString();
					datetimeString = datetimeString.Replace('/', '-');
					StringBuilder sb = new StringBuilder(logfile);
					sb.Append("_");
					sb.Append(datetimeString);
					sb.Append(".log");
					sw = new StreamWriter(sb.ToString(), true);
					for (int i = 0; i < messages.Count; i++)
					{
						string message = (string) messages[i];
						sw.WriteLine(message);
					}
					messages.Clear();
					sw.Flush();
					sw.Close();
					Thread.Sleep(1000);
				}
			}
			catch
			{
			}
		}

		public static void WriteDebug(string message)
		{
			DateTime now = DateTime.Now;
			message = now.ToShortDateString() + " " + now.ToLongTimeString() + ": " + message;
#if DEBUG
			Console.WriteLine(message);
#endif
			debugMessages.Add(message);
		}

		public static void WriteLog(string message)
		{
			var now = DateTime.Now;
			message = now.ToShortDateString() + " " + now.ToLongTimeString() + ": " + message;
#if DEBUG
			Console.WriteLine(message);
#endif
			messages.Add(message);
		}

        public static void WriteLog(Exception e)
        {
            var message = new StringBuilder();
            var now = DateTime.Now;
            message.Append(now.ToShortDateString() + " ");
            message.Append(now.ToLongTimeString() + ": " + e);
#if DEBUG
			Console.WriteLine(message.ToString());
#endif
            messages.Add(message.ToString());
        }

		public static void Closed()
		{
			isClosed = true;
		}
	}
}