using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BLToolkit.Reflection.Emit;

namespace ObjectSerializer
{
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
			var icollectionType = typeof(ICollection<>).MakeGenericType(elementType);
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
			var listType = propertyType.IsInterface ? typeof(List<>).MakeGenericType(elementType) : propertyType;
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
					.call(typeof(BinaryReader).GetMethod("Read" + elementType.Name))
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
				var desTypeName = deserializeEmit.DeclareLocal(typeof(string));
				deserializeEmit
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("ReadInt32"))
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
					.call(typeof(IDictionary<int, object>).GetMethod("ContainsKey", new Type[] { typeof(int) }))
					.brfalse(desobjectNotExistsLocal)
					.ldloc(listLocal)
					.ldarg_2
					.ldloc(indexLocal)
					.callvirt(typeof(IDictionary<int, object>).GetMethod("get_Item", new Type[] { typeof(int) }))
					.castclass(elementType)
					.call(addElementMethod)
					.br(desobjectExistsLocal)
					.MarkLabel(desobjectNotExistsLocal)
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("ReadString"))
					.stloc(desTypeName)
					.ldloc(desTypeName)
					.ldc_i4_1
					.call(typeof(Type).GetMethod("GetType", new Type[] { typeof(string), typeof(bool) }))
					.stloc(propertyInstanceType)
					.ldloc(propertyInstanceType)
					.call(typeof(Activator).GetMethod("CreateInstance", new Type[] { typeof(Type) }))
					.call(typeof(Converter).GetMethod("Convert", new[] { typeof(object) }))					
					.castclass(elementType)
					.stloc(tmpObjectLocal)
					.ldarg_2
					.ldloc(indexLocal)
					.ldloc(tmpObjectLocal)
					.call(typeof(IDictionary<int, object>).GetMethod("Add", new[] { typeof(int), typeof(object) }))
					.ldloc(tmpObjectLocal)
					.castclass(typeof(ISerializable))
					.ldarg_1
					.ldarg_2
					.call(typeof(ISerializable).GetMethod("Deserialize",
														   new Type[] { typeof(BinaryReader), typeof(IDictionary<int, object>) }))
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

		public void Emit(FieldInfo field, ref EmitHelper serializerEmit, ref EmitHelper deserializeEmit)
		{
			var fieldType = field.FieldType;
			var writeStringMethod = typeof(BinaryWriter).GetMethod("Write", new[] { typeof(string) });
			var writeIntMethod = typeof(BinaryWriter).GetMethod("Write", new[] { typeof(int) });
			var listNotNullLabel = serializerEmit.DefineLabel();
			var listNullLabel = serializerEmit.DefineLabel();
			var whileLabel = serializerEmit.DefineLabel();
			var endWhileLabel = serializerEmit.DefineLabel();
			var elementType = fieldType.GetGenericArguments()[0];
			var iLocal = serializerEmit.DeclareLocal(typeof(int));
			var lenLocal = serializerEmit.DeclareLocal(typeof(int));
			//construct ICollection<elementType> to call getCount method
			var icollectionType = typeof(ICollection<>).MakeGenericType(elementType);
			var getCountMethod = icollectionType.GetMethod("get_Count");
			serializerEmit
				.ldarg_0
				.ldfld(field)
				.brtrue(listNotNullLabel)
				.ldarg_1
				.ldc_i4_m1
				.call(writeIntMethod)
				.br(listNullLabel)
				.MarkLabel(listNotNullLabel)
				.ldc_i4_0
				.stloc(iLocal)
				.ldarg_0
				.ldfld(field)
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
					.ldfld(field)
					.ldloc(iLocal)
					.call(fieldType.GetMethod("get_Item"))
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
					.ldfld(field)
					.ldloc(iLocal)
					.dup
					.ldc_i4_1
					.add
					.stloc(iLocal)
					.call(fieldType.GetMethod("get_Item"));
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
					.ldfld(field)
					.ldloc(iLocal)
					.call(fieldType.GetMethod("get_Item"))
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

			var readIntMethod = typeof(BinaryReader).GetMethod("ReadInt32");
			var addElementMethod = typeof(ICollection<>).MakeGenericType(elementType).GetMethod("Add");
			var listNotNullLabel2 = deserializeEmit.DefineLabel();
			var listNullLabel2 = deserializeEmit.DefineLabel();
			var listLocal = deserializeEmit.DeclareLocal(fieldType);
			var len = deserializeEmit.DeclareLocal(typeof(int));
			var i = deserializeEmit.DeclareLocal(typeof(int));
			var beginForLabel = deserializeEmit.DefineLabel();
			var beginForBodyLabel = deserializeEmit.DefineLabel();
			var listType = fieldType.IsInterface ? typeof(List<>).MakeGenericType(elementType) : fieldType;
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
				.stfld(field)
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
					.call(typeof(BinaryReader).GetMethod("Read" + elementType.Name))
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
				var desTypeName = deserializeEmit.DeclareLocal(typeof(string));
				deserializeEmit
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("ReadInt32"))
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
					.call(typeof(IDictionary<int, object>).GetMethod("ContainsKey", new Type[] { typeof(int) }))
					.brfalse(desobjectNotExistsLocal)
					.ldloc(listLocal)
					.ldarg_2
					.ldloc(indexLocal)
					.callvirt(typeof(IDictionary<int, object>).GetMethod("get_Item", new Type[] { typeof(int) }))
					.castclass(elementType)
					.call(addElementMethod)
					.br(desobjectExistsLocal)
					.MarkLabel(desobjectNotExistsLocal)
					.ldarg_1
					.call(typeof(BinaryReader).GetMethod("ReadString"))
					.stloc(desTypeName)
					.ldloc(desTypeName)
					.ldc_i4_1
					.call(typeof(Type).GetMethod("GetType", new Type[] { typeof(string), typeof(bool) }))
					.stloc(propertyInstanceType)
					.ldloc(propertyInstanceType)
					.call(typeof(Activator).GetMethod("CreateInstance", new Type[] { typeof(Type) }))
					.castclass(elementType)
					.stloc(tmpObjectLocal)
					.ldarg_2
					.ldloc(indexLocal)
					.ldloc(tmpObjectLocal)
					.call(typeof(IDictionary<int, object>).GetMethod("Add", new[] { typeof(int), typeof(object) }))
					.ldloc(tmpObjectLocal)
					.castclass(typeof(ISerializable))
					.ldarg_1
					.ldarg_2
					.call(typeof(ISerializable).GetMethod("Deserialize",
														   new Type[] { typeof(BinaryReader), typeof(IDictionary<int, object>) }))
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
				.stfld(field)
				.MarkLabel(listNullLabel2);
		}
	}
}
