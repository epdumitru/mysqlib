using System;
using System.Collections.Generic;
using System.IO;

namespace ObjectSerializer
{
	public class BinaryFormatter
	{
		public byte[] Serialize(ISerializable serializable)
		{
			using (var stream = new MemoryStream())
			{
				var writer = new BinaryWriter(stream);
				serializable.Serialize(writer, new Dictionary<object, int>(), 0);
				return stream.ToArray();
			}
		}

		public byte[] Serialize(object o)
		{
			using (var stream = new MemoryStream()) 
			{
				var writer = new BinaryWriter(stream);
				var serializableObject = Converter.Convert(o, new Dictionary<object, ISerializable>());
				serializableObject.Serialize(writer, new Dictionary<object, int>(), 0);
				return stream.ToArray();
			}
		}

		public T Deserialize<T>(Stream stream) where T : class
		{
			using (var reader = new BinaryReader(stream))
			{
				var typeName = reader.ReadString();
				var type = Type.GetType(typeName);
				var originalObject = Activator.CreateInstance(type);
				var serializable = Converter.Convert(originalObject);
				var dict = new Dictionary<int, object>();
				dict.Add(0, serializable);
				serializable.Deserialize(reader, dict);
				return (T)serializable;	
			}
		}
	}
}