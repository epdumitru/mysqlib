using System.Collections.Generic;
using System.Data;
using ObjectMapping.Database;

namespace ObjectMapping.Attributes
{
	public interface IQueryExecutor
	{
		int ExecutNonQuery(Query query, IsolationLevel? isolationLevel);
		IList<T> Select<T>(SelectQuery query, IsolationLevel? isolationLevel) where T : class;
		T SelectUnique<T>(SelectQuery query, IsolationLevel? isolationLevel) where T : class;
		T SelectById<T>(long id, IsolationLevel? isolationLevel, params string[] propertyNames) where T : class;
		int Update(IDbObject dbObject, int updateDepth, IsolationLevel? isolationLevel);
		int Insert(IDbObject dbObject, IsolationLevel? isolationLevel);
		long Count<T>(IsolationLevel? isolationLevel) where T : IDbObject;
	}
}
