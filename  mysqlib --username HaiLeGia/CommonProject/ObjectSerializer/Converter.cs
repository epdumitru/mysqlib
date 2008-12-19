using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectSerializer
{
	public class Converter
	{
		public static ISerializable Convert(object o)
		{
			if (o == null)
			{
				return null;
			}
			if (o is ISerializable)
			{
				return (ISerializable) o;
			}
			var type = o.GetType();
			var serializableType = SerializableTypeManager.Instance.GetSerializableType(type);
			if (serializableType == null)
			{
				lock (type)
				{
					serializableType = SerializableTypeManager.Instance.GetSerializableType(type) ??
									   SerializableTypeFactory.CreateSerializableType(type);
					SerializableTypeManager.Instance.Add(type, serializableType);
				}
			}
			return (ISerializable) Activator.CreateInstance(serializableType);	
		}

		public static ISerializable Convert(object o, IDictionary<object, ISerializable> convertedObjects)
		{
			if (o == null)
			{
				return null;
			}
			var objectType = o.GetType();
			if (convertedObjects.ContainsKey(o))
			{
				return convertedObjects[o];
			}
			if (o is ISerializable)
			{
				return (ISerializable) o;
			}
			var serializableType = SerializableTypeManager.Instance.GetSerializableType(objectType);
			if (serializableType == null)
			{
				lock (objectType)
				{
					serializableType = SerializableTypeManager.Instance.GetSerializableType(objectType) ??
					                   SerializableTypeFactory.CreateSerializableType(objectType);
					SerializableTypeManager.Instance.Add(objectType, serializableType);
				}
			}
			return (ISerializable) Activator.CreateInstance(serializableType, o, convertedObjects);
		}
	}
}