using System;
using System.Runtime.Serialization;

namespace NTSock.Exceptions
{
	[Serializable]
	public class NTSockException : Exception
	{
		protected Exception innerExceptionDesc;

        public NTSockException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		public NTSockException()
		{
		}

		public NTSockException(Exception innerException)
		{
			innerExceptionDesc = innerException;
		}

		public Exception InnerExceptionDesc
		{
			get { return innerExceptionDesc; }
		}
	}
}