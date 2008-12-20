using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using ObjectMapping.Database;

namespace ObjectMapping
{
	public class DemoObject : IDbObject
	{
		public void Serialize(BinaryWriter writer, IDictionary<object, int> objectGraph, int index)
		{
			throw new System.NotImplementedException();
		}

		public void Deserialize(BinaryReader reader, IDictionary<int, object> objectGraph)
		{
			throw new System.NotImplementedException();
		}

		public object SyncRootObject
		{
			get { throw new System.NotImplementedException(); }
			set { throw new System.NotImplementedException(); }
		}

		public long Id
		{
			get { throw new System.NotImplementedException(); }
			set { throw new System.NotImplementedException(); }
		}

		public ClassMetaData Metadata
		{
			get { throw new System.NotImplementedException(); }
			set { throw new System.NotImplementedException(); }
		}

		public void ReadFields(DbDataReader reader, params string[] propertyNames)
		{
			throw new System.NotImplementedException();
		}

		public string Update(int updateDepth)
		{
			throw new System.NotImplementedException();
		}

		public string Insert()
		{
			throw new System.NotImplementedException();
		}
	}

	public class DbObjectFactory
	{

	}
}
