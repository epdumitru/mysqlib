using System;
using System.Text;

namespace Chord.Common
{
	[Serializable]
	public class URL
	{
		private const char DCOLON = ':';
		private const string DCOLON_SLASHES = "://";
		private const char SLASH = '/';
		private readonly string host;
		private readonly string path;
		private readonly int port;
		private readonly string protocol;
		private readonly string urlString;

		public URL(string urlString)
		{
			this.urlString = urlString;
			string[] urlParams = urlString.Split(new string[] {DCOLON_SLASHES}, StringSplitOptions.RemoveEmptyEntries);
			if (urlParams.Length != 2)
			{
				throw new ArgumentException("Cannot parse this URL: " + urlString);
			}
			protocol = urlParams[0];
			string[] otherParams = urlParams[1].Split(new char[] {SLASH}, StringSplitOptions.RemoveEmptyEntries);
			string hostPort = otherParams[0];
			string[] hostPorts = hostPort.Split(new char[] {DCOLON}, StringSplitOptions.RemoveEmptyEntries);
			if (hostPorts.Length != 2)
			{
				throw new ArgumentException("URL must specified a port.");
			}
			host = hostPorts[0];
			try
			{
				port = Int32.Parse(hostPorts[1]);
				if (port > UInt32.MaxValue || port < 0)
				{
					throw new ArgumentException("Port invalid.");
				}
			}
			catch
			{
				throw new ArgumentException("Port must be an integer");
			}
			StringBuilder sb = new StringBuilder();
			if (otherParams.Length > 1)
			{
				for (int i = 1; i < otherParams.Length - 1; i++)
				{
					sb.Append(otherParams[i] + SLASH);
				}
				sb.Append(otherParams[otherParams.Length - 1]);
			}
			path = sb.ToString();
		}

		public string UrlString
		{
			get { return urlString; }
		}

		public string Host
		{
			get { return host; }
		}

		public int Port
		{
			get { return port; }
		}

		public string Protocol
		{
			get { return protocol; }
		}

		public string Path
		{
			get { return path; }
		}

		public override string ToString()
		{
			return urlString;
		}
	}
}