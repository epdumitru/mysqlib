using System;
using NTSockv3.Messages;

namespace NTSockv3.Excutors
{
	public interface IExecutor
	{
		string ExecutorKey { get; }
		Response Execute(Request request);
	}
}