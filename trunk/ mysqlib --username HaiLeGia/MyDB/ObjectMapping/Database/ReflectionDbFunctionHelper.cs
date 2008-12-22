using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using MySql.Data.MySqlClient;

namespace ObjectMapping.Database
{
	internal class ReflectionDbFunctionHelper : IDbFunctionHelper
	{
		private DbObjectContainer dbObjectContainer;

		public DbObjectContainer DbObjectContainer
		{
			get { return dbObjectContainer; }
			set { dbObjectContainer = value; }
		}

		public int Update(IDbObject o, DbConnection connection, IDictionary<IDbObject, long> objectGraph)
		{
			objectGraph.Add(o, o.Id);
			var classMetadata = ClassMetaDataManager.Instace.GetClassMetaData(o.GetType());
			var result = 0;
			if (o.IsDirty)
			{
				var mappingTable = classMetadata.MappingTable;
				var properties = classMetadata.Properties;
				var command = connection.CreateCommand();
				var queryBuilder = new StringBuilder("UPDATE " + mappingTable + " SET ");
				foreach (var mappingInfo in properties.Values)
				{
					var mappingField = mappingInfo.MappingField;
					queryBuilder.Append("`" + mappingField + "`=@" + mappingField + ", ");
				}
				var tmpString = queryBuilder.ToString(0, queryBuilder.Length - 2);
				queryBuilder.Length = 0;
				queryBuilder.Append(tmpString);
				queryBuilder.Append(" WHERE Id = " + o.Id);
				foreach (var mappingInfo in properties.Values)
				{
					var mappingField = mappingInfo.MappingField;
					var propertyInfo = mappingInfo.PropertyInfo;
					var propertyType = propertyInfo.PropertyType;
					object value = propertyInfo.GetValue(o, null);
					if (propertyType.IsPrimitive || propertyType == typeof(string) || propertyType == typeof(DateTime) || propertyType == typeof(decimal))
					{
						command.Parameters.Add(new MySqlParameter("@" + mappingField, value));
					}
					else
					{
						byte[] blob = null;
						if (value != null)
						{
							var formatter = new BinaryFormatter();
							using (var stream = new MemoryStream())
							{
								formatter.Serialize(stream, value);
								blob = stream.ToArray();
							}	
						}
						command.Parameters.Add(new MySqlParameter("@" + mappingField, blob));
					}
				}
				result = command.ExecuteNonQuery();
			}
			UpdateRelations(o, classMetadata, connection, objectGraph);
			return result;
		}

		private void UpdateRelations(IDbObject o, ClassMetaData classMetadata, DbConnection connection, IDictionary<IDbObject, long> objectGraph)
		{
			var relationProperties = classMetadata.RelationProperties;
			foreach (var relation in relationProperties.Values)
			{
				var propertyInfo = relation.PropertyInfo;
				var mappingTable = relation.MappingTable;
				var persistentRelation = new List<long>();
				var command = connection.CreateCommand();
				if (relation.RelationKind == RelationInfo.RELATION_N_N)
				{
					command.CommandText = "SELECT `" + relation.PartnerKey + "` FROM " + mappingTable + " WHERE `" + relation.OriginalKey + "` = " + o.Id;
				}
				else
				{
					command.CommandText = "SELECT Id FROM " + mappingTable + " WHERE `" + relation.OriginalKey + "` = " + o.Id;
				}
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						persistentRelation.Add(reader.GetInt64(reader.GetOrdinal("Id")));
					}	
				}
				if (relation.RelationKind == RelationInfo.RELATION_1_1)
				{
					Update11Relation(objectGraph, o, connection, relation, command, mappingTable, persistentRelation, propertyInfo);
				}
				else if (relation.RelationKind == RelationInfo.RELATION_1_N)
				{
					Update1NRelation(objectGraph, o, connection, relation, command, mappingTable, persistentRelation, propertyInfo);					
				}
				else if (relation.RelationKind == RelationInfo.RELATION_N_N)
				{
					UpdateNNRelation(objectGraph, o, connection, relation, command, mappingTable, persistentRelation, propertyInfo);					
				}
			}
		}

		private void UpdateNNRelation(IDictionary<IDbObject, long> objectGraph, IDbObject o, DbConnection connection, RelationInfo relation, DbCommand command, string mappingTable, List<long> persistentRelation, PropertyInfo propertyInfo)
		{
			var listCurrentPropertyValue = (IList)propertyInfo.GetValue(o, null);
			for (var i = 0; i < listCurrentPropertyValue.Count; i++)
			{
				var item = (IDbObject)listCurrentPropertyValue[i];
				if (item == null || objectGraph.ContainsKey(item)) continue;
				var itemId = item.Id;
				if (persistentRelation.Contains(itemId))
				{
					Update(item, connection, objectGraph);
					persistentRelation.Remove(itemId);
				}
				else
				{
					if (itemId == 0)
					{
						Insert(item, connection, objectGraph);
					}
					else
					{
						Update(item, connection, objectGraph);
					}
					command.CommandText = "INSERT INTO " + mappingTable + "  ( `" + relation.OriginalKey + "`, `" + relation.PartnerKey + "`) VALUES (" + o.Id + ", " + item.Id + ")";
					command.ExecuteNonQuery();
				}
			}
			if (persistentRelation.Count > 0)
			{
				command.CommandText = "DELETE FROM " + mappingTable + " WHERE `" + relation.OriginalKey + "` = " + o.Id + " AND `" + relation.PartnerKey + "` = @partnerId";
				command.Prepare();
				command.Parameters.Add(new MySqlParameter("@partnerId", persistentRelation[0]));
				command.ExecuteNonQuery();
				for (var i = 1; i < persistentRelation.Count; i++)
				{
					command.Parameters["@partnerId"].Value = persistentRelation[i];
					command.ExecuteNonQuery();
				}
			}
		}

		private void Update1NRelation(IDictionary<IDbObject, long> objectGraph, IDbObject o, DbConnection connection, RelationInfo relation, DbCommand command, string mappingTable, List<long> persistentRelation, PropertyInfo propertyInfo)
		{
			var listCurrentPropertyValue = (IList) propertyInfo.GetValue(o, null);
			for (var i = 0; i < listCurrentPropertyValue.Count; i++)
			{
				var item = (IDbObject) listCurrentPropertyValue[i];
				if (item == null || objectGraph.ContainsKey(item)) continue;
				var itemId = item.Id;
				if (persistentRelation.Contains(itemId))
				{
					Update(item, connection, objectGraph);
					persistentRelation.Remove(itemId);
				}
				else
				{
					if (itemId == 0)
					{
						Insert(item, connection, objectGraph);
					}
					else
					{
						Update(item, connection, objectGraph);
					}
					command.CommandText = "UPDATE " + mappingTable + " SET `" + relation.OriginalKey + "` = " + o.Id + " WHERE Id = " + item.Id;
					command.ExecuteNonQuery();
				}
			}
			if (persistentRelation.Count > 0)
			{
				command.CommandText = "UPDATE " + mappingTable + " SET `" + relation.OriginalKey + "` = NULL WHERE Id = @id";
				command.Prepare();
				command.Parameters.Add(new MySqlParameter("@Id", persistentRelation[0]));
				command.ExecuteNonQuery();
				for (var i = 1; i < persistentRelation.Count; i++)
				{
					command.Parameters["@Id"].Value = persistentRelation[i];
					command.ExecuteNonQuery();
				}	
			}
		}

		private void Update11Relation(IDictionary<IDbObject, long> objectGraph, IDbObject o, DbConnection connection, RelationInfo relation, DbCommand command, string mappingTable, List<long> persistentRelation, PropertyInfo propertyInfo)
		{
			var propertyValue = (IDbObject) propertyInfo.GetValue(o, null);
			if (propertyValue == null || objectGraph.ContainsKey(propertyValue))
			{
				return;
			}
			var updateOldPersistent = false;
			if (persistentRelation.Count > 0)
			{
				if (propertyValue.Id != persistentRelation[0])
				{
					updateOldPersistent = true;
				}
			}
			if (updateOldPersistent)
			{
				if (persistentRelation.Count > 0)
				{
					for (var i = 0; i < persistentRelation.Count; i++)
					{
						command.CommandText = "UPDATE " + mappingTable + " SET `" + relation.OriginalKey + "`= NULL WHERE Id = " + persistentRelation[i];
					}
				}
			}
			var updateReference = true;
			if (propertyValue.Id > 0)
			{
				if (persistentRelation.Count > 0 && propertyValue.Id == persistentRelation[0])
				{
					updateReference = false;
				}	
				Update(propertyValue, connection, objectGraph);
			}
			else
			{
				Insert(propertyValue, connection, objectGraph);
			}
			if (updateReference)
			{
				command.CommandText = "UPDATE " + mappingTable + " SET `" + relation.OriginalKey + "` = " + o.Id + " WHERE Id = " + propertyValue.Id;
				command.ExecuteNonQuery();
			}
		}

		public long Insert(IDbObject o, DbConnection connection, IDictionary<IDbObject, long> objectGraph)
		{
			if (o.Id > 0)
			{
				throw new ApplicationException("Cannot add an existed object");
			}
			objectGraph.Add(o, 0);
			var classMetadata = ClassMetaDataManager.Instace.GetClassMetaData(o.GetType());
			var result = 0;
			var mappingTable = classMetadata.MappingTable;
			var properties = classMetadata.Properties;
			var command = connection.CreateCommand();
			var queryBuilder = new StringBuilder("INSERT INTO " + mappingTable + " ( ");
			foreach (var mappingInfo in properties.Values)
			{
				var mappingField = mappingInfo.MappingField;
				queryBuilder.Append("`" + mappingField + "`, ");
			}
			var tmpString = queryBuilder.ToString(0, queryBuilder.Length - 2);
			queryBuilder.Length = 0;
			queryBuilder.Append(tmpString + ") VALUES (");
			foreach (var mappingInfo in properties.Values)
			{
				var mappingField = mappingInfo.MappingField;
				queryBuilder.Append("@" + mappingField + ", ");
			}
			tmpString = queryBuilder.ToString(0, queryBuilder.Length - 2);
			queryBuilder.Length = 0;
			queryBuilder.Append(tmpString + ")");
			foreach (var mappingInfo in properties.Values)
			{
				var mappingField = mappingInfo.MappingField;
				var propertyInfo = mappingInfo.PropertyInfo;
				var propertyType = propertyInfo.PropertyType;
				object value = propertyInfo.GetValue(o, null);
				if (propertyType.IsPrimitive || propertyType == typeof(string) || propertyType == typeof(DateTime) || propertyType == typeof(decimal))
				{
					command.Parameters.Add(new MySqlParameter("@" + mappingField, value));
				}
				else
				{
					byte[] blob = null;
					if (value != null)
					{
						var formatter = new BinaryFormatter();
						using (var stream = new MemoryStream())
						{
							formatter.Serialize(stream, value);
							blob = stream.ToArray();
						}
					}
					command.Parameters.Add(new MySqlParameter("@" + mappingField, blob));
				}
			}
			o.Id = (long) command.ExecuteScalar();
			UpdateRelations(o, classMetadata, connection, objectGraph);
			return o.Id;
		}

		public object ReadObject(Type type, DbDataReader reader, IList<string> propertyNames, IDictionary<string, IDbObject> objectGraph)
		{
			throw new System.NotImplementedException();
		}

	}
}
