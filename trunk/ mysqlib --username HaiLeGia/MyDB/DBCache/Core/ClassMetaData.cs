using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using DBCache.UserComponents;

namespace DBCache.Core
{
	class ClassMetaData
	{
		private Reference id;
		private string className;
		private string assemblyName;
		private IDictionary<string, FieldInfo> allFields;
	    private IList<string> primitiveField;
	    private IList<string> objectField;
	    private IList<string> arrayField;
		private byte[] hashValue;
		private Type type;

		public ClassMetaData(object o)
		{
			if (o == null)
			{
				throw new ArgumentNullException("Parameter o cannot be null");
			}
			Initialize(o.GetType());
        }

		public ClassMetaData(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("Parameter type cannot be null");
			}
			Initialize(type);
		}

		private void Initialize(Type type)
		{
			this.type = type;
			className = type.FullName;
			assemblyName = type.AssemblyQualifiedName;
			allFields = new Dictionary<string, FieldInfo>();
			var typeDescription = new StringBuilder(assemblyName);
			typeDescription.Append(className);
			var typeFields =
				type.GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic |
				               BindingFlags.NonPublic);
			for (var i = 0; i < typeFields.Length; i++)
			{
				var field = typeFields[i];
				var fieldType = field.FieldType;
				allFields.Add(field.Name, field);
				typeDescription.Append(fieldType.AssemblyQualifiedName);
				typeDescription.Append(field.Name);
			}
			var typeHashBytes = Encoding.UTF8.GetBytes(typeDescription.ToString());
			var md5 = MD5.Create();
			hashValue = md5.ComputeHash(typeHashBytes);
			md5.Clear();
		}

		public object this[string fieldName, object o]
		{
			get
			{
				if (o == null)
				{
					throw new ArgumentNullException("Parameter o cannot be null.");
				}
				if (!type.IsAssignableFrom(o.GetType()))
				{
					throw new ArgumentException("Type is mismatch.");
				}
				var info = this[fieldName];
				if (info == null)
				{
					throw new ArgumentException("Cannot find the specified field.");
				}
				return info.GetValue(o);
			}
		}

		public FieldInfo this[string fieldName]
		{
			get
			{
				FieldInfo info;
				allFields.TryGetValue(fieldName, out info);
				return info;
			}
		}

		public Reference Id
		{
			get { return id; }
			set { id = value; }
		}

		public string ClassName
		{
			get { return className; }
		}

		public string AssemblyName
		{
			get { return assemblyName; }
		}

		public IList<FieldInfo> AllFields
		{
			get
			{
				var result = new List<FieldInfo>();
				result.AddRange(allFields.Values);
				return result;
			}
		}

		public byte[] HashValue
		{
			get { return hashValue; }
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			var other = obj as ClassMetaData;
			if (other != null)
			{
				for (var i = 0; i < hashValue.Length; i++)
				{
					if (hashValue[i] != other.hashValue[i])
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			var hash = 0;
			for (var i = 0; i < hashValue.Length; i++)
			{
				hash += 11 * hash + 8 * hashValue[i];
			}
			return hash;
		}
	}
}