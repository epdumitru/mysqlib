using System;
using System.Collections.Generic;
using System.Reflection;
using ObjectMapping.Attributes;
using ObjectMapping.Database;

namespace ObjectMapping
{
	public class GenericDictInfo
	{
		private Type keyType;
		private Type valueType;
		private PropertyInfo propertyInfo;

		public PropertyInfo PropertyInfo
		{
			get { return propertyInfo; }
			set { propertyInfo = value; }
		}

		public Type KeyType
		{
			get { return keyType; }
			set { keyType = value; }
		}

		public Type ValueType
		{
			get { return valueType; }
			set { valueType = value; }
		}
	}
	
    public class GenericListInfo
	{
		private Type elementType;
		private PropertyInfo propertyInfo;

		public PropertyInfo PropertyInfo
		{
			get { return propertyInfo; }
			set { propertyInfo = value; }
		}

		public Type ElementType
		{
			get { return elementType; }
			set { elementType = value; }
		}
	}
	
    public class RelationInfo
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
				if (relationAttr is OneToOneRelationAttribute)
				{
					relationKind = RELATION_1_1;
					partnerKey = ((OneToOneRelationAttribute) relationAttr).PartnerKey;
					partnerMetadata = ClassMetaDataManager.Instace.GetClassMetaData(propertyInfo.PropertyType);
					mappingTable = partnerMetadata.MappingTable;
					
				}
				else if (relationAttr is OneToManyRelationAttribute)
				{
					relationKind = RELATION_1_N;
					partnerKey = ((OneToManyRelationAttribute)relationAttr).PartnerKey;
					partnerMetadata = ClassMetaDataManager.Instace.GetClassMetaData(propertyInfo.PropertyType.GetGenericArguments()[0]);
					mappingTable = partnerMetadata.MappingTable;
				}
				else if (relationAttr is ManyToManyRelationAttribute)
				{
					relationKind = RELATION_N_N;
					var attr = (ManyToManyRelationAttribute) relationAttr;
					mappingTable = attr.RelationTable;
					originalKey = attr.OriginalColumn;
					partnerKey = attr.OtherPartner;
					partnerMetadata = ClassMetaDataManager.Instace.GetClassMetaData(propertyInfo.PropertyType.GetGenericArguments()[0]);
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

	public class MappingInfo
	{
		private PropertyInfo propertyInfo;
		private string mappingField;
		
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

    public class ClassMetaData
    {
        private Dictionary<string, MappingInfo> properties;
		private Dictionary<string, RelationInfo> relationProperties;
    	private Dictionary<string, GenericListInfo> listProperties;
    	private Dictionary<string, GenericDictInfo> dictProperties;
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

    	public Dictionary<string, GenericListInfo> ListProperties
    	{
    		get { return listProperties; }
    	}

    	public Dictionary<string, GenericDictInfo> DictProperties
    	{
    		get { return dictProperties; }
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

		public IDbObject GetDbObject()
		{
			return null;
		}

    	public string MappingTable
    	{
    		get { return mappingTable; }
    	}

    	public IDictionary<string, RelationInfo> RelationProperties
    	{
    		get { return relationProperties; }
    	}

    	private void Init()
        {
			var persistentAttrs = type.GetCustomAttributes(typeof(PersistentAttribute), false);
			if (persistentAttrs.Length == 0)
			{
				throw new ArgumentException("Type is not Persistent");
			}
			var persistentAttr = (PersistentAttribute) persistentAttrs[0];
			mappingTable = string.IsNullOrEmpty(persistentAttr.MappingTable)
								? type.Name
								: persistentAttr.MappingTable;
			properties = new Dictionary<string, MappingInfo>();
			relationProperties = new Dictionary<string, RelationInfo>();
			listProperties = new Dictionary<string, GenericListInfo>();
			dictProperties = new Dictionary<string, GenericDictInfo>();
            var propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach(var propertyInfo in propertyInfos)
            {
            	var ignoreAttributes = propertyInfo.GetCustomAttributes(typeof (IgnorePersistentAttribute), true);
				if (ignoreAttributes.Length > 0)
				{
					continue;
				}
				var propertyAttributes = propertyInfo.GetCustomAttributes(typeof(PropertyAttributes), true);
            	string mappingField = propertyInfo.Name;
				if (propertyAttributes.Length > 0)
            	{
					var propertyAttribute = (PropertyAttribute)propertyAttributes[0];
					mappingField = string.IsNullOrEmpty(propertyAttribute.MappingColumn)
												? propertyInfo.Name
												: propertyAttribute.MappingColumn;
					if (propertyInfo.Name == "Id")
					{
						primaryKey = propertyInfo;
						mappingPrimaryKey = mappingField;
					}
            	}
            	var propertyType = propertyInfo.PropertyType;
				if (propertyType.IsPrimitive || propertyType == typeof(DateTime) || propertyType == typeof(decimal) || propertyType == typeof(string))
				{
					var mappingInfo = new MappingInfo() { PropertyInfo = propertyInfo };
					mappingInfo.MappingField = mappingField;
					properties.Add(propertyInfo.Name, mappingInfo);	
				}
				else if (propertyType.IsGenericType)
				{
					var typeDefinition = propertyType.GetGenericTypeDefinition();
					if (typeDefinition == typeof(IList<>) || typeDefinition == typeof(List<>))
					{
						var elementType = propertyType.GetGenericArguments()[0];
						if (elementType.IsPrimitive || elementType == typeof(DateTime) || elementType == typeof(string) || elementType == typeof(decimal))
						{
							var listInfo = new GenericListInfo() {ElementType = elementType, PropertyInfo = propertyInfo};
							listProperties.Add(propertyInfo.Name, listInfo);
						}
						else if (elementType.IsClass)
						{
							var relationInfo = new RelationInfo() { PropertyInfo = propertyInfo, OriginalMetadata = this};
							relationProperties.Add(propertyInfo.Name, relationInfo);
						}
					}
					else if (typeDefinition == typeof(IDictionary<,>) || typeDefinition == typeof(Dictionary<,>))
					{
						var keyType = propertyType.GetGenericArguments()[0];
						var valueType = propertyType.GetGenericArguments()[1];
						dictProperties.Add(propertyInfo.Name, new GenericDictInfo() {PropertyInfo = propertyInfo, KeyType = keyType, ValueType = valueType});
					}
				}
				else if (propertyType.IsClass)
				{
					var relationInfo = new RelationInfo() {PropertyInfo = propertyInfo, OriginalMetadata = this};
					relationProperties.Add(propertyInfo.Name, relationInfo);
				}
            }
        }
    }
}
