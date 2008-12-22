using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using ObjectMapping.Database.Connections;

namespace ObjectMapping.Database
{
	public class QueryExecutor : IQueryExecutor
	{
		private IConnectionManager connectionManager;
		private IDbFunctionHelper dbFunctionHelper;

		public IConnectionManager ConnectionManager
		{
			get { return connectionManager; }
			set { connectionManager = value; }
		}

		public IDbFunctionHelper DbFunctionHelper
		{
			get { return dbFunctionHelper; }
			set { dbFunctionHelper = value; }
		}

		public virtual int ExecutNonQuery(Query query, IsolationLevel? isolationLevel)
		{
			using (var connection = connectionManager.GetUpdateConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = query.SqlQuery;
				if (isolationLevel != null)
				{
					var transaction = connection.BeginTransaction();
					try
					{
						var result = command.ExecuteNonQuery();
						transaction.Commit();
						return result;
					}
					catch (Exception e)
					{
						transaction.Rollback();
						throw new ApplicationException(e.ToString());
					}
				}
				else
				{
					return command.ExecuteNonQuery();
				}
			}
		}

		public virtual IList<T> Select<T>(SelectQuery query, IsolationLevel? isolationLevel) where T : class
		{
			using (var connection = connectionManager.GetReadConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = query.SqlQuery;
				DbDataReader reader;
				if (isolationLevel != null)
				{
					var transaction = connection.BeginTransaction();
					try
					{
						reader = command.ExecuteReader();
						transaction.Commit();
					}
					catch (Exception e)
					{
						transaction.Rollback();
						throw new ApplicationException(e.ToString());
					}
				}
				else
				{
					reader = command.ExecuteReader();
				}
				return CreateObjects<T>(reader, query.PropertyNames, connection);
			}
		}

		public virtual T SelectUnique<T>(SelectQuery query, IsolationLevel? isolationLevel) where T : class
		{
			using (var connection = connectionManager.GetReadConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = query.SqlQuery;
				DbDataReader reader;
				if (isolationLevel != null)
				{
					var transaction = connection.BeginTransaction();
					try
					{
						reader = command.ExecuteReader();
						transaction.Commit();
					}
					catch (Exception e)
					{
						transaction.Rollback();
						throw new ApplicationException(e.ToString());
					}
				}
				else
				{
					reader = command.ExecuteReader();
				}
				return CreateObject<T>(reader, query.PropertyNames, connection);
			}
		}

		public virtual T SelectById<T>(long id, IsolationLevel? isolationLevel, params string[] propertyNames) where T : class 
		{
			IList<string> selectProperties;
			if (propertyNames != SelectQuery.ALL_PROPS)
			{
				var tempSelectProperties = new List<string>(propertyNames);
				if (!tempSelectProperties.Contains("Id"))
				{
					tempSelectProperties.Add("Id");
				}
				if (!tempSelectProperties.Contains("UpdateTime"))
				{
					tempSelectProperties.Add("UpdateTime");
				}
				selectProperties = tempSelectProperties;
			}
			else
			{
				selectProperties = propertyNames;
			}
			var type = typeof (T);
			var metadata = ClassMetaDataManager.Instace.GetClassMetaData(type);
			var properties = metadata.Properties;
			var mappingTable = metadata.MappingTable;
			using (var connection = connectionManager.GetReadConnection())
			{
				var command = connection.CreateCommand();
				var str = new StringBuilder("SELECT ");
				if (propertyNames == SelectQuery.ALL_PROPS)
				{
					str.Append("* ");
				}
				else
				{
					for (var i = 0; i < selectProperties.Count; i++)
					{
						if (properties.ContainsKey(selectProperties[i]))
						{
							str.Append(selectProperties[i] + ", ");	
						}
					}
					var tmpString = str.ToString(0, str.Length - 2);
					str.Length = 0;
					str.Append(tmpString);
				}
				str.AppendFormat(" FROM {0} WHERE Id = {1}", mappingTable, id);
				command.CommandText = str.ToString();
				DbDataReader reader;
				if (isolationLevel != null)
				{
					var transaction = connection.BeginTransaction();
					try
					{
						reader = command.ExecuteReader(CommandBehavior.SingleRow);
						transaction.Commit();
					}
					catch (Exception e)
					{
						transaction.Rollback();
						throw new ApplicationException(e.ToString());
					}
				}
				else
				{
					reader = command.ExecuteReader(CommandBehavior.SingleRow);
				}
				return CreateObject<T>(reader, selectProperties, connection);
			}
		}

		public int Update(IDbObject dbObject, IsolationLevel? isolationLevel)
		{
			using (var connection = connectionManager.GetUpdateConnection())
			{
				if (isolationLevel != null)
				{
					var transaction = connection.BeginTransaction();
					try
					{
						var classMetadata = ClassMetaDataManager.Instace.GetClassMetaData(dbObject.GetType());
						var command = connection.CreateCommand();
						command.CommandText = "SELECT UpdateTime FROM " + classMetadata.MappingTable + " WHERE Id = " + dbObject.Id;
						long updateTime = -1;
						using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
						{
							updateTime = reader.GetInt64(reader.GetOrdinal("UpdateTime"));
							if (updateTime > dbObject.UpdateTime)
							{
								return 0;
							}
						}
						if (updateTime == -1)
						{
							throw new ApplicationException("Cannot get update time of object: " + dbObject + " with id: " + dbObject.Id);
						}
						var result = dbFunctionHelper.Update(dbObject, connection, new Dictionary<IDbObject, long>());
						transaction.Commit();
						return result;
					}
					catch(Exception e)
					{
						transaction.Rollback();
						throw new ApplicationException(e.ToString());
					}
				}
				return dbFunctionHelper.Update(dbObject, connection, new Dictionary<IDbObject, long>());
			}
		}

		public long Insert(IDbObject dbObject, IsolationLevel? isolationLevel)
		{
			using (var connection = connectionManager.GetUpdateConnection())
			{
				if (isolationLevel != null)
				{
					var transaction = connection.BeginTransaction();
					try
					{
						var result = dbFunctionHelper.Insert(dbObject, connection, new Dictionary<IDbObject, long>());
						transaction.Commit();
						return result;
					}
					catch (Exception e)
					{
						transaction.Rollback();
						throw new ApplicationException(e.ToString());
					}
				}
				else
				{
					return dbFunctionHelper.Insert(dbObject, connection, new Dictionary<IDbObject, long>());
				}
			}
		}

		public long Count<T>(IsolationLevel? isolationLevel)
		{
			var type = typeof (T);
			var classMetadata = ClassMetaDataManager.Instace.GetClassMetaData(type);
			var mappingTable = classMetadata.MappingTable;
			using (var connection = connectionManager.GetUpdateConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = "SELECT COUNT(Id) FROM " + mappingTable;
				if (isolationLevel != null)
				{
					var transaction = connection.BeginTransaction();
					try
					{
						var result = (long) command.ExecuteScalar();
						transaction.Commit();
						return result;
					}
					catch (Exception e)
					{
						transaction.Rollback();
						throw new ApplicationException(e.ToString());
					}
				}
				else
				{
					return (long) command.ExecuteScalar();
				}
			}
		}

		private T CreateObject<T>(DbDataReader reader, IList<string> propertyNames, DbConnection connection) where T : class 
		{
			var type = typeof (T);
			if (reader.Read())
			{
				return (T) dbFunctionHelper.ReadObject(type, reader, propertyNames, new Dictionary<string, IDbObject>(), connection);
			}
			return null;
		}

		private IList<T> CreateObjects<T>(DbDataReader reader, IList<string> propertyNames, DbConnection connection)
		{
			var result = new List<T>();
			var type = typeof(T);
			while (reader.Read())
			{
				result.Add((T) dbFunctionHelper.ReadObject(type, reader, propertyNames, new Dictionary<string, IDbObject>(), connection));
			}
			return result;
		}
	}
}