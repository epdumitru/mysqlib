using System;
using System.Data.Common;

namespace ObjectMapping.Database
{
	public interface IDbFunctionHelper
	{
		DbObjectContainer DbObjectContainer { get; set;}
		int Update(object o, DbConnection connection);
		int Insert(object o, DbConnection connection);
		object ReadObject(Type type, DbDataReader reader, string[] propertyNames);
	}
}
