using System;
using System.Data.Common;

namespace ObjectMapping.Database
{
	public interface IDbFunctionHelper
	{
		DbObjectContainer DbObjectContainer { get; set;}
		int Update(IDirtyObject o, DbConnection connection);
		int Insert(IDirtyObject o, DbConnection connection);
		object ReadObject(Type type, DbDataReader reader, string[] propertyNames);
	}
}
