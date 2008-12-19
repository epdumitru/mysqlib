using System;
using System.Text;

namespace NTSock.Controller
{
	[Serializable]
	public class Request : Mail
	{
		private string serviceName;
		private string receiverMethod;
		private object[] parameters;
		private long id;
		private byte zipType;
		private string requestDescription;
		private bool requestChange;

		public Request()
		{
		    type = REQUEST_TYPE;
			requestChange = false;
		}

	    public Request(string serviceName, string methodName, object[] parameters)
		{
		    this.serviceName = serviceName;
			this.receiverMethod = methodName;
			this.parameters = parameters;
			type = REQUEST_TYPE;
			var builder = new StringBuilder(serviceName + " " + methodName + " ");
			if (parameters != null)
			{
				for (var i = 0; i < parameters.Length; i++)
				{
					builder.Append(parameters[i].GetType() + " ");
				}
			}
			requestDescription = builder.ToString().Trim();
	    	requestChange = false;
		}

		public string ServiceName
		{
			get { return serviceName; }
			set
			{
			    if (value != serviceName)
			    {
			    	requestChange = true;
			        serviceName = value;    
			    }
                
			}
		}

		public string ReceiverMethod
		{
			get { return receiverMethod; }
			set
			{
			    if (receiverMethod != value)
			    {
					requestChange = true;
			        receiverMethod = value;
			    }
			}
		}

		public object[] Parameters
		{
			get { return parameters; }
			set
			{
				requestChange = true;
                parameters = value;
			}
		}

		internal override long Id
		{
			get { return id; }
			set { id = value; }
		}

		internal override byte ZipType
		{
			get { return zipType; }
			set { zipType = value; }
		}

		internal string RequestDescription
		{
			get
			{
				if (requestChange)
				{
					var builder = new StringBuilder(serviceName + " " + receiverMethod + " ");
					if (parameters != null)
					{
						for (var i = 0; i < parameters.Length; i++)
						{
							builder.Append(parameters[i].GetType() + " ");
						}
					}
					requestDescription = builder.ToString().Trim();
					requestChange = false;
				}
				return requestDescription;	
			}
            set
            {
				requestDescription = value;
            }
		}

		public override string ToString()
		{
			return String.Format("Request to {0} with methods: {1}", ServiceName, ReceiverMethod);
		}

	}
}