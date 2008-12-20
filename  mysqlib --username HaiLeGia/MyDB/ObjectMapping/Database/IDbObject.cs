using System.Collections.Generic;
using System.Data.Common;
using ObjectSerializer;

namespace ObjectMapping.Database
{
	public interface IDbObject : ISerializable
	{
		object SyncRootObject { get; set; }
		long Id { get; set; }
		ClassMetaData Metadata { get; set; }
		void ReadFields(DbDataReader reader, params string[] propertyNames);
		string Update(int updateDepth);
		string Insert();
	}
}
