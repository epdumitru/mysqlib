using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BLToolkit.Reflection.Emit;

namespace ObjectSerializer
{
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
			else if (propertyType == typeof(DateTime))
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
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
					.br(stringNullLabel)
					.MarkLabel(stringNotNullLabel)
					.ldarg_1
					.ldloc(stringLocal)
					.call(typeof(string).GetMethod("get_Length"))
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
					.ldarg_1
					.ldloc(stringLocal)
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(string) }))
					.MarkLabel(stringNullLabel);
				var valueNotNullLabel = deserializeEmit.DefineLabel();
				var valueNullLabel = deserializeEmit.DefineLabel();
				deserializeEmit
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("ReadInt32"))
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
					.call(typeof(BinaryReader).GetMethod("ReadString"))
					.call(setPropertyMethod)
					.MarkLabel(valueNullLabel);
			}
		}

		public void Emit(FieldInfo field, ref EmitHelper serializerEmit, ref EmitHelper deserializeEmit)
		{
			var fieldType = field.FieldType;
			var writerMethod = typeof(BinaryWriter).GetMethod("Write", new Type[] { fieldType });
			var readerMethod = typeof(BinaryReader).GetMethod("Read" + fieldType.Name);
			if (fieldType.IsPrimitive || fieldType == typeof(decimal))
			{
				serializerEmit
					.ldarg_1
					.ldarg_0
					.ldfld(field)
					.call(writerMethod);
				deserializeEmit
					.ldarg_0
					.ldarg_1
					.call(readerMethod)
					.stfld(field);
			}
			else if (fieldType == typeof(DateTime))
			{
				var writeLongMethod = typeof(BinaryWriter).GetMethod("Write", new[] { typeof(long) });
				var readLongMethod = typeof(BinaryReader).GetMethod("ReadInt64");
				serializerEmit.DeclareLocal(typeof(DateTime));
				serializerEmit
					.ldarg_1
					.ldarg_0
					.ldfld(field)
					.stloc_0
					.ldloca(0)
					.call(typeof(DateTime).GetMethod("get_Ticks"))
					.call(writeLongMethod);
				deserializeEmit
					.ldarg_0
					.ldarg_1
					.call(readLongMethod)
					.newobj(typeof(DateTime), new[] { typeof(long) })
					.stfld(field);
			}
			else if (fieldType == typeof(string))
			{
				var stringLocal = serializerEmit.DeclareLocal(fieldType);
				var stringNullLabel = serializerEmit.DefineLabel();
				var stringNotNullLabel = serializerEmit.DefineLabel();
				serializerEmit
					.ldarg_0
					.ldfld(field)
					.stloc(stringLocal)
					.ldloc(stringLocal)
					.brtrue(stringNotNullLabel)
					.ldarg_1
					.ldc_i4_m1
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
					.br(stringNullLabel)
					.MarkLabel(stringNotNullLabel)
					.ldarg_1
					.ldloc(stringLocal)
					.call(typeof(string).GetMethod("get_Length"))
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
					.ldarg_1
					.ldloc(stringLocal)
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(string) }))
					.MarkLabel(stringNullLabel);
				var valueNotNullLabel = deserializeEmit.DefineLabel();
				var valueNullLabel = deserializeEmit.DefineLabel();
				deserializeEmit
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("ReadInt32"))
					.ldc_i4_m1
					.ceq
					.brfalse(valueNotNullLabel)
					.ldarg_0
					.ldnull
					.stfld(field)
					.br(valueNullLabel)
					.MarkLabel(valueNotNullLabel)
					.ldarg_0
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("ReadString"))
					.stfld(field)
					.MarkLabel(valueNullLabel);
			}
		}
	}
}
