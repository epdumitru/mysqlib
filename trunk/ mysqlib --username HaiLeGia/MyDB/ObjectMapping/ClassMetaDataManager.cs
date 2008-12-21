using System;
using System.Collections.Generic;

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
					result.Create();
				}
				catch (Exception e)
				{
					Logger.Log.WriteLog("Exception while register object: " + e);
				}
			}
			return result;
		}
	}
}
