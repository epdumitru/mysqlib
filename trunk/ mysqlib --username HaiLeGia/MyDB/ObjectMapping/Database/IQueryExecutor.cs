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
        T SelectByForeignKey<T>(string foreignKeyName, long referencedId, IsolationLevel? isolationLevel,
		                        params string[] propertyNames) where T : class;
		int Update(IDbObject dbObject, IsolationLevel? isolationLevel);
		int Insert(IDbObject dbObject, IsolationLevel? isolationLevel);
		int Insert(IDbObject dbObject, IsolationLevel? isolationLevel, long referencedId, string referencedColumn);
		long Count<T>(IsolationLevel? isolationLevel);
		
	}
}