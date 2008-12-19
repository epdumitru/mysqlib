using System;
using System.Runtime.Serialization;
using ObjectSerializer;

namespace NTSock.Controller
{
	[Serializable]
	public class Response : Mail
	{
		private long id;
		private Exception exception;
		private object result;
		private byte zipType;

	    public Response()
		{
			type = RESPONSE_TYPE;
		}

        internal Response(Response other)
        {
            exception = other.exception;
            result = other.result;
        }

		public Exception ResultException 
		{ 
			get
			{
			    return exception;
			}
			set
			{
			    if (exception != value)
			    {
                    exception = value;
			    }
			}
		}

		internal override long Id
		{
			get { return id; }
			set { id = value; }
		}

		public object Result
		{
			get { return result; } 
			set
			{
			    if (result != value)
			    {
			        result = value;
			    }
                else
			    {
			        if (value != null && !value.Equals(result))
			        {
                        result = value;
			        }
			    }
			}
		}

		internal override byte ZipType
		{
			get { return zipType; } 
			set { zipType = value; }
		}

	}
}