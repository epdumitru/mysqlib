using System;
using System.IO;

namespace NTSockv3.Messages
{
	[Serializable]
	public abstract class Request : Mail
	{
		public string requestDescription;

		public Request()
		{
			type = REQUEST_TYPE;
			zipType = NO_ZIP;
		}

		public string RequestDescription
		{
			get { return requestDescription; }
			set { requestDescription = value; }
		}

		public override string ToString()
		{
			return String.Format("Request is: " + this.GetType());
		}
	}
}