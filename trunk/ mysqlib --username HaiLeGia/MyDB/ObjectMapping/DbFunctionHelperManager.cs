using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;
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

		public int Update(IDirtyObject o, DbConnection connection)
		{
			var type = o.GetType();
			var dbFunctionHelper = GetDbFunctionHelper(type);
			return dbFunctionHelper.Update(o, connection);
		}

		public int Insert(IDirtyObject o, DbConnection connection)
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
			CreateProperty(typeBuilderHelper);
			CreateUpdateFunction(type, typeBuilderHelper);
			CreateInsertFunction(type, typeBuilderHelper);
			CreateReadObjectFunction(type, typeBuilderHelper);
			var desType = typeBuilderHelper.Create();
			var result = (IDbFunctionHelper) Activator.CreateInstance(desType);
			result.DbObjectContainer = dbObjectContainer;
			return result;
		}

		private void CreateReadObjectFunction(Type type, TypeBuilderHelper helper)
		{
			
		}

		private void CreateInsertFunction(Type type, TypeBuilderHelper helper)
		{
			throw new NotImplementedException();
		}

		private void CreateUpdateFunction(Type type, TypeBuilderHelper typeBuilderHelper)
		{
			var classMetadata = ClassMetaDataManager.Instace.GetClassMetaData(type);
			var properties = classMetadata.Properties;
			var mappingTable = classMetadata.MappingTable;
			var methodEmit = typeBuilderHelper.DefineMethod(typeof (IDbFunctionHelper).GetMethod("Update")).Emitter;
			var resultLocal = methodEmit.DeclareLocal(typeof (int));
			var commandLocal = methodEmit.DeclareLocal(typeof (DbCommand));
			var commandParameters = methodEmit.DeclareLocal(typeof (DbParameterCollection));
			var queryBuilder = new StringBuilder("UPDATE " + mappingTable + " SET ");
			foreach (var mappingInfo in properties.Values)
			{
				if (mappingInfo.MappingField != "Id")
				{
					queryBuilder.Append(mappingInfo.MappingField + "=@" + mappingInfo.MappingField + ", ");	
				}
			}
			var tmpString = queryBuilder.ToString(0, queryBuilder.Length - 2);
			queryBuilder.Length = 0;
			queryBuilder.Append(tmpString);
			queryBuilder.Append(" WHERE Id = @Id");
			methodEmit
				.ldc_i4_0
				.stloc(resultLocal)
				.ldarg_2
				.call(typeof (DbConnection).GetMethod("CreateCommand"))
				.stloc(commandLocal)
				.ldloc(commandLocal)
				.ldstr(queryBuilder.ToString())
				.call(typeof (DbCommand).GetMethod("set_CommandText"))
				.ldloc(commandLocal)
				.call(typeof(DbCommand).GetMethod("get_Parameters"))
				.stloc(commandParameters)
				;
			foreach (var mappingInfo in properties.Values)
			{
			
					
			}

		}

		private void CreateProperty(TypeBuilderHelper typeBuilderHelper)
		{
			var dbObjectContainerField = typeBuilderHelper.DefineField("dbObjectContainer", typeof (DbObjectContainer), FieldAttributes.Private);
			var objectContainerProperty = typeBuilderHelper.TypeBuilder.DefineProperty("DbObjectContainer",
			                                                                           PropertyAttributes.HasDefault,
			                                                                           typeof (DbObjectContainer),
			                                                                           new[] { typeof(DbObjectContainer) });
			var attributes = MethodAttributes.Public | MethodAttributes.SpecialName |
			                 MethodAttributes.HideBySig;
			var objectContainerGetMethod = typeBuilderHelper.DefineMethod("get_DbObjectContainer",
			                                                              attributes, typeof(DbObjectContainer), Type.EmptyTypes);
			var objectContainerGetMethodEmit = objectContainerGetMethod.Emitter;
			objectContainerGetMethodEmit
				.ldarg_0
				.ldfld(dbObjectContainerField)
				.ret();

			var objectContainerSetMethod = typeBuilderHelper.DefineMethod("set_DbObjectContainer",
			                                                              attributes, null, typeof(DbObjectContainer));
			var objectContainerSetMethodEmit = objectContainerSetMethod.Emitter;
			objectContainerSetMethodEmit
				.ldarg_0
				.ldarg_1
				.stfld(dbObjectContainerField)
				.ret();
			objectContainerProperty.SetGetMethod(objectContainerGetMethod);
			objectContainerProperty.SetSetMethod(objectContainerSetMethod);
		}
	}
}
