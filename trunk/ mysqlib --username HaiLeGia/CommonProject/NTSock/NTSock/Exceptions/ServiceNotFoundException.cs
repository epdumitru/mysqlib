using System;
using System.Runtime.Serialization;

namespace NTSock.Exceptions
{
	[Serializable]
	public class ServiceNotFoundException : NTSockException
	{
		/// <summary>
		/// The service that cannot be found.
		/// </summary>
		private readonly string serviceName;

        public ServiceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		public ServiceNotFoundException()
		{
		}

		public ServiceNotFoundException(string serviceName)
			: base(null)
		{
			this.serviceName = serviceName;
		}

		public string ServiceName
		{
			get { return serviceName; }
		}

		public override string ToString()
		{
			return "Cannot find service: " + serviceName;
		}
	}
}