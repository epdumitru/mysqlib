using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using DBCache.Core;
using ObjectMapping;
using ObjectMapping.Attributes;
using ObjectMapping.Database;

namespace Persistents
{
//	[Persistent]
//	public class TestObject : IDbObject
//	{
//		private IList<TestObjectHolder> objectHolder;
//
//		[ManyToManyRelation(OriginalColumn = "TestObjectKey", RelationTable = "Relation_TestObjectHolder_TestObject", OtherPartner = "TestObjectHolderKey")]
//		public IList<TestObjectHolder> ObjectHolder
//		{
//			get { return objectHolder; }
//			set { objectHolder = value; }
//		}
//		public void Serialize(BinaryWriter writer, IDictionary<object, int> objectGraph, int index)
//		{
//			throw new System.NotImplementedException();
//		}
//
//		public void Deserialize(BinaryReader reader, IDictionary<int, object> objectGraph)
//		{
//			throw new System.NotImplementedException();
//		}
//
//		public long Id
//		{
//			get { throw new System.NotImplementedException(); }
//			set { throw new System.NotImplementedException(); }
//		}
//		[IgnorePersistent]
//		public ClassMetaData Metadata
//		{
//			get { throw new System.NotImplementedException(); }
//			set { throw new System.NotImplementedException(); }
//		}
//
//		public void ReadPrimitive(DbDataReader reader)
//		{
//			throw new System.NotImplementedException();
//		}
//
//		public void ReadObject(DbDataReader reader, string propetyName)
//		{
//			throw new System.NotImplementedException();
//		}
//
//		public void ReadObject(IList<object> otherPrimValues, string propertyName, QueryExecutor executor)
//		{
//			throw new System.NotImplementedException();
//		}
//
//		public void ReadList(DbDataReader reader, string propertyName)
//		{
//			throw new System.NotImplementedException();
//		}
//
//		public void ReadDict(DbDataReader reader, string propertyName)
//		{
//			throw new System.NotImplementedException();
//		}
//	}

	[Persistent]
	public class TestObjectHolder:IDbObject 
	{
		#region Primitive
//		private bool bo;
//		private byte by;
//		private sbyte sb;
//		private short sh;
//		private ushort us;
//		private char ch;
//		private int i;
//		private uint ui;
//		private float f;
//		private long lo;
//		private ulong ul;
//		private double d;
//		private decimal de;
//		private DateTime dt;
//		private string str;
//
//		[Property(Indexing = PropertyAttribute.DEFAULT_INDEX)]
//		public bool Bo
//		{
//			get { return bo; }
//			set
//			{
//				bo = value;
//			}
//		}
//
//		public byte By
//		{
//			get { return by; }
//			set { by = value; }
//		}
//
//		public sbyte Sb
//		{
//			get { return sb; }
//			set { sb = value; }
//		}
//
//		public short Sh
//		{
//			get { return sh; }
//			set { sh = value; }
//		}
//
//		public ushort Us
//		{
//			get { return us; }
//			set { us = value; }
//		}
//
//		public char Ch
//		{
//			get { return ch; }
//			set { ch = value; }
//		}
//
//		public int I
//		{
//			get { return i; }
//			set { i = value; }
//		}
//
//		public uint Ui
//		{
//			get { return ui; }
//			set { ui = value; }
//		}
//
//		public float F
//		{
//			get { return f; }
//			set { f = value; }
//		}
//
//		public long Lo
//		{
//			get { return lo; }
//			set { lo = value; }
//		}
//
//		public ulong Ul
//		{
//			get { return ul; }
//			set { ul = value; }
//		}
//
//		public double D
//		{
//			get { return d; }
//			set { d = value; }
//		}
//
//		public decimal De
//		{
//			get { return de; }
//			set { de = value; }
//		}
//
//		public DateTime Dt
//		{
//			get { return dt; }
//			set { dt = value; }
//		}
//
//		public string Str
//		{
//			get { return str; }
//			set { str = value; }
//		}
		#endregion
        private byte[] byteArr;
		#region Array
     	public byte[] ByteArr
		{
			get { return byteArr; }
			set { byteArr = value; }
		}

//		private TestObjectHolderChild[] objectHolderChildren;
//
//		public TestObjectHolderChild[] ObjectHolderChildren
//		{
//			get { return objectHolderChildren; }
//			set { objectHolderChildren = value; }
//		}
     
//		private byte[] byteArr;
//		private sbyte[] sbyteArr;
//		private bool[] boolArr;
//		private string[] strArr;
//		private DateTime[] dtArr;
//		private char[] chArr;
//		private double[] dArr;
//		private long[] longArr;
//		private ulong[] ulongArr;
//		private decimal[] decArr;
//
//		public byte[] ByteArr
//		{
//			get { return byteArr; }
//			set { byteArr = value; }
//		}
//
//		public sbyte[] SbyteArr
//		{
//			get { return sbyteArr; }
//			set { sbyteArr = value; }
//		}
//
//		public string[] StrArr
//		{
//			get { return strArr; }
//			set { strArr = value; }
//		}
//
//		public DateTime[] DtArr
//		{
//			get { return dtArr; }
//			set { dtArr = value; }
//		}
//
//		public char[] ChArr
//		{
//			get { return chArr; }
//			set { chArr = value; }
//		}
//
//		public double[] DArr
//		{
//			get { return dArr; }
//			set { dArr = value; }
//		}
//
//		public bool[] BoolArr
//		{
//			get { return boolArr; }
//			set { boolArr = value; }
//		}
//
//
//		public ulong[] UlongArr
//		{
//			get { return ulongArr; }
//			set { ulongArr = value; }
//		}
//
//		public long[] LongArr
//		{
//			get { return longArr; }
//			set { longArr = value; }
//		}
//
//		public decimal[] DecArr
//		{
//			get { return decArr; }
//			set { decArr = value; }
//		}
		#endregion

		#region list region
		
//		
//		private IList<TestObject> objectHolderListChildren;
//
//		[ManyToManyRelation(OriginalColumn = "TestObjectHolderKey", RelationTable = "Relation_TestObjectHolder_TestObject", OtherPartner = "TestObjectKey")]
//		public IList<TestObject> ObjectHolderListChildren
//		{
//			get { return objectHolderListChildren; }
//			set { objectHolderListChildren = value; }
//		}

//				private IList<string> strList;
//				private IList<byte> byteList;
//				private IList<int> intList;
//		
//				public IList<string> StrList
//				{
//					get { return strList; }
//					set { strList = value; }
//				}
//		
//				public IList<byte> ByteList
//				{
//					get { return byteList; }
//					set { byteList = value; }
//				}
//		        
//				public IList<int> IntList
//				{
//					get { return intList; }
//					set { intList = value; }
//				}
		

		#endregion

		#region Dictionary

//		private IDictionary<int, int> intintDict;
//
//		public IDictionary<int, int> IntintDict
//		{
//			get { return intintDict; }
//			set { intintDict = value; }
//		}
//
//		private IDictionary<int, string> intstringDict;
//
//		public IDictionary<int, string> IntstringDict
//		{
//			get { return intstringDict; }
//			set { intstringDict = value; }
//		}

//		private IDictionary<decimal, string> decStringList;

//		[NonSerialize]
//		public IDictionary<decimal, string> DecStringList
//		{
//			get { return decStringList; }
//			set { decStringList = value; }
//		}

		#endregion

		#region object

		
//		private TestObject testObjectHolderChild;
//
//		[OneToOneRelation(PartnerKey = "TestObject_Id")]
//		public TestObject TestObjectHolderChild
//		{
//			get { return testObjectHolderChild; }
//			set { testObjectHolderChild = value; }
//		}

		#endregion


		private TestObjectHolder objectHolder;

		[OneToOneRelation(PartnerKey = "holder_id")]
		public TestObjectHolder ObjectHolder
		{
			get { return objectHolder; }
			set { objectHolder = value; }
		}

//		private IList<TestObjectHolder> objectHolder;
		//
		//		[OneToManyRelation(PartnerKey = "testObjectHolderId")]
		//		public IList<TestObjectHolder> ObjectHolder
		//		{
		//			get { return objectHolder; }
		//			set { objectHolder = value; }
		//		}

		private long id;
		public void Serialize(BinaryWriter writer, IDictionary<object, int> objectGraph, int index)
		{
			throw new System.NotImplementedException();
		}

		public void Deserialize(BinaryReader reader, IDictionary<int, object> objectGraph)
		{
			throw new System.NotImplementedException();
		}

		public long Id
		{
			get { return id; }
			set { id = value; }
		}

		[IgnorePersistent]
		public ClassMetaData Metadata
		{
			get { throw new System.NotImplementedException(); }
			set { throw new System.NotImplementedException(); }
		}


		public void ReadPrimitive(DbDataReader reader)
		{
			throw new System.NotImplementedException();
		}

		public void ReadObject(DbDataReader reader, string propetyName)
		{
			throw new System.NotImplementedException();
		}

		public void ReadObject(IList<object> otherPrimValues, string propertyName, QueryExecutor executor)
		{
			throw new System.NotImplementedException();
		}

		public void ReadList(DbDataReader reader, string propertyName)
		{
			throw new System.NotImplementedException();
		}

		public void ReadDict(DbDataReader reader, string propertyName)
		{
			throw new System.NotImplementedException();
		}
	}

//	[Persistent]
//	public class ParentObject : IDbObject
//	{
//		private string userName;
//		private string password;
//
//		public string UserName
//		{
//			get { return userName; }
//			set { userName = value; }
//		}
//
//		public string Password
//		{
//			get { return password; }
//			set { password = value; }
//		}
//
//		private IList<ParentObject> parent;
//
//		[OneToManyRelation(PartnerKey = "Id")]
//		public IList<ParentObject> Parent
//		{
//			get { return parent; }
//			set { parent = value; }
//		}
//
//		public void Serialize(BinaryWriter writer, IDictionary<object, int> objectGraph, int index)
//		{
//			throw new System.NotImplementedException();
//		}
//
//		public void Deserialize(BinaryReader reader, IDictionary<int, object> objectGraph)
//		{
//			throw new System.NotImplementedException();
//		}
//
//		private long id;
//		public long Id
//		{
//			get { return id; }
//			set { id = value; }
//		}
//
//		[IgnorePersistent]
//		public ClassMetaData Metadata
//		{
//			get { throw new System.NotImplementedException(); }
//			set { throw new System.NotImplementedException(); }
//		}
//
//		public void ReadPrimitive(DbDataReader reader)
//		{
//			throw new System.NotImplementedException();
//		}
//
//		public void ReadObject(DbDataReader reader, string propetyName)
//		{
//			throw new System.NotImplementedException();
//		}
//
//		public void ReadObject(IList<object> otherPrimValues, string propertyName, QueryExecutor executor)
//		{
//			throw new System.NotImplementedException();
//		}
//
//		public void ReadList(DbDataReader reader, string propertyName)
//		{
//			throw new System.NotImplementedException();
//		}
//
//		public void ReadDict(DbDataReader reader, string propertyName)
//		{
//			throw new System.NotImplementedException();
//		}
//	}

/*	[Persistent]
	public class ChildObject : ParentObject
	{
		private string address;

		public string Address
		{
			get { return address; }
			set { address = value; }
		}

		private IList<ParentObject> parent;

		[OneToManyRelation(PartnerKey = "Id")]
		public IList<ParentObject> Parent
		{
			get { return parent; }
			set { parent = value; }
		}

	}*/
}
