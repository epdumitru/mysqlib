using System;
using System.Collections.Generic;
using System.IO;
using ObjectSerializer;

namespace NTSockv3.Messages
{
	public class ExceptionResponse : Response, ISerializable
	{
		private string exception;

		public ExceptionResponse()
		{
			this.exception = "";
		}

		public ExceptionResponse(long id, Exception e)
		{
			exception = e.ToString();
			this.id = id;
		}

		public string Exception
		{
			get { return exception; }
		}


		public void Serialize(BinaryWriter writer, IDictionary<object, int> objectGraph, int index)
		{
			writer.Write("NTSockv3.Messages.ExceptionResponse, NTSockv3");
			writer.Write(id);
			writer.Write(type);
			writer.Write(zipType);
			writer.Write(exception);
		}

		public void Deserialize(BinaryReader reader, IDictionary<int, object> objectGraph)
		{
			id = reader.ReadInt64();
			type = reader.ReadInt32();
			zipType = reader.ReadByte();
			exception = reader.ReadString();
		}

		public override object GetResult()
		{
			return null;
		}
	}
}
