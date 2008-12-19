using System;
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

		public virtual IList<T> ExecuteReader<T>(Query query) where T : class
		{
			using (var connection = connectionManager.GetReadConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = query.SqlQuery;
				var reader = command.ExecuteReader();
				return CreateObjects<T>(reader);
			}
		}

		public virtual T ExecuteUnique<T>(Query query) where T : class
		{
			using (var connection = connectionManager.GetReadConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = query.SqlQuery;
				var reader = command.ExecuteReader(CommandBehavior.SingleRow);
				return CreateObject<T>(reader);
			}
		}

		public virtual T GetObjectByKey<T>(object primValue) where T : class 
		{
			var type = typeof (T);
			var metadata = ClassMetaDataManager.Instace.GetClassMetaData(type);
			var mappingTable = metadata.MappingTable;
			var mappingPrimKey = metadata.MappingPrimaryKey;
			using (var connection = connectionManager.GetReadConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = {2}", mappingTable, mappingPrimKey, primValue);
				var reader = command.ExecuteReader(CommandBehavior.SingleRow);
				return CreateObject<T>(reader);
			}
		}

		public virtual void LoadObjectField(IDbObject dbObject, string propertyName)
		{
			var metaData = dbObject.Metadata;
			if (metaData.RelationProperties.ContainsKey(propertyName))
			{
				var relationInfo = metaData.RelationProperties[propertyName];
				var mappingKind = relationInfo.RelationKind;
				var mappingTable = relationInfo.MappingTable;

				if (mappingTable != null)
				{
					var sqlQuery = string.Format("SELECT * FROM {0} WHERE {1} = {2}", mappingTable, relationInfo.PartnerKey, dbObject.Id);
					using (var connection = connectionManager.GetReadConnection())
					{
						var command = connection.CreateCommand();
						command.CommandText = sqlQuery;
						DbDataReader reader;
						switch (mappingKind)
						{
							case RelationInfo.RELATION_1_1:
								reader = command.ExecuteReader(CommandBehavior.SingleResult);
								dbObject.ReadObject(reader, propertyName);
								break;
							case RelationInfo.RELATION_1_N:
								reader = command.ExecuteReader();
								dbObject.ReadObject(reader, propertyName);
								break;
							case RelationInfo.RELATION_N_N:
								reader = command.ExecuteReader();
								var listObject = new List<object>();
								int ordinal = reader.GetOrdinal(relationInfo.PartnerKey);
								while (reader.Read())
								{
									listObject.Add(reader.GetValue(ordinal));
								}
								dbObject.ReadObject(listObject, propertyName, this);
								break;
						}
					}
				}	
			}
			else if (metaData.ListProperties.ContainsKey(propertyName))
			{
				var listInfor = metaData.ListProperties[propertyName];

			}
			else if (metaData.DictProperties.ContainsKey(propertyName))
			{
				
			}
			else throw new ArgumentException("Unknown property: " + propertyName);
		}

		public virtual void LoadGenericField(IDbObject dbObject, string propertyName)
		{
			
		}
		private static T CreateObject<T>(DbDataReader reader) where T : class 
		{
			var type = typeof (T);
			var classMetadata = ClassMetaDataManager.Instace.GetClassMetaData(type);
			if (reader.Read())
			{
				var o = classMetadata.GetDbObject();
				o.ReadPrimitive(reader);
				return (T) o;
			}
			return null;
		}

		private static IList<T> CreateObjects<T>(DbDataReader reader)
		{
			var result = new List<T>();
			var type = typeof(T);
			var classMetadata = ClassMetaDataManager.Instace.GetClassMetaData(type);
			while (reader.Read())
			{
				var o = classMetadata.GetDbObject();
				o.ReadPrimitive(reader);
				result.Add((T) o);
			}
			return result;
		}
	}
}