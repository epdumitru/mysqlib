using System.Data.Common;

namespace ObjectMapping.Database
{
	public interface IDbFunctionHelper
	{
		DbObjectContainer DbObjectContainer { get; set;}
		string GetUpdateString(object o);
		string GetInsertString(object o);
		object ReadObject(string typeName, DbDataReader reader, string[] propertyNames);
	}
}
