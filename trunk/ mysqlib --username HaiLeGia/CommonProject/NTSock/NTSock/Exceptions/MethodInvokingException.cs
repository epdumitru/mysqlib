using System;
using System.Runtime.Serialization;

namespace NTSock.Exceptions
{
	[Serializable]
	public class MethodInvokingException : NTSockException
	{
		/// <summary>
		/// Class is executing
		/// </summary>
		private readonly string className;

		/// <summary>
		/// Method that throw exception
		/// </summary>
		private readonly string methodName;

        public MethodInvokingException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public MethodInvokingException()
		{
		}

		public MethodInvokingException(string className, string methoName, Exception e)
			: base(e)
		{
			this.className = className;
			methodName = methoName;
		}

		/// <summary>
		/// Class is executing
		/// </summary>
		public string ClassName
		{
			get { return className; }
		}

		/// <summary>
		/// Method that throw exception
		/// </summary>
		public string MethodName
		{
			get { return methodName; }
		}

		public override string ToString()
		{
			return "Exception while invoke: " + className + "." + methodName + " because of: " + innerExceptionDesc;
		}
	}
}