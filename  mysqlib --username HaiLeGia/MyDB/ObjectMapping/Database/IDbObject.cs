using System.Collections.Generic;
using System.Data.Common;
using ObjectSerializer;

namespace ObjectMapping.Database
{
	public interface IDbObject : ISerializable
	{
		long Id { get; set; }
		ClassMetaData Metadata { get; set; }
		void ReadPrimitive(DbDataReader reader);
		void ReadObject(DbDataReader reader, string propetyName);
		void ReadObject(IList<object> otherPrimValues, string propertyName, QueryExecutor executor);
		void ReadList(DbDataReader reader, string propertyName);
		void ReadDict(DbDataReader reader, string propertyName);
	}
}
