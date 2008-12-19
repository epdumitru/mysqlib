using System;
using System.Runtime.Serialization;

namespace NTSockv3.Exceptions
{
	[Serializable]
	public class MethodParamNullException : NTSockException
	{
		/// <summary>
		/// index of the param
		/// </summary>
		private readonly int index;

		public MethodParamNullException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		public MethodParamNullException()
		{
		}

		public MethodParamNullException(int index)
			: base(null)
		{
			this.index = index;
		}

		public MethodParamNullException(int index, Exception innerException)
			: base(innerException)
		{
			this.index = index;
		}

		/// <summary>
		/// index of the param
		/// </summary>
		public int Index
		{
			get { return index; }
		}

		public override string ToString()
		{
			return "Parameter " + index + " cannot be null. Inner exception: " + innerExceptionDesc;
		}
	}
}