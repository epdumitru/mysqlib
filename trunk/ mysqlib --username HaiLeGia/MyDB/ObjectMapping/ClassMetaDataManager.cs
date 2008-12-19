using System;
using System.Collections.Generic;
using System.Reflection;
using ObjectMapping.Database;
using ObjectMapping.Database.Connections;
using ObjectMapping.MySql.Connections;

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
		    var dict = new Dictionary<ClassMetaData, bool>();
			var types = assembly.GetTypes();
			for (var i = 0; i < types.Length; i++)
			{
				var metadata = GetClassMetaData(types[i]);
			    if (!dict.ContainsKey(metadata))
			    {
			        dict.Add(metadata, true);
			    }
            }
		    var exexutor = new TableExecutor();
		    var selectionAlgorithm = new RoundRobinConnectionSelection();
		    var localhostInfor = new ConnectionInfo()
		                             {DatabaseName = "test", HostName = "127.0.0.1", Username = "root", Password = "master"};
		    var listMaster = new List<ConnectionInfo>() {localhostInfor};
		    var listSlave = new List<ConnectionInfo>() {localhostInfor};
            exexutor.ConnectionManager = new MySqlConnectionManager() {MasterConnectionSelection = selectionAlgorithm, SlaveConnectionSelection = selectionAlgorithm, MasterInfos = listMaster, SlaveInfos = listSlave};
            
            exexutor.CreateTables(dict.Keys);
            exexutor.CreateRelation(dict.Keys);
            exexutor.CreateGenericType(dict.Keys);
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
