using System;
using System.Net;

namespace NTSock
{
	public interface INetworkListener
	{
		void NetworkException(Exception e);
		void NetworkClose(EndPoint endPoint);
		void NetworkConnected(EndPoint endPoint);
	}
}