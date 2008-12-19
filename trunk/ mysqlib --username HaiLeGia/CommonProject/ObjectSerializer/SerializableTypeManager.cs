using System;
using CommonLib;

namespace ObjectSerializer
{
	class SerializableTypeManager
	{
		public static readonly SerializableTypeManager Instance = new SerializableTypeManager();
		private TimeSpan deadTime = new TimeSpan(1, 0, 0, 0);
		private Cache<Type, Type> serializableTypeMaps;

		private SerializableTypeManager()
		{
			serializableTypeMaps = new Cache<Type, Type>(24 * 3600 * 1000);
		}

		public void Add(Type originalType, Type serializableType)
		{
			serializableTypeMaps.Insert(originalType, serializableType, deadTime, true, null);
		}

		public Type GetSerializableType(Type originalType)
		{
			if (originalType.IsSubclassOf(typeof(ISerializable)))
			{
				return originalType;
			}
			return serializableTypeMaps.Get(originalType);
		}
	}
}