using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ObjectMapping.Attributes;
using ObjectMapping.Database.Connections;

namespace ObjectMapping.Database
{
	internal class TableExecutor : ITableExecutor
	{
		private IConnectionManager connectionManager;

		public IConnectionManager ConnectionManager
		{
			get { return connectionManager; }
			set { connectionManager = value; }
		}

		public virtual void CreateTables(ICollection<ClassMetaData> metaDatas)
		{
            foreach(var metadata in metaDatas)
            {
                CreateTable(metadata);
            }
		}

		public virtual int CreateTable(ClassMetaData metadata)
		{
			var tableName = metadata.MappingTable;
			var properties = metadata.Properties;
			var queryBuilder = new StringBuilder("CREATE TABLE IF NOT EXISTS " + tableName);
			var createDefinitionBuilder = new StringBuilder("(");
			foreach (var pair in properties)
			{
				var mappingInfo = pair.Value;
				var propertyInfo = mappingInfo.PropertyInfo;
				var columnType = GetType(propertyInfo);
				var columnName = mappingInfo.MappingField;
				createDefinitionBuilder.Append("`" + columnName + "`" + " " + columnType);
				if (mappingInfo.NotNull)
				{
					createDefinitionBuilder.Append(" NOT NULL");
				}
				if (mappingInfo.DefaultValue != null)
				{
					createDefinitionBuilder.Append(" DEFAULT " + mappingInfo.DefaultValue);
				}
				if (mappingInfo.AutoIncrement)
				{
					createDefinitionBuilder.Append(" AUTO_INCREMENT");
				}
				if (propertyInfo.Name != "Id" && mappingInfo.Unique)
				{
					createDefinitionBuilder.Append(" UNIQUE");
				}
				createDefinitionBuilder.Append(", ");
				if (mappingInfo.Indexing != PropertyAttribute.NO_INDEX)
				{
					createDefinitionBuilder.Append("INDEX (`" + columnName + "`), ");
				}
			}
			createDefinitionBuilder.Append(" PRIMARY KEY (Id) ");
			var createDefinitionBuilderStr = createDefinitionBuilder.ToString();
			queryBuilder.Append(createDefinitionBuilderStr + ")");
			using (var connection = connectionManager.GetUpdateConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = queryBuilder.ToString();
				return command.ExecuteNonQuery();
			}
		}

		public virtual void CreateRelation(ICollection<ClassMetaData> metaDatas)
		{
            foreach(var metadata in metaDatas)
            {
                var relationProperties = metadata.RelationProperties;
                foreach (var pair in relationProperties)
                {
					CreateTable(pair.Value);
                	var originalMappingTable = metadata.MappingTable;
                	var queryBuilder = new StringBuilder("ALTER TABLE " + originalMappingTable + " ");
                	queryBuilder.Append("ADD COLUMN " + pair.Value.PartnerKey + "Info BLOB NULL");
					using (var connection = connectionManager.GetUpdateConnection())
					{
						var command = connection.CreateCommand();
						command.CommandText = queryBuilder.ToString();
						command.ExecuteNonQuery();
					}
                }
            }
		}

		public virtual int CreateTable(RelationInfo relationInfo)
		{
			var tableName = relationInfo.MappingTable;
			var queryBuilder = new StringBuilder();
			if (relationInfo.RelationKind == RelationInfo.RELATION_1_1 || relationInfo.RelationKind == RelationInfo.RELATION_1_N)
			{
				var originalTable = relationInfo.OriginalMetadata.MappingTable;
				queryBuilder.Append("ALTER TABLE " + tableName + " ");
				queryBuilder.Append("ADD COLUMN " + relationInfo.OriginalKey + " BIGINT(20) NULL, ");
				queryBuilder.Append("ADD FOREIGN KEY (" + relationInfo.OriginalKey + ") REFERENCES " + originalTable +
				                    " (Id) ON DELETE SET NULL, ");
				queryBuilder.Append("ADD INDEX (" + relationInfo.OriginalKey + ")");
			}
			else  if (relationInfo.RelationKind == RelationInfo.RELATION_N_N)
			{
				var originalTable = relationInfo.OriginalMetadata.MappingTable;
				var partnerTable = relationInfo.PartnerMetadata.MappingTable;
				queryBuilder.Append("CREATE TABLE IF NOT EXISTS " + tableName + "(");
				queryBuilder.Append("Id BIGINT(20) NOT NULL AUTO_INCREMENT PRIMARY KEY,");
				queryBuilder.Append(relationInfo.OriginalKey + " BIGINT(20) NOT NULL, ");
				queryBuilder.Append(" FOREIGN KEY (" + relationInfo.OriginalKey + ") REFERENCES " + originalTable +
				                    " (Id) ON DELETE CASCADE, ");
				queryBuilder.Append(relationInfo.PartnerKey + " BIGINT(20) NOT NULL, ");
				queryBuilder.Append(" FOREIGN KEY (" + relationInfo.PartnerKey + ") REFERENCES " + partnerTable +
				                    " (Id) ON DELETE CASCADE, ");
				queryBuilder.Append(" INDEX (" + relationInfo.OriginalKey + "), ");
				queryBuilder.Append(" INDEX (" + relationInfo.PartnerKey + ")");
				queryBuilder.Append(")");
			}
			using (var connection = connectionManager.GetUpdateConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = queryBuilder.ToString();
				return command.ExecuteNonQuery();
			}
		}

		public static  string GetType(Type elementType)
		{
			string strtype = ConvertPrimaryType(elementType);
			if (strtype != null)
			{
				return strtype;
			}
			else if (elementType == typeof(string))
			{
				return "TEXT";
			}
			return null;
		}

		public static string GetType(PropertyInfo propertyInfo)
		{
			Type typeP = propertyInfo.PropertyType;
			string strtype = ConvertPrimaryType(typeP);
			if (strtype != null)
			{
				return strtype;
			}
			if (typeP == typeof(string))
			{
				object[] attribute = propertyInfo.GetCustomAttributes(typeof(StringAttribute), true);
				if (attribute.Length > 0)
				{
					var @string = (StringAttribute)attribute[0];
					strtype = ConverString(@string);
				}
				else
				{
					strtype = "VARCHAR(255)";
				}
				return strtype;
			}
			else if (typeP.IsGenericType)
			{
				var propertyDefinition = typeP.GetGenericTypeDefinition();
				if (propertyDefinition == typeof(IList<>) || propertyDefinition == typeof(List<>) || 
					propertyDefinition == typeof(IDictionary<,>) || propertyDefinition == typeof(Dictionary<,>))
				{
					return "BLOB";
				}
			}
			else if (typeP.IsArray)
			{
				return "BLOB";
			}
			return null;
		}

		public static string ConvertPrimaryType(Type type)
		{
			if (type.IsPrimitive)
			{
				if (type == typeof(Char))
				{
					return "CHAR(1)";
				}
				else if (type == typeof(Int32))
				{
					return "INTEGER";
				}
				else if (type == typeof(Int16))
				{
					return "MEDIUMINT";
				}
				else if (type == typeof(Int64))
				{
					return "BIGINT(20)";
				}
				else if (type == typeof(UInt32))
				{
					return "INTEGER UNSIGNED";
				}
				else if (type == typeof(UInt16))
				{
					return "MEDIUMINT UNSIGNED";
				}
				else if (type == typeof(UInt64))
				{
					return "BIGINT(20) UNSIGNED";
				}
				else if (type == typeof(Byte[]))
				{
					return "BLOB";
				}
				else if (type == typeof(Byte))
				{
					return "TINYINT(127)";
				}
				else if (type == typeof(SByte))
				{
					return "TINYINT(127) unsigned";
				}
				else if (type == typeof(Boolean))
				{
					return "TINYINT(1)";
				}
				else if (type == typeof(Single))
				{
					return "FLOAT";
				}
				return type.Name;
			}
			else if (type == typeof(DateTime))
			{
				return "DATETIME";
			}
			else if (type == typeof(Decimal))
			{
				return "DECIMAL";
			}
			return null;
		}

		public static string ConverString(StringAttribute attribute)
		{
			switch (attribute.Type)
			{
				case StringAttribute.UNLIMITED_STRING:
					return "TEXT";
				case StringAttribute.LIMIT_STRING:
					int n = attribute.NumberOfChar;
					return String.Format("VARCHAR({0})", n);
				default:
					return "VARCHAR (255)";
			}
		}
	}
}
