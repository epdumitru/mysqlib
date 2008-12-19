using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BLToolkit.Reflection.Emit;

namespace ObjectSerializer
{
	public interface SerializeEmitter
	{
		void Emit(PropertyInfo property, ref EmitHelper serializerEmit, ref EmitHelper deserializeEmit);
		void Emit(FieldInfo field, ref EmitHelper serializerEmit, ref EmitHelper deserializeEmit);
	}
}
