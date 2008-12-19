using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BLToolkit.Reflection.Emit;

namespace ObjectSerializer
{
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
				.call(typeof(Converter).GetMethod("Convert", new[] { typeof(object) }))
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

		public void Emit(FieldInfo field, ref EmitHelper serializerEmit, ref EmitHelper deserializeEmit)
		{
			EmitSerialize(field, ref serializerEmit);
			EmitDeserialize(field, ref deserializeEmit);
		}

		private void EmitDeserialize(FieldInfo field, ref EmitHelper deserializeEmit)
		{
			var fieldType = field.FieldType;
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
				.stfld(field)
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
				.castclass(fieldType)
				.stfld(field)
				.br(objectExistsLocal)
				.MarkLabel(objectNotExistsLocal)
				.ldarg_1
				.call(typeof(BinaryReader).GetMethod("ReadString"))
				.call(typeof(Type).GetMethod("GetType", new Type[] { typeof(string) }))
				.stloc(propertyInstanceType)
				.ldarg_0
				.ldloc(propertyInstanceType)
				.call(typeof(Activator).GetMethod("CreateInstance", new Type[] { typeof(Type) }))
				.call(typeof(Converter).GetMethod("Convert", new[] {typeof(object)}))
				.castclass(fieldType)
				.stfld(field)
				.ldarg_2
				.ldloc(indexLocal)
				.ldarg_0
				.ldfld(field)
				.call(typeof(IDictionary<int, object>).GetMethod("Add", new[] { typeof(int), typeof(object) }))
				.ldarg_0
				.ldfld(field)
				.castclass(typeof(ISerializable))
				.ldarg_1
				.ldarg_2
				.call(typeof(ISerializable).GetMethod("Deserialize", new Type[] { typeof(BinaryReader), typeof(IDictionary<int, object>) }))
				.MarkLabel(objectExistsLocal)
				.MarkLabel(propertyNullLabel);
		}

		private void EmitSerialize(FieldInfo field, ref EmitHelper serializerEmit)
		{
			var obj = serializerEmit.DeclareLocal(typeof(object));
			var propertyNullLabel = serializerEmit.DefineLabel();
			var propertyNotNullLabel = serializerEmit.DefineLabel();
			var objectExistsLocal = serializerEmit.DefineLabel();
			var objectNotExistsLocal = serializerEmit.DefineLabel();
			serializerEmit
				.ldarg_0
				.ldfld(field)
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
	}
}