using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Threading;
using BLToolkit.Reflection.Emit;
using ObjectMapping.Database;

namespace ObjectMapping
{
	public class DbFunctionHelperManager : IDbFunctionHelper 
	{
		private IDictionary<Type, IDbFunctionHelper> dbFunctionHelperMap;
		private ReaderWriterLockSlim dbMapLock;
		private AssemblyBuilderHelper assemblyBuilderHelper;

		public DbFunctionHelperManager()
		{
			dbFunctionHelperMap = new Dictionary<Type, IDbFunctionHelper>();
			dbMapLock = new ReaderWriterLockSlim();
			assemblyBuilderHelper = new AssemblyBuilderHelper("DbFunctionBuilderHelper.dll");
		}

		#region Implementation of IDbFunctionHelper

		private DbObjectContainer dbObjectContainer;
		public DbObjectContainer DbObjectContainer
		{
			get { return dbObjectContainer; }
			set { dbObjectContainer = value; }
		}

		public int Update(object o, DbConnection connection)
		{
			var type = o.GetType();
			var dbFunctionHelper = GetDbFunctionHelper(type);
			return dbFunctionHelper.Update(o, connection);
		}

		public int Insert(object o, DbConnection connection)
		{
			var type = o.GetType();
			var dbFunctionHelper = GetDbFunctionHelper(type);
			return dbFunctionHelper.Insert(o, connection);
		}

		public object ReadObject(Type type, DbDataReader reader, string[] propertyNames)
		{
			var dbFunctionHelper = GetDbFunctionHelper(type);
			return dbFunctionHelper.ReadObject(type, reader, propertyNames);
		}

		#endregion

		private IDbFunctionHelper GetDbFunctionHelper(Type type)
		{
			IDbFunctionHelper dbFunctionHelper;
			dbFunctionHelperMap.TryGetValue(type, out dbFunctionHelper);
			if (dbFunctionHelper == null)
			{
				lock (type.Name)
				{
					dbFunctionHelper = CreateDbFunctionHelper(type);
					dbFunctionHelperMap.Add(type, dbFunctionHelper);
				}
			}
			return dbFunctionHelper;
		}

		private IDbFunctionHelper CreateDbFunctionHelper(Type type)
		{
			var typeBuilderHelper = assemblyBuilderHelper.DefineType(type.Name + "Helper", typeof (object),
			                                                         typeof (IDbFunctionHelper));
			typeBuilderHelper.DefineField("dbObjectContainer", typeof (DbObjectContainer), FieldAttributes.Private);
			var objectContainerProperty = typeBuilderHelper.TypeBuilder.DefineProperty("")
			var desType = typeBuilderHelper.Create();
			var result = (IDbFunctionHelper) Activator.CreateInstance(desType);
			result.DbObjectContainer = dbObjectContainer;
			return result;
		}
	}
}
