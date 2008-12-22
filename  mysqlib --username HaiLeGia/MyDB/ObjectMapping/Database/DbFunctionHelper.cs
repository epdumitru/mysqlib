using System;
using System.Collections.Generic;
using System.Data.Common;

namespace ObjectMapping.Database
{
	public interface IDbFunctionHelper
	{
		DbObjectContainer DbObjectContainer { get; set;}
		int Update(IDbObject o, DbConnection connection, long updateTime);
		int Insert(IDbObject o, DbConnection connection);
		object ReadObject(Type type, DbDataReader reader, IList<string> propertyNames);
		int Insert(IDbObject o, DbConnection connection, long referenceId, string referenceColumn);
	}
}
