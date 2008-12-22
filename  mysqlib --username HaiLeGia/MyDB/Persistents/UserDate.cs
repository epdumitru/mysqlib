using System.Collections.Generic;
using ObjectMapping;
using ObjectMapping.Attributes;

namespace Persistents
{
	[PersistentAttribute]
	public class UserData : IDbObject
	{
		private long id;
		private string username;
		private string password;
		private UserData other;
		private string[] strArray;
		private long updateTime;

		public long Id
		{
			get { return id; }
			set { id = value; }
		}

		public long UpdateTime
		{
			get { return updateTime;  }
			set { updateTime = value; }
		}

		public string Username
		{
			get { return username; }
			set { username = value; }
		}

		public string Password
		{
			get { return password; }
			set { password = value; }
		}

		[OneToOneRelation(OriginalKey = "Id")]
		public UserData Other
		{
			get { return other; }
			set { other = value; }
		}

		public string[] StrArray
		{
			get { return strArray; }
			set { strArray = value; }
		}

		#region Implementation of IDbObject

		private bool isDirty;
		[IgnorePersistent]
		public bool IsDirty
		{
			get { return isDirty; }
			set { isDirty = value; }
		}

		#endregion
	}

	[PersistentAttribute]
	public class A : IDbObject
	{
		private long id;
		private long updateTime;
		private bool isDirty;
		private string str;
		private List<B> bt;

		public long Id
		{
			get { return id; }
			set { id = value; }
		}

		public long UpdateTime
		{
			get { return updateTime; }
			set { updateTime = value; }
		}

		public bool IsDirty
		{
			get { return isDirty; }
			set { isDirty = value; }
		}

		public string Str
		{
			get { return str; }
			set { str = value; }
		}
		[OneToManyRelation]
//		[ManyToManyRelation(OriginalColumn = "Aid", RelationTable = "AB" , OtherPartner = "Cid")]
		public List<B> Bt
		{
			get { return bt; }
			set { bt = value; }
		}
	}

	[PersistentAttribute]
	public class B : IDbObject
	{
		private long id;
		private long updateTime;
		private bool isDirty;
		private string str;
		private string name;
		

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public long Id
		{
			get { return id; }
			set { id = value; }
		}

		public long UpdateTime
		{
			get { return updateTime; }
			set { updateTime = value; }
		}

		public bool IsDirty
		{
			get { return isDirty; }
			set { isDirty = value; }
		}

		public string Str
		{
			get { return str; }
			set { str = value; }
		}

		private A at;
		[OneToOneRelation]
		public A At
		{
			get { return at; }
			set { at = value; }
		}

//		private List<A> at;
//		[ManyToManyRelation(OriginalColumn = "Aid", RelationTable = "AB", OtherPartner = "Cid")]
//		public List<A> At
//		{
//			get { return at; }
//			set { at = value; }
//		}
	}
}