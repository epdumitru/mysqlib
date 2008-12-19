using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectMapping.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class PropertyAttribute : Attribute
	{
		#region Index constant
		public const int NO_INDEX = -1;
		public const int DEFAULT_INDEX = 0;
		public const int HASH_INDEX = 1;
		public const int BTREE_INDEX = 2;
		#endregion

		private bool notNull;
		private bool autoIncrement;
		private int indexing;
		private bool unique;
		private object defaultValue;
		private string mappingColumn;

		public PropertyAttribute()
		{
			notNull = false;
			autoIncrement = false;
			indexing = NO_INDEX;
			defaultValue = null;
			mappingColumn = null;
			unique = false;
		}

		public bool Unique
		{
			get { return unique; }
			set { unique = value; }
		}

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

		public object DefaultValue
		{
			get { return defaultValue; }
			set { defaultValue = value; }
		}

		public string MappingColumn
		{
			get { return mappingColumn; }
			set { mappingColumn = value; }
		}
	}
}
