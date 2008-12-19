using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectMapping
{
	internal class ClassMetaDataManager
	{
		private IDictionary<Type, ClassMetaData> metadataMaps;
		public static ClassMetaDataManager Instace = new ClassMetaDataManager();

		private ClassMetaDataManager()
		{
			metadataMaps = new Dictionary<Type, ClassMetaData>();	
		}

		public void Register(Assembly assembly)
		{
			var types = assembly.GetTypes();
			for (var i = 0; i < types.Length; i++)
			{
				GetClassMetaData(types[i]);
			}
		}

		public ClassMetaData GetClassMetaData(Type type)
		{
			ClassMetaData result;
			metadataMaps.TryGetValue(type, out result);
			if (result == null)
			{
				try
				{
					result = new ClassMetaData(type);
					metadataMaps.Add(type, result);
				}
				catch {}
			}
			return result;
		}
	}
}
