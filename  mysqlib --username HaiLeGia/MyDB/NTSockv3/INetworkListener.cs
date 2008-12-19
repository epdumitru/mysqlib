using System;
using System.Net;

namespace NTSockv3
{
	public interface INetworkListener
	{
		void NetworkException(Exception e);
		void NetworkClose(EndPoint endPoint);
		void NetworkConnected(EndPoint endPoint);
	}
}