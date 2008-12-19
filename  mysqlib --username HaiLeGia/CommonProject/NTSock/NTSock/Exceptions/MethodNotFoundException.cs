using System;
using System.Runtime.Serialization;

namespace NTSock.Exceptions
{
	[Serializable]
	public class MethodNotFoundException : NTSockException
	{
		/// <summary>
		/// Class that is executing.
		/// </summary>
		private readonly string className;

		/// <summary>
		/// Method that cannot be found.
		/// </summary>
		private readonly string methodName;

        public MethodNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		public MethodNotFoundException()
		{
		}

		public MethodNotFoundException(string className, string methodName)
			: base(null)
		{
			this.className = className;
			this.methodName = methodName;
		}


		/// <summary>
		/// Class that is executing.
		/// </summary>
		public string ClassName
		{
			get { return className; }
		}

		/// <summary>
		/// Method that cannot be found.
		/// </summary>
		public string MethodName
		{
			get { return methodName; }
		}

		public override string ToString()
		{
			return "Cannot find method: " + methodName + " in class: " + className + " because of: " + innerExceptionDesc;
		}
	}
}