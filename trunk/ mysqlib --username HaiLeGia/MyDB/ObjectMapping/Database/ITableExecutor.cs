using System.Collections.Generic;
using ObjectMapping.Database.Connections;

namespace ObjectMapping.Database
{
	internal interface ITableExecutor
	{
		IConnectionManager ConnectionManager { get; set; }
		void CreateTables(ICollection<ClassMetaData> metaDatas);
		int CreateTable(ClassMetaData metadata);
		void CreateRelation(ICollection<ClassMetaData> metaDatas);
		int CreateTable(RelationInfo relationInfo);
	}
}
