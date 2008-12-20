using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using ObjectMapping.Database.Connections;

namespace ObjectMapping.Database
{
	public class QueryExecutor
	{
		private IConnectionManager connectionManager;

		public IConnectionManager ConnectionManager
		{
			get { return connectionManager; }
			set { connectionManager = value; }
		}

		public virtual int ExecutNonQuery(Query query)
		{
			using (var connection = connectionManager.GetUpdateConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = query.SqlQuery;
				return command.ExecuteNonQuery();
			}
		}

		public virtual IList<T> Select<T>(SelectQuery query) where T : class
		{
			using (var connection = connectionManager.GetReadConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = query.SqlQuery;
				var reader = command.ExecuteReader();
				return CreateObjects<T>(reader, query.PropertyNames);
			}
		}

		public virtual T SelectUnique<T>(SelectQuery query) where T : class
		{
			using (var connection = connectionManager.GetReadConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = query.SqlQuery;
				var reader = command.ExecuteReader(CommandBehavior.SingleRow);
				return CreateObject<T>(reader, query.PropertyNames);
			}
		}

		public virtual T SelectById<T>(long id, params string[] propertyNames) where T : class 
		{
			var type = typeof (T);
			var metadata = ClassMetaDataManager.Instace.GetClassMetaData(type);
			var mappingTable = metadata.MappingTable;
			var mappingPrimKey = metadata.MappingPrimaryKey;
			using (var connection = connectionManager.GetReadConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = {2}", mappingTable, mappingPrimKey, id);
				var reader = command.ExecuteReader(CommandBehavior.SingleRow);
				return CreateObject<T>(reader, propertyNames);
			}
		}

		private static T CreateObject<T>(DbDataReader reader, string[] propertyNames) where T : class 
		{
			var type = typeof (T);
			var classMetadata = ClassMetaDataManager.Instace.GetClassMetaData(type);
			if (reader.Read())
			{
				var o = classMetadata.GetDbObject();
				o.ReadFields(reader, propertyNames);
				return (T) o;
			}
			return null;
		}

		private static IList<T> CreateObjects<T>(DbDataReader reader, string[] propertyNames)
		{
			var result = new List<T>();
			var type = typeof(T);
			var classMetadata = ClassMetaDataManager.Instace.GetClassMetaData(type);
			while (reader.Read())
			{
				var o = classMetadata.GetDbObject();
				o.ReadFields(reader, propertyNames);
				result.Add((T) o);
			}
			return result;
		}
	}
}