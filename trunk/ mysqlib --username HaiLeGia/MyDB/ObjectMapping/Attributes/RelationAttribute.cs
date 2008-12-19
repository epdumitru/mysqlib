using System;

namespace ObjectMapping.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public abstract class RelationAttribute : Attribute
    {
    	
    }

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
	public class OneToOneRelationAttribute : RelationAttribute
	{
		private string partnerKey;

		public string PartnerKey
		{
			get { return partnerKey; }
			set { partnerKey = value; }
		}
	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
	public class OneToManyRelationAttribute : RelationAttribute
	{
		private string partnerKey;

		public string PartnerKey
		{
			get { return partnerKey; }
			set { partnerKey = value; }
		}
	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
	public class ManyToManyRelationAttribute : RelationAttribute
	{
		private string relationTable;
		private string originalColumn;
		private string otherPartner;

		public string RelationTable
		{
			get { return relationTable; }
			set { relationTable = value; }
		}

		public string OriginalColumn
		{
			get { return originalColumn; }
			set { originalColumn = value; }
		}

		public string OtherPartner
		{
			get { return otherPartner; }
			set { otherPartner = value; }
		}
	}
}
