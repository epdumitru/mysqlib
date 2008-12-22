#define DEBUG
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using BLToolkit.Reflection.Emit;
using ObjectMapping.Database;
using ObjectMapping.Database.Connections;
using ObjectMapping.MySql.Connections;

namespace ObjectMapping
{
	public class DemoObject
	{
		private long id;
		private object syncRootObject;

		public long Id
		{
			get { return id; }
			set { id = value; }
		}

		public object SyncRootObject
		{
			get { return syncRootObject; }
			set { syncRootObject = value; }
		}

		
	}

	public class DbObjectContainer
	{
		private IConnectionManager connectionManager;
		private IQueryExecutor queryExecutor;
		private ITableExecutor tableExecutor;

		public IConnectionManager ConnectionManager
		{
			get { return connectionManager; }
		}

		internal ITableExecutor TableExecutor
		{
			get { return tableExecutor; }
		}

		public IQueryExecutor QueryExecutor
		{
			get { return queryExecutor; }
		}

		public DbObjectContainer()
		{
			Config(null);
		}

		public void Config(XmlDocument document)
		{
			var selectionAlgorithm = new RoundRobinConnectionSelection();
			var localhostInfor = new ConnectionInfo() { DatabaseName = "test", HostName = "127.0.0.1", Username = "root", Password = "root" };
			var listMaster = new List<ConnectionInfo>() { localhostInfor };
			var listSlave = new List<ConnectionInfo>() { localhostInfor };
			selectionAlgorithm.Infors = listSlave;
			connectionManager = new MySqlConnectionManager()
			                        	{
			                        		MasterConnectionSelection = selectionAlgorithm,
			                        		SlaveConnectionSelection = selectionAlgorithm,
			                        		MasterInfos = listMaster,
			                        		SlaveInfos = listSlave
			                        	};
			tableExecutor = new TableExecutor() {ConnectionManager = connectionManager};
			queryExecutor = new QueryExecutor() {ConnectionManager = connectionManager, DbFunctionHelper = new ReflectionDbFunctionHelper{DbObjectContainer = this}};
		}

		public void Register(Assembly assembly)
		{
			var types = assembly.GetTypes();
			var asmBuilderHelper = new AssemblyBuilderHelper("Db" + assembly.GetName().Name + ".dll");
		    var dict = new Dictionary<ClassMetaData, bool>();
            foreach (var type in types)
			{
				var result = ClassMetaDataManager.Instace.GetClassMetaData(type);

                if (result != null && !dict.ContainsKey(result))
                {
                    dict.Add(result, true);
                    tableExecutor.CreateTable(result);
                }
			}
            tableExecutor.CreateRelation(dict.Keys);
#if DEBUG
			asmBuilderHelper.Save();
#endif
		}
		
	}
}
