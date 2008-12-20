using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using ObjectMapping.Attributes;
using ObjectMapping.Database.Connections;

namespace ObjectMapping.Database
{
	public class QueryExecutor : IQueryExecutor
	{
		private IConnectionManager connectionManager;

		public IConnectionManager ConnectionManager
		{
			get { return connectionManager; }
			set { connectionManager = value; }
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
				return CreateObjects<T>(reader, query.PropertyNames);
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
				return CreateObject<T>(reader, query.PropertyNames);
			}
		}

		public virtual T SelectById<T>(long id, IsolationLevel? isolationLevel, params string[] propertyNames) where T : class 
		{
			var type = typeof (T);
			var metadata = ClassMetaDataManager.Instace.GetClassMetaData(type);
			var mappingTable = metadata.MappingTable;
			var mappingPrimKey = metadata.MappingPrimaryKey;
			using (var connection = connectionManager.GetReadConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = string.Format("SELECT * FROM {0} WHERE {1} = {2}", mappingTable, mappingPrimKey, id);
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
				return CreateObject<T>(reader, propertyNames);
			}
		}

		public int Update(IDbObject dbObject, int updateDepth, IsolationLevel? isolationLevel)
		{
			using (var connection = connectionManager.GetUpdateConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = dbObject.Update(updateDepth);
				if (isolationLevel != null)
				{
					var transaction = connection.BeginTransaction();
					try
					{
						var result = command.ExecuteNonQuery();
						transaction.Commit();
						return result;
					}
					catch(Exception e)
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

		public int Insert(IDbObject dbObject, IsolationLevel? isolationLevel)
		{
			using (var connection = connectionManager.GetUpdateConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = dbObject.Insert();
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

		public long Count<T>(IsolationLevel? isolationLevel) where T : IDbObject
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