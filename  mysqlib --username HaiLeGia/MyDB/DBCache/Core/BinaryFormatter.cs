using System;
using System.Collections.Generic;
using System.IO;

namespace DBCache.Core
{
	public class BinaryFormatter
	{
		public void Serialize(ISerializable serializable, BinaryWriter writer)
		{
			serializable.Serialize(writer, new Dictionary<object, int>(), 0);
		}

		public void Serialize(object o, BinaryWriter writer)
		{
			var serializableObject = Converter.Convert(o, new Dictionary<object, ISerializable>());
			serializableObject.Serialize(writer, new Dictionary<object, int>(), 0);
		}

		public ISerializable Deserialize(BinaryReader reader)
		{
			var typeName = reader.ReadString();
			var type = Type.GetType(typeName);
			var serializable = (ISerializable) Activator.CreateInstance(type);
			var dict = new Dictionary<int, object>();
			dict.Add(0, serializable);
			serializable.Deserialize(reader, dict);
			return serializable;
		}

		public T Deserialize<T>(BinaryReader reader) where T : class
		{
			var typeName = reader.ReadString();
			var type = Type.GetType(typeName);
			var serializable = (ISerializable) Activator.CreateInstance(type);
			var dict = new Dictionary<int, object>();
			dict.Add(0, serializable);
			serializable.Deserialize(reader, dict);
			return (T) serializable;
		}
	}
}
