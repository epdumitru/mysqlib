using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ObjectMapping.Attributes;
using ObjectMapping.Database.Connections;

namespace ObjectMapping.Database
{
	public class TableExecutor
	{
		private IConnectionManager connectionManager;

		public IConnectionManager ConnectionManager
		{
			get { return connectionManager; }
			set { connectionManager = value; }
		}

		public virtual void CreateTables(IList<ClassMetaData> metaDatas)
		{
			for (var i = 0; i < metaDatas.Count; i++)
			{
				CreateTable(metaDatas[i]);
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
				var propertyAttr = (PropertyAttribute) propertyInfo.GetCustomAttributes(typeof (PropertyAttribute), true)[0];
				var columnType = GetType(propertyInfo);
				var columnName = mappingInfo.MappingField;
				createDefinitionBuilder.Append(columnName + " " + columnType);
				if (propertyAttr.NotNull)
				{
					createDefinitionBuilder.Append(" NOT NULL");
				}
				if (propertyAttr.DefaultValue != null)
				{
					createDefinitionBuilder.Append(" DEFAULT " + propertyAttr.DefaultValue);
				}
				if (propertyAttr.AutoIncrement)
				{
					createDefinitionBuilder.Append(" AUTO_INCREMENT");
				}
				if (propertyAttr.PrimaryKey)
				{
					createDefinitionBuilder.Append(" PRIMARY KEY");
				}
				else if (propertyAttr.Unique)
				{
					createDefinitionBuilder.Append(" UNIQUE");
				}
				createDefinitionBuilder.Append(", ");
				if (propertyAttr.Indexing != PropertyAttribute.NO_INDEX)
				{
					createDefinitionBuilder.Append("INDEX " + columnName + ", ");
				}
			}
			var createDefinitionBuilderStr = createDefinitionBuilder.ToString(0, createDefinitionBuilder.Length - 1);
			queryBuilder.Append(createDefinitionBuilderStr + ")");
			using (var connection = connectionManager.GetUpdateConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = queryBuilder.ToString();
				return command.ExecuteNonQuery();
			}
		}

		public virtual void CreateRelation(IList<ClassMetaData> metaDatas)
		{
			for (var i = 0; i < metaDatas.Count; i++)
			{
				var relationProperties = metaDatas[i].RelationProperties;
				foreach (var pair in relationProperties)
				{
					CreateTable(pair.Value);	
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
				queryBuilder.Append("ADD COLUMN " + relationInfo.OtherPartner + " BIGINT(20) NOT NULL REFERENCES " + originalTable + " Id ON DELETE = CASCADE, ");
				queryBuilder.Append("ADD INDEX " + relationInfo.OtherPartner);
			}
			else  if (relationInfo.RelationKind == RelationInfo.RELATION_N_N)
			{
				var originalTable = relationInfo.OriginalMetadata.MappingTable;
				var partnerTable = relationInfo.PartnerMetadata.MappingTable;
				queryBuilder.Append("CREATE TABLE IF NOT EXISTS " + tableName + "(");
				queryBuilder.Append("Id BIGINT(20) NOT NULL AUTO_INCREMENT PRIMARY KEY,");
				queryBuilder.Append(relationInfo.OriginalType + " BIGINT(20) NOT NULL REFERENCES " + originalTable + " Id ON DELETE = CASCADE, ");
				queryBuilder.Append(relationInfo.OtherPartner + " BIGINT(20) NOT NULL REFERENCES " + partnerTable + " Id ON DELETE = CASCADE, ");
				queryBuilder.Append("INDEX " + relationInfo.OriginalType + ", ");
				queryBuilder.Append("INDEX " + relationInfo.OtherPartner);
				queryBuilder.Append(")");
			}
			using (var connection = connectionManager.GetUpdateConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = queryBuilder.ToString();
				return command.ExecuteNonQuery();
			}
		}

		public virtual void CreateGenericType(IList<ClassMetaData> metaDatas)
		{
			for (var i = 0; i < metaDatas.Count; i++)
			{
				var listInfos = metaDatas[i].ListProperties;
				foreach (var pair in listInfos)
				{
					CreateTable(pair.Value);
				}
				var dictInfos = metaDatas[i].DictProperties;
				foreach (var info in dictInfos)
				{
					CreateTable(info.Value);
				}
			}
		}

		public virtual int CreateTable(GenericDictInfo dictInfo)
		{
			var keyType = dictInfo.KeyType;
			var valueType = dictInfo.ValueType;
			var sqlKeyType = GetType(keyType);
			var sqlValueType = GetType(valueType);
			var tableName = "Dictionary_" + sqlKeyType + "_" + sqlValueType;
			var queryBuilder = new StringBuilder();
			queryBuilder.Append("CREATE TABLE IF NOT EXISTS " + tableName + "(");
			queryBuilder.Append("Id BIGINT(20) NOT NULL AUTO_INCREMENT PRIMARY KEY, ");
			queryBuilder.Append("ContainerType VARCHAR(512) NOT NULL, ");
			queryBuilder.Append("ContainerId BIGINT(20) NOT NULL, ");
			queryBuilder.Append("Key " + sqlKeyType + " NOT NULL, ");
			queryBuilder.Append("Value " + sqlValueType + " NULL, ");
			queryBuilder.Append("INDEX ContainerType, ");
			queryBuilder.Append("INDEX ContainerId");
			queryBuilder.Append(")");
			using (var connection = connectionManager.GetUpdateConnection())
			{
				var command = connection.CreateCommand();
				command.CommandText = queryBuilder.ToString();
				return command.ExecuteNonQuery();
			}
		}

		public virtual int CreateTable(GenericListInfo listInfo)
		{
			var elementType = listInfo.ElementType;
			var sqlType = GetType(elementType);
			var tableName = "List_" + sqlType;
			var queryBuilder = new StringBuilder();
			queryBuilder.Append("CREATE TABLE IF NOT EXISTS " + tableName + "(");
			queryBuilder.Append("Id BIGINT(20) NOT NULL AUTO_INCREMENT PRIMARY KEY, ");
			queryBuilder.Append("ContainerType VARCHAR(512) NOT NULL, ");
			queryBuilder.Append("ContainerId BIGINT(20) NOT NULL, ");
			queryBuilder.Append("Value " + sqlType + " NULL, ");
			queryBuilder.Append("INDEX ContainerType, ");
			queryBuilder.Append("INDEX ContainerId");
			if (elementType != typeof(string))
			{
				queryBuilder.Append(", INDEX Value");
			}
			queryBuilder.Append(")");
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
				object[] attribute = propertyInfo.GetCustomAttributes(typeof(StringAttribute), false);
				if (attribute.Length > 0)
				{
					var @string = (StringAttribute)attribute[0];
					strtype = ConverString(@string);
				}
				else
				{
					strtype = "varchar(255)";
				}
				return strtype;
			}
			return null;
		}

		public static string ConvertPrimaryType(Type type)
		{
			if (type.IsPrimitive)
			{
				if (type == typeof(Char))
				{
					return "varchar(1)";
				}
				else if (type == typeof(Int32))
				{
					return "integer";
				}
				else if (type == typeof(Int16))
				{
					return "tinyInt";
				}
				else if (type == typeof(Int64))
				{
					return "bigInt";
				}
				else if (type == typeof(UInt32))
				{
					return "integer unsigned default '0'";
				}
				else if (type == typeof(UInt16))
				{
					return "tinyInt unsigned default '0'";
				}
				else if (type == typeof(UInt64))
				{
					return "bigInt unsigned default '0'";
				}
				else if (type == typeof(Byte[]))
				{
					return "blob";
				}
				else if (type == typeof(Byte))
				{
					return "tinyInt(127)";
				}
				else if (type == typeof(SByte))
				{
					return "tinyInt(127) unsigned default '0'";
				}
				else if (type == typeof(Boolean))
				{
					return "tinyInt(1)";
				}
				else if (type == typeof(Single))
				{
					return "float";
				}
				return type.Name;
			}
			else if (type == typeof(DateTime))
			{
				return "dateTime";
			}
			else if (type == typeof(Decimal))
			{
				return "decimal";
			}
			return null;
		}

		public static string ConverString(StringAttribute attribute)
		{
			switch (attribute.Type)
			{
				case StringAttribute.UNLIMITED_STRING:
					return "text";
				case StringAttribute.LIMIT_STRING:
					int n = attribute.NumberOfChar;
					return String.Format("varchar({0})", n);
				default:
					return "varchar (255)";
			}
		}
	}
}
