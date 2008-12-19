using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BLToolkit.Reflection.Emit;

namespace ObjectSerializer
{
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
			var keyValuePairType = typeof(KeyValuePair<,>);
			keyValuePairType = keyValuePairType.MakeGenericType(keyType, valueType);
			var getKeyMethod = keyValuePairType.GetMethod("get_Key");
			var getValueMethod = keyValuePairType.GetMethod("get_Value");
			var icollectionType = typeof(ICollection<>).MakeGenericType(keyValuePairType);
			var getCountMethod = icollectionType.GetMethod("get_Count");
			var enumeratorType = typeof(IEnumerator<>).MakeGenericType(keyValuePairType);
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
				.call(typeof(BinaryWriter).GetMethod("Write", new[] { keyType }));
			if (valueType == typeof(string))
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
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
					.br(valueLocalNullLabel)
					.MarkLabel(valueLocalNotNullLabel)
					.ldarg_1
					.ldloc(valueLocal)
					.call(typeof(string).GetMethod("get_Length"))
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
					.ldarg_1
					.ldloc(valueLocal)
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(string) }))
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
			var addElementMethod = typeof(IDictionary<,>).MakeGenericType(keyType, valueType).GetMethod("Add", new[] { keyType, valueType });
			var dictNotNullLabel2 = deserializeEmit.DefineLabel();
			var dictNullLabel2 = deserializeEmit.DefineLabel();
			var deserializeDictLocal = deserializeEmit.DeclareLocal(property.PropertyType);
			var len = deserializeEmit.DeclareLocal(typeof(int));
			var i = deserializeEmit.DeclareLocal(typeof(int));
			var beginForLabel = deserializeEmit.DefineLabel();
			var beginForBodyLabel = deserializeEmit.DefineLabel();

			var dictType = propertyType.IsInterface ? typeof(Dictionary<,>).MakeGenericType(keyType, valueType) : propertyType;
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
					.call(typeof(BinaryReader).GetMethod("ReadString"))
					.stobj(typeof(string));
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
					.call(typeof(BinaryReader).GetMethod("ReadInt64"))
					.newobj(typeof(DateTime), typeof(long))
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

		public void Emit(FieldInfo field, ref EmitHelper serializerEmit, ref EmitHelper deserializeEmit)
		{
			var fieldType = field.FieldType;
			var writeIntMethod = typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) });
			var dictNotNullLabel = serializerEmit.DefineLabel();
			var dictNullLabel = serializerEmit.DefineLabel();
			var keyType = fieldType.GetGenericArguments()[0];
			if (!keyType.IsPrimitive && !(keyType == typeof(DateTime)) && !(keyType == typeof(string)) && !(keyType == typeof(decimal)))
			{
				throw new ArgumentException("Key type does not supported: " + keyType);
			}
			var valueType = fieldType.GetGenericArguments()[1];
			if (!valueType.IsPrimitive && !(valueType == typeof(DateTime)) && !(valueType == typeof(string)) && !(valueType == typeof(decimal)))
			{
				throw new ArgumentException("Value type does not supported: " + valueType);
			}
			var dictLocal = serializerEmit.DeclareLocal(fieldType);
			//construct ICollection<elementType> to call getCount method
			var keyValuePairType = typeof(KeyValuePair<,>);
			keyValuePairType = keyValuePairType.MakeGenericType(keyType, valueType);
			var getKeyMethod = keyValuePairType.GetMethod("get_Key");
			var getValueMethod = keyValuePairType.GetMethod("get_Value");
			var icollectionType = typeof(ICollection<>).MakeGenericType(keyValuePairType);
			var getCountMethod = icollectionType.GetMethod("get_Count");
			var enumeratorType = typeof(IEnumerator<>).MakeGenericType(keyValuePairType);
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
				.ldfld(field)
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
				.call(typeof(BinaryWriter).GetMethod("Write", new[] { keyType }));
			if (valueType == typeof(string))
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
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
					.br(valueLocalNullLabel)
					.MarkLabel(valueLocalNotNullLabel)
					.ldarg_1
					.ldloc(valueLocal)
					.call(typeof(string).GetMethod("get_Length"))
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) }))
					.ldarg_1
					.ldloc(valueLocal)
					.call(typeof(BinaryWriter).GetMethod("Write", new[] { typeof(string) }))
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

			var readIntMethod = typeof(BinaryReader).GetMethod("ReadInt32");
			var addElementMethod = typeof(IDictionary<,>).MakeGenericType(keyType, valueType).GetMethod("Add", new[] { keyType, valueType });
			var dictNotNullLabel2 = deserializeEmit.DefineLabel();
			var dictNullLabel2 = deserializeEmit.DefineLabel();
			var deserializeDictLocal = deserializeEmit.DeclareLocal(fieldType);
			var len = deserializeEmit.DeclareLocal(typeof(int));
			var i = deserializeEmit.DeclareLocal(typeof(int));
			var beginForLabel = deserializeEmit.DefineLabel();
			var beginForBodyLabel = deserializeEmit.DefineLabel();

			var dictType = fieldType.IsInterface ? typeof(Dictionary<,>).MakeGenericType(keyType, valueType) : fieldType;
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
				.stfld(field)
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
					.call(typeof(BinaryReader).GetMethod("ReadString"))
					.stobj(typeof(string));
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
					.call(typeof(BinaryReader).GetMethod("ReadInt64"))
					.newobj(typeof(DateTime), typeof(long))
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
				.stfld(field)
				.MarkLabel(dictNullLabel2);
		}
	}
}
