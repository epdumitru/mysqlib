using System;
using System.Collections.Generic;
using System.Reflection;
using ObjectMapping.Attributes;
using ObjectMapping.Database;

namespace ObjectMapping
{
	internal class RelationInfo
	{
		public const int RELATION_1_1 = 0;
		public const int RELATION_1_N = 1;
		public const int RELATION_N_N = 2;

		private int relationKind;
		private ClassMetaData originalMetadata;
		private ClassMetaData partnerMetadata;
		private string originalKey;
		private string partnerKey;
		private string mappingTable;
		private PropertyInfo propertyInfo;

		public PropertyInfo PropertyInfo
		{
			get { return propertyInfo; }
			set
			{
				propertyInfo = value;
				var relationAttrs = propertyInfo.GetCustomAttributes(typeof (RelationAttribute), true);
				var relationAttr = (RelationAttribute) relationAttrs[0];
				var propertyType = propertyInfo.PropertyType;
				if (relationAttr is OneToOneRelationAttribute)
				{
					relationKind = RELATION_1_1;
					partnerMetadata = ClassMetaDataManager.Instace.GetClassMetaData(propertyType);
					originalKey = originalMetadata.Type.Name + ((OneToOneRelationAttribute) relationAttr).OriginalKey;
					mappingTable = partnerMetadata.MappingTable;
				}
				else if (relationAttr is OneToManyRelationAttribute)
				{
					relationKind = RELATION_1_N;
					originalKey = originalMetadata.Type.Name + ((OneToManyRelationAttribute)relationAttr).OriginalKey;
					partnerMetadata = ClassMetaDataManager.Instace.GetClassMetaData(propertyType.GetGenericArguments()[0]);
					mappingTable = partnerMetadata.MappingTable;
				}
				else if (relationAttr is ManyToManyRelationAttribute)
				{
					relationKind = RELATION_N_N;
					var attr = (ManyToManyRelationAttribute) relationAttr;
					mappingTable = attr.RelationTable;
					originalKey = attr.OriginalColumn;
					partnerKey = attr.OtherPartner;
					partnerMetadata = ClassMetaDataManager.Instace.GetClassMetaData(propertyType.GetGenericArguments()[0]);
				}
			}
		}

		public int RelationKind
		{
			get { return relationKind; }
		}

		public string MappingTable
		{
			get { return mappingTable; }
		}

		public string OriginalKey
		{
			get { return originalKey; }
		}

		public string PartnerKey
		{
			get { return partnerKey; }
		}

		public ClassMetaData OriginalMetadata
		{
			get { return originalMetadata; }
			set { originalMetadata = value; }
		}

		public ClassMetaData PartnerMetadata
		{
			get { return partnerMetadata; }
			set { partnerMetadata = value; }
		}
	}

	internal class MappingInfo
	{
		private PropertyInfo propertyInfo;
		private string mappingField;
		private bool notNull = false;
		private bool autoIncrement = false;
		private int indexing = PropertyAttribute.NO_INDEX ;
		private bool unique = false;
		private object defaultValue = null;

		public bool NotNull
		{
			get { return notNull; }
			set { notNull = value; }
		}

		public bool AutoIncrement
		{
			get { return autoIncrement; }
			set { autoIncrement = value; }
		}

		public int Indexing
		{
			get { return indexing; }
			set { indexing = value; }
		}

		public bool Unique
		{
			get { return unique; }
			set { unique = value; }
		}

		public object DefaultValue
		{
			get { return defaultValue; }
			set { defaultValue = value; }
		}

		public PropertyInfo PropertyInfo
		{
			get { return propertyInfo; }
			set { propertyInfo = value; }
		}

		public string MappingField
		{
			get { return mappingField; }
			set { mappingField = value; }
		}

	}

	internal class ClassMetaData
	{
		private Dictionary<string, MappingInfo> properties;
		private Dictionary<string, RelationInfo> relationProperties;
		private IList<string> allPropertiesName;
		private PropertyInfo primaryKey;
		private string mappingTable;
		private string mappingPrimaryKey;
		private Type type;

		public ClassMetaData(object o)
		{
			if (o == null)
			{
				throw new ArgumentNullException("parameter o can not be null");
			}
			type = o.GetType();
			Init();
		}

		public ClassMetaData(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("Parameter o cannot be null");
			}
			this.type = type;
			Init();
		}

		public Dictionary<string, MappingInfo> Properties
		{
			get { return properties; }
		}

		public PropertyInfo PrimaryKey
		{
			get { return primaryKey; }
		}

		public string MappingPrimaryKey
		{
			get { return mappingPrimaryKey; }
		}

		public Type Type
		{
			get { return type; }
		}

		public string MappingTable
		{
			get { return mappingTable; }
		}

		public IDictionary<string, RelationInfo> RelationProperties
		{
			get { return relationProperties; }
		}

		public IList<string> AllPropertiesName
		{
			get { return allPropertiesName; }
		}

		private void Init()
		{
			var persistentAttrs = type.GetCustomAttributes(typeof(PersistentAttribute), true);
			if (persistentAttrs.Length == 0)
			{
			    
				throw new ArgumentException("Type is not Persistent");
			}
			var persistentAttr = (PersistentAttribute)persistentAttrs[0];
			mappingTable = string.IsNullOrEmpty(persistentAttr.MappingTable)
								? type.Name
								: persistentAttr.MappingTable;
			properties = new Dictionary<string, MappingInfo>();
			relationProperties = new Dictionary<string, RelationInfo>();
			allPropertiesName = new List<string>();
		}

		public void Create()
		{

			var propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (var propertyInfo in propertyInfos)
			{
				var ignoreAttributes = propertyInfo.GetCustomAttributes(typeof(IgnorePersistentAttribute), true);
				if (ignoreAttributes.Length > 0)
				{
					continue;
				}
				allPropertiesName.Add(propertyInfo.Name);
				var propertyType = propertyInfo.PropertyType;
				if (propertyType.IsPrimitive || propertyType == typeof(DateTime) || propertyType == typeof(decimal) || propertyType == typeof(string))
				{
					var mappingInfo = new MappingInfo() { PropertyInfo = propertyInfo };
					mappingInfo.MappingField = propertyInfo.Name;
					if (propertyInfo.Name == "Id")
					{
						mappingInfo.AutoIncrement = true;
						mappingInfo.NotNull = true;
					}
					else
					{
						if (propertyType == typeof(byte) || propertyType == typeof(sbyte) || propertyType == typeof(bool)
						|| propertyType == typeof(short) || propertyType == typeof(ushort)
						|| propertyType == typeof(int) || propertyType == typeof(uint)
						|| propertyType == typeof(float) || propertyType == typeof(long)
						|| propertyType == typeof(ulong) || propertyType == typeof(double)
						|| propertyType == typeof(decimal))
						{
							mappingInfo.DefaultValue = 0;
						}
						else if (propertyType == typeof(char))
						{
							mappingInfo.DefaultValue = "\'\0\'";
						}
						else if (propertyType == typeof(DateTime))
						{
							mappingInfo.DefaultValue = "\'0001-01-01 12:00:00\'";
						}
						else if (propertyType == typeof(string))
						{
							mappingInfo.DefaultValue = "\'\'";
						}
					}
					var propertyAttributes = propertyInfo.GetCustomAttributes(typeof(PropertyAttribute), true);
					if (propertyAttributes.Length > 0)
					{
						var propertyAttribute = (PropertyAttribute)propertyAttributes[0];
						mappingInfo.MappingField = string.IsNullOrEmpty(propertyAttribute.MappingColumn)
													? propertyInfo.Name
													: propertyAttribute.MappingColumn;
						if (propertyInfo.Name == "Id")
						{
							primaryKey = propertyInfo;
							mappingPrimaryKey = mappingInfo.MappingField;
						}
						mappingInfo.NotNull = propertyAttribute.NotNull;
						mappingInfo.AutoIncrement = propertyAttribute.AutoIncrement;
						if (propertyAttribute.DefaultValue != null)
						{
							mappingInfo.DefaultValue = propertyAttribute.DefaultValue;
						}
						mappingInfo.Indexing = propertyAttribute.Indexing;
						mappingInfo.Unique = propertyAttribute.Unique;
					}
					properties.Add(propertyInfo.Name, mappingInfo);
				}
				else if (propertyType.IsGenericType)
				{
					var typeDefinition = propertyType.GetGenericTypeDefinition();
					if ((typeDefinition == typeof(IList<>) || typeDefinition == typeof(List<>)) && propertyType.GetGenericArguments()[0].IsClass)
					{
						var propertyAttributes = propertyInfo.GetCustomAttributes(typeof(PropertyAttribute), true);
						if (propertyAttributes.Length > 0)
						{
							var propertyAttribute = (PropertyAttribute) propertyAttributes[0];
							if (propertyAttribute.Attach)
							{
								var mappingInfo = new MappingInfo()
								{
									AutoIncrement = false,
									DefaultValue = null,
									Indexing = PropertyAttribute.NO_INDEX,
									NotNull = false,
									MappingField = propertyInfo.Name,
									Unique = false,
									PropertyInfo = propertyInfo
								};
								if (propertyAttribute.MappingColumn != null)
								{
									mappingInfo.MappingField = propertyAttribute.MappingColumn;
								}
								properties.Add(propertyInfo.Name, mappingInfo);
								continue;
							}
						}
						var relationInfo = new RelationInfo() { OriginalMetadata = this, PropertyInfo = propertyInfo };
						relationProperties.Add(propertyInfo.Name, relationInfo);
					}
					else if ((typeDefinition == typeof(IDictionary<,>) || typeDefinition == typeof(Dictionary<,>))
								|| (typeDefinition == typeof(IList<>) || typeDefinition == typeof(List<>)))
					{
						var mappingInfo = new MappingInfo()
							{
								AutoIncrement = false,
								DefaultValue = null,
								Indexing = PropertyAttribute.NO_INDEX,
								NotNull = false,
								MappingField = propertyInfo.Name,
								Unique = false,
								PropertyInfo = propertyInfo
							};
						properties.Add(propertyInfo.Name, mappingInfo);
						var propertyAttributes = propertyInfo.GetCustomAttributes(typeof(PropertyAttribute), true);
						if (propertyAttributes.Length > 0)
						{
							var propertyAttribute = (PropertyAttribute)propertyAttributes[0];
							if (propertyAttribute.MappingColumn != null)
							{
								mappingInfo.MappingField = propertyAttribute.MappingColumn;
							}
						}
					}
				}
                else if (propertyType.IsArray)
                {

                    var mappingInfo = new MappingInfo()
                    {
                        AutoIncrement = false,
                        DefaultValue = null,
                        Indexing = PropertyAttribute.NO_INDEX,
                        NotNull = false,
                        MappingField = propertyInfo.Name,
                        Unique = false,
                        PropertyInfo = propertyInfo
                    };
                    var propertyAttributes = propertyInfo.GetCustomAttributes(typeof(PropertyAttribute), true);
                    if (propertyAttributes.Length > 0)
                    {
                        var propertyAttribute = (PropertyAttribute)propertyAttributes[0];
                        if (propertyAttribute.MappingColumn != null)
                        {
                            mappingInfo.MappingField = propertyAttribute.MappingColumn;
                        }
                    }
                    properties.Add(propertyInfo.Name, mappingInfo);
                }
                else if (propertyType.IsClass)
                {
                    var relationInfo = new RelationInfo() { OriginalMetadata = this, PropertyInfo = propertyInfo };
                    relationProperties.Add(propertyInfo.Name, relationInfo);
                }
			}
		}
	}
}
