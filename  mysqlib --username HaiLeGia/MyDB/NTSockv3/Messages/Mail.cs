using System;

namespace NTSockv3.Messages
{
	[Serializable]
	public abstract class Mail
	{
		public const byte NO_ZIP = 0;
		public const int REQUEST_TYPE = 0;
		public const int RESPONSE_TYPE = 1;
		protected long id;
		protected int type;
		protected byte zipType;

		/// <summary>
		/// If this is a response, this field tell us which request is being responsed
		/// </summary>
		public long Id
		{
			get
			{
				return id;
			} 
			set
			{
				id = value;
			}
		}

		public byte ZipType
		{
			get { return zipType; }
			set { zipType = value; }
		}

		public int Type
		{
			get { return type; }
			set { type = value; }
		}
	}
}