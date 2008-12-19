#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BLToolkit.Reflection.Emit;

namespace ObjectSerializer
{
	public class SerializableTypeFactory
	{
		private static PrimitiveSerializeEmitter primitiveSerializeEmitter = new PrimitiveSerializeEmitter();
		private static ArraySerializeEmitter arraySerializeEmitter = new ArraySerializeEmitter();
		private static IListGenericEmitter iListGenericEmitter = new IListGenericEmitter();
		private static IDictGenericEmitter dictGenericEmitter = new IDictGenericEmitter();
		private static IObjectGenericEmitter objectGenericEmitter = new IObjectGenericEmitter();

		public static Type CreateSerializableType(Type originalType)
		{
			var originalAsmName = new AssemblyName(originalType.FullName + "Proxy");
			var assemblyBuilderHelper = new AssemblyBuilderHelper(originalAsmName.Name + ".dll");
			var returnType = CreateType(originalType, assemblyBuilderHelper);
			
#if DEBUG
			assemblyBuilderHelper.Save();
#endif
			return returnType;
		}

		public static Assembly CreateSerializableAssembly(Assembly originalAssembly)
		{
			var originalAsmName = originalAssembly.GetName();
			var assemblyBuilderHelper = new AssemblyBuilderHelper(originalAsmName.Name + "Proxy" + ".dll");
			var types = originalAssembly.GetTypes();
			foreach (var type in types)
			{
				CreateType(type, assemblyBuilderHelper);
			}
			
#if DEBUG
			assemblyBuilderHelper.Save();
#endif
			return assemblyBuilderHelper;

		}

		public static Type CreateType(Type type, AssemblyBuilderHelper assemblyBuilderHelper)
		{
			var typeName = type.Name;
			var typeNamespace = type.Namespace + ".Serializable";
			var typeBuilderHelper = assemblyBuilderHelper.DefineType(typeNamespace + "." + typeName, type,
			                                                         typeof (ISerializable));
			typeBuilderHelper.SetCustomAttribute(typeof(SerializableAttribute));
			var constructorEmit = typeBuilderHelper.DefaultConstructor.Emitter;
			var defaultConstructor = type.GetConstructor(Type.EmptyTypes);
			constructorEmit
				.ldarg_0
				.call(defaultConstructor)
				.ret();

			constructorEmit = typeBuilderHelper.DefinePublicConstructor(type, typeof(IDictionary<object, ISerializable>)).Emitter;
			constructorEmit
				.ldarg_0
				.call(defaultConstructor)
				.ldarg_2
				.ldarg_1
				.ldarg_0
				.call(typeof (IDictionary<object, ISerializable>).GetMethod("Add", new[] {typeof (object), typeof (ISerializable)}));
			//Serialize
			var propperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var serializerEmit = typeBuilderHelper.DefineMethod(typeof(ISerializable).GetMethod("Serialize", new Type[] { typeof(BinaryWriter), typeof(IDictionary<object, int>), typeof(int) })).Emitter;
			var deserializeEmit =
				typeBuilderHelper.DefineMethod(typeof(ISerializable).GetMethod("Deserialize", new Type[] { typeof(BinaryReader), typeof(IDictionary<int, object>) })).Emitter;
			
			//Write type name so we can construct it again
			serializerEmit
				.ldarg_1
				.ldstr(type.FullName + ", " + type.Assembly)
				.call(typeof (BinaryWriter).GetMethod("Write", new[] {typeof (string)}))
				//Add current object to the object graph
				.ldarg_2
				.ldarg_0
				.ldarg_3
				.call(typeof (IDictionary<object, int>).GetMethod("Add", new[] {typeof (object), typeof (int)}))
				.ldarg_3
				.ldc_i4_1
				.add
				.starg(3);
			//Serialize/Deserialize properties
			foreach (var property in propperties)
			{
				var propertyType = property.PropertyType;
				if (property.GetCustomAttributes(typeof(NonSerializeAttribute), false).Length > 0 || !property.CanWrite || !property.CanRead)
				{
					continue;
				}
				if (propertyType.IsPrimitive || propertyType == typeof(string) || propertyType == typeof(DateTime) || propertyType == typeof(decimal))
				{
					constructorEmit
						.ldarg_0
						.ldarg_1
						.call(property.GetGetMethod())
						.call(property.GetSetMethod());
					primitiveSerializeEmitter.Emit(property, ref serializerEmit, ref deserializeEmit);
				}
				else if (propertyType.IsArray)
				{
					var elementType = propertyType.GetElementType();
					if (elementType.IsPrimitive || elementType == typeof(string) || elementType == typeof(DateTime) || elementType == typeof(decimal) || elementType.IsSubclassOf(typeof(ISerializable)))
					{
						constructorEmit
							.ldarg_0
							.ldarg_1
							.call(property.GetGetMethod())
							.call(property.GetSetMethod());
					}
					else if (elementType.IsClass)
					{
						var originalArrayLocal = constructorEmit.DeclareLocal(propertyType);
						var originalArrayLocalNull = constructorEmit.DefineLabel();
						var originalArrayLocalNotNull = constructorEmit.DefineLabel();
						var arrayLocal = constructorEmit.DeclareLocal(propertyType);
						var ilocal = constructorEmit.DeclareLocal(typeof(int));
						var lenLocal = constructorEmit.DeclareLocal(typeof(int));
						var beginForLabel = constructorEmit.DefineLabel();
						var beginForBodyLabel = constructorEmit.DefineLabel();
						constructorEmit
							.ldarg_1
							.call(property.GetGetMethod())
							.stloc(originalArrayLocal)
							.ldloc(originalArrayLocal)
							.brtrue(originalArrayLocalNotNull)
							.ldarg_0
							.ldnull
							.call(property.GetSetMethod())
							.br(originalArrayLocalNull)
							.MarkLabel(originalArrayLocalNotNull)
							.ldloc(originalArrayLocal)
							.ldlen
							.conv_i4
							.stloc(lenLocal)
							.ldloc(lenLocal)
							.newarr(elementType)
							.stloc(arrayLocal)
							.ldarg_0
							.ldloc(arrayLocal)
							.call(property.GetSetMethod())
							.ldc_i4_0
							.stloc(ilocal)
							.br(beginForLabel)
							.MarkLabel(beginForBodyLabel)
							.ldloc(arrayLocal)
							.ldloc(ilocal)
							.ldloc(originalArrayLocal)
							.ldloc(ilocal)
							.ldelem_ref
							.ldarg_2
							.call(typeof(Converter).GetMethod("Convert", new Type[] { typeof(object), typeof(IDictionary<object, ISerializable>) }))
							.castclass(elementType)
							.stelem_ref
							.ldloc(ilocal)
							.ldc_i4_1
							.add
							.stloc(ilocal)
							.MarkLabel(beginForLabel)
							.ldloc(ilocal)
							.ldloc(lenLocal)
							.blt(beginForBodyLabel)
							.MarkLabel(originalArrayLocalNull);
					}
					else
					{
						throw new ArgumentException("Element of list not supported: " + elementType);
					}
					arraySerializeEmitter.Emit(property, ref serializerEmit, ref deserializeEmit);
				}
				else if (propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(IDictionary<,>) || propertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
				{
					constructorEmit
						.ldarg_0
						.ldarg_1
						.call(property.GetGetMethod())
						.call(property.GetSetMethod());
					dictGenericEmitter.Emit(property, ref serializerEmit, ref deserializeEmit);
				}
				else if (propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(IList<>) || propertyType.GetGenericTypeDefinition() == typeof(List<>)))
				{
					var elementType = propertyType.GetGenericArguments()[0];
					if (elementType.IsPrimitive || elementType == typeof(string) || elementType == typeof(DateTime) || elementType == typeof(decimal) || elementType.IsSubclassOf(typeof(ISerializable)))
					{
						constructorEmit
							.ldarg_0
							.ldarg_1
							.call(property.GetGetMethod())
							.call(property.GetSetMethod());
					}
					else if (elementType.IsClass)
					{
						var originalListLocal = constructorEmit.DeclareLocal(propertyType);
						var originalListLocalNull = constructorEmit.DefineLabel();
						var originalListLocalNotNull = constructorEmit.DefineLabel();
						var listLocal = constructorEmit.DeclareLocal(propertyType);
						var ilocal = constructorEmit.DeclareLocal(typeof (int));
						var lenLocal = constructorEmit.DeclareLocal(typeof (int));
						var beginForLabel = constructorEmit.DefineLabel();
						var beginForBodyLabel = constructorEmit.DefineLabel();
						var getCountMethod = typeof (ICollection<>).MakeGenericType(elementType).GetMethod("get_Count");
						var addElementMethod = typeof (ICollection<>).MakeGenericType(elementType).GetMethod("Add");
						var propertyInstanceType = propertyType.IsInterface ? typeof (List<>).MakeGenericType(elementType) : propertyType;

						constructorEmit
							.ldarg_1
							.call(property.GetGetMethod())
							.stloc(originalListLocal)
							.ldloc(originalListLocal)
							.brtrue(originalListLocalNotNull)
							.ldarg_0
							.ldnull
							.call(property.GetSetMethod())
							.br(originalListLocalNull)
							.MarkLabel(originalListLocalNotNull)
							.ldloc(originalListLocal)
							.call(getCountMethod)
							.stloc(lenLocal)
							.ldloc(lenLocal)
							.newobj(propertyInstanceType, typeof(int))
							.stloc(listLocal)
							.ldarg_0
							.ldloc(listLocal)
							.call(property.GetSetMethod())
							.ldc_i4_0
							.stloc(ilocal)
							.br(beginForLabel)
							.MarkLabel(beginForBodyLabel)
							.ldloc(listLocal)
							.ldloc(originalListLocal)
							.ldloc(ilocal)
							.call(propertyType.GetMethod("get_Item"))
							.ldarg_2
							.call(typeof(Converter).GetMethod("Convert", new Type[] {typeof(object), typeof(IDictionary<object, ISerializable>)}))
							.castclass(elementType)
							.call(addElementMethod)
							.ldloc(ilocal)
							.ldc_i4_1
							.add
							.stloc(ilocal)
							.MarkLabel(beginForLabel)
							.ldloc(ilocal)
							.ldloc(lenLocal)
							.blt(beginForBodyLabel)
							.MarkLabel(originalListLocalNull);
					}
					else
					{
						throw new ArgumentException("Element of list not supported: " + elementType);
					}
					iListGenericEmitter.Emit(property, ref serializerEmit, ref deserializeEmit);
				}
				else if (propertyType.IsClass)
				{
					constructorEmit
						.ldarg_0
						.ldarg_1
						.call(property.GetGetMethod())
						.ldarg_2
						.call(typeof (Converter).GetMethod("Convert", new[] {typeof (object), typeof(IDictionary<object, ISerializable>)}))
						.castclass(propertyType)
						.call(property.GetSetMethod());
					objectGenericEmitter.Emit(property, ref serializerEmit, ref deserializeEmit);
				}
			}
			serializerEmit.ret();
			deserializeEmit.ret();
			constructorEmit.ret();
			var serializableType = typeBuilderHelper.Create();
			SerializableTypeManager.Instance.Add(type, serializableType);
			return serializableType;
		}

		public static void UpdateType(string typeName, TypeBuilderHelper typeBuilderHelper, FieldInfo[] fieldInfos)
		{
			//Serialize
			var serializerEmit = typeBuilderHelper.DefineMethod(typeof(ISerializable).GetMethod("Serialize", new Type[] { typeof(BinaryWriter), typeof(IDictionary<object, int>), typeof(int) })).Emitter;
			var deserializeEmit =
				typeBuilderHelper.DefineMethod(typeof(ISerializable).GetMethod("Deserialize", new Type[] { typeof(BinaryReader), typeof(IDictionary<int, object>) })).Emitter;

			//Write type name so we can construct it again
			serializerEmit
				.ldarg_1
				.ldstr(typeName)
				.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(string) }))
				//Add current object to the object graph
				.ldarg_2
				.ldarg_0
				.ldarg_3
				.call(typeof(IDictionary<object, int>).GetMethod("Add", new[] { typeof(object), typeof(int) }))
				.ldarg_3
				.ldc_i4_1
				.add
				.starg(3);
			//Serialize/Deserialize properties
			foreach (var fieldInfo in fieldInfos)
			{
				var fieldType = fieldInfo.FieldType;
				if (fieldType.IsPrimitive || fieldType == typeof(string) || fieldType == typeof(DateTime) || fieldType == typeof(decimal))
				{
					primitiveSerializeEmitter.Emit(fieldInfo, ref serializerEmit, ref deserializeEmit);
				}
				else if (fieldType.IsArray)
				{
					arraySerializeEmitter.Emit(fieldInfo, ref serializerEmit, ref deserializeEmit);
				}
				else if (fieldType.IsGenericType && (fieldType.GetGenericTypeDefinition() == typeof(IDictionary<,>) || fieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
				{
					dictGenericEmitter.Emit(fieldInfo, ref serializerEmit, ref deserializeEmit);
				}
				else if (fieldType.IsGenericType && (fieldType.GetGenericTypeDefinition() == typeof(IList<>) || fieldType.GetGenericTypeDefinition() == typeof(List<>)))
				{
					iListGenericEmitter.Emit(fieldInfo, ref serializerEmit, ref deserializeEmit);
				}
				else if (fieldType.IsClass)
				{
					objectGenericEmitter.Emit(fieldInfo, ref serializerEmit, ref deserializeEmit);
				}
			}
			serializerEmit.ret();
			deserializeEmit.ret();
		}
	}

	public interface ISerializable
	{
		void Serialize(BinaryWriter writer, IDictionary<object, int> objectGraph, int index);
		void Deserialize(BinaryReader reader, IDictionary<int, object> objectGraph);
	}
}