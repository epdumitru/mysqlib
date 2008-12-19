using System;
using ObjectSerializer;

namespace NTSock.Controller
{
	[Serializable]
	public abstract class Mail
	{
		public const byte NO_ZIP = 0;
		public const byte REQUEST_TYPE = 0;
		public const byte RESPONSE_TYPE = 1;
		protected byte type;

		/// <summary>
		/// If this is a response, this field tell us which request is being responsed
		/// </summary>
		internal abstract long Id { get; set; }

		internal abstract byte ZipType { get; set; }

		internal byte Type
		{
			get { return type; }
		}

	}
}