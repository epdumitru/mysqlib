using System;
using System.Collections.Generic;
using System.Data;
using ObjectMapping.Database.Connections;

namespace ObjectMapping.Database
{
	public interface IQueryExecutor
	{
		IConnectionManager ConnectionManager { get; set; }
		int ExecutNonQuery(Query query, IsolationLevel? isolationLevel);
		IList<T> Select<T>(SelectQuery query, IsolationLevel? isolationLevel) where T : class;
		T SelectUnique<T>(SelectQuery query, IsolationLevel? isolationLevel) where T : class;
		T SelectById<T>(long id, IsolationLevel? isolationLevel, params string[] propertyNames) where T : class;
		object SelectById(Type type, long id, IsolationLevel? isolationLevel, params string[] propertyNames);
        int Update(IDbObject dbObject, IsolationLevel? isolationLevel);
		long Insert(IDbObject dbObject, IsolationLevel? isolationLevel);
		long Count<T>(IsolationLevel? isolationLevel);
		
	}
}