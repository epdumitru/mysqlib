using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ObjectSerializer;

namespace NTSockv3.Messages
{
	public class VoidResultResponse : Response, ISerializable
	{
		public void Serialize(BinaryWriter writer, IDictionary<object, int> objectGraph, int index)
		{
			writer.Write("NTSockv3.Messages.VoidResultResponse, NTSockv3");	
			writer.Write(id);
			writer.Write(type);
			writer.Write(zipType);
		}

		public void Deserialize(BinaryReader reader, IDictionary<int, object> objectGraph)
		{
			id = reader.ReadInt64();
			type = reader.ReadInt32();
			zipType = reader.ReadByte();
		}

		public override object GetResult()
		{
			return null;
		}
	}
}
