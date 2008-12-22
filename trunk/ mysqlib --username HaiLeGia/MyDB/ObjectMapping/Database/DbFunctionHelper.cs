using System;
using System.Collections.Generic;
using System.Data.Common;

namespace ObjectMapping.Database
{
	public interface IDbFunctionHelper
	{
		DbObjectContainer DbObjectContainer { get; set;}
		int Update(IDbObject o, DbConnection connection, IDictionary<IDbObject, long> objectGraph);
		long Insert(IDbObject o, DbConnection connection, IDictionary<IDbObject, long> objectGraph);
		object ReadObject(Type type, DbDataReader reader, IList<string> propertyNames, IDictionary<string, IDbObject> objectGraph);
	}
}
