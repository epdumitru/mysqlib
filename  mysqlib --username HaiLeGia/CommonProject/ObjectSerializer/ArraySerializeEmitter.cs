using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BLToolkit.Reflection.Emit;

namespace ObjectSerializer
{
	internal class ArraySerializeEmitter : SerializeEmitter
	{
//		private IObjectGenericEmitter objectGenericEmitter = new IObjectGenericEmitter();
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
			var len = deserializeEmit.DeclareLocal(typeof(int));
			var i = deserializeEmit.DeclareLocal(typeof(int));
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
				.call(typeof(Converter).GetMethod("Convert", new[] { typeof(object) }))
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

		public void Emit(FieldInfo field, ref EmitHelper serializerEmit, ref EmitHelper deserializeEmit)
		{
			var writeIntMethod = typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) });
			var writeStringMethod = typeof(BinaryWriter).GetMethod("Write", new[] { typeof(string) });
			var arrayNotNullLabel = serializerEmit.DefineLabel();
			var arrayNullLabel = serializerEmit.DefineLabel();
			var whileLabel = serializerEmit.DefineLabel();
			var endWhileLabel = serializerEmit.DefineLabel();
			var elementType = field.FieldType.GetElementType();
			var iLocal = serializerEmit.DeclareLocal(typeof(int));
			var lenLocal = serializerEmit.DeclareLocal(typeof(int));
			serializerEmit
				.ldarg_0
				.ldfld(field)
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
				.ldfld(field)
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
					.ldfld(field)
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
					.ldfld(field)
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
					.ldfld(field)
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

			var readIntMethod = typeof(BinaryReader).GetMethod("ReadInt32");
			var arrayNotNullLabel2 = deserializeEmit.DefineLabel();
			var arrayNullLabel2 = deserializeEmit.DefineLabel();
			var arrayLocal = deserializeEmit.DeclareLocal(field.FieldType);
			var len = deserializeEmit.DeclareLocal(typeof(int));
			var i = deserializeEmit.DeclareLocal(typeof(int));
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
				.stfld(field)
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
				.stfld(field)
				.MarkLabel(arrayNullLabel2);
		}
	}
}
