using System.Collections.Generic;
using System.Data.Common;
using ObjectSerializer;

namespace ObjectMapping.Database
{
	public interface IDbObject : ISerializable
	{
		long Id { get; set; }
		ClassMetaData Metadata { get; set; }
		void ReadField(DbDataReader reader, QueryExecutor executor, params string[] propertyNames);
	}
}
