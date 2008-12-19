using System;

namespace NTSockv3.Messages
{
	[Serializable]
	public abstract class Response : Mail
	{

		public Response()
		{
			type = RESPONSE_TYPE;
			zipType = NO_ZIP;
		}

		public abstract object GetResult();
	}
}