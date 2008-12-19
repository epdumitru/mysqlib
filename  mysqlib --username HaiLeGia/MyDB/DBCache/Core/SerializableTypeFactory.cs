#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BLToolkit.Reflection.Emit;
using DBCache.UserComponents;

namespace DBCache.Core
{
	public interface SerializeEmitter
	{
		void Emit(PropertyInfo property, ref EmitHelper serializerEmit, ref EmitHelper deserializeEmit);
	}

	internal class PrimitiveSerializeEmitter : SerializeEmitter	
	{
		public void Emit(PropertyInfo property, ref EmitHelper serializerEmit, ref EmitHelper deserializeEmit)
		{
			var propertyType = property.PropertyType;
			var getPropertyMethod = property.GetGetMethod();
			var setPropertyMethod = property.GetSetMethod();
			var writerMethod = typeof(BinaryWriter).GetMethod("Write", new Type[] { propertyType });
			var readerMethod = typeof(BinaryReader).GetMethod("Read" + propertyType.Name);
			if (propertyType.IsPrimitive || propertyType == typeof(decimal))
			{
				serializerEmit
					.ldarg_1
					.ldarg_0
					.call(getPropertyMethod)
					.call(writerMethod);
				deserializeEmit
					.ldarg_0
					.ldarg_1
					.call(readerMethod)
					.call(setPropertyMethod);	
			} 
			else if (propertyType == typeof (DateTime))
			{
				var writeLongMethod = typeof(BinaryWriter).GetMethod("Write", new[] { typeof(long) });
				var readLongMethod = typeof(BinaryReader).GetMethod("ReadInt64");
				serializerEmit.DeclareLocal(typeof(DateTime));
				serializerEmit
					.ldarg_1
					.ldarg_0
					.call(getPropertyMethod)
					.stloc_0
					.ldloca(0)
					.call(typeof(DateTime).GetMethod("get_Ticks"))
					.call(writeLongMethod);
				deserializeEmit
					.ldarg_0
					.ldarg_1
					.call(readLongMethod)
					.newobj(typeof(DateTime), new[] { typeof(long) })
					.call(setPropertyMethod);
			}
			else if (propertyType == typeof(string))
			{
				var stringLocal = serializerEmit.DeclareLocal(propertyType);
				var stringNullLabel = serializerEmit.DefineLabel();
				var stringNotNullLabel = serializerEmit.DefineLabel();
				serializerEmit
					.ldarg_0
					.call(getPropertyMethod)
					.stloc(stringLocal)
					.ldloc(stringLocal)
					.brtrue(stringNotNullLabel)
					.ldarg_1
					.ldc_i4_m1
					.call(typeof(BinaryWriter).GetMethod("Write", new[] {typeof(int)}))
					.br(stringNullLabel)
                    .MarkLabel(stringNotNullLabel)
					.ldarg_1
					.ldloc(stringLocal)
					.call(typeof(string).GetMethod("get_Length"))
					.call(typeof(BinaryWriter).GetMethod("Write", new[] {typeof(int)}))
					.ldarg_1
					.ldloc(stringLocal)
					.call(typeof(BinaryWriter).GetMethod("Write", new[] {typeof(string)}))
					.MarkLabel(stringNullLabel);
				var valueNotNullLabel = deserializeEmit.DefineLabel();
				var valueNullLabel = deserializeEmit.DefineLabel();
				deserializeEmit
					.ldarg_1
					.call(typeof (BinaryReader).GetMethod("ReadInt32"))
					.ldc_i4_m1
					.ceq
					.brfalse(valueNotNullLabel)
					.ldarg_0
					.ldnull
					.call(setPropertyMethod)
					.br(valueNullLabel)
					.MarkLabel(valueNotNullLabel)
					.ldarg_0
					.ldarg_1
					.call(typeof (BinaryReader).GetMethod("ReadString"))
					.call(setPropertyMethod)
					.MarkLabel(valueNullLabel);
			}
		}
	}

	internal class ArraySerializeEmitter : SerializeEmitter
	{
		private IObjectGenericEmitter objectGenericEmitter = new IObjectGenericEmitter();
		public void Emit(PropertyInfo property, ref EmitHelper serializerEmit, ref EmitHelper deserializeEmit)
		{
			var writeIntMethod = typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) });
			var writeStringMethod = typeof(BinaryWriter).GetMethod("Write", new[] { typeof(string) });
			var getPropertyMethod = property.GetGetMethod();
			var arrayNotNullLabel = serializerEmit.DefineLabel();
			var arrayNullLabel = serializerEmit.DefineLabel();
			var whileLabel = serializerEmit.DefineLabel();
			var endWhileLabel = serializerEmit.DefineLabel();
			var elementType = property.PropertyType.GetElementType();
			var iLocal = serializerEmit.DeclareLocal(typeof(int));
			var lenLocal = serializerEmit.DeclareLocal(typeof(int));
			serializerEmit
				.ldarg_0
				.call(getPropertyMethod)
				.brtrue(arrayNotNullLabel)
				.ldarg_1
				.ldc_i4_m1
				.call(writeIntMethod)
				.br(arrayNullLabel);
			serializerEmit
				.MarkLabel(arrayNotNullLabel)
				.ldc_i4_0
				.stloc(iLocal)
				.ldarg_0
				.call(getPropertyMethod)
				.ldlen
				.conv_i4
				.stloc(lenLocal)
				.ldarg_1
				.ldloc(lenLocal)
				.call(writeIntMethod)
				.br(endWhileLabel)
				.MarkLabel(whileLabel);
			#region string
			if (elementType == typeof(string))
			{
				var strElement = serializerEmit.DeclareLocal(typeof(string));
				var strElementNullLbl = serializerEmit.DefineLabel();
				var strElementNotNullLbl = serializerEmit.DefineLabel();
				serializerEmit
					.ldarg_0
					.call(getPropertyMethod)
					.ldloc(iLocal)
					.ldelem_ref
					.stloc(strElement)
					.ldloc(strElement)
					.brtrue(strElementNotNullLbl)
					.ldarg_1
					.ldc_i4_m1
					.call(writeIntMethod)
					.br(strElementNullLbl)
					.MarkLabel(strElementNotNullLbl)
					.ldarg_1
					.ldloc(strElement)
					.call(typeof(string).GetMethod("get_Length"))
					.call(writeIntMethod)
					.ldarg_1
					.ldloc(strElement)
					.call(writeStringMethod)
					.MarkLabel(strElementNullLbl)
					.ldloc(iLocal)
					.ldc_i4_1
					.add
					.stloc(iLocal);
			}
			#endregion

			#region Primitive, DateTime, decimal
			else if (elementType.IsPrimitive || elementType == typeof(DateTime) || elementType == typeof(decimal))
			{
				serializerEmit
					.ldarg_1
					.ldarg_0
					.call(getPropertyMethod)
					.ldloc(iLocal)
					.dup
					.ldc_i4_1
					.add
					.stloc(iLocal);
				if (elementType == typeof(bool) || elementType == typeof(sbyte))
				{
					serializerEmit = serializerEmit.ldelem_i1;
				}
				else if (elementType == typeof(byte))
				{
					serializerEmit = serializerEmit.ldelem_u1;
				}
				else if (elementType == typeof(char) || elementType == typeof(ushort))
				{
					serializerEmit = serializerEmit.ldelem_u2;
				}
				else if (elementType == typeof(short))
				{
					serializerEmit = serializerEmit.ldelem_i2;
				}
				else if (elementType == typeof(int))
				{
					serializerEmit = serializerEmit.ldelem_i4;
				}
				else if (elementType == typeof(uint))
				{
					serializerEmit = serializerEmit.ldelem_u4;
				}
				else if (elementType == typeof(float))
				{
					serializerEmit = serializerEmit.ldelem_r4;
				}
				else if (elementType == typeof(long) || elementType == typeof(ulong))
				{
					serializerEmit = serializerEmit.ldelem_i8;
				}
				else if (elementType == typeof(double))
				{
					serializerEmit = serializerEmit.ldelem_r8;
				}
				else if (elementType == typeof(decimal))
				{
					serializerEmit = serializerEmit.ldelema(elementType).ldobj(elementType);
				}
				else if (elementType == typeof(DateTime))
				{
					serializerEmit = serializerEmit.ldelema(elementType).call(typeof(DateTime).GetMethod("get_Ticks"));
				}
				var writeElementType = elementType != typeof(DateTime) ? elementType : typeof(long);
				serializerEmit
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { writeElementType }));
			}
			#endregion
			
			#region object
			else if (elementType.IsClass)
			{
				var obj = serializerEmit.DeclareLocal(typeof(object));
				var elementNullLabel = serializerEmit.DefineLabel();
				var elementNotNullLabel = serializerEmit.DefineLabel();
				var objectExistsLocal = serializerEmit.DefineLabel();
				var objectNotExistsLocal = serializerEmit.DefineLabel();
				serializerEmit
					.ldarg_0
					.call(getPropertyMethod)
					.ldloc(iLocal)
					.ldelem_ref
					.stloc(obj)
					.ldloc(obj)
					.brtrue(elementNotNullLabel)
					.ldarg_1
					.ldc_i4_m1
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
					.br(elementNullLabel)
					.MarkLabel(elementNotNullLabel)
					.ldarg_2
					.ldloc(obj)
					.call(typeof(IDictionary<object, int>).GetMethod("ContainsKey", new[] { typeof(object) }))
					.brtrue(objectExistsLocal)
					.ldarg_1
					.ldarg_3
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
					.ldloc(obj)
					.castclass(typeof(ISerializable))
					.ldarg_1
					.ldarg_2
					.ldarg_3
					.call(typeof(ISerializable).GetMethod("Serialize"))
					.br(objectNotExistsLocal)
					.MarkLabel(objectExistsLocal)
					.ldarg_1
					.ldarg_2
					.ldloc(obj)
					.call(typeof(IDictionary<object, int>).GetMethod("get_Item", new[] { typeof(object) }))
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
					.MarkLabel(objectNotExistsLocal)
					.MarkLabel(elementNullLabel)
					.ldloc(iLocal)
					.ldc_i4_1
					.add
					.stloc(iLocal);
			}
			#endregion

			serializerEmit
				.MarkLabel(endWhileLabel)
				.ldloc(iLocal)
				.ldloc(lenLocal)
				.blt(whileLabel)
				.MarkLabel(arrayNullLabel);

			var setPropertyMethod = property.GetSetMethod();
			var readIntMethod = typeof(BinaryReader).GetMethod("ReadInt32");
			var arrayNotNullLabel2 = deserializeEmit.DefineLabel();
			var arrayNullLabel2 = deserializeEmit.DefineLabel();
			var arrayLocal = deserializeEmit.DeclareLocal(property.PropertyType);
			var len = deserializeEmit.DeclareLocal(typeof (int));
			var i = deserializeEmit.DeclareLocal(typeof (int));
			var beginForLabel = deserializeEmit.DefineLabel();
			var beginForBodyLabel = deserializeEmit.DefineLabel();
			deserializeEmit = deserializeEmit
				.ldarg_1
				.call(readIntMethod)
				.stloc(len)
				.ldloc(len)
				.ldc_i4_m1
				.ceq
				.brfalse(arrayNotNullLabel2)
				.ldarg_0
				.ldnull
				.call(setPropertyMethod)
				.br(arrayNullLabel2)
				.MarkLabel(arrayNotNullLabel2)
				.ldloc(len)
				.newarr(elementType)
				.stloc(arrayLocal)
				.ldc_i4_0
				.stloc(i)
				.br(beginForLabel)
				.MarkLabel(beginForBodyLabel);
			#region Primitive, decimal, DateTime
			if (elementType.IsPrimitive)
			{
				deserializeEmit
					.ldloc(arrayLocal)
					.ldloc(i)
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("Read" + elementType.Name));
				if (elementType == typeof(bool) || elementType == typeof(sbyte) || elementType == typeof(byte))
				{
					deserializeEmit = deserializeEmit.stelem_i1;
				}
				else if (elementType == typeof(char) || elementType == typeof(ushort) || elementType == typeof(short))
				{
					deserializeEmit = deserializeEmit.stelem_i2;
				}
				else if (elementType == typeof(int) || elementType == typeof(uint))
				{
					deserializeEmit = deserializeEmit.stelem_i4;
				}
				else if (elementType == typeof(float))
				{
					deserializeEmit = deserializeEmit.stelem_r4;
				}
				else if (elementType == typeof(long) || elementType == typeof(ulong))
				{
					deserializeEmit = deserializeEmit.stelem_i8;
				}
				else if (elementType == typeof(double))
				{
					deserializeEmit = deserializeEmit.stelem_r8;
				}
			}
			else if (elementType == typeof(decimal))
			{
				deserializeEmit
					.ldloc(arrayLocal)
					.ldloc(i)
					.ldelema(typeof(decimal))
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("ReadDecimal"))
					.stobj(typeof(decimal));
			}
			else if (elementType == typeof(DateTime))
			{
				deserializeEmit
					.ldloc(arrayLocal)
					.ldloc(i)
					.ldelema(typeof(DateTime))
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("ReadInt64"))
					.newobj(typeof(DateTime), typeof(long))
					.stobj(typeof(DateTime));
			}
			#endregion

			#region string
			else if (elementType == typeof(string))
			{
				var strLen = deserializeEmit.DeclareLocal(typeof(int));
				var strNullLabel = deserializeEmit.DefineLabel();
				var strNotNullLabel = deserializeEmit.DefineLabel();
				deserializeEmit
					.ldarg_1
					.call(readIntMethod)
					.stloc(strLen)
					.ldloc(strLen)
					.ldc_i4_m1
					.ceq
					.brfalse(strNotNullLabel)
					.ldloc(arrayLocal)
					.ldloc(i)
					.ldelema(typeof(string))
					.ldnull
					.stobj(typeof(string))
					.br(strNullLabel)
					.MarkLabel(strNotNullLabel)
					.ldloc(arrayLocal)
					.ldloc(i)
					.ldelema(typeof(string))
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("ReadString"))
					.stobj(typeof(string))
					.MarkLabel(strNullLabel);
			}
			#endregion

			#region object
			var desElementNullLabel = deserializeEmit.DefineLabel();
			var desElementNotNullLabel = deserializeEmit.DefineLabel();
			var propertyInstanceType = deserializeEmit.DeclareLocal(typeof(Type));
			var indexLocal = deserializeEmit.DeclareLocal(typeof(int));
			var desobjectExistsLocal = deserializeEmit.DefineLabel();
			var desobjectNotExistsLocal = deserializeEmit.DefineLabel();
			deserializeEmit
				.ldarg_1
				.call(typeof(BinaryReader).GetMethod("ReadInt32"))
				.stloc(indexLocal)
				.ldloc(indexLocal)
				.ldc_i4_m1
				.ceq
				.brfalse(desElementNotNullLabel)
				.ldloc(arrayLocal)
				.ldloc(i)
				.ldelema(elementType)
				.ldnull
				.stobj(elementType)
				.br(desElementNullLabel)
				.MarkLabel(desElementNotNullLabel)
				.ldarg_2
				.ldloc(indexLocal)
				.call(typeof(IDictionary<int, object>).GetMethod("ContainsKey", new Type[] { typeof(int) }))
				.brfalse(desobjectNotExistsLocal)
				.ldloc(arrayLocal)
				.ldloc(i)
				.ldelema(elementType)
                .ldarg_2
				.ldloc(indexLocal)
				.call(typeof(IDictionary<int, object>).GetMethod("get_Item", new Type[] { typeof(int) }))
				.castclass(elementType)
				.stobj(elementType)
				.br(desobjectExistsLocal)
				.MarkLabel(desobjectNotExistsLocal)
				.ldarg_1
				.call(typeof(BinaryReader).GetMethod("ReadString"))
				.call(typeof(Type).GetMethod("GetType", new Type[] { typeof(string) }))
				.stloc(propertyInstanceType)
				.ldloc(arrayLocal)
				.ldloc(i)
				.ldelema(elementType)
				.ldloc(propertyInstanceType)
				.call(typeof(Activator).GetMethod("CreateInstance", new Type[] { typeof(Type) }))
				.castclass(elementType)
				.stobj(elementType)
				.ldarg_2
				.ldloc(indexLocal)
				.ldloc(arrayLocal)
				.ldloc(i)
				.ldelem_ref
				.call(typeof(IDictionary<int, object>).GetMethod("Add", new[] { typeof(int), typeof(object) }))
				.ldloc(arrayLocal)
				.ldloc(i)
				.ldelem_ref
				.castclass(typeof(ISerializable))
				.ldarg_1
				.ldarg_2
				.call(typeof(ISerializable).GetMethod("Deserialize", new Type[] { typeof(BinaryReader), typeof(IDictionary<int, object>) }))
				.MarkLabel(desobjectExistsLocal)
				.MarkLabel(desElementNullLabel);
			#endregion

			deserializeEmit
				.ldloc(i)
				.ldc_i4_1
				.add
				.stloc(i)
				.MarkLabel(beginForLabel)
				.ldloc(i)
				.ldloc(len)
				.clt
				.brtrue(beginForBodyLabel)
				.ldarg_0
				.ldloc(arrayLocal)
				.call(setPropertyMethod)
				.MarkLabel(arrayNullLabel2);
		}
	}

	internal class IListGenericEmitter : SerializeEmitter
	{
		public void Emit(PropertyInfo property, ref EmitHelper serializerEmit, ref EmitHelper deserializeEmit)
		{
			var propertyType = property.PropertyType;
			var writeStringMethod = typeof(BinaryWriter).GetMethod("Write", new[] { typeof(string) });
			var writeIntMethod = typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) });
			var getPropertyMethod = property.GetGetMethod();
			var listNotNullLabel = serializerEmit.DefineLabel();
			var listNullLabel = serializerEmit.DefineLabel();
			var whileLabel = serializerEmit.DefineLabel();
			var endWhileLabel = serializerEmit.DefineLabel();
			var elementType = property.PropertyType.GetGenericArguments()[0];
			var iLocal = serializerEmit.DeclareLocal(typeof(int));
			var lenLocal = serializerEmit.DeclareLocal(typeof(int));
			//construct ICollection<elementType> to call getCount method
			var icollectionType = typeof (ICollection<>).MakeGenericType(elementType);
			var getCountMethod = icollectionType.GetMethod("get_Count");
			serializerEmit
				.ldarg_0
				.call(getPropertyMethod)
				.brtrue(listNotNullLabel)
				.ldarg_1
				.ldc_i4_m1
				.call(writeIntMethod)
				.br(listNullLabel)
                .MarkLabel(listNotNullLabel)
				.ldc_i4_0
				.stloc(iLocal)
				.ldarg_0
				.call(getPropertyMethod)
				.call(getCountMethod)
				.stloc(lenLocal)
				.ldarg_1
				.ldloc(lenLocal)
				.call(writeIntMethod)
				.br(endWhileLabel)
				.MarkLabel(whileLabel);
			if (elementType == typeof(string))
			{
				var strElement = serializerEmit.DeclareLocal(typeof(string));
				var strElementNullLbl = serializerEmit.DefineLabel();
				var strElementNotNullLbl = serializerEmit.DefineLabel();
				serializerEmit
					.ldarg_0
					.call(getPropertyMethod)
					.ldloc(iLocal)
					.call(propertyType.GetMethod("get_Item"))
					.stloc(strElement)
					.ldloc(strElement)
					.brtrue(strElementNotNullLbl)
					.ldarg_1
					.ldc_i4_m1
					.call(writeIntMethod)
					.br(strElementNullLbl)
					.MarkLabel(strElementNotNullLbl)
					.ldarg_1
					.ldloc(strElement)
					.call(typeof(string).GetMethod("get_Length"))
					.call(writeIntMethod)
					.ldarg_1
					.ldloc(strElement)
					.call(writeStringMethod)
					.MarkLabel(strElementNullLbl)
					.ldloc(iLocal)
					.ldc_i4_1
					.add
					.stloc(iLocal);
			}
			else if (elementType.IsPrimitive || elementType == typeof(DateTime) || elementType == typeof(decimal))
			{
				serializerEmit
					.ldarg_1
					.ldarg_0
					.call(getPropertyMethod)
					.ldloc(iLocal)
					.dup
					.ldc_i4_1
					.add
					.stloc(iLocal)
					.call(propertyType.GetMethod("get_Item"));
				var writeElementType = elementType != typeof(DateTime) ? elementType : typeof(long);
				serializerEmit
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { writeElementType }));
			}
			else if (elementType.IsClass)
			{
				var obj = serializerEmit.DeclareLocal(typeof(object));
				var elementNullLabel = serializerEmit.DefineLabel();
				var elementNotNullLabel = serializerEmit.DefineLabel();
				var objectExistsLocal = serializerEmit.DefineLabel();
				var objectNotExistsLocal = serializerEmit.DefineLabel();
				serializerEmit
					.ldarg_0
					.call(getPropertyMethod)
					.ldloc(iLocal)
					.call(propertyType.GetMethod("get_Item"))
					.stloc(obj)
					.ldloc(obj)
					.brtrue(elementNotNullLabel)
					.ldarg_1
					.ldc_i4_m1
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
					.br(elementNullLabel)
					.MarkLabel(elementNotNullLabel)
					.ldarg_2
					.ldloc(obj)
					.call(typeof(IDictionary<object, int>).GetMethod("ContainsKey", new[] { typeof(object) }))
					.brtrue(objectExistsLocal)
					.ldarg_1
					.ldarg_3
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
					.ldloc(obj)
					.castclass(typeof(ISerializable))
					.ldarg_1
					.ldarg_2
					.ldarg_3
					.call(typeof(ISerializable).GetMethod("Serialize"))
					.br(objectNotExistsLocal)
					.MarkLabel(objectExistsLocal)
					.ldarg_1
					.ldarg_2
					.ldloc(obj)
					.call(typeof(IDictionary<object, int>).GetMethod("get_Item", new[] { typeof(object) }))
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
					.MarkLabel(objectNotExistsLocal)
					.MarkLabel(elementNullLabel)
					.ldloc(iLocal)
					.ldc_i4_1
					.add
					.stloc(iLocal);
			}
			else
			{
				throw new ArgumentException("Element type does not supported: " + elementType);
			}
			serializerEmit
				.MarkLabel(endWhileLabel)
				.ldloc(iLocal)
				.ldloc(lenLocal)
				.blt(whileLabel)
				.MarkLabel(listNullLabel);

			var setPropertyMethod = property.GetSetMethod();
			var readIntMethod = typeof(BinaryReader).GetMethod("ReadInt32");
			var addElementMethod = typeof(ICollection<>).MakeGenericType(elementType).GetMethod("Add");
			var listNotNullLabel2 = deserializeEmit.DefineLabel();
			var listNullLabel2 = deserializeEmit.DefineLabel();
			var listLocal = deserializeEmit.DeclareLocal(property.PropertyType);
			var len = deserializeEmit.DeclareLocal(typeof(int));
			var i = deserializeEmit.DeclareLocal(typeof(int));
			var beginForLabel = deserializeEmit.DefineLabel();
			var beginForBodyLabel = deserializeEmit.DefineLabel();
			var listType = propertyType.IsInterface ? typeof (List<>).MakeGenericType(elementType) : propertyType;
			deserializeEmit = deserializeEmit
				.ldarg_1
				.call(readIntMethod)
				.stloc(len)
				.ldloc(len)
				.ldc_i4_m1
				.ceq
				.brfalse(listNotNullLabel2)
				.ldarg_0
				.ldnull
				.call(setPropertyMethod)
				.br(listNullLabel2)
				.MarkLabel(listNotNullLabel2)
				.ldloc(len)
				.newobj(listType, typeof(int))
				.stloc(listLocal)
				.ldc_i4_0
				.stloc(i)
				.br(beginForLabel)
				.MarkLabel(beginForBodyLabel);
			if (elementType.IsPrimitive)
			{
				deserializeEmit
					.ldloc(listLocal)
					.ldarg_1
					.call(typeof (BinaryReader).GetMethod("Read" + elementType.Name))
					.call(addElementMethod);
			}
			else if (elementType == typeof(DateTime))
			{
				deserializeEmit
					.ldloc(listLocal)
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("ReadInt64"))
					.newobj(typeof(DateTime), typeof(long))
					.call(addElementMethod);
			}
			else if (elementType == typeof(string))
			{
				var strLen = deserializeEmit.DeclareLocal(typeof(int));
				var strNullLabel = deserializeEmit.DefineLabel();
				var strNotNullLabel = deserializeEmit.DefineLabel();
				deserializeEmit
					.ldarg_1
					.call(readIntMethod)
					.stloc(strLen)
					.ldloc(strLen)
					.ldc_i4_m1
					.ceq
					.brfalse(strNotNullLabel)
					.ldloc(listLocal)
					.ldnull
					.call(addElementMethod)
					.br(strNullLabel)
					.MarkLabel(strNotNullLabel)
					.ldloc(listLocal)
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("ReadString"))
					.call(addElementMethod)
					.MarkLabel(strNullLabel);
			}
			else if (elementType.IsClass)
			{
				var desElementNullLabel = deserializeEmit.DefineLabel();
				var desElementNotNullLabel = deserializeEmit.DefineLabel();
				var propertyInstanceType = deserializeEmit.DeclareLocal(typeof(Type));
				var indexLocal = deserializeEmit.DeclareLocal(typeof(int));
				var desobjectExistsLocal = deserializeEmit.DefineLabel();
				var desobjectNotExistsLocal = deserializeEmit.DefineLabel();
				var tmpObjectLocal = deserializeEmit.DeclareLocal(elementType);
				var desTypeName = deserializeEmit.DeclareLocal(typeof (string));
				var desTypeNullLabel = deserializeEmit.DefineLabel();
				var desTypeNotNullLabel = deserializeEmit.DefineLabel();
				deserializeEmit
					.ldarg_1
					.call(typeof (BinaryReader).GetMethod("ReadInt32"))
					.stloc(indexLocal)
					.ldloc(indexLocal)
					.ldc_i4_m1
					.ceq
					.brfalse(desElementNotNullLabel)
					.ldloc(listLocal)
					.ldnull
					.call(addElementMethod)
					.br(desElementNullLabel)
					.MarkLabel(desElementNotNullLabel)
					.ldarg_2
					.ldloc(indexLocal)
					.call(typeof (IDictionary<int, object>).GetMethod("ContainsKey", new Type[] {typeof (int)}))
					.brfalse(desobjectNotExistsLocal)
					.ldloc(listLocal)
					.ldarg_2
					.ldloc(indexLocal)
					.callvirt(typeof (IDictionary<int, object>).GetMethod("get_Item", new Type[] {typeof (int)}))
					.castclass(elementType)
					.call(addElementMethod)
					.br(desobjectExistsLocal)
					.MarkLabel(desobjectNotExistsLocal)
					.ldarg_1
					.call(typeof (BinaryReader).GetMethod("ReadString"))
					.stloc(desTypeName)
					.ldloc(desTypeName)
					.ldc_i4_1
					.call(typeof (Type).GetMethod("GetType", new Type[] {typeof (string), typeof(bool)}))
                    .stloc(propertyInstanceType)
					.ldloc(propertyInstanceType)
					.call(typeof (Activator).GetMethod("CreateInstance", new Type[] {typeof (Type)}))
					.castclass(elementType)
					.stloc(tmpObjectLocal)
					.ldarg_2
					.ldloc(indexLocal)
					.ldloc(tmpObjectLocal)
					.call(typeof(IDictionary<int, object>).GetMethod("Add", new[] { typeof(int), typeof(object) }))
					.ldloc(tmpObjectLocal)
					.castclass(typeof (ISerializable))
					.ldarg_1
					.ldarg_2
					.call(typeof (ISerializable).GetMethod("Deserialize",
					                                       new Type[] {typeof (BinaryReader), typeof (IDictionary<int, object>)}))
					.ldloc(listLocal)
					.ldloc(tmpObjectLocal)
					.call(addElementMethod)
					.MarkLabel(desobjectExistsLocal)
					.MarkLabel(desElementNullLabel);
			}
			deserializeEmit
				.ldloc(i)
				.ldc_i4_1
				.add
				.stloc(i)
				.MarkLabel(beginForLabel)
				.ldloc(i)
				.ldloc(len)
				.clt
				.brtrue(beginForBodyLabel)
				.ldarg_0
				.ldloc(listLocal)
				.call(setPropertyMethod)
				.MarkLabel(listNullLabel2);
		}
	}

	internal class IDictGenericEmitter : SerializeEmitter
	{
		public void Emit(PropertyInfo property, ref EmitHelper serializerEmit, ref EmitHelper deserializeEmit)
		{
			var propertyType = property.PropertyType;
			var writeIntMethod = typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) });
			var getPropertyMethod = property.GetGetMethod();
			var dictNotNullLabel = serializerEmit.DefineLabel();
			var dictNullLabel = serializerEmit.DefineLabel();
			var keyType = property.PropertyType.GetGenericArguments()[0];
			if (!keyType.IsPrimitive && !(keyType == typeof(DateTime)) && !(keyType == typeof(string)) && !(keyType == typeof(decimal)))
			{
				throw new ArgumentException("Key type does not supported: " + keyType);
			}
			var valueType = property.PropertyType.GetGenericArguments()[1];
			if (!valueType.IsPrimitive && !(valueType == typeof(DateTime)) && !(valueType == typeof(string)) && !(valueType == typeof(decimal)))
			{
				throw new ArgumentException("Value type does not supported: " + valueType);
			}
			var dictLocal = serializerEmit.DeclareLocal(propertyType);
			//construct ICollection<elementType> to call getCount method
			var keyValuePairType = typeof (KeyValuePair<,>);
			keyValuePairType = keyValuePairType.MakeGenericType(keyType, valueType);
			var getKeyMethod = keyValuePairType.GetMethod("get_Key");
			var getValueMethod = keyValuePairType.GetMethod("get_Value");
			var icollectionType = typeof(ICollection<>).MakeGenericType(keyValuePairType);
			var getCountMethod = icollectionType.GetMethod("get_Count");
			var enumeratorType = typeof (IEnumerator<>).MakeGenericType(keyValuePairType);
			var getEmumeratorMethod = typeof(IEnumerable<>).MakeGenericType(keyValuePairType).GetMethod("GetEnumerator");
			var getCurrentElementMethod = enumeratorType.GetMethod("get_Current");
			var keyValuePairLocal = serializerEmit.DeclareLocal(keyValuePairType);
			var enumeratorLocal = serializerEmit.DeclareLocal(enumeratorType);
			var startWhileLocal = serializerEmit.DefineLabel();
			var startWhileBodyLocal = serializerEmit.DefineLabel();
			var valueLocal = serializerEmit.DeclareLocal(valueType);
			var endFinallyLabel = serializerEmit.DefineLabel();
			serializerEmit
				.ldarg_0
				.call(getPropertyMethod)
				.stloc(dictLocal)
				.ldloc(dictLocal)
				.brtrue(dictNotNullLabel)
				.ldarg_1
				.ldc_i4_m1
				.call(writeIntMethod)
				.br(dictNullLabel)
				.MarkLabel(dictNotNullLabel);
				var exTryCatchFinallyLabel = serializerEmit.BeginExceptionBlock();
			serializerEmit
				.ldloc(dictLocal)
				.call(getEmumeratorMethod)
				.stloc(enumeratorLocal)
				.ldarg_1
				.ldloc(dictLocal)
				.call(getCountMethod)
				.call(writeIntMethod)
				.br(startWhileLocal)
				.MarkLabel(startWhileBodyLocal)
				.ldloc(enumeratorLocal)
				.call(getCurrentElementMethod)
				.stloc(keyValuePairLocal)
				.ldarg_1
				.ldloca(keyValuePairLocal)
				.call(getKeyMethod)
				.call(typeof(BinaryWriter).GetMethod("Write", new[] {keyType}));
			if(valueType == typeof(string))
			{
				var valueLocalNotNullLabel = serializerEmit.DefineLabel();
				var valueLocalNullLabel = serializerEmit.DefineLabel();
				serializerEmit
					.ldloca(keyValuePairLocal)
					.call(getValueMethod)
					.stloc(valueLocal)
					.ldloc(valueLocal)
					.brtrue(valueLocalNotNullLabel)
					.ldarg_1
					.ldc_i4_m1
					.call(typeof (BinaryWriter).GetMethod("Write", new[] {typeof (int)}))
					.br(valueLocalNullLabel)
					.MarkLabel(valueLocalNotNullLabel)
					.ldarg_1
					.ldloc(valueLocal)
					.call(typeof (string).GetMethod("get_Length"))
					.call(typeof (BinaryWriter).GetMethod("Write", new[] {typeof (int)}))
					.ldarg_1
					.ldloc(valueLocal)
					.call(typeof (BinaryWriter).GetMethod("Write", new[] {typeof (string)}))
					.MarkLabel(valueLocalNullLabel);
			}
			else if (valueType.IsPrimitive)
			{
				serializerEmit
					.ldarg_1
					.ldloca(keyValuePairLocal)
					.call(getValueMethod)
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { valueType }));
			}
			serializerEmit
				.MarkLabel(startWhileLocal)
				.ldloc(enumeratorLocal)
				.call(typeof(IEnumerator).GetMethod("MoveNext"))
				.brtrue(startWhileBodyLocal)
				.leave(exTryCatchFinallyLabel)
				.BeginFinallyBlock()
				.ldloc(enumeratorLocal)
				.brfalse(endFinallyLabel)
				.ldloc(enumeratorLocal)
				.call(typeof(IDisposable).GetMethod("Dispose"))
				.MarkLabel(endFinallyLabel)
				.EndExceptionBlock()
				.MarkLabel(dictNullLabel);

			var setPropertyMethod = property.GetSetMethod();
			var readIntMethod = typeof(BinaryReader).GetMethod("ReadInt32");
			var addElementMethod = typeof(IDictionary<, >).MakeGenericType(keyType, valueType).GetMethod("Add", new[] {keyType, valueType});
			var dictNotNullLabel2 = deserializeEmit.DefineLabel();
			var dictNullLabel2 = deserializeEmit.DefineLabel();
			var deserializeDictLocal = deserializeEmit.DeclareLocal(property.PropertyType);
			var len = deserializeEmit.DeclareLocal(typeof(int));
			var i = deserializeEmit.DeclareLocal(typeof(int));
			var beginForLabel = deserializeEmit.DefineLabel();
			var beginForBodyLabel = deserializeEmit.DefineLabel();

			var dictType = propertyType.IsInterface ? typeof (Dictionary<,>).MakeGenericType(keyType, valueType) : propertyType;
			var keyLocal = deserializeEmit.DeclareLocal(keyType);
			var valueLocal2 = deserializeEmit.DeclareLocal(valueType);

			deserializeEmit = deserializeEmit
				.ldarg_1
				.call(readIntMethod)
				.stloc(len)
				.ldloc(len)
				.ldc_i4_m1
				.ceq
				.brfalse(dictNotNullLabel2)
				.ldarg_0
				.ldnull
				.call(setPropertyMethod)
				.br(dictNullLabel2)
				.MarkLabel(dictNotNullLabel2)
				.ldloc(len)
				.newobj(dictType, typeof(int))
				.stloc(deserializeDictLocal)
				.ldc_i4_0
				.stloc(i)
				.br(beginForLabel)
				.MarkLabel(beginForBodyLabel);
			if (keyType.IsPrimitive || keyType == typeof(decimal))
			{
				deserializeEmit
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("Read" + keyType.Name))
					.stloc(keyLocal);
			}
			else if (keyType == typeof(DateTime))
			{
				deserializeEmit
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("ReadInt64"))
					.newobj(typeof(DateTime), typeof(long))
					.stloc(keyLocal);
			}
			else if (keyType == typeof(string))
			{
				deserializeEmit
					.ldloca(keyLocal)
					.ldarg_1
					.call(typeof (BinaryReader).GetMethod("ReadString"))
					.stobj(typeof (string));
			}
			if (valueType.IsPrimitive || valueType == typeof(decimal))
			{
				deserializeEmit
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("Read" + keyType.Name))
					.stloc(valueLocal2);
			}
			else if (valueType == typeof(DateTime))
			{
				deserializeEmit
					.ldarg_1
					.call(typeof (BinaryReader).GetMethod("ReadInt64"))
					.newobj(typeof (DateTime), typeof (long))
					.stloc(valueLocal2);
			}
			else if (valueType == typeof(string))
			{
				var strNullLabel = deserializeEmit.DefineLabel();
				var strNotNullLabel = deserializeEmit.DefineLabel();
				deserializeEmit
					.ldarg_1
					.call(readIntMethod)
					.ldc_i4_m1
					.ceq
					.brfalse(strNotNullLabel)
					.ldnull
					.stloc(valueLocal2)
					.br(strNullLabel)
					.MarkLabel(strNotNullLabel)
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("ReadString"))
					.stloc(valueLocal2)
					.MarkLabel(strNullLabel);
			}
			deserializeEmit
				.ldloc(deserializeDictLocal)
				.ldloc(keyLocal)
				.ldloc(valueLocal2)
				.call(addElementMethod)
				.ldloc(i)
				.ldc_i4_1
				.add
				.stloc(i)
				.MarkLabel(beginForLabel)
				.ldloc(i)
				.ldloc(len)
				.clt
				.brtrue(beginForBodyLabel)
				.ldarg_0
				.ldloc(deserializeDictLocal)
				.call(setPropertyMethod)
				.MarkLabel(dictNullLabel2);

		}
	}

	internal class IObjectGenericEmitter : SerializeEmitter
	{
		public void EmitSerialize(PropertyInfo property, ref EmitHelper serializerEmit)
		{
			var obj = serializerEmit.DeclareLocal(typeof(object));
			var propertyNullLabel = serializerEmit.DefineLabel();
			var propertyNotNullLabel = serializerEmit.DefineLabel();
			var objectExistsLocal = serializerEmit.DefineLabel();
			var objectNotExistsLocal = serializerEmit.DefineLabel();
			serializerEmit
				.ldarg_0
				.call(property.GetGetMethod())
				.stloc(obj)
				.ldloc(obj)
				.brtrue(propertyNotNullLabel)
				.ldarg_1
				.ldc_i4_m1
				.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
				.br(propertyNullLabel)
				.MarkLabel(propertyNotNullLabel)
				.ldarg_2
				.ldloc(obj)
				.call(typeof(IDictionary<object, int>).GetMethod("ContainsKey", new[] { typeof(object) }))
				.brtrue(objectExistsLocal)
				.ldarg_1
				.ldarg_3
				.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
				.ldloc(obj)
				.castclass(typeof(ISerializable))
				.ldarg_1
				.ldarg_2
				.ldarg_3
				.call(typeof(ISerializable).GetMethod("Serialize"))
				.br(objectNotExistsLocal)
				.MarkLabel(objectExistsLocal)
				.ldarg_1
				.ldarg_2
				.ldloc(obj)
				.call(typeof(IDictionary<object, int>).GetMethod("get_Item", new[] { typeof(object) }))
				.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
				.MarkLabel(objectNotExistsLocal)
				.MarkLabel(propertyNullLabel);
		}

		public void EmitDeserialize(PropertyInfo property, ref EmitHelper deserializeEmit)
		{
			var propertyType = property.PropertyType;
			var propertyNullLabel = deserializeEmit.DefineLabel();
			var propertyNotNullLabel = deserializeEmit.DefineLabel();
			var propertyInstanceType = deserializeEmit.DeclareLocal(typeof(Type));
			var indexLocal = deserializeEmit.DeclareLocal(typeof(int));
			var objectExistsLocal = deserializeEmit.DefineLabel();
			var objectNotExistsLocal = deserializeEmit.DefineLabel();
			deserializeEmit
				.ldarg_1
				.call(typeof(BinaryReader).GetMethod("ReadInt32"))
				.stloc(indexLocal)
				.ldloc(indexLocal)
				.ldc_i4_m1
				.ceq
				.brfalse(propertyNotNullLabel)
				.ldarg_0
				.ldnull
				.call(property.GetSetMethod())
				.br(propertyNullLabel)
				.MarkLabel(propertyNotNullLabel)
				.ldarg_2
				.ldloc(indexLocal)
				.call(typeof(IDictionary<int, object>).GetMethod("ContainsKey", new Type[] { typeof(int) }))
				.brfalse(objectNotExistsLocal)
				.ldarg_0
				.ldarg_2
				.ldloc(indexLocal)
				.call(typeof(IDictionary<int, object>).GetMethod("get_Item", new Type[] { typeof(int) }))
				.castclass(propertyType)
				.call(property.GetSetMethod())
				.br(objectExistsLocal)
				.MarkLabel(objectNotExistsLocal)
				.ldarg_1
				.call(typeof(BinaryReader).GetMethod("ReadString"))
				.call(typeof(Type).GetMethod("GetType", new Type[] { typeof(string) }))
				.stloc(propertyInstanceType)
				.ldarg_0
				.ldloc(propertyInstanceType)
				.call(typeof(Activator).GetMethod("CreateInstance", new Type[] { typeof(Type) }))
				.castclass(propertyType)
				.call(property.GetSetMethod())
				.ldarg_2
				.ldloc(indexLocal)
				.ldarg_0
				.call(property.GetGetMethod())
				.call(typeof(IDictionary<int, object>).GetMethod("Add", new[] { typeof(int), typeof(object) }))
				.ldarg_0
				.call(property.GetGetMethod())
				.castclass(typeof(ISerializable))
				.ldarg_1
				.ldarg_2
				.call(typeof(ISerializable).GetMethod("Deserialize", new Type[] { typeof(BinaryReader), typeof(IDictionary<int, object>) }))
				.MarkLabel(objectExistsLocal)
				.MarkLabel(propertyNullLabel);
		}

		public void Emit(PropertyInfo property, ref EmitHelper serializerEmit, ref EmitHelper deserializeEmit)
		{
			EmitSerialize(property, ref serializerEmit);			
			EmitDeserialize(property, ref deserializeEmit);
		}
	}

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

		private static Type CreateType(Type type, AssemblyBuilderHelper assemblyBuilderHelper)
		{
			var typeName = type.Name;// +"Proxy";
			var typeNamespace = type.Namespace;
			var typeBuilderHelper = assemblyBuilderHelper.DefineType(typeNamespace + "." + typeName, type,
			                                                         typeof (ISerializable));
			typeBuilderHelper.SetCustomAttribute(typeof(SerializableAttribute));
			var referenceFieldBuilder = typeBuilderHelper.DefineField("reference", typeof (Reference), FieldAttributes.Private);
			var constructorEmit = typeBuilderHelper.DefaultConstructor.Emitter;
			var referenceDefaultInfo = typeof (Reference).GetField("NULL", BindingFlags.Public | BindingFlags.Static);
			var defaultConstructor = type.GetConstructor(Type.EmptyTypes);
			constructorEmit
				.ldarg_0
				.call(defaultConstructor)
				.ldarg_0
				.ldsfld(referenceDefaultInfo)
				.stfld(referenceFieldBuilder)
				.ret();

			constructorEmit = typeBuilderHelper.DefinePublicConstructor(type, typeof(IDictionary<object, ISerializable>)).Emitter;
			constructorEmit
				.ldarg_0
				.call(defaultConstructor)
				.ldarg_2
				.ldarg_1
				.ldarg_0
				.call(typeof(IDictionary<object, ISerializable>).GetMethod("Add", new[] {typeof(object), typeof(ISerializable)}))
				.ldarg_0
				.ldsfld(referenceDefaultInfo)
				.stfld(referenceFieldBuilder);
			var objectContainerBuilder = typeBuilderHelper.DefineField("objectContainer", typeof (ISerializable), FieldAttributes.Private);
			//Generate code for get_Reference 
			var emit = typeBuilderHelper.DefineMethod(typeof(ISerializable).GetProperty("Reference").GetGetMethod()).Emitter;
			emit
				.ldarg_0
				.ldfld(referenceFieldBuilder)
				.ret();
			//Generate code for set_Reference 
			emit = typeBuilderHelper.DefineMethod(typeof(ISerializable).GetProperty("Reference").GetSetMethod()).Emitter;
			emit
				.ldarg_0
				.ldarg_1
				.stfld(referenceFieldBuilder)
				.ret();
			//Generate code for get_ObjectContainer 
			emit = typeBuilderHelper.DefineMethod(typeof (ISerializable).GetProperty("ObjectContainer").GetGetMethod()).Emitter;
			emit
				.ldarg_0
				.ldfld(objectContainerBuilder)
				.ret();
			//Generate code for set_ObjectContainer
			emit = typeBuilderHelper.DefineMethod(typeof(ISerializable).GetProperty("ObjectContainer").GetSetMethod()).Emitter;
			emit
				.ldarg_0
				.ldarg_1
				.stfld(objectContainerBuilder)
				.ret();
			//Serialize
			var propperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			var serializerEmit = typeBuilderHelper.DefineMethod(typeof(ISerializable).GetMethod("Serialize", new Type[] { typeof(BinaryWriter), typeof(IDictionary<object, int>), typeof(int) })).Emitter;
			var deserializeEmit =
				typeBuilderHelper.DefineMethod(typeof(ISerializable).GetMethod("Deserialize", new Type[] { typeof(BinaryReader), typeof(IDictionary<int, object>) })).Emitter;
				
			
			//Write type name so we can construct it again
			serializerEmit
				.ldarg_1
				.ldstr(typeNamespace + "." + typeName + ", " + assemblyBuilderHelper.AssemblyName)
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
			//Serialize/Deserialize reference
			serializerEmit
				.ldarg_1
				.ldarg_0
				.ldfld(referenceFieldBuilder)
				.call(typeof(Reference).GetMethod("get_Pointer"))
				.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(byte[]) }));

			deserializeEmit
				.ldarg_0
				.ldarg_1
				.ldc_i4_s(12)
				.call(typeof(BinaryReader).GetMethod("ReadBytes"))
				.newobj(typeof(Reference), typeof(byte[]))
				.stfld(referenceFieldBuilder);
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
			constructorEmit.ret();
			serializerEmit.ret();
			deserializeEmit.ret();
			var serializableType = typeBuilderHelper.Create();
			SerializableTypeManager.Instance.Add(type, serializableType);
			return serializableType;
		}
	}

	public interface ISerializable
	{
		Reference Reference { get; set; }
		IObjectContainer ObjectContainer { get; set; }
		void Serialize(BinaryWriter writer, IDictionary<object, int> objectGraph, int index);
		void Deserialize(BinaryReader reader, IDictionary<int, object> objectGraph);
	}

}
